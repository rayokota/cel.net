using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Enum = System.Enum;
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
namespace Cel.Common.Types;

/// <summary>
///     Int type that implements ref.Val as well as comparison and math operators.
/// </summary>
public sealed class IntT : BaseVal, IAdder, IComparer, IDivider, IModder, IMultiplier, INegater, ISubtractor
{
    /// <summary>
    ///     IntType singleton.
    /// </summary>
    public static readonly IType IntType = TypeT.NewTypeValue(TypeEnum.Int, Trait.AdderType, Trait.ComparerType,
        Trait.DividerType, Trait.ModderType, Trait.MultiplierType, Trait.NegatorType, Trait.SubtractorType);

    /// <summary>
    ///     Int constants used for comparison results. IntZero is the zero-value for Int
    /// </summary>
    public static readonly IntT IntZero = new(0);

    public static readonly IntT IntOne = new(1);
    public static readonly IntT IntNegOne = new(-1);

    /// <summary>
    ///     maxIntJSON is defined as the Number.MAX_SAFE_INTEGER value per EcmaScript 6.
    /// </summary>
    public static readonly long MaxIntJson = (1L << 53) - 1;

    /// <summary>
    ///     minIntJSON is defined as the Number.MIN_SAFE_INTEGER value per EcmaScript 6.
    /// </summary>
    public static readonly long MinIntJson = -MaxIntJson;

    private readonly long i;

    private IntT(long i)
    {
        this.i = i;
    }

    /// <summary>
    ///     isJSONSafe indicates whether the int is safely representable as a floating point value in JSON.
    /// </summary>
    public bool JsonSafe => i >= MinIntJson && i <= MaxIntJson;

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public IVal Add(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "add", other);

        try
        {
            return IntOf(Overflow.AddInt64Checked(i, ((IntT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public IVal Compare(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "compare", other);

        return IntOf(i.CompareTo(((IntT)other).i));
    }

    /// <summary>
    ///     Divide implements traits.Divider.Divide.
    /// </summary>
    public IVal Divide(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "divide", other);

        var otherInt = ((IntT)other).i;
        if (otherInt == 0L) return Err.DivideByZero();

        try
        {
            return IntOf(Overflow.DivideInt64Checked(i, ((IntT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    /// <summary>
    ///     Modulo implements traits.Modder.Modulo.
    /// </summary>
    public IVal Modulo(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "modulo", other);

        var otherInt = ((IntT)other).i;
        if (otherInt == 0L) return Err.ModulusByZero();

        try
        {
            return IntOf(Overflow.ModuloInt64Checked(i, ((IntT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    /// <summary>
    ///     Multiply implements traits.Multiplier.Multiply.
    /// </summary>
    public IVal Multiply(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "multiply", other);

        try
        {
            return IntOf(Overflow.MultiplyInt64Checked(i, ((IntT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    /// <summary>
    ///     Negate implements traits.Negater.Negate.
    /// </summary>
    public IVal Negate()
    {
        try
        {
            return IntOf(Overflow.NegateInt64Checked(i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    /// <summary>
    ///     Subtract implements traits.Subtractor.Subtract.
    /// </summary>
    public IVal Subtract(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "subtract", other);

        try
        {
            return IntOf(Overflow.SubtractInt64Checked(i, ((IntT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrIntOverflow;
        }
    }

    public static IntT IntOfCompare(int compareToResult)
    {
        if (compareToResult < 0)
            return IntNegOne;
        if (compareToResult > 0)
            return IntOne;
        return IntZero;
    }

    public static IntT IntOf(long i)
    {
        if (i == 0L) return IntZero;

        if (i == 1L) return IntOne;

        if (i == -1L) return IntNegOne;

        return new IntT(i);
    }

    public override long IntValue()
    {
        return i;
    }

    public override ulong UintValue()
    {
        return Convert.ToUInt64(i);
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeDesc == typeof(long) || typeDesc == typeof(object))
            return i;

        if (typeDesc == typeof(int) || typeDesc == typeof(Enum))
        {
            if (i < int.MinValue || i > int.MaxValue)
                Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "int"));

            return Convert.ToInt32(i);
        }

        if (typeDesc == typeof(uint))
            return Convert.ToUInt32(i);

        if (typeDesc == typeof(ulong))
            return Convert.ToUInt64(i);

        if (typeDesc == typeof(Any))
        {
            var value = new Int64Value();
            value.Value = i;
            return Any.Pack(value);
        }

        if (typeDesc == typeof(Int64Value)) return i;

        if (typeDesc == typeof(Int32Value))
        {
            if (i < int.MinValue || i > int.MaxValue)
                Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "int"));

            return Convert.ToInt32(i);
        }

        if (typeDesc == typeof(IVal) || typeDesc == typeof(IntT)) return this;

        if (typeDesc == typeof(Value))
        {
            // The proto-to-JSON conversion rules would convert all 64-bit integer values to JSON
            // decimal strings. Because CEL ints might come from the automatic widening of 32-bit
            // values in protos, the JSON type is chosen dynamically based on the value.
            //
            // - Integers -2^53-1 < n < 2^53-1 are encoded as JSON numbers.
            // - Integers outside this range are encoded as JSON strings.
            //
            // The integer to float range represents the largest interval where such a conversion
            // can round-trip accurately. Thus, conversions from a 32-bit source can expect a JSON
            // number as with protobuf. Those consuming JSON from a 64-bit source must be able to
            // handle either a JSON number or a JSON decimal string. To handle these cases safely
            // the string values must be explicitly converted to int() within a CEL expression;
            // however, it is best to simply stay within the JSON number range when building JSON
            // objects in CEL.
            if (i >= MinIntJson && i <= MaxIntJson)
            {
                var value = new Value();
                value.NumberValue = i;
                return value;
            }

            // Proto3 to JSON conversion requires string-formatted int64 values
            // since the conversion to floating point would result in truncation.
            var stringValue = new Value();
            stringValue.StringValue = Convert.ToString(i);
            return stringValue;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", IntType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeValue)
    {
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Int:
                return this;
            case TypeEnum.InnerEnum.Uint:
                if (i < 0) return Err.RangeError(i, "uint");

                return UintT.UintOf(Convert.ToUInt64(i));
            case TypeEnum.InnerEnum.Double:
                return DoubleT.DoubleOf(i);
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(Convert.ToString(i));
            case TypeEnum.InnerEnum.Timestamp:
                // The maximum positive value that can be passed to time.Unix is math.MaxInt64 minus the
                // number of seconds between year 1 and year 1970. See comments on unixToInternal.
                if (i < TimestampT.MinUnixTime || i > TimestampT.MaxUnixTime) return Err.ErrTimestampOverflow;

                return TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(i).InZone(TimestampT.ZoneIdZ));
            case TypeEnum.InnerEnum.Type:
                return IntType;
        }

        return Err.NewTypeConversionError(IntType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        if (!(other is IntT)) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(i == ((IntT)other).i);
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return IntType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return i;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var intT = (IntT)o;
        return i == intT.i;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), i);
    }
}