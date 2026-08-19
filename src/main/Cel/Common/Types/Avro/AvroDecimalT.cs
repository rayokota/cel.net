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
using Cel.Common.Types.Ref;
using Type = System.Type;

namespace Cel.Common.Types.Avro;

/// <summary>
///     Passthrough CEL value for an Avro <see cref="AvroDecimal" /> (the decode of a
///     <c>decimal</c> logical-type field). CEL has no decimal type, so rather than lose the
///     scale (raw bytes) or depend on a culture-sensitive string form, this simply carries the
///     <see cref="AvroDecimal" /> unchanged: <see cref="Value" /> returns it, so a caller can
///     reconstruct the exact value from its unscaled value and scale.
/// </summary>
public sealed class AvroDecimalT : BaseVal
{
    /// <summary>Runtime type value for a carried Avro decimal.</summary>
    public static readonly IType AvroDecimalType = TypeT.NewObjectTypeValue("avro.decimal");

    private readonly AvroDecimal value;

    private AvroDecimalT(AvroDecimal value)
    {
        this.value = value;
    }

    public static AvroDecimalT Of(AvroDecimal value)
    {
        return new AvroDecimalT(value);
    }

    public override object Value()
    {
        return value;
    }

    public override IType Type()
    {
        return AvroDecimalType;
    }

    public override IVal Equal(IVal other)
    {
        return other is AvroDecimalT o ? Types.BoolOf(value.Equals(o.value)) : BoolT.False;
    }

    public override IVal ConvertToType(IType typeValue)
    {
        if (typeValue.TypeEnum().InnerEnumValue == TypeEnum.InnerEnum.Type)
        {
            return AvroDecimalType;
        }

        if (typeValue.TypeName().Equals(AvroDecimalType.TypeName()))
        {
            return this;
        }

        return Err.NewTypeConversionError(AvroDecimalType, typeValue);
    }

    public override object ConvertToNative(Type typeDesc)
    {
        if (typeDesc == typeof(AvroDecimal) || typeDesc == typeof(object))
        {
            return value;
        }

        return Err.NewTypeConversionError(AvroDecimalType, typeDesc.Name);
    }

    public override bool Equals(object? o)
    {
        return o is AvroDecimalT other && value.Equals(other.value);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
