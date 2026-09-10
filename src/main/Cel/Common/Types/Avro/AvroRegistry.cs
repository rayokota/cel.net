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

using Avro;
using Avro.Generic;
using Cel.Common.Types.Ref;
using FieldType = Cel.Common.Types.Ref.FieldType;

namespace Cel.Common.Types.Avro;

/// <summary>
///     CEL <seealso cref="ITypeRegistry" /> to use Avro objects as input values for CEL scripts.
///     <para>
///         The implementation does not support the construction of Avro objects in CEL expressions and
///         therefore returning Avro objects from CEL expressions is not possible/implemented and results
///         in <seealso cref="System.NotSupportedException" />s.
///     </para>
/// </summary>
public sealed class AvroRegistry : ITypeRegistry
{
    private readonly IDictionary<RecordSchema, AvroTypeDescription> knownTypes =
        new Dictionary<RecordSchema, AvroTypeDescription>();

    private readonly IDictionary<string, AvroTypeDescription> knownTypesByName =
        new Dictionary<string, AvroTypeDescription>();

    private readonly IDictionary<EnumSchema, AvroEnumDescription> enumMap =
        new Dictionary<EnumSchema, AvroEnumDescription>();

    private readonly IDictionary<string, AvroEnumValue> enumValues = new Dictionary<string, AvroEnumValue>();

    // Types registered by name, as on ProtoTypeRegistry. A caller that owns a value's CEL
    // representation (see customAdapter below) names it, and no Avro schema declares that name.
    private readonly IDictionary<string, IType> revTypeMap = new Dictionary<string, IType>();

    // The CEL built-in type names (`bytes`, `string`, ...) must resolve through FindIdent, the same
    // as ProtoTypeRegistry does. Without this, AbsoluteAttribute.TryResolve's fallback to
    // provider.FindIdent(nm) returns null for these names, and an expression like
    // `type(x) == bytes` fails at eval time with "undeclared reference" instead of resolving the
    // constant - the checker itself needs no help, since the standard declarations already contain
    // these identifiers regardless of registry.
    private static readonly IDictionary<string, IType> PrimitiveTypes = new Dictionary<string, IType>
    {
        [BoolT.BoolType.TypeName()] = BoolT.BoolType,
        [BytesT.BytesType.TypeName()] = BytesT.BytesType,
        [DoubleT.DoubleType.TypeName()] = DoubleT.DoubleType,
        [DurationT.DurationType.TypeName()] = DurationT.DurationType,
        [IntT.IntType.TypeName()] = IntT.IntType,
        [ListT.ListType.TypeName()] = ListT.ListType,
        [MapT.MapType.TypeName()] = MapT.MapType,
        [NullT.NullType.TypeName()] = NullT.NullType,
        [StringT.StringType.TypeName()] = StringT.StringType,
        [TimestampT.TimestampType.TypeName()] = TimestampT.TimestampType,
        [TypeT.TypeType.TypeName()] = TypeT.TypeType,
        [UintT.UintType.TypeName()] = UintT.UintType
    };

    private readonly Func<object, IVal?>? customAdapter;

    private AvroRegistry(Func<object, IVal?>? customAdapter)
    {
        this.customAdapter = customAdapter;
    }

    public static ITypeRegistry NewRegistry()
    {
        return new AvroRegistry(null);
    }

    /// <summary>
    ///     Runtime type name a carried Avro decimal reports - what <c>Type().TypeName()</c>
    ///     answers for the value <see cref="NativeToValue" /> produces from an
    ///     <see cref="AvroDecimal" />.
    ///     <para>
    ///         Public for a <b>host</b>, which is the only thing that can act on it: code that
    ///         inspects the carried value, or a custom adapter deciding whether to take the type
    ///         over. It is <i>not</i> usable from a rule - <see cref="FindIdent" /> resolves only
    ///         registered records, enums and primitives, so <c>avro.decimal</c> is an undeclared
    ///         reference in an expression and <c>type(x) == avro.decimal</c> does not compile.
    ///     </para>
    /// </summary>
    public const string DecimalTypeName = "avro.decimal";

