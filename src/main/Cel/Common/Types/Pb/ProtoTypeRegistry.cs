using System.Collections;
using Cel.Common.Types.Ref;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Enum = System.Enum;
using FieldType = Cel.Common.Types.Ref.FieldType;

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

public sealed class ProtoTypeRegistry : ITypeRegistry
{
    private readonly Db pbdb;
    private readonly IDictionary<string, IType> revTypeMap;

    private ProtoTypeRegistry(IDictionary<string, IType> revTypeMap, Db pbdb)
    {
        this.revTypeMap = revTypeMap;
        this.pbdb = pbdb;
    }

    /// <summary>
    ///     Copy implements the ref.TypeRegistry interface method which copies the current state of the
    ///     registry into its own memory space.
    /// </summary>
    public ITypeRegistry Copy()
    {
        return new ProtoTypeRegistry(new Dictionary<string, IType>(revTypeMap),
            pbdb.Copy());
    }

    public void Register(object t)
    {
        if (t is Message)
        {
            var fds = Db.CollectFileDescriptorSet((Message)t);
            foreach (var fd in fds) RegisterDescriptor(fd);

            RegisterMessage((Message)t);
        }
        else if (t is IType)
        {
            RegisterType((IType)t);
        }
        else
        {
            throw new Exception(string.Format("unsupported type: {0}", t.GetType().FullName));
        }
    }

    public IVal EnumValue(string enumName)
    {
        var enumVal = pbdb.DescribeEnum(enumName);
        if (enumVal == null) return Err.NewErr("unknown enum name '{0}'", enumName);

        return IntT.IntOf(enumVal.Value());
    }

    public FieldType? FindFieldType(string messageType, string fieldName)
    {
        var msgType = pbdb.DescribeType(messageType);
        if (msgType == null) return null;

        var field = msgType.FieldByName(fieldName);
        if (field == null) return null;

        return new FieldType(field.CheckedType(), field.HasField, field.GetField);
    }

    public IVal? FindIdent(string identName)
    {
        revTypeMap.TryGetValue(identName, out var t);
        if (t != null) return t;

        var enumVal = pbdb.DescribeEnum(identName);
        if (enumVal != null) return IntT.IntOf(enumVal.Value());

        return null;
    }

    public Google.Api.Expr.V1Alpha1.Type? FindType(string typeName)
    {
        if (pbdb.DescribeType(typeName) == null) return null;

        if (typeName.Length > 0 && typeName[0] == '.') typeName = typeName.Substring(1);

        var type = new Google.Api.Expr.V1Alpha1.Type();
        type.MessageType = typeName;
        var result = new Google.Api.Expr.V1Alpha1.Type();
        result.Type_ = type;
        return result;
    }

    public IVal NewValue(string typeName, IDictionary<string, IVal> fields)
    {
        var td = pbdb.DescribeType(typeName);
        if (td == null) return Err.UnknownType(typeName);

        var msg = td.NewMessageBuilder();
        var err = NewValueSetFields(fields, td, msg);
        if (err != null) return err;

        return NativeToValue(msg);
    }

    public void RegisterType(params IType[] types)
    {
        foreach (var t in types) revTypeMap[t.TypeName()] = t;
        // TODO: generate an error when the type name is registered more than once.
    }

    public TypeAdapter ToTypeAdapter()
    {
        return NativeToValue;
    }

