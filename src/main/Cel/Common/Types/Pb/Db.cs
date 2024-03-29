﻿using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

/*
 * Copyright (C) 2022 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Cel.Common.Types.Pb;

using Descriptor = MessageDescriptor;
using Message = IMessage;

/// <summary>
///     Db maps from file / message / enum name to file description.
///     <para>
///         Each Db is isolated from each other, and while information about protobuf descriptors may be
///         fetched from the global protobuf registry, no descriptors are added to this registry, else the
///         isolation guarantees of the Db object would be violated.
///     </para>
/// </summary>
public sealed class Db
{
    /// <summary>
    ///     DefaultDb used at evaluation time or unless overridden at check time.
    /// </summary>
    public static readonly Db DefaultDb = new(new Dictionary<string, FileDescription>(),
        new List<FileDescription>());

    /// <summary>
    ///     files contains the deduped set of FileDescriptions whose types are contained in the pb.Db.
    /// </summary>
    private readonly IList<FileDescription> files;

    private readonly IDictionary<string, FileDescription> revFileDescriptorMap;

    static Db()
    {
        // Describe well-known types to ensure they can always be resolved by the check and interpret
        // execution phases.
        //
        // The following subset of message types is enough to ensure that all well-known types can
        // resolved in the runtime, since describing the value results in describing the whole file
        // where the message is declared.
        DefaultDb.RegisterMessage(new Any());
        DefaultDb.RegisterMessage(new Duration());
        DefaultDb.RegisterMessage(new Empty());
        DefaultDb.RegisterMessage(new Timestamp());
        DefaultDb.RegisterMessage(new Value());
        DefaultDb.RegisterMessage(new BoolValue());
    }

    private Db(IDictionary<string, FileDescription> revFileDescriptorMap, IList<FileDescription> files)
    {
        this.revFileDescriptorMap = revFileDescriptorMap;
        this.files = files;
    }

    /// <summary>
    ///     NewDb creates a new `pb.Db` with an empty type name to file description map.
    /// </summary>
    public static Db NewDb()
    {
        // The FileDescription objects in the default db contain lazily initialized TypeDescription
        // values which may point to the state contained in the DefaultDb irrespective of this shallow
        // copy; however, the type graph for a field is idempotently computed, and is guaranteed to
        // only be initialized once thanks to atomic values within the TypeDescription objects, so it
        // is safe to share these values across instances.
        return DefaultDb.Copy();
    }

    /// <summary>
    ///     Copy creates a copy of the current database with its own internal descriptor mapping.
    /// </summary>
    public Db Copy()
    {
        IDictionary<string, FileDescription> revFileDescriptorMap =
            new Dictionary<string, FileDescription>(this.revFileDescriptorMap);
        IList<FileDescription> files = new List<FileDescription>(this.files);
        return new Db(revFileDescriptorMap, files);
    }

    /// <summary>
    ///     FileDescriptions returns the set of file descriptions associated with this db.
    /// </summary>
    public IList<FileDescription> FileDescriptions()
    {
        return files;
    }

    /// <summary>
    ///     RegisterDescriptor produces a `FileDescription` from a `FileDescriptor` and registers the
    ///     message and enum types into the `pb.Db`.
    /// </summary>
    public FileDescription RegisterDescriptor(FileDescriptor fileDesc)
    {
        var path = Path(fileDesc);
        revFileDescriptorMap.TryGetValue(path, out var fd);
        if (fd != null) return fd;

        // Make sure to search the global registry to see if a protoreflect.FileDescriptor for
        // the file specified has been linked into the binary. If so, use the copy of the descriptor
        // from the global cache.
        //
        // Note: Proto reflection relies on descriptor values being object equal rather than object
        // equivalence. This choice means that a FieldDescriptor generated from a FileDescriptorProto
        // will be incompatible with the FieldDescriptor in the global registry and any message created
        // from that global registry.
        //
        //    globalFD := protoregistry.GlobalFiles.FindFileByPath(fileDesc.Path())
        //    if err == nil {
        //      fileDesc = globalFD
        //    }
        fd = FileDescription.NewFileDescription(fileDesc);
        foreach (var enumValName in fd.EnumNames) revFileDescriptorMap[enumValName] = fd;

        foreach (var msgTypeName in fd.TypeNames) revFileDescriptorMap[msgTypeName] = fd;

        revFileDescriptorMap[path] = fd;

        // Return the specific file descriptor registered.
        files.Add(fd);
        return fd;
    }

    private string Path(FileDescriptor fileDesc)
    {
        return fileDesc.Package + ':' + fileDesc.Name;
    }

    /// <summary>
    ///     RegisterMessage produces a `FileDescription` from a `message` and registers the message and all
    ///     other definitions within the message file into the `pb.Db`.
    /// </summary>
    public FileDescription RegisterMessage(Message message)
    {
        var msgDesc = message.Descriptor;
        var msgName = msgDesc.FullName;
        var typeName = FileDescription.SanitizeProtoName(msgName);
        revFileDescriptorMap.TryGetValue(typeName, out var fd);
        if (fd == null)
        {
            fd = RegisterDescriptor(msgDesc.File);
            revFileDescriptorMap[typeName] = fd;
        }

        DescribeType(typeName).UpdateReflectType(message);
        return fd;
    }

    /// <summary>
    ///     DescribeEnum takes a qualified enum name and returns an `EnumDescription` if it exists in the
    ///     `pb.Db`.
    /// </summary>
    public EnumValueDescription? DescribeEnum(string enumName)
    {
        enumName = FileDescription.SanitizeProtoName(enumName);
        revFileDescriptorMap.TryGetValue(enumName, out var fd);
        return fd != null ? fd.GetEnumDescription(enumName) : null;
    }

    /// <summary>
    ///     DescribeType returns a `TypeDescription` for the `typeName` if it exists in the `pb.Db`.
    /// </summary>
    public PbTypeDescription? DescribeType(string typeName)
    {
        typeName = FileDescription.SanitizeProtoName(typeName);
        revFileDescriptorMap.TryGetValue(typeName, out var fd);
        return fd != null ? fd.GetTypeDescription(typeName) : null;
    }

    /// <summary>
    ///     CollectFileDescriptorSet builds a file descriptor set associated with the file where the input
    ///     message is declared.
    /// </summary>
    public static ISet<FileDescriptor> CollectFileDescriptorSet(Message message)
    {
        ISet<FileDescriptor> fdMap = new HashSet<FileDescriptor>();
        var messageDesc = message.Descriptor;
        var messageFile = messageDesc.File;
        fdMap.Add(messageFile);
        fdMap.UnionWith(messageFile.PublicDependencies);

        //    parentFile = message.ProtoReflect().Descriptor().ParentFile()
        //    fdMap[parentFile.Path()] = parentFile
        //    // Initialize list of dependencies
        //    deps := make([]protoreflect.FileImport, parentFile.Imports().Len())
        //    for i := 0; i < parentFile.Imports().Len(); i++ {
        //      deps[i] = parentFile.Imports().Get(i)
        //    }
        //    // Expand list for new dependencies
        //    for i := 0; i < len(deps); i++ {
        //      dep := deps[i]
        //      if _, found := fdMap[dep.Path()]; found {
        //        continue
        //      }
        //      fdMap[dep.Path()] = dep.FileDescriptor
        //      for j := 0; j < dep.FileDescriptor.Imports().Len(); j++ {
        //        deps = append(deps, dep.FileDescriptor.Imports().Get(j))
        //      }
        //    }
        return fdMap;
    }

    public override string ToString()
    {
        return "Db{" + "revFileDescriptorMap.size=" + revFileDescriptorMap.Count + ", files=" + files.Count + '}';
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var db = (Db)o;
        return Equals(revFileDescriptorMap, db.revFileDescriptorMap) && Equals(files, db.files);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(revFileDescriptorMap, files);
    }
}