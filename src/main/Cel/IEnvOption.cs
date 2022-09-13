using Cel.Common.Types.Ref;
using Cel.Parser;
using Google.Api.Expr.V1Alpha1;

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
namespace Cel;

/// <summary>
///     EnvOption is a functional interface for configuring the environment.
/// </summary>
public delegate Env EnvOption(Env e);

public interface IEnvOption
{
    // These constants beginning with "Feature" enable optional behavior in
    // the library.  See the documentation for each constant to see its
    // effects, compatibility restrictions, and standard conformance.

    /// <summary>
    ///     ClearMacros options clears all parser macros.
    ///     <para>
    ///         Clearing macros will ensure CEL expressions can only contain linear evaluation paths, as
    ///         comprehensions such as `all` and `exists` are enabled only via macros.
    ///     </para>
    /// </summary>
    public static EnvOption ClearMacros()
    {
        return e =>
        {
            e.macros.Clear();
            return e;
        };
    }

    /// <summary>
    ///     CustomTypeAdapter swaps the default ref.TypeAdapter implementation with a custom one.
    ///     <para>
    ///         Note: This option must be specified before the Types and TypeDescs options when used
    ///         together.
    ///     </para>
    /// </summary>
    public static EnvOption CustomTypeAdapter(TypeAdapter adapter)
    {
        return e =>
        {
            e.adapter = adapter;
            return e;
        };
    }

    /// <summary>
    ///     CustomTypeProvider swaps the default ref.TypeProvider implementation with a custom one.
    ///     <para>
    ///         Note: This option must be specified before the Types and TypeDescs options when used
    ///         together.
    ///     </para>
    /// </summary>
    public static EnvOption CustomTypeProvider(ITypeProvider provider)
    {
        return e =>
        {
            e.provider = provider;
            return e;
        };
    }

    /// <summary>
    ///     Declarations option extends the declaration set configured in the environment.
    ///     <para>
    ///         Note: Declarations will by default be appended to the pre-existing declaration set
    ///         configured for the environment. The NewEnv call builds on top of the standard CEL declarations.
    ///         For a purely custom set of declarations use NewCustomEnv.
    ///     </para>
    /// </summary>
    public static EnvOption Declarations(IList<Decl> decls)
    {
        // TODO: provide an alternative means of specifying declarations that doesn't refer
        //  to the underlying proto implementations.
        return e =>
        {
            ((List<Decl>)e.declarations).AddRange(decls);
            return e;
        };
    }

    public static EnvOption Declarations(params Decl[] decls)
    {
        return Declarations(new List<Decl>(decls));
    }

    /// <summary>
    ///     Features sets the given feature flags. See list of Feature constants above.
    /// </summary>
    public static EnvOption Features(params EnvFeature[] flags)
    {
        return e =>
        {
            foreach (var flag in flags) e.Feature = flag;

            return e;
        };
    }

    /// <summary>
    ///     HomogeneousAggregateLiterals option ensures that list and map literal entry types must agree
    ///     during type-checking.
    ///     <para>
    ///         Note, it is still possible to have heterogeneous aggregates when provided as variables to
    ///         the expression, as well as via conversion of well-known dynamic types, or with unchecked
    ///         expressions.
    ///     </para>
    /// </summary>
    public static EnvOption HomogeneousAggregateLiterals()
    {
        return Features(EnvFeature.FeatureDisableDynamicAggregateLiterals);
    }

    public static EnvOption Macros(params Macro[] macros)
    {
        return Macros(new List<Macro>(macros));
    }

    /// <summary>
    ///     Macros option extends the macro set configured in the environment.
    ///     <para>
    ///         Note: This option must be specified after ClearMacros if used together.
    ///     </para>
    /// </summary>
    public static EnvOption Macros(IList<Macro> macros)
    {
        return e =>
        {
            ((List<Macro>)e.macros).AddRange(macros);
            return e;
        };
    }

