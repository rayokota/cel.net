using System.Text;
using Cel.Common.Types;
using Microsoft.VisualBasic.CompilerServices;

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
namespace Cel.Interpreter.Functions
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.BoolType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BytesT.BytesType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DoubleT.DoubleType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DurationT.DurationType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntNegOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.TimestampType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TypeT.TypeType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.UintType;

    using Operator = Cel.Common.Operators.Operator;
    using IterableT = Cel.Common.Types.IterableT;
    using IteratorT = Cel.Common.Types.IteratorT;
    using Overloads = Cel.Common.Types.Overloads;
    using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
    using Val = Cel.Common.Types.Ref.Val;
    using Adder = Cel.Common.Types.Traits.Adder;
    using Comparer = Cel.Common.Types.Traits.Comparer;
    using Container = Cel.Common.Types.Traits.Container;
    using Divider = Cel.Common.Types.Traits.Divider;
    using Indexer = Cel.Common.Types.Traits.Indexer;
    using Matcher = Cel.Common.Types.Traits.Matcher;
    using Modder = Cel.Common.Types.Traits.Modder;
    using Multiplier = Cel.Common.Types.Traits.Multiplier;
    using Negater = Cel.Common.Types.Traits.Negater;
    using Sizer = Cel.Common.Types.Traits.Sizer;
    using Subtractor = Cel.Common.Types.Traits.Subtractor;
    using Trait = Cel.Common.Types.Traits.Trait;

    /// <summary>
    /// Overload defines a named overload of a function, indicating an operand trait which must be
    /// present on the first argument to the overload as well as one of either a unary, binary, or
    /// function implementation.
    /// 
    /// <para>The majority of operators within the expression language are unary or binary and the
    /// specializations simplify the call contract for implementers of types with operator overloads. Any
    /// added complexity is assumed to be handled by the generic FunctionOp.
    /// </para>
    /// </summary>
    public sealed class Overload
    {
        /// <summary>
        /// Operator name as written in an expression or defined within operators.go. </summary>
        public readonly string @operator;

        /// <summary>
        /// Operand trait used to dispatch the call. The zero-value indicates a global function overload or
        /// that one of the Unary / Binary / Function definitions should be used to execute the call.
        /// </summary>
        public readonly Trait? operandTrait;

        /// <summary>
        /// Unary defines the overload with a UnaryOp implementation. May be nil. </summary>
        public readonly UnaryOp unary;

        /// <summary>
        /// Binary defines the overload with a BinaryOp implementation. May be nil. </summary>
        public readonly BinaryOp binary;

        /// <summary>
        /// Function defines the overload with a FunctionOp implementation. May be nil. </summary>
        public readonly FunctionOp function;

        public static Overload Unary(Operator @operator, UnaryOp op)
        {
            return Unary(@operator.id, op);
        }

        public static Overload Unary(string @operator, UnaryOp op)
        {
            return Unary(@operator, null, op);
        }

        public static Overload Unary(Operator @operator, Trait? trait, UnaryOp op)
        {
            return Unary(@operator.id, trait, op);
        }

        public static Overload Unary(string @operator, Trait? trait, UnaryOp op)
        {
            return new Overload(@operator, trait, op, null, null);
        }

        public static Overload Binary(Operator @operator, BinaryOp op)
        {
            return Binary(@operator.id, op);
        }

        public static Overload Binary(string @operator, BinaryOp op)
        {
            return Binary(@operator, null, op);
        }

        public static Overload Binary(Operator @operator, Trait? trait, BinaryOp op)
        {
            return Binary(@operator.id, trait, op);
        }

        public static Overload Binary(string @operator, Trait? trait, BinaryOp op)
        {
            return new Overload(@operator, trait, null, op, null);
        }

        public static Overload Function(string @operator, FunctionOp op)
        {
            return Function(@operator, null, op);
        }

        public static Overload Function(string @operator, Trait? trait, FunctionOp op)
        {
            return new Overload(@operator, trait, null, null, op);
        }

        public static Overload overload(string @operator, Trait trait, UnaryOp unary, BinaryOp binary,
            FunctionOp function)
        {
            return new Overload(@operator, trait, unary, binary, function);
        }

        private Overload(string @operator, Trait? operandTrait, UnaryOp unary, BinaryOp binary, FunctionOp function)
        {
            this.@operator = @operator;
            this.operandTrait = operandTrait;
            this.unary = unary;
            this.binary = binary;
            this.function = function;
        }

        public override string ToString()
        {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder sb = new StringBuilder("Overload{");
            StringBuilder sb = new StringBuilder("Overload{");
            sb.Append(@operator).Append('\'');
            sb.Append(", trait=").Append(operandTrait);
            if (unary != null)
            {
                sb.Append(", unary");
            }

            if (binary != null)
            {
                sb.Append(", binary");
            }

            if (binary != null)
            {
                sb.Append(", function");
            }

            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// StandardOverloads returns the definitions of the built-in overloads. </summary>
        public static Overload[] StandardOverloads()
        {
            return new Overload[]
            {
                Unary(Operator.LogicalNot, Trait.NegatorType, v =>
                {
                    if (v.Type().TypeEnum() == Cel.Common.Types.Ref.TypeEnum.Bool)
                    {
                        return ((Negater)v).Negate();
                    }

                    return Err.NoSuchOverload(null, Operator.LogicalNot.id, v);
                }),
                Unary(Operator.NotStrictlyFalse, Overload.NotStrictlyFalse),
                Unary(Operator.OldNotStrictlyFalse, Overload.NotStrictlyFalse), Binary(Operator.Less,
                    Trait.ComparerType, (lhs, rhs) =>
                    {
                        Cel.Common.Types.Ref.Val cmp = ((Comparer)lhs).Compare(rhs);
                        if (cmp == IntT.IntNegOne)
                        {
                            return BoolT.True;
                        }

                        if (cmp == IntT.IntOne || cmp == IntT.IntZero)
                        {
                            return BoolT.False;
                        }

                        return cmp;
                    }),
                Binary(Operator.LessEquals, Trait.ComparerType, (lhs, rhs) =>
                {
                    Cel.Common.Types.Ref.Val cmp = ((Comparer)lhs).Compare(rhs);
                    if (cmp == IntT.IntNegOne || cmp == IntT.IntZero)
                    {
                        return BoolT.True;
                    }

                    if (cmp == IntT.IntOne)
                    {
                        return BoolT.False;
                    }

                    return cmp;
                }),
                Binary(Operator.Greater, Trait.ComparerType, (lhs, rhs) =>
                {
                    Cel.Common.Types.Ref.Val cmp = ((Comparer)lhs).Compare(rhs);
                    if (cmp == IntT.IntOne)
                    {
                        return BoolT.True;
                    }

                    if (cmp == IntT.IntNegOne || cmp == IntT.IntZero)
                    {
                        return BoolT.False;
                    }

                    return cmp;
                }),
                Binary(Operator.GreaterEquals, Trait.ComparerType, (lhs, rhs) =>
                {
                    Cel.Common.Types.Ref.Val cmp = ((Comparer)lhs).Compare(rhs);
                    if (cmp == IntT.IntOne || cmp == IntT.IntZero)
                    {
                        return BoolT.True;
                    }

                    if (cmp == IntT.IntNegOne)
                    {
                        return BoolT.False;
                    }

                    return cmp;
                }),
                Binary(Operator.Add, Trait.AdderType, (lhs, rhs) => ((Adder)lhs).Add(rhs)),
                Binary(Operator.Subtract, Trait.SubtractorType, (lhs, rhs) => ((Subtractor)lhs).Subtract(rhs)),
                Binary(Operator.Multiply, Trait.MultiplierType, (lhs, rhs) => ((Multiplier)lhs).Multiply(rhs)),
                Binary(Operator.Divide, Trait.DividerType, (lhs, rhs) => ((Divider)lhs).Divide(rhs)),
                Binary(Operator.Modulo, Trait.ModderType, (lhs, rhs) => ((Modder)lhs).Modulo(rhs)), Unary(
                    Operator.Negate, Trait.NegatorType, v =>
                    {
                        if (v.Type().TypeEnum() != Cel.Common.Types.Ref.TypeEnum.Bool)
                        {
                            return ((Negater)v).Negate();
                        }
                        return Err.NoSuchOverload(null, Operator.Negate.id, v);
                    }),
                Binary(Operator.Index, Trait.IndexerType, (lhs, rhs) => ((Indexer)lhs).Get(rhs)),
                Unary(Overloads.Size, Trait.SizerType, (v) => ((Sizer)v).Size()),
                Binary(Operator.In, Overload.InAggregate), Binary(Operator.OldIn, Overload.InAggregate),
                Binary(Overloads.Matches, Trait.MatcherType, (lhs, rhs) => ((Matcher)lhs).Match(rhs)),
                Unary(Overloads.TypeConvertInt, v => v.ConvertToType(IntT.IntType)),
                Unary(Overloads.TypeConvertUint, v => v.ConvertToType(UintT.UintType)),
                Unary(Overloads.TypeConvertDouble, v => v.ConvertToType(DoubleT.DoubleType)),
                Unary(Overloads.TypeConvertBool, v => v.ConvertToType(BoolT.BoolType)),
                Unary(Overloads.TypeConvertBytes, v => v.ConvertToType(BytesT.BytesType)),
                Unary(Overloads.TypeConvertString, v => v.ConvertToType(StringT.StringType)),
                Unary(Overloads.TypeConvertTimestamp, v => v.ConvertToType(TimestampT.TimestampType)),
                Unary(Overloads.TypeConvertDuration, v => v.ConvertToType(DurationT.DurationType)),
                Unary(Overloads.TypeConvertType, v => v.ConvertToType(TypeT.TypeType)),
                Unary(Overloads.TypeConvertDyn, v => v),
                Unary(Overloads.Iterator, Trait.IterableType, v => ((IterableT)v).Iterator()),
                Unary(Overloads.HasNext, Trait.IteratorType, v => ((IteratorT)v).HasNext()),
                Unary(Overloads.Next, Trait.IteratorType, v => ((IteratorT)v).Next())
            };
        }

        internal static Val NotStrictlyFalse(Val value)
        {
            if (value.Type().TypeEnum() == TypeEnum.Bool)
            {
                return value;
            }

            return BoolT.True;
        }

        internal static Val InAggregate(Val lhs, Val rhs)
        {
            if (rhs.Type().HasTrait(Trait.ContainerType))
            {
                return ((Container)rhs).Contains(lhs);
            }

            return Err.NoSuchOverload(lhs, Operator.In.id, rhs);
        }
    }
}