    /// <summary>
    ///     A registry that offers every native value to <paramref name="customAdapter" /> before
    ///     applying the standard mapping, so a caller can own the CEL representation of a type
    ///     Avro decodes to — the <c>decimal</c> logical type's <see cref="AvroDecimal" />, say,
    ///     which a caller may want carried as its own decimal value rather than as the opaque
    ///     <c>avro.decimal</c> below. Returning null defers to the standard mapping.
    ///     <para>
    ///         The adapter reaches record *fields* as well as top-level values, because the
    ///         object value built below adapts its fields through this same registry. That is why
    ///         a caller cannot get the same effect by wrapping the registry from outside.
    ///     </para>
    /// </summary>
    public static ITypeRegistry NewRegistry(Func<object, IVal?> customAdapter)
    {
        return new AvroRegistry(customAdapter);
    }

    public ITypeRegistry Copy()
    {
        return this;
    }

    public void Register(object t)
    {
        TypeDescription(AvroTypeDescription.GetSchema(t));
    }

    /// <summary>
    ///     Registers a type by the name it reports, so <see cref="FindIdent" /> and
    ///     <see cref="FindType" /> resolve it. Mirrors <c>ProtoTypeRegistry.RegisterType</c>,
    ///     the port of cel-go's <c>Registry.RegisterType</c>. Threw
    ///     <see cref="NotSupportedException" /> before, so a caller-owned type was nameable
    ///     under the protobuf registry and not under this one.
    /// </summary>
    public void RegisterType(params IType[] types)
    {
        foreach (var t in types) revTypeMap[t.TypeName()] = t;
    }

    public TypeAdapter ToTypeAdapter()
    {
        return NativeToValue;
    }

    public IVal EnumValue(string enumName)
    {
        enumValues.TryGetValue(enumName, out var enumVal);
        if (enumVal == null)
        {
            return Err.NewErr("unknown enum name '{0}'", enumName);
        }

        return enumVal.StringValue();
    }

    public IVal? FindIdent(string identName)
    {
        // A registered type first, as in ProtoTypeRegistry.
        revTypeMap.TryGetValue(identName, out var registered);
        if (registered != null) return registered;

        knownTypesByName.TryGetValue(identName, out var td);
        if (td != null) return td.Type();

        enumValues.TryGetValue(identName, out var enumVal);
        if (enumVal != null) return enumVal.StringValue();

        PrimitiveTypes.TryGetValue(identName, out var primitiveType);
        if (primitiveType != null) return primitiveType;

        return null;
    }

    public Google.Api.Expr.V1Alpha1.Type? FindType(string typeName)
    {
        // As in ProtoTypeRegistry.FindType: the type *of* the type, which is what lets the name
        // stand as a type expression.
        if (revTypeMap.ContainsKey(typeName))
        {
            var named = new Google.Api.Expr.V1Alpha1.Type();
            named.MessageType = typeName;
            var asType = new Google.Api.Expr.V1Alpha1.Type();
            asType.Type_ = named;
            return asType;
        }

        knownTypesByName.TryGetValue(typeName, out var td);
        if (td == null) return null;
        return td.PbType();
    }

    public FieldType? FindFieldType(string messageType, string fieldName)
    {
        knownTypesByName.TryGetValue(messageType, out var td);
        if (td == null) return null;
        return td.FieldType(fieldName);
    }

    public IVal NewValue(string typeName, IDictionary<string, IVal> fields)
    {
        throw new NotSupportedException();
    }

