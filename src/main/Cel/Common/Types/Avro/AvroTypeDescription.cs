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
using Avro.Specific;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Field = Avro.Field;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types.Avro;

public sealed class AvroTypeDescription : ITypeDescription
{
    public delegate Type TypeQuery(Schema type);

    private readonly Schema schema;
    private readonly string fullName;
    private readonly IType type;
    private readonly Type pbType;

    private readonly IDictionary<string, AvroFieldType> fieldTypes;

    public AvroTypeDescription(RecordSchema schema, TypeQuery typeQuery)
    {
        this.schema = schema;
        this.fullName = schema.Fullname;
        this.type = TypeT.NewObjectTypeValue(fullName);
        this.pbType = new Type { MessageType = fullName };

        fieldTypes = new Dictionary<string, AvroFieldType>();

        foreach (Field field in schema.Fields)
        {
            string n = field.Name;

            AvroFieldType ft =
                new AvroFieldType(
                    FindTypeForAvroType(field.Schema, typeQuery),
                    target => FromObject(target, n) != null,
                    target => FromObject(target, n),
                    field.Schema);
            fieldTypes[n] = ft;
        }
    }

    Type FindTypeForAvroType(Schema schema, TypeQuery typeQuery)
    {
        Schema.Type type = schema.Tag;
        switch (type)
        {
            case Schema.Type.Boolean:
                return Checked.CheckedBool;
            case Schema.Type.Int:
            case Schema.Type.Long:
                return Checked.CheckedInt;
            case Schema.Type.Bytes:
                return Checked.CheckedBytes;
            case Schema.Type.Float:
            case Schema.Type.Double:
                return Checked.CheckedDouble;
            case Schema.Type.String:
                return Checked.CheckedString;
            // TODO duration, timestamp
            case Schema.Type.Array:
                return Checked.CheckedListDyn;
            case Schema.Type.Map:
                return Checked.CheckedMapStringDyn;
            case Schema.Type.Enumeration:
                return typeQuery(schema);
            case Schema.Type.Null:
                return Checked.CheckedNull;
            default:
                return typeQuery(schema);
        }
    }

    public bool HasProperty(string property)
    {
        return fieldTypes.ContainsKey(property);
    }

    public object? FromObject(object value, string property)
    {
        fieldTypes.TryGetValue(property, out var ft);
        if (ft == null)
        {
            throw new ArgumentException($"No property named '{property}'");
        }

        RecordSchema schema = GetSchema(value);
        schema.TryGetField(property, out var f);
        if (value is GenericRecord)
        {
            if (!((GenericRecord)value).TryGetValue(f.Pos, out var result))
            {
                return null;
            }

            return result;
        }
        else if (value is ISpecificRecord)
        {
            return ((ISpecificRecord)value).Get(f.Pos);
        }

        throw new ArgumentException($"Cannot get property {property} for {value.GetType()}");
    }

    public IType Type()
    {
        return type;
    }

    public Type PbType()
    {
        return pbType;
    }

    public FieldType? FieldType(string fieldName)
    {
        fieldTypes.TryGetValue(fieldName, out var ft);
        return ft;
    }

    public string Name()
    {
        return fullName;
    }

    public System.Type ReflectType()
    {
        return System.Type.GetType(fullName);
    }

    public static RecordSchema GetSchema(object message)
    {
        if (message is GenericRecord)
        {
            return ((GenericRecord)message).Schema;
        }
        else if (message is ISpecificRecord)
        {
            return (RecordSchema)((ISpecificRecord)message).Schema;
        }

        throw new ArgumentException($"Cannot get schema for ${message.GetType()}");
    }
}