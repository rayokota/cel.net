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

using Cel.Checker;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Google.Api.Expr.V1Alpha1;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel;

/// <summary>
///     Comparisons whose operands are numbers of different types, gated behind
///     <see cref="EnvFeature.FeatureCrossTypeNumericComparisons" />.
/// </summary>
public class CrossTypeNumericComparisonTest
{
    private static Env EnvWith(bool crossType, params Decl[] declarations)
    {
        var opts = new List<EnvOption> { EnvOptions.Declarations(declarations) };
        if (crossType) opts.Add(EnvOptions.Features(EnvFeature.FeatureCrossTypeNumericComparisons));
        return Env.NewEnv(opts.ToArray());
    }

    /// <summary>
    ///     Evaluates expr, or returns null if it does not type-check.
    /// </summary>
    private static object? Eval(bool crossType, string expr, IDictionary<string, object>? vars = null,
        params Decl[] declarations)
    {
        var env = EnvWith(crossType, declarations);
        var astIss = env.Compile(expr);
        if (astIss.HasIssues()) return null;
        var prgErr = env.Program(astIss.Ast!);
        var result = prgErr.Eval(vars ?? new Dictionary<string, object>());
        return result.Val;
    }

    /// <summary>
    ///     The check errors for expr, or null if it checks cleanly. Lets a test assert on why
    ///     an expression was rejected rather than only that it was.
    /// </summary>
    private static string? CheckErrors(bool crossType, string expr, params Decl[] declarations)
    {
        var astIss = EnvWith(crossType, declarations).Compile(expr);
        return astIss.HasIssues() ? astIss.Issues!.ToString() : null;
    }

    private static Decl VarUint => Decls.NewVar("u", Decls.Uint);
    private static Decl VarInt => Decls.NewVar("i", Decls.Int);
    private static Decl VarDouble => Decls.NewVar("d", Decls.Double);

    [Test]
    public virtual void OffByDefault()
    {
        // Every pairing of differing numeric types is a check failure without the feature,
        // exactly as before it existed.
        foreach (var expr in new[]
                 {
                     "u > 0", "u < 0", "u >= 0", "u <= 0",
                     "i > 0u", "i < 1.0", "d > 0", "d < 1u"
                 })
        {
            var errors = CheckErrors(false, expr, VarUint, VarInt, VarDouble);
            Assert.That(errors, Is.Not.Null, "expected a check failure for: " + expr);
            Assert.That(errors, Does.Contain("no matching overload"),
                "expected an overload failure, not something else, for: " + expr);
        }
    }

    [Test]
    public virtual void OnByFeature()
    {
        foreach (var expr in new[]
                 {
                     "u > 0", "u < 0", "u >= 0", "u <= 0",
                     "i > 0u", "i < 1.0", "d > 0", "d < 1u"
                 })
        {
            Assert.That(CheckErrors(true, expr, VarUint, VarInt, VarDouble), Is.Null,
                "expected a clean check for: " + expr);
            Assert.That(Eval(true, expr, TestUtil.BindingsOf("u", UintT.UintOf(1), "i", IntT.IntOf(1),
                    "d", DoubleT.DoubleOf(1.0)), VarUint, VarInt, VarDouble),
                Is.InstanceOf<BoolT>(), "expected a boolean for: " + expr);
        }
    }

    [Test]
    public virtual void SameTypeComparisonsStillWorkWithoutTheFeature()
    {
        Assert.That(Eval(false, "u > 0u", TestUtil.BindingsOf("u", UintT.UintOf(1)), VarUint),
            Is.SameAs(BoolT.True));
        Assert.That(Eval(false, "1 < 2"), Is.SameAs(BoolT.True));
        Assert.That(Eval(false, "1.5 >= 2.5"), Is.SameAs(BoolT.False));
    }