    /// <summary>
    ///     Container sets the container for resolving variable names. Defaults to an empty container.
    ///     <para>
    ///         If all references within an expression are relative to a protocol buffer package, then
    ///         specifying a container of `google.type` would make it possible to write expressions such as
    ///         `Expr{expression: 'a &lt; b'}` instead of having to write `google.type.Expr{...}`.
    ///     </para>
    /// </summary>
    public static EnvOption Container(string name)
    {
        return e =>
        {
            e.container = e.container.Extend(Common.Containers.Container.Name(name))!;
            return e;
        };
    }

    /// <summary>
    ///     Abbrevs configures a set of simple names as abbreviations for fully-qualified names.
    ///     <para>
    ///         An abbreviation (abbrev for short) is a simple name that expands to a fully-qualified name.
    ///         Abbreviations can be useful when working with variables, functions, and especially types from
    ///         multiple namespaces:
    ///         <pre>
    ///             <code>
    ///    // CEL object construction
    ///    qual.pkg.version.ObjTypeName{
    ///       field: alt.container.ver.FieldTypeName{value: ...}
    ///    }
    /// </code>
    ///         </pre>
    ///     </para>
    ///     <para>
    ///         Only one the qualified names above may be used as the CEL container, so at least one of
    ///         these references must be a long qualified name within an otherwise short CEL program. Using the
    ///         following abbreviations, the program becomes much simpler:
    ///         <pre>
    ///             <code>
    ///    // CEL Go option
    ///    Abbrevs("qual.pkg.version.ObjTypeName", "alt.container.ver.FieldTypeName")
    ///    // Simplified Object construction
    ///    ObjTypeName{field: FieldTypeName{value: ...}}
    /// </code>
    ///         </pre>
    ///     </para>
    ///     <para>
    ///         There are a few rules for the qualified names and the simple abbreviations generated from
    ///         them:
    ///         <ul>
    ///             <li>
    ///                 Qualified names must be dot-delimited, e.g. `package.subpkg.name`.
    ///             </li>
    ///             <li>
    ///                 The last element in the qualified name is the abbreviation.
    ///             </li>
    ///             <li>
    ///                 Abbreviations must not collide with each other.
    ///             </li>
    ///             <li>
    ///                 The abbreviation must not collide with unqualified names in use.
    ///             </li>
    ///         </ul>
    ///     </para>
    ///     <para>
    ///         Abbreviations are distinct from container-based references in the following important ways:
    ///         <ul>
    ///             <li>
    ///                 Abbreviations must expand to a fully-qualified name.
    ///             </li>
    ///             <li>
    ///                 Expanded abbreviations do not participate in namespace resolution.
    ///             </li>
    ///             <li>
    ///                 Abbreviation expansion is done instead of the container search for a matching identifier.
    ///             </li>
    ///             <li>
    ///                 Containers follow C++ namespace resolution rules with searches from the most qualified
    ///                 name to the least qualified name.
    ///             </li>
    ///             <li>
    ///                 Container references within the CEL program may be relative, and are resolved to fully
    ///                 qualified names at either type-check time or program plan time, whichever comes first.
    ///             </li>
    ///         </ul>
    ///     </para>
    ///     <para>
    ///         If there is ever a case where an identifier could be in both the container and as an
    ///         abbreviation, the abbreviation wins as this will ensure that the meaning of a program is
    ///         preserved between compilations even as the container evolves.
    ///     </para>
    /// </summary>
    public static EnvOption Abbrevs(params string[] qualifiedNames)
    {
        return e =>
        {
            e.container = e.container.Extend(Common.Containers.Container.Abbrevs(qualifiedNames))!;
            return e;
        };
    }

    /// <summary>
    ///     Types adds one or more type declarations to the environment, allowing for construction of
    ///     type-literals whose definitions are included in the common expression built-in set.
    ///     <para>
    ///         The input types may either be instances of `proto.Message` or `ref.Type`. Any other type
    ///         provided to this option will result in an error.
    ///     </para>
    ///     <para>
    ///         Well-known protobuf types within the `google.protobuf.*` package are included in the
    ///         standard environment by default.
    ///     </para>
    ///     <para>
    ///         Note: This option must be specified after the CustomTypeProvider option when used together.
    ///     </para>
    /// </summary>
    public static EnvOption Types(IList<object> addTypes)
    {
        return e =>
        {
            if (!(e.provider is ITypeRegistry))
                throw new Exception(string.Format("custom types not supported by provider: {0}",
                    e.provider.GetType()));

            var reg = (ITypeRegistry)e.provider;
            foreach (var t in addTypes) reg.Register(t);

            return e;
        };
    }

