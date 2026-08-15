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
using Cel.Extension;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel;

/// <summary>
///     The namespaced math extension - <see cref="MathLib" /> - ported from cel-go's
///     <a href="https://github.com/google/cel-go/blob/master/ext/math_test.go">ext/math_test.go</a>.
/// </summary>
public class MathLibTest
{
    private static Env NewEnv()
    {
        return Env.NewEnv(
            MathLib.Math(),
            EnvOptions.Declarations(
                Decls.NewVar("a", Decls.Int),
                Decls.NewVar("b", Decls.Int),
                Decls.NewVar("numbers", Decls.NewListType(Decls.Double))));
    }

    private static object EvalExpr(string expr, IDictionary<string, object>? vars = null)
    {
        var env = NewEnv();
        var astIss = env.Compile(expr);
        Assert.That(astIss.HasIssues(), Is.False, () => "compile failed for " + expr + ": " + astIss.Issues);
        var result = env.Program(astIss.Ast!).Eval(vars ?? new Dictionary<string, object>());
        return result.Val.Value();
    }

    // Every expression is written to evaluate to true, exactly as in cel-go's table.
    private static readonly string[] TrueExprs =
    {
        // math.least
        "math.least(-0.5) == -0.5",
        "math.least(-1) == -1",
        "math.least(1u) == 1u",
        "math.least(42.0, -0.5) == -0.5",
        "math.least(-1, 0) == -1",
        "math.least(-1, -1) == -1",
        "math.least(1u, 42u) == 1u",
        "math.least(42.0, -0.5, -0.25) == -0.5",
        "math.least(-1, 0, 1) == -1",
        "math.least(-1, -1, -1) == -1",
        "math.least(1u, 42u, 0u) == 0u",
        // two-arg overloads across type
        "math.least(1, 1.0) == 1",
        "math.least(1, -2.0) == -2.0",
        "math.least(2, 1u) == 1u",
        "math.least(1.5, 2) == 1.5",
        "math.least(1.5, -2) == -2",
        "math.least(2.5, 1u) == 1u",
        "math.least(1u, 2) == 1u",
        "math.least(1u, -2) == -2",
        "math.least(2u, 2.5) == 2u",
        // dynamic values across type
        "math.least(1u, dyn(42)) == 1",
        "math.least(1u, dyn(42), dyn(0.0)) == 0u",
        // list literal
        "math.least([1u, 42u, 0u]) == 0u",

        // math.greatest
        "math.greatest(-0.5) == -0.5",
        "math.greatest(-1) == -1",
        "math.greatest(1u) == 1u",
        "math.greatest(42.0, -0.5) == 42.0",
        "math.greatest(-1, 0) == 0",
        "math.greatest(-1, -1) == -1",
        "math.greatest(1u, 42u) == 42u",
        "math.greatest(42.0, -0.5, -0.25) == 42.0",
        "math.greatest(-1, 0, 1) == 1",
        "math.greatest(-1, -1, -1) == -1",
        "math.greatest(1u, 42u, 0u) == 42u",
        // two-arg overloads across type
        "math.greatest(1, 1.0) == 1",
        "math.greatest(1, -2.0) == 1",
        "math.greatest(2, 1u) == 2",
        "math.greatest(1.5, 2) == 2",
        "math.greatest(1.5, -2) == 1.5",
        "math.greatest(2.5, 1u) == 2.5",
        "math.greatest(1u, 2) == 2",
        "math.greatest(1u, -2) == 1u",
        "math.greatest(2u, 2.5) == 2.5",
        // dynamic values across type
        "math.greatest(1u, dyn(42)) == 42.0",
        "math.greatest(1u, dyn(0.0), 0u) == 1",
        // list literal
        "math.greatest([1u, dyn(0.0), 0u]) == 1",

        // bitwise, signed
        "math.bitAnd(1, 2) == 0",
        "math.bitAnd(1, -1) == 1",
        "math.bitAnd(1, 3) == 1",
        "math.bitOr(1, 2) == 3",
        "math.bitXor(1, 3) == 2",
        "math.bitXor(3, 5) == 6",
        "math.bitNot(1) == -2",
        "math.bitNot(0) == -1",
        "math.bitNot(-1) == 0",
        "math.bitShiftLeft(1, 2) == 4",
        "math.bitShiftLeft(1, 200) == 0",
        "math.bitShiftLeft(-1, 200) == 0",
        "math.bitShiftRight(1024, 2) == 256",
        "math.bitShiftRight(1024, 64) == 0",
        "math.bitShiftRight(-1024, 3) == 2305843009213693824",
        "math.bitShiftRight(-1024, 64) == 0",
        // bitwise, unsigned
        "math.bitAnd(1u, 2u) == 0u",
        "math.bitAnd(1u, 3u) == 1u",
        "math.bitOr(1u, 2u) == 3u",
        "math.bitXor(1u, 3u) == 2u",
        "math.bitXor(3u, 5u) == 6u",
        "math.bitNot(1u) == 18446744073709551614u",
        "math.bitNot(0u) == 18446744073709551615u",
        "math.bitShiftLeft(1u, 2) == 4u",
        "math.bitShiftLeft(1u, 200) == 0u",
        "math.bitShiftRight(1024u, 2) == 256u",
        "math.bitShiftRight(1024u, 64) == 0u",

        // floating-point helpers
        "math.isNaN(0.0/0.0)",
        "!math.isNaN(1.0/0.0)",
        "math.isFinite(1.0/1.5)",
        "!math.isFinite(1.0/0.0)",
        "math.isInf(1.0/0.0)",

        // rounding
        "math.ceil(1.2) == 2.0",
        "math.ceil(-1.2) == -1.0",
        "math.floor(1.2) == 1.0",
        "math.floor(-1.2) == -2.0",
        "math.round(1.2) == 1.0",
        "math.round(1.5) == 2.0",
        "math.round(-1.5) == -2.0",
        "math.isNaN(math.round(0.0/0.0))",
        "math.round(-1.2) == -1.0",
        "math.trunc(-1.3) == -1.0",
        "math.trunc(1.3) == 1.0",

        // signedness
        "math.sign(-42) == -1",
        "math.sign(0) == 0",
        "math.sign(42) == 1",
        "math.sign(0u) == 0u",
        "math.sign(42u) == 1u",
        "math.sign(-0.3) == -1.0",
        "math.sign(0.0) == 0.0",
        "math.isNaN(math.sign(0.0/0.0))",
        "math.sign(1.0/0.0) == 1.0",
        "math.sign(-1.0/0.0) == -1.0",
        "math.sign(0.3) == 1.0",
        "math.abs(-1) == 1",
        "math.abs(1) == 1",
        "math.abs(-234.5) == 234.5",
        "math.abs(234.5) == 234.5",

        // square root
        "math.sqrt(49.0) == 7.0",
        "math.sqrt(0) == 0.0",
        "math.sqrt(1) == 1.0",
        "math.sqrt(25u) == 5.0",
        "math.sqrt(82) == 9.055385138137417",
        "math.sqrt(985.25) == 31.388692231439016",
        "math.isNaN(math.sqrt(-15.34))"
    };

