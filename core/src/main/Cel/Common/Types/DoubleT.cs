using System;
using System.Numerics;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;

/*
 * Copyright (C) 2021 The Authors of CEL-Java
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
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.rangeError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOfCompare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.uintOf;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
    using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
    using Val = global::Cel.Common.Types.Ref.Val;
    using Adder = global::Cel.Common.Types.Traits.Adder;
    using Comparer = global::Cel.Common.Types.Traits.Comparer;
    using Divider = global::Cel.Common.Types.Traits.Divider;
    using Multiplier = global::Cel.Common.Types.Traits.Multiplier;
    using Negater = global::Cel.Common.Types.Traits.Negater;
    using Subtractor = global::Cel.Common.Types.Traits.Subtractor;
    using Trait = global::Cel.Common.Types.Traits.Trait;

    /// <summary>
    /// Double type that implements ref.Val, comparison, and mathematical operations. </summary>
    public sealed class DoubleT : BaseVal, Adder, Comparer, Divider, Multiplier, Negater, Subtractor
    {
        /// <summary>
        /// DoubleType singleton. </summary>
        public static readonly Type DoubleType = TypeT.NewTypeValue(TypeEnum.Double, Trait.AdderType,
            Trait.ComparerType, Trait.DividerType, Trait.MultiplierType, Trait.NegatorType, Trait.SubtractorType);

        public static DoubleT DoubleOf(double d)
        {
            return new DoubleT(d);
        }

        private readonly double d;

        private DoubleT(double d)
        {
            this.d = d;
        }

        /// <summary>
        /// Add implements traits.Adder.Add. </summary>
        public Val Add(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "add", other);
            }

            return DoubleOf(d + ((DoubleT)other).d);
        }

        /// <summary>
        /// Compare implements traits.Comparer.Compare. </summary>
        public Val Compare(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "compare", other);
            }

            double od = ((DoubleT)other).d;
            if (d == od)
            {
                // work around for special case of -0.0d == 0.0d (IEEE 754)
                return IntT.IntZero;
            }

            return IntT.IntOfCompare(d.CompareTo(od));
        }

        /// <summary>
        /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc == typeof(Double) || typeDesc == typeof(double) || typeDesc == typeof(object))
            {
                return Convert.ToDouble(d);
            }

            if (typeDesc == typeof(float) || typeDesc == typeof(float))
            {
                // TODO needs overflow check
                return Convert.ToSingle((float)d);
            }

            if (typeDesc == typeof(Any))
            {
                DoubleValue value = new DoubleValue();
                value.Value = d;
                return Any.Pack(value);
            }

            if (typeDesc == typeof(DoubleValue))
            {
                DoubleValue value = new DoubleValue();
                value.Value = d;
                return value;
            }

            if (typeDesc == typeof(FloatValue))
            {
                // TODO needs overflow check
                FloatValue value = new FloatValue();
                value.Value = (float)d;
                return value;
            }

            if (typeDesc == typeof(Val) || typeDesc == typeof(DoubleT))
            {
                return this;
            }

            if (typeDesc == typeof(Value))
            {
                Value value = new Value();
                value.NumberValue = d;
                return value;
            }

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", DoubleType,
                typeDesc.FullName));
        }

        private static readonly BigInteger MAX_UINT64 = BigInteger.Subtract(BigInteger.One << 64, BigInteger.One);

        /// <summary>
        /// ConvertToType implements ref.Val.ConvertToType. </summary>
        public override Val ConvertToType(Type typeValue)
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
                    long r = (long)d; // ?? Math.round(d);
                    if (r == long.MinValue || r == long.MaxValue)
                    {
                        return Err.RangeError(d, "int");
                    }

                    return IntT.IntOf(r);
                case TypeEnum.InnerEnum.Uint:
                    // hack to support uint64
                    decimal dec = new decimal(d);
                    BigInteger bi = (BigInteger)d;
                    if (d < 0 || bi.CompareTo(MAX_UINT64) > 0)
                    {
                        return Err.RangeError(d, "int");
                    }

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
        /// Divide implements traits.Divider.Divide. </summary>
        public Val Divide(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "divide", other);
            }

            return DoubleOf(d / ((DoubleT)other).d);
        }

        /// <summary>
        /// Equal implements ref.Val.Equal. </summary>
        public override Val Equal(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "equal", other);
            }

            /// <summary>
            /// TODO: Handle NaNs properly. </summary>
            return Types.BoolOf(d == ((DoubleT)other).d);
        }

        /// <summary>
        /// Multiply implements traits.Multiplier.Multiply. </summary>
        public Val Multiply(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "multiply", other);
            }

            return DoubleOf(d * ((DoubleT)other).d);
        }

        /// <summary>
        /// Negate implements traits.Negater.Negate. </summary>
        public Val Negate()
        {
            return DoubleOf(-d);
        }

        /// <summary>
        /// Subtract implements traits.Subtractor.Subtract. </summary>
        public Val Subtract(Val other)
        {
            if (!(other is DoubleT))
            {
                return Err.NoSuchOverload(this, "subtract", other);
            }

            return DoubleOf(d - ((DoubleT)other).d);
        }

        /// <summary>
        /// Type implements ref.Val.Type. </summary>
        public override Type Type()
        {
            return DoubleType;
        }

        /// <summary>
        /// Value implements ref.Val.Value. </summary>
        public override object Value()
        {
            return d;
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

            DoubleT doubleT = (DoubleT)o;
            double od = ((DoubleT)o).d;
            if (d == od)
            {
                // work around for special case of -0.0d == 0.0d (IEEE 754)
                return true;
            }

            return doubleT.d.CompareTo(d) == 0;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), d);
        }
    }
}