    [Test]
    public virtual void EqualityStaysHomogeneous()
    {
        // The feature widens ordering only. Equality is declared over a single type
        // parameter, so it rejects mixed operands whether the feature is on or off -
        // matching cel-go and cel-java.
        Assert.That(Eval(true, "u == 1", null, VarUint), Is.Null);
        Assert.That(Eval(true, "u != 1", null, VarUint), Is.Null);
        Assert.That(Eval(true, "1 == 1u"), Is.Null);
    }

    /// <summary>
    ///     A uint above long.MaxValue is above every int, and a negative int is below every
    ///     uint. Casting either operand to the other's type would wrap and invert both.
    /// </summary>
    [Test]
    public virtual void UintAboveLongMaxComparesAboveEveryInt()
    {
        var vars = TestUtil.BindingsOf("u", UintT.UintOf(ulong.MaxValue - 4), "i", IntT.IntOf(-5));
        Assert.That(Eval(true, "u > i", vars, VarUint, VarInt), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "i < u", vars, VarUint, VarInt), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "u < i", vars, VarUint, VarInt), Is.SameAs(BoolT.False));

        // long.MaxValue as a uint, against long.MaxValue as an int: equal, not wrapped.
        var atBoundary = TestUtil.BindingsOf("u", UintT.UintOf(long.MaxValue), "i", IntT.IntOf(long.MaxValue));
        Assert.That(Eval(true, "u <= i", atBoundary, VarUint, VarInt), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "u >= i", atBoundary, VarUint, VarInt), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "u > i", atBoundary, VarUint, VarInt), Is.SameAs(BoolT.False));

        // One past it, on the uint side only.
        var pastBoundary = TestUtil.BindingsOf("u", UintT.UintOf((ulong)long.MaxValue + 1),
            "i", IntT.IntOf(long.MaxValue));
        Assert.That(Eval(true, "u > i", pastBoundary, VarUint, VarInt), Is.SameAs(BoolT.True));
    }

    /// <summary>
    ///     A double outside the integer range has no representable counterpart, so the
    ///     comparison is settled by range rather than by conversion.
    /// </summary>
    [Test]
    public virtual void DoublesOutsideTheIntegerRangesCompareByRange()
    {
        Assert.That(Eval(true, "d < i", TestUtil.BindingsOf("d", DoubleT.DoubleOf(-1e300),
            "i", IntT.IntOf(long.MinValue)), VarDouble, VarInt), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "d > i", TestUtil.BindingsOf("d", DoubleT.DoubleOf(1e300),
            "i", IntT.IntOf(long.MaxValue)), VarDouble, VarInt), Is.SameAs(BoolT.True));
        // A negative double is below every uint, including zero.
        Assert.That(Eval(true, "d < u", TestUtil.BindingsOf("d", DoubleT.DoubleOf(-0.5),
            "u", UintT.UintOf(0)), VarDouble, VarUint), Is.SameAs(BoolT.True));
        Assert.That(Eval(true, "d > u", TestUtil.BindingsOf("d", DoubleT.DoubleOf(1e300),
            "u", UintT.UintOf(ulong.MaxValue)), VarDouble, VarUint), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void NaNIsNotOrdered()
    {
        foreach (var expr in new[] { "d < i", "d > i", "i < d", "d < u", "u > d" })
        {
            var result = Eval(true, expr, TestUtil.BindingsOf("d", DoubleT.DoubleOf(double.NaN),
                "i", IntT.IntOf(1), "u", UintT.UintOf(1)), VarDouble, VarInt, VarUint);
            Assert.That(result, Is.InstanceOf<Err>(), "expected an error for: " + expr);
        }
    }

    [Test]
    public virtual void NonNumericOperandsStillHaveNoOverload()
    {
        Assert.That(IntT.IntOf(1).Compare(StringT.StringOf("1")), Is.InstanceOf<Err>());
        Assert.That(UintT.UintOf(1).Compare(BoolT.True), Is.InstanceOf<Err>());
        Assert.That(DoubleT.DoubleOf(1.0).Compare(StringT.StringOf("1")), Is.InstanceOf<Err>());
    }
}