    [TestCaseSource(nameof(TrueExprs))]
    public virtual void EvaluatesToTrue(string expr)
    {
        Assert.That(EvalExpr(expr), Is.EqualTo(true), expr);
    }

    [Test]
    public virtual void LeastAndGreatestOverExpressionArguments()
    {
        var ab = TestUtil.BindingsOf("a", IntT.IntOf(1), "b", IntT.IntOf(2));
        Assert.That(EvalExpr("math.least(a, b) == a", ab), Is.EqualTo(true));
        Assert.That(EvalExpr("math.greatest(a, b) == b", ab), Is.EqualTo(true));
    }

    [Test]
    public virtual void LeastAndGreatestOverAListVariable()
    {
        var numbers = new List<double> { -21.0, -10.5, 1.0 };
        Assert.That(EvalExpr("math.least(numbers) == dyn(a)",
            TestUtil.BindingsOf("a", IntT.IntOf(-21), "numbers", numbers)), Is.EqualTo(true));
        Assert.That(EvalExpr("math.greatest(numbers) == dyn(a)",
            TestUtil.BindingsOf("a", IntT.IntOf(1), "numbers", numbers)), Is.EqualTo(true));
    }

    // Expressions the compiler must reject, with the substring the error contains. Ported from
    // cel-go's TestMathStaticErrors.
    private static readonly object[] ErrorCases =
    {
        new object[] { "math.least()", "math.least() requires at least one argument" },
        new object[] { "math.least('hello')", "math.least() invalid single argument value" },
        new object[] { "math.least({})", "math.least() invalid single argument value" },
        new object[] { "math.least(1, true)", "math.least() simple literal arguments must be numeric" },
        new object[] { "math.least(1, 2, true)", "math.least() simple literal arguments must be numeric" },
        new object[] { "math.greatest()", "math.greatest() requires at least one argument" },
        new object[] { "math.greatest(true)", "math.greatest() invalid single argument value" },
        new object[] { "math.greatest([])", "math.greatest() invalid single argument value" },
        new object[] { "math.greatest([1, true])", "math.greatest() invalid single argument value" },
        new object[] { "math.greatest(1, true)", "math.greatest() simple literal arguments must be numeric" },
        new object[] { "math.greatest(1, 2, true)", "math.greatest() simple literal arguments must be numeric" }
    };

    [TestCaseSource(nameof(ErrorCases))]
    public virtual void RejectsAtCompileTime(string expr, string expected)
    {
        var astIss = NewEnv().Compile(expr);
        Assert.That(astIss.HasIssues(), Is.True, expr);
        Assert.That(astIss.Issues!.ToString(), Does.Contain(expected), expr);
    }

    /// <summary>
    ///     greatest/least are macros on the math namespace alone; on any other receiver the parser
    ///     leaves an ordinary call, which does not resolve rather than being expanded.
    /// </summary>
    [Test]
    public virtual void DoesNotHijackOtherReceivers()
    {
        Assert.That(NewEnv().Compile("[1, 2].least()").HasIssues(), Is.True);
        Assert.That(NewEnv().Compile("'abc'.greatest(1)").HasIssues(), Is.True);
    }

    /// <summary>
    ///     abs(minInt) has no representable positive counterpart, so it errors rather than wrapping.
    /// </summary>
    [Test]
    public virtual void AbsOfTheMostNegativeIntOverflows()
    {
        var astIss = NewEnv().Compile("math.abs(-9223372036854775807 - 1)");
        Assert.That(astIss.HasIssues(), Is.False);
        var result = NewEnv().Program(astIss.Ast!).Eval(new Dictionary<string, object>());
        Assert.That(result.Val, Is.InstanceOf<Err>());
    }

    /// <summary>
    ///     A negative shift offset is an evaluation error, not a wrap.
    /// </summary>
    [Test]
    public virtual void NegativeShiftOffsetIsAnError()
    {
        foreach (var expr in new[] { "math.bitShiftLeft(1, -1)", "math.bitShiftRight(8, -1)" })
        {
            var astIss = NewEnv().Compile(expr);
            Assert.That(astIss.HasIssues(), Is.False, expr);
            var result = NewEnv().Program(astIss.Ast!).Eval(new Dictionary<string, object>());
            Assert.That(result.Val, Is.InstanceOf<Err>(), expr);
        }
    }
}
