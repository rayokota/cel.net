using Cel.Checker;
using Cel.Common;
using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Google.Api.Expr.Test.V1.Proto3;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
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
namespace Cel.Interpreter;

internal class AttributesTest
{
    [Test]
    public virtual void AttributesAbsoluteAttr()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var cont = Container.NewContainer(Container.Name("acme.ns"));
        var attrs = IAttributeFactory.NewAttributeFactory(cont!, reg.ToTypeAdapter(), reg);
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("acme.a",
            TestUtil.MapOf("b", TestUtil.MapOf(4L, TestUtil.MapOf(false, "success")))));

        // acme.a.b[4][false]
        var attr = attrs.AbsoluteAttribute(1, "acme.a");
        var qualB = attrs.NewQualifier(null, 2, "b");
        var qual4 = attrs.NewQualifier(null, 3, 4L);
        var qualFalse = attrs.NewQualifier(null, 4, false);
        attr.AddQualifier(qualB);
        attr.AddQualifier(qual4);
        attr.AddQualifier(qualFalse);
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo("success"));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesAbsoluteAttrType()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);

        // int
        var attr = attrs.AbsoluteAttribute(1, "int");
        var @out = attr.Resolve(IActivation.EmptyActivation());
        Assert.That(@out, Is.SameAs(IntT.IntType));
        Assert.That(@out, Is.SameAs(IntT.IntType));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesRelativeAttr()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a", TestUtil.MapOf(-1, new[] { 2, 42 }), "b", 1);
        var vars = IActivation.NewActivation(data);

        // The relative attribute under test is applied to a map literal:
        // {
        //   a: {-1: [2, 42], b: 1}
        //   b: 1
        // }
        //
        // The expression being evaluated is: <map-literal>.a[-1][b] -> 42
        var op = IInterpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
        var attr = attrs.RelativeAttribute(1, op);
        var qualA = attrs.NewQualifier(null, 2, "a");
        var qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
        attr.AddQualifier(qualA);
        attr.AddQualifier(qualNeg1);
        attr.AddQualifier(attrs.AbsoluteAttribute(4, "b"));
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesRelativeAttrOneOf()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var cont = Container.NewContainer(Container.Name("acme.ns"));
        var attrs = IAttributeFactory.NewAttributeFactory(cont!, reg.ToTypeAdapter(), reg);
        var
            data = TestUtil.BindingsOf("a", TestUtil.MapOf(-1, new[] { 2, 42 }), "acme.b", 1);
        var vars = IActivation.NewActivation(data);

        // The relative attribute under test is applied to a map literal:
        // {
        //   a: {-1: [2, 42], b: 1}
        //   b: 1
        // }
        //
        // The expression being evaluated is: <map-literal>.a[-1][b] -> 42
        //
        // However, since the test is validating what happens with maybe attributes
        // the attribute resolution must also consider the following variations:
        // - <map-literal>.a[-1][acme.ns.b]
        // - <map-literal>.a[-1][acme.b]
        //
        // The correct behavior should yield the value of the last alternative.
        var op = IInterpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
        var attr = attrs.RelativeAttribute(1, op);
        var qualA = attrs.NewQualifier(null, 2, "a");
        var qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
        attr.AddQualifier(qualA);
        attr.AddQualifier(qualNeg1);
        attr.AddQualifier(attrs.MaybeAttribute(4, "b"));
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesRelativeAttrConditional()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a", TestUtil.MapOf(-1, new[] { 2, 42 }), "b",
            new[] { 0, 1 }, "c", new object[] { 1, 0 });
        var vars = IActivation.NewActivation(data);

        // The relative attribute under test is applied to a map literal:
        // {
        //   a: {-1: [2, 42], b: 1}
        //   b: [0, 1],
        //   c: {1, 0},
        // }
        //
        // The expression being evaluated is:
        // <map-literal>.a[-1][(false ? b : c)[0]] -> 42
        //
        // Effectively the same as saying <map-literal>.a[-1][c[0]]
        var cond = IInterpretable.NewConstValue(2, BoolT.False);
        var condAttr = attrs.ConditionalAttribute(4, cond, attrs.AbsoluteAttribute(5, "b"),
            attrs.AbsoluteAttribute(6, "c"));
        var qual0 = attrs.NewQualifier(null, 7, 0);
        condAttr.AddQualifier(qual0);

        var obj = IInterpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
        var attr = attrs.RelativeAttribute(1, obj);
        var qualA = attrs.NewQualifier(null, 2, "a");
        var qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
        attr.AddQualifier(qualA);
        attr.AddQualifier(qualNeg1);
        attr.AddQualifier(condAttr);
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesRelativeAttrRelative()
    {
        var cont = Container.NewContainer(Container.Name("acme.ns"));
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(cont!, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a",
            TestUtil.MapOf(-1, TestUtil.MapOf("first", 1, "second", 2, "third", 3)), "b", 2L);
        var vars = IActivation.NewActivation(data);

        // The environment declares the following variables:
        // {
        //   a: {
        //     -1: {
        //       "first": 1u,
        //       "second": 2u,
        //       "third": 3u,
        //     }
        //   },
        //   b: 2u,
        // }
        //
        // The map of input variables is also re-used as a map-literal <obj> in the expression.
        //
        // The relative object under test is the following map literal.
        // <mp> {
        //   1u: "first",
        //   2u: "second",
        //   3u: "third",
        // }
        //
        // The expression under test is:
        //   <obj>.a[-1][<mp>[b]]
        //
        // This is equivalent to:
        //   <obj>.a[-1]["second"] -> 2u
        var obj = IInterpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
        var mp = IInterpretable.NewConstValue(1,
            reg.ToTypeAdapter()(TestUtil.MapOf(1, "first", 2, "second", 3, "third")));
        var relAttr = attrs.RelativeAttribute(4, mp);
        var qualB = attrs.NewQualifier(null, 5, attrs.AbsoluteAttribute(5, "b"));
        relAttr.AddQualifier(qualB);
        var attr = attrs.RelativeAttribute(1, obj);
        var qualA = attrs.NewQualifier(null, 2, "a");
        var qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
        attr.AddQualifier(qualA);
        attr.AddQualifier(qualNeg1);
        attr.AddQualifier(relAttr);

        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo(IntT.IntOf(2)));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesOneofAttr()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var cont = Container.NewContainer(Container.Name("acme.ns"));
        var attrs = IAttributeFactory.NewAttributeFactory(cont!, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a", TestUtil.MapOf("b", new[] { 2, 42 }), "acme.a.b",
            1, "acme.ns.a.b", "found");
        var vars = IActivation.NewActivation(data);

        // a.b -> should resolve to acme.ns.a.b per namespace resolution rules.
        var attr = attrs.MaybeAttribute(1, "a");
        var qualB = attrs.NewQualifier(null, 2, "b");
        attr.AddQualifier(qualB);
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo("found"));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesConditionalAttrTrueBranch()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a", TestUtil.MapOf(-1, new[] { 2, 42 }), "b",
            TestUtil.MapOf("c", TestUtil.MapOf(-1, new[] { 2, 42 })));
        var vars = IActivation.NewActivation(data);

        // (true ? a : b.c)[-1][1]
        var tv = attrs.AbsoluteAttribute(2, "a");
        var fv = attrs.MaybeAttribute(3, "b");
        var qualC = attrs.NewQualifier(null, 4, "c");
        fv.AddQualifier(qualC);
        var cond = attrs.ConditionalAttribute(1, IInterpretable.NewConstValue(0, BoolT.True), tv, fv);
        var qualNeg1 = attrs.NewQualifier(null, 5, IntT.IntOf(-1));
        var qual1 = attrs.NewQualifier(null, 6, IntT.IntOf(1));
        cond.AddQualifier(qualNeg1);
        cond.AddQualifier(qual1);
        var @out = cond.Resolve(vars);
        Assert.That(@out, Is.EqualTo(42));
        Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
        // Note: migrated to JMH
    }

    [Test]
    public virtual void AttributesConditionalAttrFalseBranch()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var data = TestUtil.BindingsOf("a", TestUtil.MapOf(-1, new[] { 2, 42 }), "b",
            TestUtil.MapOf("c", TestUtil.MapOf(-1, new[] { 2, 42 })));
        var vars = IActivation.NewActivation(data);

        // (false ? a : b.c)[-1][1]
        var tv = attrs.AbsoluteAttribute(2, "a");
        var fv = attrs.MaybeAttribute(3, "b");
        var qualC = attrs.NewQualifier(null, 4, "c");
        fv.AddQualifier(qualC);
        var cond = attrs.ConditionalAttribute(1, IInterpretable.NewConstValue(0, BoolT.False), tv, fv);
        var qualNeg1 = attrs.NewQualifier(null, 5, IntT.IntOf(-1));
        var qual1 = attrs.NewQualifier(null, 6, IntT.IntOf(1));
        cond.AddQualifier(qualNeg1);
        cond.AddQualifier(qual1);
        var @out = cond.Resolve(vars);
        Assert.That(@out, Is.EqualTo(42));
        Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
        // Note: migrated to JMH
    }

    [Test]
    public virtual void AttributesConditionalAttrErrorUnknown()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);

        // err ? a : b
        var tv = attrs.AbsoluteAttribute(2, "a");
        var fv = attrs.MaybeAttribute(3, "b");
        var cond =
            attrs.ConditionalAttribute(1, IInterpretable.NewConstValue(0, Err.NewErr("test error")), tv, fv);
        var @out = cond.Resolve(IActivation.EmptyActivation());
        Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));

        // unk ? a : b
        var condUnk =
            attrs.ConditionalAttribute(1, IInterpretable.NewConstValue(0, UnknownT.UnknownOf(1)), tv, fv);
        @out = condUnk.Resolve(IActivation.EmptyActivation());
        Assert.That(@out, Is.InstanceOf(typeof(UnknownT)));
        Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void BenchmarkResolverFieldQualifier()
    {
        var nestedMsg = new TestAllTypes.Types.NestedMessage();
        nestedMsg.Bb = 123;
        var msg = new TestAllTypes();
        msg.SingleNestedMessage = nestedMsg;
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(msg);
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("msg", msg));
        var attr = attrs.AbsoluteAttribute(1, "msg");
        var opType = reg.FindType("google.api.expr.test.v1.proto3.TestAllTypes");
        Assert.That(opType, Is.Not.Null);
        var fieldType = reg.FindType("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage");
        Assert.That(fieldType, Is.Not.Null);
        attr.AddQualifier(MakeQualifier(attrs, opType.Type_, 2, "single_nested_message"));
        attr.AddQualifier(MakeQualifier(attrs, fieldType.Type_, 3, "bb"));
        // Note: migrated to JMH
    }

    [Test]
    public virtual void ResolverCustomQualifier()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        IAttributeFactory attrs =
            new CustAttrFactory(
                IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg));
        var msg = new TestAllTypes.Types.NestedMessage();
        msg.Bb = 123;
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("msg", msg));
        var attr = attrs.AbsoluteAttribute(1, "msg");
        var type = new Type();
        type.MessageType = "google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage";
        var qualBB =
            attrs.NewQualifier(type, 2, "bb");
        attr.AddQualifier(qualBB);
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.EqualTo(123));
        Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
        Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
    }

    [Test]
    public virtual void AttributesMissingMsg()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var any = Any.Pack(new TestAllTypes());
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("missing_msg", any));

        // missing_msg.field
        var attr = attrs.AbsoluteAttribute(1, "missing_msg");
        var field = attrs.NewQualifier(null, 2, "field");
        attr.AddQualifier(field);
        Assert.That(() => attr.Resolve(vars), Throws.Exception.TypeOf(typeof(InvalidOperationException)));
    }

    [Test]
    public virtual void AttributeMissingMsgUnknownField()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var attrs = AttributePattern.NewPartialAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var any = Any.Pack(new TestAllTypes());
        IActivation vars = IActivation.NewPartialActivation(TestUtil.BindingsOf("missing_msg", any),
            AttributePattern.NewAttributePattern("missing_msg").QualString("field"));

        // missing_msg.field
        var attr = attrs.AbsoluteAttribute(1, "missing_msg");
        var field = attrs.NewQualifier(null, 2, "field");
        attr.AddQualifier(field);
        var @out = attr.Resolve(vars);
        Assert.That(@out, Is.TypeOf(typeof(UnknownT)));
    }

    internal static TestDef[] AttributeStateTrackingTests()
    {
        return new[]
        {
            new TestDef("[{\"field\": true}][0].field").Env().In(TestUtil.BindingsOf()).Out(BoolT.True).State(
                TestUtil.MapOf(1L, true, 6L, TestUtil.MapOf(StringT.StringOf("field"), BoolT.True), 8L, true)),
            new TestDef("a[1]['two']")
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.String, Decls.Bool))))
                .In(TestUtil.BindingsOf("a", TestUtil.MapOf(1L, TestUtil.MapOf("two", true)))).Out(BoolT.True)
                .State(TestUtil.MapOf(1L, true, 2L, TestUtil.MapOf("two", true), 4L, true)),
            new TestDef("a[1][2][3]")
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.Dyn, Decls.Dyn))))
                .In(TestUtil.BindingsOf("a",
                    TestUtil.MapOf(1, TestUtil.MapOf(1L, 0L, 2L, new[] { "index", "middex", "outdex", "dex" }))))
                .Out(StringT.StringOf("dex")).State(TestUtil.MapOf(1L, "dex", 2L,
                    TestUtil.MapOf(1L, 0L, 2L, new[] { "index", "middex", "outdex", "dex" }), 4L,
                    new[] { "index", "middex", "outdex", "dex" }, 6L, "dex")),
            new TestDef("a[1][2][a[1][1]]")
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.Dyn, Decls.Dyn))))
                .In(TestUtil.BindingsOf("a",
                    TestUtil.MapOf(1L,
                        TestUtil.MapOf(1L, 0L, 2L, new[] { "index", "middex", "outdex", "dex" }))))
                .Out(StringT.StringOf("index")).State(TestUtil.MapOf(1L, "index", 2L,
                    TestUtil.MapOf(1L, 0L, 2L, new[] { "index", "middex", "outdex", "dex" }), 4L,
                    new[] { "index", "middex", "outdex", "dex" }, 6L, "index", 8L,
                    TestUtil.MapOf(1L, 0L, 2L, new[] { "index", "middex", "outdex", "dex" }), 10L,
                    IntT.IntOf(0)))
        };
    }

    [TestCaseSource(nameof(AttributeStateTrackingTests))]
    public virtual void AttributeStateTracking(TestDef tc)
    {
        var src = ISource.NewTextSource(tc.expr);
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);
        var cont = Container.DefaultContainer;
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
        if (tc.env != null) env.Add(tc.env);

        var checkResult = Checker.Checker.Check(parsed, src, env);
        if (parsed.HasErrors()) throw new ArgumentException(parsed.Errors.ToDisplayString());

        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var interp = IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        // Show that program planning will now produce an error.
        var st = IEvalState.NewEvalState();
        var i = interp.NewInterpretable(checkResult.CheckedExpr, IInterpreter.Optimize(),
            IInterpreter.TrackState(st))!;
        var @in = IActivation.NewActivation(tc.@in);
        var @out = i.Eval(@in);
        Assert.That(@out, Is.EqualTo(tc.@out));
        foreach (var iv in tc.state)
        {
            var id = Convert.ToInt64(iv.Key);
            var val = iv.Value;
            var stVal = st.Value(id);
            Assert.That(stVal, Is.Not.Null);
            Assert.That(stVal, Is.EqualTo(DefaultTypeAdapter.Instance.NativeToValue(val)));
            TestUtil.DeepEquals(string.Format("id({0:D})", id), stVal.Value(), val);
        }
    }

    [Test]
    public virtual void BenchmarkResolverCustomQualifier()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        IAttributeFactory attrs =
            new CustAttrFactory(
                IAttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg));
        var msg = new TestAllTypes.Types.NestedMessage();
        msg.Bb = 123;
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("msg", msg));
        var attr = attrs.AbsoluteAttribute(1, "msg");
        var type = new Type();
        type.MessageType = "google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage";
        var qualBB = attrs.NewQualifier(type, 2, "bb");
        attr.AddQualifier(qualBB);
        // Note: Migrated to JMH
    }

    internal static IQualifier MakeQualifier(IAttributeFactory attrs, Type typ, long qualID, object val)
    {
        var qual = attrs.NewQualifier(typ, qualID, val);
        return qual;
    }

    internal class TestDef
    {
        internal readonly string expr;
        internal IList<Decl>? env;
        internal IDictionary<string, object>? @in;
        internal IVal? @out;
        internal IDictionary<object, object>? state;

        internal TestDef(string expr)
        {
            this.expr = expr;
        }

        internal virtual TestDef Env(params Decl[] env)
        {
            this.env = new List<Decl>(env);
            return this;
        }

        internal virtual TestDef In(IDictionary<string, object> @in)
        {
            this.@in = @in;
            return this;
        }

        internal virtual TestDef Out(IVal @out)
        {
            this.@out = @out;
            return this;
        }

        internal virtual TestDef State(IDictionary<object, object> state)
        {
            this.state = state;
            return this;
        }

        public override string ToString()
        {
            return expr;
        }
    }

    internal class CustAttrFactory : IAttributeFactory
    {
        internal readonly IAttributeFactory af;

        public CustAttrFactory(IAttributeFactory af)
        {
            this.af = af;
        }

        public virtual IQualifier NewQualifier(Type? objType, long qualID, object val)
        {
            if (objType.MessageType.Equals("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage"))
                return new NestedMsgQualifier(qualID, (string)val);

            return af.NewQualifier(objType, qualID, val);
        }

        public virtual INamespacedAttribute AbsoluteAttribute(long id, params string[] names)
        {
            return af.AbsoluteAttribute(id, names);
        }

        public virtual IAttribute ConditionalAttribute(long id, IInterpretable expr, IAttribute t, IAttribute f)
        {
            return af.ConditionalAttribute(id, expr, t, f);
        }

        public virtual IAttribute MaybeAttribute(long id, string name)
        {
            return af.MaybeAttribute(id, name);
        }

        public virtual IAttribute RelativeAttribute(long id, IInterpretable operand)
        {
            return af.RelativeAttribute(id, operand);
        }
    }

    internal class NestedMsgQualifier : ICoster, IQualifier
    {
        internal readonly string field;
        internal readonly long id;

        internal NestedMsgQualifier(long id, string field)
        {
            this.id = id;
            this.field = field;
        }

        /// <summary>
        ///     Cost implements the Coster interface method. It returns zero for testing purposes.
        /// </summary>
        public virtual Cost Cost()
        {
            return Interpreter.Cost.None;
        }

        public virtual long Id()
        {
            return id;
        }

        public virtual object? Qualify(IActivation vars, object obj)
        {
            return ((TestAllTypes.Types.NestedMessage)obj).Bb;
        }
    }
}