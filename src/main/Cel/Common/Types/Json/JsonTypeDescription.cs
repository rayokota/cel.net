using System.Collections;
using Cel.Checker;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Common.Types.Json;

public sealed class JsonTypeDescription : ITypeDescription
{
    public delegate Type TypeQuery(System.Type Type);

    private readonly IDictionary<string, JsonFieldType> fieldTypes;
    private readonly string name;
    private readonly Type pbType;
    private readonly IType refType;
    private readonly System.Type type;

    public JsonTypeDescription(System.Type type, JsonSerializer ser, TypeQuery typeQuery)
    {
        this.type = type;
        name = type.FullName!;
        refType = TypeT.NewObjectTypeValue(name);
        pbType = new Type { MessageType = name };

        fieldTypes = new Dictionary<string, JsonFieldType>();

        var contract = ser.ContractResolver.ResolveContract(type);
        if (contract is JsonObjectContract)
        {
            var props = ((JsonObjectContract)contract).Properties;
            foreach (var prop in props)
            {
                var pw = prop.ValueProvider!;
                var n = prop.PropertyName!;

                var ft = new JsonFieldType(FindTypeForJsonType(prop.PropertyType!, typeQuery),
                    target => FromObject(target, n) != null, target => FromObject(target, n), pw);
                fieldTypes[n] = ft;
            }
        }
    }

    public string Name()
    {
        return name;
    }

    public System.Type ReflectType()
    {
        return type;
    }

    public Type FindTypeForJsonType(System.Type type, TypeQuery typeQuery)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null) type = underlyingType;

        if (type == typeof(bool)) return Checked.CheckedBool;

        if (type == typeof(long) || type == typeof(int) ||
            type == typeof(short) || type == typeof(sbyte) ||
            type == typeof(byte))
            return Checked.CheckedInt;

        if (type == typeof(uint) || type == typeof(ulong)) return Checked.CheckedUint;

        if (type == typeof(byte[]) || type == typeof(ByteString)) return Checked.CheckedBytes;

        if (type == typeof(double) || type == typeof(float)) return Checked.CheckedDouble;

        if (type == typeof(string)) return Checked.CheckedString;

        if (type == typeof(Duration) || type == typeof(Period)) return Checked.CheckedDuration;

        if (type == typeof(Timestamp) || type == typeof(Instant) ||
            type == typeof(ZonedDateTime))
            return Checked.CheckedTimestamp;

        if (type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
             type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
        {
            var arguments = type.GetGenericArguments();
            var keyType = FindTypeForJsonType(arguments[0], typeQuery);
            var valueType = FindTypeForJsonType(arguments[1], typeQuery);
            return Decls.NewMapType(keyType, valueType);
        }

        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var objType = FindTypeForJsonType(typeof(object), typeQuery);
            return Decls.NewMapType(objType, objType);
        }

        if (type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            var arguments = type.GetGenericArguments();
            var valueType = FindTypeForJsonType(arguments[0], typeQuery);
            return Decls.NewListType(valueType);
        }

        if (typeof(IList).IsAssignableFrom(type))
        {
            var objType = FindTypeForJsonType(typeof(object), typeQuery);
            return Decls.NewListType(objType);
        }

        if (type.IsEnum) return typeQuery(type);

        var t = typeQuery(type);
        if (t == null) throw new NotSupportedException(string.Format("Unsupported Type '{0}'", type));

        return t;
    }

    public bool HasProperty(string property)
    {
        return fieldTypes.ContainsKey(property);
    }

    public object? FromObject(object value, string property)
    {
        fieldTypes.TryGetValue(property, out var ft);
        if (ft == null) throw new ArgumentException(string.Format("No property named '{0}'", property));

        var pw = ft.PropertyWriter();

        if (pw == null)
            return null;
        return pw.GetValue(value);
    }

    public IType Type()
    {
        return refType;
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
}