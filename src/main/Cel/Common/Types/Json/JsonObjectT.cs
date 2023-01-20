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

using Cel.Common.Types.Ref;

namespace Cel.Common.Types.Json;

internal sealed class JsonObjectT : ObjectT
{
    private JsonObjectT(JsonRegistry registry, object value, JsonTypeDescription typeDesc) : base(
        registry.ToTypeAdapter(), value, typeDesc, typeDesc.Type())
    {
    }

    internal static JsonObjectT NewObject(JsonRegistry registry, object value, JsonTypeDescription typeDesc)
    {
        return new JsonObjectT(registry, value, typeDesc);
    }

    internal JsonTypeDescription TypeDesc()
    {
        return (JsonTypeDescription)typeDesc;
    }

    public override IVal IsSet(IVal field)
    {
        if (!(field is StringT)) return Err.NoSuchOverload(this, "isSet", field);

        var fieldName = (string)field.Value();

        if (!TypeDesc().HasProperty(fieldName)) return Err.NoSuchField(fieldName);

        var value = TypeDesc().FromObject(Value(), fieldName);

        return Types.BoolOf(value != null);
    }

    public override IVal Get(IVal index)
    {
        if (!(index is StringT)) return Err.NoSuchOverload(this, "get", index);

        var fieldName = (string)index.Value();

        if (!TypeDesc().HasProperty(fieldName)) return Err.NoSuchField(fieldName);

        var v = TypeDesc().FromObject(Value(), fieldName);

        return adapter(v);
    }

    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeDesc.IsAssignableFrom(value.GetType())) return value;

        if (typeDesc.IsAssignableFrom(GetType())) return this;

        throw new NotSupportedException();
    }
}