    /// <summary>
    ///     NewRegistry accepts a list of proto message instances and returns a type provider which can
    ///     create new instances of the provided message or any message that proto depends upon in its
    ///     FileDescriptor.
    /// </summary>
    public static ProtoTypeRegistry NewRegistry(params Message[] types)
    {
        var p =
            new ProtoTypeRegistry(new Dictionary<string, IType>(), Db.NewDb());
        p.RegisterType(
            BoolT.BoolType,
            BytesT.BytesType,
            DoubleT.DoubleType,
            DurationT.DurationType,
            IntT.IntType,
            ListT.ListType,
            MapT.MapType,
            NullT.NullType,
            StringT.StringType,
            TimestampT.TimestampType,
            TypeT.TypeType,
            UintT.UintType);

        FileDescriptor[] fds =
        {
            DoubleValue.Descriptor.File,
            Empty.Descriptor.File,
            Timestamp.Descriptor.File,
            UInt64Value.Descriptor.File,
            Any.Descriptor.File,
            /*
            Google.Protobuf.WellKnownTypes.NullValue
            */
            Struct.Descriptor.File,
            StringValue.Descriptor.File,
            ListValue.Descriptor.File,
            BytesValue.Descriptor.File,
            Value.Descriptor.File,
            Int32Value.Descriptor.File,
            UInt32Value.Descriptor.File,
            Duration.Descriptor.File,
            FloatValue.Descriptor.File,
            BoolValue.Descriptor.File,
            Int64Value.Descriptor.File
        };
        ISet<FileDescriptor> pbDescriptors = new HashSet<FileDescriptor>(fds);
        foreach (var fDesc in pbDescriptors)
        {
            var fd = FileDescription.NewFileDescription(fDesc);
            p.RegisterAllTypes(fd);
        }

        // This block ensures that the well-known protobuf types are registered by default.
        foreach (var fd in p.pbdb.FileDescriptions()) p.RegisterAllTypes(fd);

        foreach (var msgType in types) p.RegisterMessage(msgType);

        return p;
    }

    /// <summary>
    ///     NewEmptyRegistry returns a registry which is completely unconfigured.
    /// </summary>
    public static ProtoTypeRegistry NewEmptyRegistry()
    {
        return new ProtoTypeRegistry(new Dictionary<string, IType>(), Db.NewDb());
    }

    private IVal? NewValueSetFields(IDictionary<string, IVal> fields, PbTypeDescription td, Message builder)
    {
        var fieldMap = td.FieldMap();
        foreach (var nv in fields)
        {
            var name = nv.Key;
            fieldMap.TryGetValue(name, out var field);
            if (field == null) return Err.NoSuchField(name);

            // TODO resolve inefficiency for maps: first converted from a MapT to a native Java map and
            //  then to a protobuf struct. The intermediate step (the Java map) could be omitted.

            var value = nv.Value.ConvertToNative(field.ReflectType())!;
            if (value.GetType().IsArray) value = new ArrayList((Array)value);

            var pbDesc = field.Descriptor();

            if (pbDesc.FieldType == Google.Protobuf.Reflection.FieldType.Enum)
                value = IntToProtoEnumValues(field, value);

            if (pbDesc.IsMap)
            {
                value = (IDictionary)ToProtoMapStructure(pbDesc, value);
                var map = (IDictionary)pbDesc.Accessor.GetValue(builder);
                foreach (DictionaryEntry entry in (IDictionary)value) map[entry.Key] = entry.Value;
            }
            else if (pbDesc.IsRepeated)
            {
                var list = (IList)pbDesc.Accessor.GetValue(builder);
                foreach (var o in (IList)value) list.Add(o);
            }
            else
            {
                pbDesc.Accessor.SetValue(builder, value);
            }
        }

        return null;
    }

    /// <summary>
    ///     Converts {@code value}, of the map-field {@code fieldDesc} from its Java
    ///     <seealso cref="System.Collections.IDictionary" />
    ///     representation to the protobuf-y {@code <seealso cref="System.Collections.IList" /><<seealso cref="MapEntry" />>}
    ///     representation.
    /// </summary>
    private object ToProtoMapStructure(FieldDescriptor fieldDesc, object value)
    {
        var mesgType = fieldDesc.MessageType;
        var keyType = mesgType.FindFieldByNumber(1);
        var valueType = mesgType.FindFieldByNumber(2);
        var keyReflectType = FieldDescription.NewFieldDescription(keyType).ReflectType();
        var valueReflectType = FieldDescription.NewFieldDescription(valueType).ReflectType();
        if (value is IDictionary)
        {
            IDictionary newDict = new Dictionary<object, object>();
            foreach (DictionaryEntry e in (IDictionary)value)
            {
                var v = e.Value;
                var k = e.Key;

                k = NativeToValue(k).ConvertToNative(keyReflectType);

                // TODO improve the type-A-to-B-conversion
                // if (!(k instanceof String)) {
                //   return Err.newTypeConversionError(k.getClass().getName(), String.class.getName());
                // }
                if (valueType.FieldType == Google.Protobuf.Reflection.FieldType.Message && !(v is Message))
                    v = NativeToValue(v).ConvertToNative(typeof(Value));
                else
                    v = NativeToValue(v).ConvertToNative(valueReflectType);

                newDict[k] = v;
            }

            value = newDict;
        }

