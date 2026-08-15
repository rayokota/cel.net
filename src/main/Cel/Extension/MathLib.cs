using Cel.Checker;
using Cel.Common;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter.Functions;
using Cel.Parser;
using Google.Api.Expr.V1Alpha1;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Extension;

/// <summary>
///     MathLib provides a <seealso cref="IEnvOption" /> to configure namespaced mathematical
///     helper functions and macros. The implementation is ported from
///     <a href="https://github.com/google/cel-go/blob/master/ext/math.go">cel-go</a>.
///     <para>
///         All functions are namespaced under <c>math.</c>. The greatest and least helpers are
///         receiver macros on the <c>math</c> namespace that expand to the internal
///         <c>math.@max</c> / <c>math.@min</c> functions; the remainder are ordinary namespaced
///         functions resolved by the checker's qualified-name folding.
///     </para>
///     <para>
///         <b>math.greatest / math.least</b> - the maximum or minimum of their arguments, taken
///         over two or more numeric arguments or a single non-empty list of numbers. Operands may
///         mix int, uint and double; the comparison is exact across those types.
///         <pre>    math.greatest(1, 2u, 3.0)   // 3.0</pre>
///         <pre>    math.least([4, 2, 8])       // 2</pre>
///     </para>
///     <para>
///         <b>Rounding</b> - <c>math.ceil</c>, <c>math.floor</c>, <c>math.round</c>,
///         <c>math.trunc</c> over a double.
///     </para>
///     <para>
///         <b>Classification</b> - <c>math.isInf</c>, <c>math.isNaN</c>, <c>math.isFinite</c> over
///         a double, returning a bool.
///     </para>
///     <para>
///         <b>Sign / magnitude</b> - <c>math.abs</c>, <c>math.sign</c> over int, uint or double;
///         <c>math.sqrt</c> over int, uint or double, returning a double.
///     </para>
///     <para>
///         <b>Bitwise</b> - <c>math.bitAnd</c>, <c>math.bitOr</c>, <c>math.bitXor</c> over a pair
///         of int or a pair of uint; <c>math.bitNot</c> over a single int or uint;
///         <c>math.bitShiftLeft</c>, <c>math.bitShiftRight</c> over an int or uint shifted by a
///         non-negative int.
///     </para>
/// </summary>
public class MathLib : ILibrary
{
    private const string MathNamespace = "math";

    // greatest / least are receiver macros on the math namespace; they expand to the internal
    // @max / @min functions, which carry the actual overloads.
    private const string GreatestMacro = "greatest";
    private const string LeastMacro = "least";
    private const string MinFunc = "math.@min";
    private const string MaxFunc = "math.@max";

    private const string Abs = "math.abs";
    private const string Ceil = "math.ceil";
    private const string Floor = "math.floor";
    private const string Round = "math.round";
    private const string Trunc = "math.trunc";
    private const string Sign = "math.sign";
    private const string IsInf = "math.isInf";
    private const string IsNaN = "math.isNaN";
    private const string IsFinite = "math.isFinite";
    private const string Sqrt = "math.sqrt";
    private const string BitAnd = "math.bitAnd";
    private const string BitOr = "math.bitOr";
    private const string BitXor = "math.bitXor";
    private const string BitNot = "math.bitNot";
    private const string BitShiftLeft = "math.bitShiftLeft";
    private const string BitShiftRight = "math.bitShiftRight";