    public IVal NativeToValue(object value)
    {
        if (value is IVal) return (IVal)value;
        if (customAdapter != null)
        {
            var custom = customAdapter(value);
            if (custom != null) return custom;
        }

        var maybe = TypeAdapterSupport.MaybeNativeToValue(ToTypeAdapter(), value);
        if (maybe != null) return maybe;

        if (value is GenericFixed fixedValue)
        {
            // Avro's `fixed` is a fixed-width byte string, so present it as CEL bytes - the same
            // way `bytes` is presented, and what the Java reference does (GenericFixed ->
            // CelByteString). Without this arm it fell through to the record fallback and became an
            // opaque object, so `size(this.fx)` and a comparison against a bytes literal both
            // failed to find an overload.
            return BytesT.BytesOf(fixedValue.Value);
        }

        if (value is AvroDecimal dec)
        {
            // Avro's `decimal` logical type decodes to AvroDecimal. CEL has no decimal type,
            // so preserve the value as-is in a passthrough carrier whose Value() returns the
            // AvroDecimal; callers reconstruct the exact value from its unscaled value + scale.
            // Without this arm, an AvroDecimal field read would fall through to the record
            // fallback below and throw "Cannot get schema for Avro.AvroDecimal".
            return OpaqueT.Of(dec, DecimalTypeName);
        }

        if (value is GenericEnum)
        {
            string fq = AvroEnumValue.FullyQualifiedName(((GenericEnum)value));
            enumValues.TryGetValue(fq, out var v);
            if (v == null) return Err.NewErr("unknown enum name '{0}'", fq);
            return v.StringValue();
        }

        if (value is Enum)
        {
            string fq = value.GetType().FullName + "." + value;
            enumValues.TryGetValue(fq, out var v);
            if (v == null) return Err.NewErr("unknown enum name '{0}'", fq);
            return v.StringValue();
        }

        try
        {
            return AvroObjectT.NewObject(this, value, TypeDescription(AvroTypeDescription.GetSchema(value)));
        }
        catch (Exception e)
        {
            // The fallback for a CLR value none of the arms above recognised: it is treated as an
            // Avro record, which needs a schema for its type. Naming the type matters, because this
            // is what a caller sees when a value reaches CEL that the registry has no arm for, and
            // the type is the only thing that tells them which one is missing.
            // The type *and* the value, as cel-go's UnsupportedRefValConversionErr does
            // ("unsupported conversion to ref.Val: (%T)%v"). The value is what identifies which
            // one of a container's elements was unrepresentable; the type alone names the arm
            // that is missing but not the data that reached it.
            throw new Exception(
                $"cannot represent a value of type {value.GetType().FullName} as an Avro CEL " +
                $"value: {value}",
                e);
        }
    }

    AvroEnumDescription EnumDescription(EnumSchema schema)
    {
        if (schema.Tag != Schema.Type.Enumeration)
        {
            throw new ArgumentException("only enum allowed here");
        }

        enumMap.TryGetValue(schema, out var ed);
        if (ed != null) return ed;
        ed = ComputeEnumDescription(schema);
        enumMap[schema] = ed;
        return ed;
    }

    private AvroEnumDescription ComputeEnumDescription(EnumSchema schema)
    {
        AvroEnumDescription enumDesc = new AvroEnumDescription(schema);
        enumMap[schema] = enumDesc;

        foreach (var v in enumDesc.BuildValues()) enumValues[v.FullyQualifiedName()] = v;

        return enumDesc;
    }

    AvroTypeDescription TypeDescription(RecordSchema schema)
    {
        if (schema.Tag == Schema.Type.Enumeration)
        {
            throw new ArgumentException("enum not allowed here");
        }

        knownTypes.TryGetValue(schema, out var td);
        if (td != null) return td;
        td = ComputeTypeDescription(schema);
        knownTypes[schema] = td;
        return td;
    }

    private AvroTypeDescription ComputeTypeDescription(RecordSchema schema)
    {
        AvroTypeDescription typeDesc = new AvroTypeDescription(schema, TypeQuery);
        knownTypesByName[schema.Fullname] = typeDesc;

        return typeDesc;
    }

    private Google.Api.Expr.V1Alpha1.Type TypeQuery(Schema schema)
    {
        if (schema.Tag == Schema.Type.Enumeration)
        {
            return EnumDescription((EnumSchema)schema).PbType();
        }

        return TypeDescription((RecordSchema)schema).PbType();
    }
}