        return value;
    }

    /// <summary>
    ///     Converts a value of type <seealso cref="Number" /> to <seealso cref="EnumValueDescriptor" />, also works for arrays
    ///     and <seealso cref="System.Collections.IList" />s containing <seealso cref="Number" />s.
    /// </summary>
    private object IntToProtoEnumValues(FieldDescription field, object value)
    {
        var enumType = field.Descriptor().EnumType;
        if (value is int)
        {
            var enumValue = (int)value;
            value = findEnum(enumType, enumValue);
        }
        else if (value is IList)
        {
            var list = (IList)value;
            IList newList = new List<object>(list.Count);
            foreach (var o in list)
            {
                var enumValue = Convert.ToInt32(o);
                newList.Add(findEnum(enumType, enumValue));
            }

            value = newList;
        }
        else if (value.GetType().IsArray)
        {
            var array = (int[])value;
            var l = array.Length;
            var newArr = new object[l];
            for (var i = 0; i < l; i++)
            {
                var enumValue = array[i];
                newArr[i] = findEnum(enumType, enumValue);
            }

            value = newArr;
        }

        return value;
    }

    private object findEnum(EnumDescriptor enumType, int value)
    {
        var enumValue = enumType.FindValueByNumber(value);
        return Enum.ToObject(enumType.ClrType, value);
    }

    /// <summary>
    ///     RegisterDescriptor registers the contents of a protocol buffer `FileDescriptor`.
    /// </summary>
    public void RegisterDescriptor(FileDescriptor fileDesc)
    {
        var fd = pbdb.RegisterDescriptor(fileDesc);
        RegisterAllTypes(fd);
    }

    /// <summary>
    ///     RegisterMessage registers a protocol buffer message and its dependencies.
    /// </summary>
    public void RegisterMessage(Message message)
    {
        var fd = pbdb.RegisterMessage(message);
        RegisterAllTypes(fd);
    }

    /// <summary>
    ///     NativeToValue converts various "native" types to ref.Val with this specific implementation
    ///     providing support for custom proto-based types.
    ///     <para>
    ///         This method should be the inverse of ref.Val.ConvertToNative.
    ///     </para>
    /// </summary>
    public IVal NativeToValue(object? value)
    {
        IVal? val;
        if (value is Message)
        {
            var v = (Message)value;
            var typeName = PbTypeDescription.TypeNameFromMessage(v);
            if (typeName.Length == 0) return Err.AnyWithEmptyType();

            var td = pbdb.DescribeType(typeName);
            if (td == null) return Err.UnknownType(typeName);

            var unwrapped = td.MaybeUnwrap(pbdb, v);
            if (unwrapped != null)
            {
                var further = DefaultTypeAdapter.MaybeUnwrapValue(unwrapped);
                if (further != unwrapped) return NativeToValue(further);

                val = TypeAdapterSupport.MaybeNativeToValue(ToTypeAdapter(), unwrapped);
                if (val != null) return val;

                if (unwrapped is Message) v = (Message)unwrapped;
            }

            var typeVal = FindIdent(typeName);
            if (typeVal == null) return Err.UnknownType(typeName);

            return PbObjectT.NewObject(ToTypeAdapter(), td, (TypeT)typeVal, v);
        }

        val = DefaultTypeAdapter.NativeToValue(pbdb, ToTypeAdapter(), value);
        if (val != null) return val;

        return Err.UnsupportedRefValConversionErr(value);
    }

    internal void RegisterAllTypes(FileDescription fd)
    {
        foreach (var typeName in fd.TypeNames) RegisterType(TypeT.NewObjectTypeValue(typeName));
    }

    public override string ToString()
    {
        return "ProtoTypeRegistry{" + "revTypeMap.size=" + revTypeMap.Count + ", pbdb=" + pbdb + '}';
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (ProtoTypeRegistry)o;
        return Equals(revTypeMap, that.revTypeMap) && Equals(pbdb, that.pbdb);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(revTypeMap, pbdb);
    }
}