    public virtual IList<EnvOption> CompileOptions
    {
        get
        {
            IList<EnvOption> list = new List<EnvOption>();

            // greatest / least are variadic receiver macros. The parser leaves a call it does
            // not expand as an ordinary receiver call, so the expanders decline (return null)
            // for any target that is not the bare `math` identifier.
            list.Add(EnvOptions.Macros(
                Macro.NewReceiverVarArgMacro(LeastMacro, MathLeast),
                Macro.NewReceiverVarArgMacro(GreatestMacro, MathGreatest)));

            var minMaxOverloads = new List<Type> { Decls.Int, Decls.Uint, Decls.Double };
            var listMin = new List<Decl.Types.FunctionDecl.Types.Overload>();
            listMin.Add(Decls.NewOverload("math_@min_int", new List<Type> { Decls.Int }, Decls.Int));
            listMin.Add(Decls.NewOverload("math_@min_uint", new List<Type> { Decls.Uint }, Decls.Uint));
            listMin.Add(Decls.NewOverload("math_@min_double", new List<Type> { Decls.Double }, Decls.Double));
            AddNumericPairOverloads(listMin, "math_@min");
            listMin.Add(Decls.NewOverload("math_@min_list_int",
                new List<Type> { Decls.NewListType(Decls.Int) }, Decls.Int));
            listMin.Add(Decls.NewOverload("math_@min_list_uint",
                new List<Type> { Decls.NewListType(Decls.Uint) }, Decls.Uint));
            listMin.Add(Decls.NewOverload("math_@min_list_double",
                new List<Type> { Decls.NewListType(Decls.Double) }, Decls.Double));

            var listMax = new List<Decl.Types.FunctionDecl.Types.Overload>();
            listMax.Add(Decls.NewOverload("math_@max_int", new List<Type> { Decls.Int }, Decls.Int));
            listMax.Add(Decls.NewOverload("math_@max_uint", new List<Type> { Decls.Uint }, Decls.Uint));
            listMax.Add(Decls.NewOverload("math_@max_double", new List<Type> { Decls.Double }, Decls.Double));
            AddNumericPairOverloads(listMax, "math_@max");
            listMax.Add(Decls.NewOverload("math_@max_list_int",
                new List<Type> { Decls.NewListType(Decls.Int) }, Decls.Int));
            listMax.Add(Decls.NewOverload("math_@max_list_uint",
                new List<Type> { Decls.NewListType(Decls.Uint) }, Decls.Uint));
            listMax.Add(Decls.NewOverload("math_@max_list_double",
                new List<Type> { Decls.NewListType(Decls.Double) }, Decls.Double));

            list.Add(EnvOptions.Declarations(
                Decls.NewFunction(MinFunc, listMin),
                Decls.NewFunction(MaxFunc, listMax),
                Decls.NewFunction(Abs,
                    Decls.NewOverload("math_abs_int", new List<Type> { Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_abs_uint", new List<Type> { Decls.Uint }, Decls.Uint),
                    Decls.NewOverload("math_abs_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(Sign,
                    Decls.NewOverload("math_sign_int", new List<Type> { Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_sign_uint", new List<Type> { Decls.Uint }, Decls.Uint),
                    Decls.NewOverload("math_sign_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(Ceil,
                    Decls.NewOverload("math_ceil_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(Floor,
                    Decls.NewOverload("math_floor_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(Round,
                    Decls.NewOverload("math_round_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(Trunc,
                    Decls.NewOverload("math_trunc_double", new List<Type> { Decls.Double }, Decls.Double)),
                Decls.NewFunction(IsInf,
                    Decls.NewOverload("math_isInf_double", new List<Type> { Decls.Double }, Decls.Bool)),
                Decls.NewFunction(IsNaN,
                    Decls.NewOverload("math_isNaN_double", new List<Type> { Decls.Double }, Decls.Bool)),
                Decls.NewFunction(IsFinite,
                    Decls.NewOverload("math_isFinite_double", new List<Type> { Decls.Double }, Decls.Bool)),
                Decls.NewFunction(Sqrt,
                    Decls.NewOverload("math_sqrt_double", new List<Type> { Decls.Double }, Decls.Double),
                    Decls.NewOverload("math_sqrt_int", new List<Type> { Decls.Int }, Decls.Double),
                    Decls.NewOverload("math_sqrt_uint", new List<Type> { Decls.Uint }, Decls.Double)),
                Decls.NewFunction(BitAnd,
                    Decls.NewOverload("math_bitAnd_int_int", new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_bitAnd_uint_uint", new List<Type> { Decls.Uint, Decls.Uint },
                        Decls.Uint)),
                Decls.NewFunction(BitOr,
                    Decls.NewOverload("math_bitOr_int_int", new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_bitOr_uint_uint", new List<Type> { Decls.Uint, Decls.Uint },
                        Decls.Uint)),
                Decls.NewFunction(BitXor,
                    Decls.NewOverload("math_bitXor_int_int", new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_bitXor_uint_uint", new List<Type> { Decls.Uint, Decls.Uint },
                        Decls.Uint)),
                Decls.NewFunction(BitNot,
                    Decls.NewOverload("math_bitNot_int", new List<Type> { Decls.Int }, Decls.Int),
                    Decls.NewOverload("math_bitNot_uint", new List<Type> { Decls.Uint }, Decls.Uint)),
                Decls.NewFunction(BitShiftLeft,
                    Decls.NewOverload("math_bitShiftLeft_int_int", new List<Type> { Decls.Int, Decls.Int },
                        Decls.Int),
                    Decls.NewOverload("math_bitShiftLeft_uint_int", new List<Type> { Decls.Uint, Decls.Int },
                        Decls.Uint)),
                Decls.NewFunction(BitShiftRight,
                    Decls.NewOverload("math_bitShiftRight_int_int", new List<Type> { Decls.Int, Decls.Int },
                        Decls.Int),
                    Decls.NewOverload("math_bitShiftRight_uint_int", new List<Type> { Decls.Uint, Decls.Int },
                        Decls.Uint))));
            return list;
        }
    }

    public virtual IList<ProgramOption> ProgramOptions
    {
        get
        {
            IList<ProgramOption> list = new List<ProgramOption>();
            // One binding per function name; each op dispatches on the runtime operand types,
            // the way StringsLib binds fewer ops than it declares overloads.
            var functions = global::Cel.ProgramOptions.Functions(
                Overload.NewOverload(MinFunc, Trait.None, MinUnary, MinPair, null),
                Overload.NewOverload(MaxFunc, Trait.None, MaxUnary, MaxPair, null),
                Overload.Unary(Abs, AbsOp),
                Overload.Unary(Sign, SignOp),
                Overload.Unary(Ceil, v => DoubleUnary(v, System.Math.Ceiling, Ceil)),
                Overload.Unary(Floor, v => DoubleUnary(v, System.Math.Floor, Floor)),
                Overload.Unary(Round, v => DoubleUnary(v, RoundHalfAwayFromZero, Round)),
                Overload.Unary(Trunc, v => DoubleUnary(v, System.Math.Truncate, Trunc)),
                Overload.Unary(IsInf, v => DoublePredicate(v, double.IsInfinity, IsInf)),
                Overload.Unary(IsNaN, v => DoublePredicate(v, double.IsNaN, IsNaN)),
                Overload.Unary(IsFinite, v => DoublePredicate(v, IsFiniteValue, IsFinite)),
                Overload.Unary(Sqrt, SqrtOp),
                Overload.Binary(BitAnd, (l, r) => BitwisePair(l, r, (a, b) => a & b, (a, b) => a & b, BitAnd)),
                Overload.Binary(BitOr, (l, r) => BitwisePair(l, r, (a, b) => a | b, (a, b) => a | b, BitOr)),
                Overload.Binary(BitXor, (l, r) => BitwisePair(l, r, (a, b) => a ^ b, (a, b) => a ^ b, BitXor)),
                Overload.Unary(BitNot, BitNotOp),
                Overload.Binary(BitShiftLeft, (v, b) => ShiftOp(v, b, true)),
                Overload.Binary(BitShiftRight, (v, b) => ShiftOp(v, b, false)));
            list.Add(functions);
            return list;
        }
    }

    public static EnvOption Math()
    {
        return LibraryOptions.Lib(new MathLib());
    }

    private static void AddNumericPairOverloads(IList<Decl.Types.FunctionDecl.Types.Overload> into, string prefix)
    {
        var types = new (Type, string)[] { (Decls.Int, "int"), (Decls.Uint, "uint"), (Decls.Double, "double") };
        foreach (var (lt, ln) in types)
        foreach (var (rt, rn) in types)
        {
            // A same-type pair keeps that type; a mixed pair is dyn, matching cel-go.
            var result = ln == rn ? lt : Decls.Dyn;
            into.Add(Decls.NewOverload($"{prefix}_{ln}_{rn}", new List<Type> { lt, rt }, result));
        }
    }

    // ---- Macros ----------------------------------------------------------------------------

    private static Expr? MathLeast(IExprHelper eh, Expr? target, IList<Expr> args)
    {
        return MinMaxMacro(eh, target, args, MinFunc, "math.least()");
    }

    private static Expr? MathGreatest(IExprHelper eh, Expr? target, IList<Expr> args)
    {
        return MinMaxMacro(eh, target, args, MaxFunc, "math.greatest()");
    }

    private static Expr? MinMaxMacro(IExprHelper eh, Expr? target, IList<Expr> args, string func, string label)
    {
        if (target == null || Macro.ExtractIdent(target) != MathNamespace)
            // Not math.least/greatest - decline so the parser keeps it as an ordinary call.
            return null;

        switch (args.Count)
        {
            case 0:
                throw new ErrorWithLocation(null, label + " requires at least one argument");
            case 1:
                if (IsListLiteralWithNumericArgs(args[0]) || IsNumericArgType(args[0]))
                    return eh.GlobalCall(func, args);
                throw new ErrorWithLocation(null, label + " invalid single argument value");
            case 2:
                CheckInvalidArgs(label, args);
                return eh.GlobalCall(func, args);
            default:
                CheckInvalidArgs(label, args);
                return eh.GlobalCall(func, new List<Expr> { eh.NewList(args) });
        }
    }

    private static void CheckInvalidArgs(string label, IList<Expr> args)
    {
        foreach (var arg in args)
            if (!IsNumericArgType(arg))
                throw new ErrorWithLocation(null, label + " simple literal arguments must be numeric");
    }

    private static bool IsNumericArgType(Expr arg)
    {
        switch (arg.ExprKindCase)
        {
            case Expr.ExprKindOneofCase.ConstExpr:
                var c = arg.ConstExpr;
                return c.ConstantKindCase is Constant.ConstantKindOneofCase.Int64Value
                    or Constant.ConstantKindOneofCase.Uint64Value
                    or Constant.ConstantKindOneofCase.DoubleValue;
            case Expr.ExprKindOneofCase.ListExpr:
            case Expr.ExprKindOneofCase.StructExpr:
                return false;
            default:
                // A dynamic expression (ident, call, select, ...) is only known at runtime.
                return true;
        }
    }

    private static bool IsListLiteralWithNumericArgs(Expr arg)
    {
        if (arg.ExprKindCase != Expr.ExprKindOneofCase.ListExpr) return false;
        var elems = arg.ListExpr.Elements;
        if (elems.Count == 0) return false;
        foreach (var e in elems)
            if (!IsNumericArgType(e))
                return false;
        return true;
    }

    // ---- min / max -------------------------------------------------------------------------

    private static IVal MinUnary(IVal v)
    {
        if (v is ILister) return MinList(v);
        if (v is IntT or UintT or DoubleT) return v;
        return Err.NoSuchOverload(v, MinFunc, null);
    }

    private static IVal MaxUnary(IVal v)
    {
        if (v is ILister) return MaxList(v);
        if (v is IntT or UintT or DoubleT) return v;
        return Err.NoSuchOverload(v, MaxFunc, null);
    }

    private static IVal MinPair(IVal first, IVal second)
    {
        if (first is not IComparer cmp) return Err.NoSuchOverload(first, MinFunc, null);
        var cmpVal = cmp.Compare(second);
        if (Err.IsError(cmpVal)) return cmpVal;
        // Compare returns 1 when first > second, so the smaller of the two is second.
        if (cmpVal is IntT it && it.LongValue == 1) return second;
        return first;
    }

    private static IVal MaxPair(IVal first, IVal second)
    {
        if (first is not IComparer cmp) return Err.NoSuchOverload(first, MaxFunc, null);
        var cmpVal = cmp.Compare(second);
        if (Err.IsError(cmpVal)) return cmpVal;
        // Compare returns -1 when first < second, so the greater of the two is second.
        if (cmpVal is IntT it && it.LongValue == -1) return second;
        return first;
    }

    private static IVal MinList(IVal numList)
    {
        var list = (ILister)numList;
        var size = ((IntT)list.Size()).LongValue;
        if (size == 0) return Err.NewErr("math.@min(list) argument must not be empty");
        var min = list.Get(IntT.IntOf(0));
        for (long i = 1; i < size; i++) min = MinPair(min, list.Get(IntT.IntOf(i)));
        if (min is IntT or UintT or DoubleT) return min;
        if (Err.IsError(min)) return min;
        return Err.NewErr("no such overload: math.@min");
    }

    private static IVal MaxList(IVal numList)
    {
        var list = (ILister)numList;
        var size = ((IntT)list.Size()).LongValue;
        if (size == 0) return Err.NewErr("math.@max(list) argument must not be empty");
        var max = list.Get(IntT.IntOf(0));
        for (long i = 1; i < size; i++) max = MaxPair(max, list.Get(IntT.IntOf(i)));
        if (max is IntT or UintT or DoubleT) return max;
        if (Err.IsError(max)) return max;
        return Err.NewErr("no such overload: math.@max");
    }

    // ---- rounding / classification ---------------------------------------------------------

    private static IVal DoubleUnary(IVal v, Func<double, double> op, string name)
    {
        if (v is not DoubleT d) return Err.NoSuchOverload(v, name, null);
        return DoubleT.DoubleOf(op(d.DoubleValue));
    }

    private static IVal DoublePredicate(IVal v, Func<double, bool> op, string name)
    {
        if (v is not DoubleT d) return Err.NoSuchOverload(v, name, null);
        return op(d.DoubleValue) ? BoolT.True : BoolT.False;
    }

    private static double RoundHalfAwayFromZero(double v)
    {
        // Go's math.Round rounds halves away from zero; .NET's default rounds to even.
        return System.Math.Round(v, MidpointRounding.AwayFromZero);
    }

    private static bool IsFiniteValue(double v)
    {
        return !double.IsInfinity(v) && !double.IsNaN(v);
    }

    // ---- abs / sign / sqrt -----------------------------------------------------------------

    private static IVal AbsOp(IVal v)
    {
        switch (v)
        {
            case DoubleT d:
                return DoubleT.DoubleOf(System.Math.Abs(d.DoubleValue));
            case IntT i:
                if (i.LongValue == long.MinValue) return Err.NewErr("integer overflow");
                return i.LongValue >= 0 ? v : IntT.IntOf(-i.LongValue);
            case UintT:
                return v;
            default:
                return Err.NoSuchOverload(v, Abs, null);
        }
    }

    private static IVal SignOp(IVal v)
    {
        switch (v)
        {
            case DoubleT d:
                if (double.IsNaN(d.DoubleValue)) return v;
                if (d.DoubleValue > 0) return DoubleT.DoubleOf(1);
                if (d.DoubleValue < 0) return DoubleT.DoubleOf(-1);
                return DoubleT.DoubleOf(0);
            case IntT i:
                return IntT.IntOf(System.Math.Sign(i.LongValue));
            case UintT u:
                return UintT.UintOf(u.ULongValue == 0 ? 0UL : 1UL);
            default:
                return Err.NoSuchOverload(v, Sign, null);
        }
    }

    private static IVal SqrtOp(IVal v)
    {
        switch (v)
        {
            case DoubleT d:
                return DoubleT.DoubleOf(System.Math.Sqrt(d.DoubleValue));
            case IntT i:
                return DoubleT.DoubleOf(System.Math.Sqrt(i.LongValue));
            case UintT u:
                return DoubleT.DoubleOf(System.Math.Sqrt(u.ULongValue));
            default:
                return Err.NoSuchOverload(v, Sqrt, null);
        }
    }

    // ---- bitwise ---------------------------------------------------------------------------

    private static IVal BitwisePair(IVal lhs, IVal rhs, Func<long, long, long> onInt,
        Func<ulong, ulong, ulong> onUint, string name)
    {
        if (lhs is IntT li && rhs is IntT ri) return IntT.IntOf(onInt(li.LongValue, ri.LongValue));
        if (lhs is UintT lu && rhs is UintT ru) return UintT.UintOf(onUint(lu.ULongValue, ru.ULongValue));
        return Err.NoSuchOverload(lhs, name, rhs);
    }

    private static IVal BitNotOp(IVal v)
    {
        switch (v)
        {
            case IntT i:
                return IntT.IntOf(~i.LongValue);
            case UintT u:
                return UintT.UintOf(~u.ULongValue);
            default:
                return Err.NoSuchOverload(v, BitNot, null);
        }
    }

    private static IVal ShiftOp(IVal value, IVal bits, bool left)
    {
        if (bits is not IntT b) return Err.NoSuchOverload(value, left ? BitShiftLeft : BitShiftRight, bits);
        var shift = b.LongValue;
        if (shift < 0)
            return Err.NewErr((left ? "math.bitShiftLeft" : "math.bitShiftRight") + "() negative offset: {0}",
                shift);
        // A shift of 64 or more clears the value, matching Go's shift semantics rather than
        // C#'s, which masks the count to the low six bits.
        switch (value)
        {
            case IntT i:
                if (left)
                    return IntT.IntOf(shift >= 64 ? 0L : i.LongValue << (int)shift);
                // Right shift is logical (unsigned) then reinterpreted, as in cel-go.
                return IntT.IntOf(shift >= 64 ? 0L : (long)((ulong)i.LongValue >> (int)shift));
            case UintT u:
                if (left)
                    return UintT.UintOf(shift >= 64 ? 0UL : u.ULongValue << (int)shift);
                return UintT.UintOf(shift >= 64 ? 0UL : u.ULongValue >> (int)shift);
            default:
                return Err.NoSuchOverload(value, left ? BitShiftLeft : BitShiftRight, bits);
        }
    }
}
