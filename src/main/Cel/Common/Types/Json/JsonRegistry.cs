﻿using Cel.Common.Types.Ref;
using Newtonsoft.Json;
using Type = System.Type;

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

/// <summary>
///     CEL <seealso cref="TypeRegistry" /> to use Json objects as input values for CEL scripts.
///     <para>
///         The implementation does not support the construction of Json objects in CEL expressions and
///         therefore returning Json objects from CEL expressions is not possible/implemented and results
///         in <seealso cref="System.NotSupportedException" />s.
///     </para>
/// </summary>
public sealed class JsonRegistry : TypeRegistry
{
    private readonly IDictionary<Type, JsonEnumDescription> enumMap = new Dictionary<Type, JsonEnumDescription>();
    private readonly IDictionary<string, JsonEnumValue> enumValues = new Dictionary<string, JsonEnumValue>();
    private readonly IDictionary<Type, JsonTypeDescription> knownTypes = new Dictionary<Type, JsonTypeDescription>();

    private readonly IDictionary<string, JsonTypeDescription> knownTypesByName =
        new Dictionary<string, JsonTypeDescription>();

    private readonly JsonSerializer serializer;

    private JsonRegistry()
    {
        serializer = new JsonSerializer();
    }

    public TypeRegistry Copy()
    {
        return this;
    }

    public void Register(object t)
    {
        var cls = t is Type ? (Type)t : t.GetType();
        TypeDescription(cls);
    }

    public void RegisterType(params Ref.Type[] types)
    {
        throw new NotSupportedException();
    }

    public TypeAdapter ToTypeAdapter()
    {
        return NativeToValue;
    }

    public Val EnumValue(string enumName)
    {
        enumValues.TryGetValue(enumName, out var enumVal);
        if (enumVal == null) return Err.NewErr("unknown enum name '{0}'", enumName);
        return enumVal.OrdinalValue();
    }

    public Val FindIdent(string identName)
    {
        knownTypesByName.TryGetValue(identName, out var td);
        if (td != null) return td.Type();

        enumValues.TryGetValue(identName, out var enumVal);
        if (enumVal != null) return enumVal.OrdinalValue();
        return null;
    }

    public Google.Api.Expr.V1Alpha1.Type FindType(string typeName)
    {
        knownTypesByName.TryGetValue(typeName, out var td);
        if (td == null) return null;
        return td.PbType();
    }

    public FieldType FindFieldType(string messageType, string fieldName)
    {
        knownTypesByName.TryGetValue(messageType, out var td);
        if (td == null) return null;
        return td.FieldType(fieldName);
    }

    public Val NewValue(string typeName, IDictionary<string, Val> fields)
    {
        throw new NotSupportedException();
    }

    public static TypeRegistry NewRegistry()
    {
        return new JsonRegistry();
    }

    public Val NativeToValue(object value)
    {
        if (value is Val) return (Val)value;
        var maybe = TypeAdapterSupport.MaybeNativeToValue(ToTypeAdapter(), value);
        if (maybe != null) return maybe;

        if (value is Enum)
        {
            var fq = JsonEnumValue.FullyQualifiedName((Enum)value);
            enumValues.TryGetValue(fq, out var v);
            if (v == null) return Err.NewErr("unknown enum name '{0}'", fq);
            return v.OrdinalValue();
        }

        try
        {
            return JsonObjectT.NewObject(this, value, TypeDescription(value.GetType()));
        }
        catch (Exception e)
        {
            throw new Exception("oops", e);
        }
    }

    public JsonEnumDescription EnumDescription(Type clazz)
    {
        if (!clazz.IsEnum) throw new ArgumentException("only enum allowed here");

        enumMap.TryGetValue(clazz, out var ed);
        if (ed != null) return ed;
        ed = ComputeEnumDescription(clazz);
        enumMap[clazz] = ed;
        return ed;
    }

    private JsonEnumDescription ComputeEnumDescription(Type type)
    {
        var enumDesc = new JsonEnumDescription(type);
        enumMap[type] = enumDesc;

        foreach (var v in enumDesc.BuildValues())
        {
            enumValues[v.FullyQualifiedName()] = v;
        }

        return enumDesc;
    }

    public JsonTypeDescription TypeDescription(Type clazz)
    {
        if (clazz.IsEnum) throw new ArgumentException("enum not allowed here");

        knownTypes.TryGetValue(clazz, out var td);
        if (td != null) return td;
        td = ComputeTypeDescription(clazz);
        knownTypes[clazz] = td;
        return td;
    }

    private JsonTypeDescription ComputeTypeDescription(Type type)
    {
        var typeDesc = new JsonTypeDescription(type, serializer, TypeQuery);
        knownTypesByName[type.FullName] = typeDesc;

        return typeDesc;
    }

    private Google.Api.Expr.V1Alpha1.Type TypeQuery(Type type)
    {
        if (type.IsEnum) return EnumDescription(type).PbType();
        return TypeDescription(type).PbType();
    }
}