    public static EnvOption Types(params object[] addTypes)
    {
        return Types(new List<object>(addTypes));
    }

    //  /**
    //   * TypeDescs adds type declarations from any protoreflect.FileDescriptor, protoregistry.Files,
    //   * google.protobuf.FileDescriptorProto or google.protobuf.FileDescriptorSet provided.
    //   *
    //   * <p>Note that messages instantiated from these descriptors will be *dynamicpb.Message values
    //   * rather than the concrete message type.
    //   *
    //   * <p>TypeDescs are hermetic to a single Env object, but may be copied to other Env values via
    //   * extension or by re-using the same EnvOption with another NewEnv() call.
    //   */
    //  static EnvOption typeDescs(Object... descs) {
    //    return e -> {
    //      if (!(e.provider instanceof TypeRegistry)) {
    //        throw new RuntimeException(
    //            String.Format(
    //                "custom types not supported by provider: {0}", e.provider.getClass().getName()));
    //      }
    //      TypeRegistry reg = (TypeRegistry) e.provider;
    //      // Scan the input descriptors for FileDescriptorProto messages and accumulate them into a
    //      // synthetic FileDescriptorSet as the FileDescriptorProto messages may refer to each other
    //      // and will not resolve properly unless they are part of the same set.
    //      //		FileDescriptorSet fds = null;
    //      for (Object d : descs) {
    //        if (d instanceof FileDescriptorProto) {
    //          throw new RuntimeException(
    //              String.Format("unsupported type descriptor: {0}", d.getClass().getName()));
    //          //				if (fds == null) {
    //          //					fds = &descpb.FileDescriptorSet{
    //          //						File: []*descpb.FileDescriptorProto{},
    //          //					}
    //          //				}
    //          //				fds.File = append(fds.File, f)
    //        }
    //      }
    //      //		if (fds != null) {
    //      //			registerFileSet(reg, fds);
    //      //		}
    //      for (Object d : descs) {
    //        //			if (d instanceof protoregistry.Files) {
    //        //				if err := registerFiles(reg, f); err != nil {
    //        //					return nil, err
    //        //				}
    //        //			} else
    //        if (d instanceof FileDescriptor) {
    //          reg.registerDescriptor((FileDescriptor) d);
    //          //			} else if (d instanceof FileDescriptorSet) {
    //          //				registerFileSet(reg, (FileDescriptorSet) d);
    //        } else if (d instanceof FileDescriptorProto) {
    //
    //        } else {
    //          throw new RuntimeException(
    //              String.Format("unsupported type descriptor: {0}", d.getClass().getName()));
    //        }
    //      }
    //      return e;
    //    };
    //  }

    //	static void registerFileSet(TypeRegistry ref, FileDescriptorSet fileSet) {
    //	files = protodesc.NewFiles(fileSet);
    //	return registerFiles(reg, files);
    // }

    // static void registerFiles(TypeRegistry ref, protoregistry.Files files) {
    //	var err error
    //	files.RangeFiles(func(fd protoreflect.FileDescriptor) bool {
    //		err = reg.RegisterDescriptor(fd)
    //		return err == nil
    //	})
    //	return err
    // }
}

public enum EnvFeature
{
    /// <summary>
    ///     Disallow heterogeneous aggregate (list, map) literals. Note, it is still possible to have
    ///     heterogeneous aggregates when provided as variables to the expression, as well as via
    ///     conversion of well-known dynamic types, or with unchecked expressions. Affects checking.
    ///     Provides a subset of standard behavior.
    /// </summary>
    FeatureDisableDynamicAggregateLiterals
}