using System.Numerics;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;
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
///     Double type that implements ref.Val, comparison, and mathematical operations.
/// </summary>
public sealed class DoubleT : BaseVal, IAdder, IComparer, IDivider, IMultiplier, INegater, ISubtractor
{
    /// <summary>
    ///     DoubleType singleton.
    /// </summary>
    public static readonly IType DoubleType = TypeT.NewTypeValue(TypeEnum.Double, Trait.AdderType,
        Trait.ComparerType, Trait.DividerType, Trait.MultiplierType, Trait.NegatorType, Trait.SubtractorType);

    private static readonly BigInteger MaxUint64 = BigInteger.Subtract(BigInteger.One << 64, BigInteger.One);

    private readonly double d;

    private DoubleT(double d)
    {
        this.d = d;
    }

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public IVal Add(IVal other)
    {
        if (!(other is DoubleT)) return Err.NoSuchOverload(this, "add", other);

        return DoubleOf(d + ((DoubleT)other).d);
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public IVal Compare(IVal other)
    {
        // NaN has no place in an ordering; report it rather than let CompareTo impose the
        // total order it uses for sorting.
        if (double.IsNaN(d)) return Err.NewErr("NaN values cannot be ordered");

        switch (other)
        {
            case DoubleT o:
                if (double.IsNaN(o.d)) return Err.NewErr("NaN values cannot be ordered");
                // Compares equal for the special case of -0.0d == 0.0d (IEEE 754).
                return IntT.IntOfCompare(NumericCompare.CompareDoubleDouble(d, o.d));
            case IntT o:
                return IntT.IntOfCompare(NumericCompare.CompareDoubleLong(d, o.LongValue));
            case UintT o:
                return IntT.IntOfCompare(NumericCompare.CompareDoubleULong(d, o.ULongValue));
            default:
                return Err.NoSuchOverload(this, "compare", other);
        }
    }

    /// <summary>
    ///     The wrapped value, for the cross-type comparisons in <see cref="Compare" />.
    /// </summary>
    internal double DoubleValue => d;

    /// <summary>
    ///     Divide implements traits.Divider.Divide.
    /// </summary>
    public IVal Divide(IVal other)
    {
        if (!(other is DoubleT)) return Err.NoSuchOverload(this, "divide", other);

        return DoubleOf(d / ((DoubleT)other).d);
    }

    /// <summary>
    ///     Multiply implements traits.Multiplier.Multiply.
    /// </summary>
    public IVal Multiply(IVal other)
    {
        if (!(other is DoubleT)) return Err.NoSuchOverload(this, "multiply", other);

        return DoubleOf(d * ((DoubleT)other).d);
    }

    /// <summary>
    ///     Negate implements traits.Negater.Negate.
    /// </summary>
    public IVal Negate()
    {
        return DoubleOf(-d);
    }

    /// <summary>
    ///     Subtract implements traits.Subtractor.Subtract.
    /// </summary>
    public IVal Subtract(IVal other)
    {
        if (!(other is DoubleT)) return Err.NoSuchOverload(this, "subtract", other);

        return DoubleOf(d - ((DoubleT)other).d);
    }

    public static DoubleT DoubleOf(double d)
    {
        return new DoubleT(d);
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeDesc == typeof(double) || typeDesc == typeof(object))
            return Convert.ToDouble(d);

        if (typeDesc == typeof(float))
            // TODO needs overflow check
            return Convert.ToSingle((float)d);

        if (typeDesc == typeof(Any))
        {
            var value = new DoubleValue();
            value.Value = d;
            return Any.Pack(value);
        }

        if (typeDesc == typeof(DoubleValue)) return d;

        if (typeDesc == typeof(FloatValue)) return (float)d;

        if (typeDesc == typeof(IVal) || typeDesc == typeof(DoubleT)) return this;

        if (typeDesc == typeof(Value))
        {
            var value = new Value();
            value.NumberValue = d;
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", DoubleType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeValue)
    {
        // NOTE: the original Go test assert on `intOf(-5)`, because Go's implementation uses
        // the Go `math.Round(float64)` function. The implementation of Go's `math.Round(float64)`
        // behaves differently to Java's `Math.round(double)` (or `Math.rint()`).
        // Further, the CEL-spec conformance tests assert on a different behavior and therefore those
        // conformance-tests fail against the Go implementation.
        // Even more complicated: the CEL-spec says: "CEL provides no way to control the finer points
        // of floating-point arithmetic, such as expression evaluation, rounding mode, or exception
        // handling. However, any two not-a-number values will compare equal even if their underlying
        // properties are different."
        // (see https://github.com/google/cel-spec/blob/master/doc/langdef.md#numeric-values)
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Int:
                var r = (long)d; // ?? Math.round(d);
                if (r == long.MinValue || r == long.MaxValue) return Err.RangeError(d, "int");

                return IntT.IntOf(r);
            case TypeEnum.InnerEnum.Uint:
                // hack to support uint64
                var bi = (BigInteger)d;
                if (d < 0 || bi.CompareTo(MaxUint64) > 0) return Err.RangeError(d, "int");

                return UintT.UintOf((ulong)bi);
            case TypeEnum.InnerEnum.Double:
                return this;
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(Convert.ToString(d));
            case TypeEnum.InnerEnum.Type:
                return DoubleType;
        }

        return Err.NewTypeConversionError(DoubleType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        // Numeric equality is cross-type, matching cel-go and cel-java; a NaN operand - on
        // either side - is never equal. See IntT.Equal for why this is independent of the
        // ordering feature.
        if (double.IsNaN(d)) return BoolT.False;
        switch (other)
        {
            case DoubleT o:
                if (double.IsNaN(o.d)) return BoolT.False;
                return Types.BoolOf(d == o.d);
            case IntT o:
                return Types.BoolOf(NumericCompare.CompareDoubleLong(d, o.LongValue) == 0);
            case UintT o:
                return Types.BoolOf(NumericCompare.CompareDoubleULong(d, o.ULongValue) == 0);
            default:
                return Err.NoSuchOverload(this, "equal", other);
        }
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return DoubleType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return d;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var doubleT = (DoubleT)o;
        var od = ((DoubleT)o).d;
        if (d == od)
            // work around for special case of -0.0d == 0.0d (IEEE 754)
            return true;

        return doubleT.d.CompareTo(d) == 0;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), d);
    }
}