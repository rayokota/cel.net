using System;
using System.Numerics;

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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.divideByZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errUintOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.modulusByZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.rangeError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.maxIntJSON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using OverflowException = global::Cel.Common.Types.Overflow.OverflowException;
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
    using Val = global::Cel.Common.Types.Ref.Val;
    using Adder = global::Cel.Common.Types.Traits.Adder;
    using Comparer = global::Cel.Common.Types.Traits.Comparer;
    using Divider = global::Cel.Common.Types.Traits.Divider;
    using Modder = global::Cel.Common.Types.Traits.Modder;
    using Multiplier = global::Cel.Common.Types.Traits.Multiplier;
    using Subtractor = global::Cel.Common.Types.Traits.Subtractor;
    using Trait = global::Cel.Common.Types.Traits.Trait;

    /// <summary>
    /// Uint type implementation which supports comparison and math operators. </summary>
    public sealed class UintT : BaseVal, Adder, Comparer, Divider, Modder, Multiplier, Subtractor
    {
        /// <summary>
        /// UintType singleton. </summary>
        public static readonly Type UintType = TypeT.NewTypeValue(TypeEnum.Uint, Trait.AdderType, Trait.ComparerType,
            Trait.DividerType, Trait.ModderType, Trait.MultiplierType, Trait.SubtractorType);

        /// <summary>
        /// Uint constants </summary>
        public static readonly UintT UintZero = new UintT(0);

        public static UintT UintOf(ulong i)
        {
            if (i == 0L)
            {
                return UintZero;
            }

            return new UintT(i);
        }

        private readonly ulong i;

        private UintT(ulong i)
        {
            this.i = i;
        }

        public override long IntValue()
        {
            return (int)i;
        }

        /// <summary>
        /// Add implements traits.Adder.Add. </summary>
        public Val Add(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "add", other);
            }

            try
            {
                return UintOf((ulong)Overflow.AddUint64Checked((long)i, (long)((UintT)other).i));
            }
            catch (OverflowException)
            {
                return Err.ErrUintOverflow;
            }
        }

        /// <summary>
        /// Compare implements traits.Comparer.Compare. </summary>
        public Val Compare(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "compare", other);
            }

            return IntT.IntOf(i.CompareTo(((UintT)other).i));
        }

        /// <summary>
        /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc == typeof(long) || typeDesc == typeof(object))
            {
                if (i < 0)
                {
                    Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "Java long"));
                }

                return Convert.ToInt64(i);
            }

            if (typeDesc == typeof(int))
            {
                if (i < 0 || i > int.MaxValue)
                {
                    Err.ThrowErrorAsIllegalStateException(Err.RangeError(i, "Java int"));
                }

                return Convert.ToInt32((int)i);
            }

            if (typeDesc == typeof(ulong))
            {
                return i;
            }

            if (typeDesc == typeof(Any))
            {
                UInt64Value value = new UInt64Value();
                value.Value = i;
                return Any.Pack(value);
            }

            if (typeDesc == typeof(UInt64Value))
            {
                UInt64Value value = new UInt64Value();
                value.Value = i;
                return value;
            }

            if (typeDesc == typeof(UInt32Value))
            {
                UInt32Value value = new UInt32Value();
                value.Value = Convert.ToUInt32(i);
                return value;
            }

            if (typeDesc == typeof(Val) || typeDesc == typeof(UintT))
            {
                return this;
            }

            if (typeDesc == typeof(Value))
            {
                if ((int)i <= IntT.maxIntJSON)
                {
                    // JSON can accurately represent 32-bit uints as floating point values.
                    Value value = new Value();
                    value.NumberValue = i;
                    return value;
                }
                else
                {
                    // Proto3 to JSON conversion requires string-formatted uint64 values
                    // since the conversion to floating point would result in truncation.
                    Value value = new Value();
                    value.StringValue = i.ToString();
                    return value;
                }
            }

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", UintType,
                typeDesc.FullName));
        }

        /// <summary>
        /// ConvertToType implements ref.Val.ConvertToType. </summary>
        public override Val ConvertToType(Type typeValue)
        {
            switch (typeValue.TypeEnum().InnerEnumValue)
            {
                case TypeEnum.InnerEnum.Int:
                    if (i < 0L)
                    {
                        return Err.RangeError(i.ToString(), "int");
                    }

                    return IntT.IntOf((int)i);
                case TypeEnum.InnerEnum.Uint:
                    return this;
                case TypeEnum.InnerEnum.Double:
                    if (i < 0L)
                    {
                        return DoubleT.DoubleOf(Convert.ToDouble(i));
                    }

                    return DoubleT.DoubleOf(i);
                case TypeEnum.InnerEnum.String:
                    return StringT.StringOf(i.ToString());
                case TypeEnum.InnerEnum.Type:
                    return UintType;
            }

            return Err.NewTypeConversionError(UintType, typeValue);
        }

        /// <summary>
        /// Divide implements traits.Divider.Divide. </summary>
        public Val Divide(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "divide", other);
            }

            ulong otherInt = ((UintT)other).i;
            if (otherInt == 0L)
            {
                return Err.DivideByZero();
            }

            return UintOf(i / otherInt);
        }

        /// <summary>
        /// Equal implements ref.Val.Equal. </summary>
        public override Val Equal(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "equal", other);
            }

            return Types.BoolOf(i == ((UintT)other).i);
        }

        /// <summary>
        /// Modulo implements traits.Modder.Modulo. </summary>
        public Val Modulo(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "modulo", other);
            }

            ulong otherInt = ((UintT)other).i;
            if (otherInt == 0L)
            {
                return Err.ModulusByZero();
            }

            return UintOf(i % otherInt);
        }

        /// <summary>
        /// Multiply implements traits.Multiplier.Multiply. </summary>
        public Val Multiply(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "multiply", other);
            }

            try
            {
                return UintOf((uint)Overflow.MultiplyUint64Checked((long)i, (long)((UintT)other).i));
            }
            catch (OverflowException)
            {
                return Err.ErrUintOverflow;
            }
        }

        /// <summary>
        /// Subtract implements traits.Subtractor.Subtract. </summary>
        public Val Subtract(Val other)
        {
            if (other.Type() != UintType)
            {
                return Err.NoSuchOverload(this, "subtract", other);
            }

            try
            {
                return UintOf((uint)Overflow.SubtractUint64Checked((long)i, (long)((UintT)other).i));
            }
            catch (OverflowException)
            {
                return Err.ErrUintOverflow;
            }
        }

        /// <summary>
        /// Type implements ref.Val.Type. </summary>
        public override Type Type()
        {
            return UintType;
        }

        /// <summary>
        /// Value implements ref.Val.Value. </summary>
        public override object Value()
        {
            return i;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }

            UintT uintT = (UintT)o;
            return i == uintT.i;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), i);
        }

        /// <summary>
        /// isJSONSafe indicates whether the uint is safely representable as a floating point value in
        /// JSON.
        /// </summary>
        public bool JSONSafe
        {
            get { return i >= 0 && (long)i <= IntT.maxIntJSON; }
        }
    }
}