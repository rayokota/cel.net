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
///     Uint type implementation which supports comparison and math operators.
/// </summary>
public sealed class UintT : BaseVal, IAdder, IComparer, IDivider, IModder, IMultiplier, ISubtractor
{
    /// <summary>
    ///     UintType singleton.
    /// </summary>
    public static readonly IType UintType = TypeT.NewTypeValue(TypeEnum.Uint, Trait.AdderType, Trait.ComparerType,
        Trait.DividerType, Trait.ModderType, Trait.MultiplierType, Trait.SubtractorType);

    /// <summary>
    ///     Uint constants
    /// </summary>
    public static readonly UintT UintZero = new(0);

    private readonly ulong i;

    private UintT(ulong i)
    {
        this.i = i;
    }

    /// <summary>
    ///     isJSONSafe indicates whether the uint is safely representable as a floating point value in
    ///     JSON.
    /// </summary>
    public bool JsonSafe => i >= 0 && IntValue() <= IntT.MaxIntJson;

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public IVal Add(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "add", other);

        try
        {
            return UintOf(Overflow.AddUint64Checked(i, ((UintT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrUintOverflow;
        }
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public IVal Compare(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "compare", other);

        return IntT.IntOf(i.CompareTo(((UintT)other).i));
    }

    /// <summary>
    ///     Divide implements traits.Divider.Divide.
    /// </summary>
    public IVal Divide(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "divide", other);

        var otherInt = ((UintT)other).i;
        if (otherInt == 0L) return Err.DivideByZero();

        return UintOf(i / otherInt);
    }

    /// <summary>
    ///     Modulo implements traits.Modder.Modulo.
    /// </summary>
    public IVal Modulo(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "modulo", other);

        var otherInt = ((UintT)other).i;
        if (otherInt == 0L) return Err.ModulusByZero();

        return UintOf(i % otherInt);
    }

    /// <summary>
    ///     Multiply implements traits.Multiplier.Multiply.
    /// </summary>
    public IVal Multiply(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "multiply", other);

        try
        {
            return UintOf(Overflow.MultiplyUint64Checked(i, ((UintT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrUintOverflow;
        }
    }

    /// <summary>
    ///     Subtract implements traits.Subtractor.Subtract.
    /// </summary>
    public IVal Subtract(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "subtract", other);

        try
        {
            return UintOf(Overflow.SubtractUint64Checked(i, ((UintT)other).i));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrUintOverflow;
        }
    }

    public static UintT UintOf(ulong i)
    {
        if (i == 0L) return UintZero;

        return new UintT(i);
    }

    public override long IntValue()
    {
        return Convert.ToInt64(i);
    }

    public override ulong UintValue()
    {
        return i;
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeDesc == typeof(long) || typeDesc == typeof(object))
        {
            if (i < 0) Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "long"));

            return Convert.ToInt64(i);
        }

        if (typeDesc == typeof(int))
        {
            if (i < 0 || i > int.MaxValue) Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "int"));

            return Convert.ToInt32(i);
        }

        if (typeDesc == typeof(uint))
        {
            if (i > uint.MaxValue)
                Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "uint"));

            return Convert.ToUInt32(i);
        }

        if (typeDesc == typeof(ulong)) return i;

        if (typeDesc == typeof(Any))
        {
            var value = new UInt64Value();
            value.Value = i;
            return Any.Pack(value);
        }

        if (typeDesc == typeof(UInt64Value)) return i;

        if (typeDesc == typeof(UInt32Value))
        {
            if (i > uint.MaxValue)
                Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "uint"));

            return Convert.ToUInt32(i);
        }

        if (typeDesc == typeof(IVal) || typeDesc == typeof(UintT)) return this;

        if (typeDesc == typeof(Value))
        {
            if (IntValue() <= IntT.MaxIntJson)
            {
                // JSON can accurately represent 32-bit uints as floating point values.
                var value = new Value();
                value.NumberValue = i;
                return value;
            }
            else
            {
                // Proto3 to JSON conversion requires string-formatted uint64 values
                // since the conversion to floating point would result in truncation.
                var value = new Value();
                value.StringValue = i.ToString();
                return value;
            }
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", UintType,
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
                if (i > long.MaxValue) return Err.RangeError(i.ToString(), "long");

                return IntT.IntOf(Convert.ToInt64(i));
            case TypeEnum.InnerEnum.Uint:
                return this;
            case TypeEnum.InnerEnum.Double:
                return DoubleT.DoubleOf(i);
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(i.ToString());
            case TypeEnum.InnerEnum.Type:
                return UintType;
        }

        return Err.NewTypeConversionError(UintType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        if (other.Type() != UintType) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(i == ((UintT)other).i);
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return UintType;
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

        var uintT = (UintT)o;
        return i == uintT.i;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), i);
    }
}