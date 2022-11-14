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

    private AvroRegistry()
    {
    }

    public static ITypeRegistry NewRegistry()
    {
        return new AvroRegistry();
    }

    public ITypeRegistry Copy()
    {
        return this;
    }

    public void Register(object t)
    {
        TypeDescription(AvroTypeDescription.GetSchema(t));
    }

    public void RegisterType(params IType[] types)
    {
        throw new NotSupportedException();
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
        knownTypesByName.TryGetValue(identName, out var td);
        if (td != null) return td.Type();

        enumValues.TryGetValue(identName, out var enumVal);
        if (enumVal != null) return enumVal.StringValue();
        return null;
    }

    public Google.Api.Expr.V1Alpha1.Type? FindType(string typeName)
    {
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
        var maybe = TypeAdapterSupport.MaybeNativeToValue(ToTypeAdapter(), value);
        if (maybe != null) return maybe;

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
            throw new Exception("oops", e);
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