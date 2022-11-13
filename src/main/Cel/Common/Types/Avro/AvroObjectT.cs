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

namespace Cel.Common.Types.Avro;

internal sealed class AvroObjectT : ObjectT
{
    private AvroObjectT(AvroRegistry registry, object value, AvroTypeDescription typeDesc) : base(
        registry.ToTypeAdapter(), value, typeDesc, typeDesc.Type())
    {
    }

    internal static AvroObjectT NewObject(
        AvroRegistry registry, object value, AvroTypeDescription typeDesc)
    {
        return new AvroObjectT(registry, value, typeDesc);
    }

    internal AvroTypeDescription TypeDesc()
    {
        return (AvroTypeDescription)typeDesc;
    }

    public override IVal IsSet(IVal field)
    {
        if (!(field is StringT)) return Err.NoSuchOverload(this, "isSet", field);
        string fieldName = (string)field.Value();

        if (!TypeDesc().HasProperty(fieldName)) return Err.NoSuchField(fieldName);

        var value = TypeDesc().FromObject(Value(), fieldName);

        return Types.BoolOf(value != null);
    }

    public override IVal Get(IVal index)
    {
        if (!(index is StringT)) return Err.NoSuchOverload(this, "get", index);
        string fieldName = (string)index.Value();

        if (!TypeDesc().HasProperty(fieldName)) return Err.NoSuchField(fieldName);

        var v = TypeDesc().FromObject(Value(), fieldName);

        return adapter(v);
    }

    public override object? ConvertToNative(Type typeDesc)
    {
        throw new NotSupportedException();
    }
}