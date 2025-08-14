using Cel.Common;
using Cel.Common.Containers;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Parser;
using NUnit.Framework;

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
namespace Cel.Interpreter;

internal class PruneTest
{
    internal static TestCase[] PruneTestCases()
    {
        return new[]
        {
            new(TestUtil.BindingsOf("msg", TestUtil.BindingsOf("foo", "bar")), "msg",
                "{\"foo\": \"bar\"}"),
            new TestCase(null, "true && false", "false"),
            new TestCase(UnknownActivation("x"), "(true || false) && x", "x"),
            new TestCase(UnknownActivation("x"), "(false || false) && x", "false"),
            new TestCase(UnknownActivation("a"), "a && [1, 1u, 1.0].exists(x, type(x) == uint)", "a"),
            new TestCase(null, "{'hello': 'world'.size()}", "{\"hello\": 5}"),
            new TestCase(null, "[b'bytes-string']", "[b\"bytes-string\"]"),
            new TestCase(null, "[b\"\\142\\171\\164\\145\\163\\055\\163\\164\\162\\151\\156\\147\"]",
                "[b\"bytes-string\"]"),
            new TestCase(null, "[b'bytes'] + [b'-' + b'string']", "[b\"bytes\", b\"-string\"]"),
            new TestCase(null, "1u + 3u", "4u"),
            new TestCase(null, "2 < 3", "true"),
            new TestCase(UnknownActivation(), "test == null", "test == null"),
            new TestCase(UnknownActivation(), "test == null && false", "false"),
            new TestCase(UnknownActivation("b", "c"), "true ? b < 1.2 : c == ['hello']", "b < 1.2"),
            new TestCase(UnknownActivation(), "[1+3, 2+2, 3+1, four]", "[4, 4, 4, four]"),
            new TestCase(UnknownActivation(), "test == {'a': 1, 'field': 2}.field", "test == 2"),
            new TestCase(UnknownActivation(), "test in {'a': 1, 'field': [2, 3]}.field", "test in [2, 3]"),
            new TestCase(UnknownActivation(), "test == {'field': [1 + 2, 2 + 3]}", "test == {\"field\": [3, 5]}"),
            new TestCase(UnknownActivation(), "test in {'a': 1, 'field': [test, 3]}.field",
                "test in {\"a\": 1, \"field\": [test, 3]}.field")
        };
    }

    [TestCaseSource(nameof(PruneTestCases))]
    public virtual void Prune(TestCase tc)
    {
        var parseResult = Parser.Parser.ParseAllMacros(SourceFactory.NewStringSource(tc.expr, "<input>"));
        if (parseResult.HasErrors()) Assert.Fail(parseResult.Errors.ToDisplayString());

        var state = EvalStateFactory.NewEvalState();
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = AttributePattern.NewPartialAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var interp = InterpreterUtils.NewStandardInterpreter(Container.DefaultContainer, reg, reg.ToTypeAdapter(), attrs);

        var interpretable = interp.NewUncheckedInterpretable(parseResult.Expr!, InterpreterUtils.ExhaustiveEval(state))!;
        interpretable.Eval(TestActivation(tc.@in));
        var newExpr = AstPruner.PruneAst(parseResult.Expr!, state);
        var actual = Unparser.Unparse(newExpr!, null);
        Assert.That(actual, Is.EqualTo(tc.expect));
    }

    internal static IPartialActivation UnknownActivation(params string[] vars)
    {
        var pats = new AttributePattern[vars.Length];
        for (var i = 0; i < vars.Length; i++)
        {
            var v = vars[i];
            pats[i] = AttributePattern.NewAttributePattern(v);
        }

        return ActivationFactory.NewPartialActivation(new Dictionary<string, object>(), pats);
    }

    internal virtual IActivation TestActivation(object @in)
    {
        if (@in == null) return ActivationFactory.EmptyActivation();

        return ActivationFactory.NewActivation(@in);
    }

    internal class TestCase
    {
        internal readonly string expect;
        internal readonly string expr;
        internal readonly object? @in;

        internal TestCase(object? @in, string expr, string expect)
        {
            this.@in = @in;
            this.expr = expr;
            this.expect = expect;
        }

        public override string ToString()
        {
            return expr;
        }
    }
}