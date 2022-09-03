using System.Collections.Generic;
using Cel.Checker;
using Cel.Common;
using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
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
namespace Cel.Interpreter
{
    using TestAllTypes = Google.Api.Expr.Test.V1.Proto3.TestAllTypes;
    using NestedMessage = Google.Api.Expr.Test.V1.Proto3.TestAllTypes.Types.NestedMessage;
    using Decl = Google.Api.Expr.V1Alpha1.Decl;
    using Type = Google.Api.Expr.V1Alpha1.Type;
    using Any = Google.Protobuf.WellKnownTypes.Any;

    internal class AttributesTest
    {
[Test]
        public virtual void AttributesAbsoluteAttr()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            Container cont = Container.NewContainer(Container.Name("acme.ns"));
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            Activation vars = Activation.NewActivation(TestUtil.MapOf("acme.a",
                TestUtil.MapOf("b", TestUtil.MapOf(4L, TestUtil.MapOf(false, "success")))));

            // acme.a.b[4][false]
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "acme.a");
            Qualifier qualB = attrs.NewQualifier(null, 2, "b");
            Qualifier qual4 = attrs.NewQualifier(null, 3, 4L);
            Qualifier qualFalse = attrs.NewQualifier(null, 4, false);
            attr.AddQualifier(qualB);
            attr.AddQualifier(qual4);
            attr.AddQualifier(qualFalse);
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo("success"));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesAbsoluteAttrType()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);

            // int
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "int");
            object @out = attr.Resolve(Activation.EmptyActivation());
            Assert.That(@out, Is.SameAs(IntT.IntType));
            Assert.That(@out, Is.SameAs(IntT.IntType));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesRelativeAttr()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a", TestUtil.MapOf(-1, new int[] { 2, 42 }), "b", 1);
            Activation vars = Activation.NewActivation(data);

            // The relative attribute under test is applied to a map literal:
            // {
            //   a: {-1: [2, 42], b: 1}
            //   b: 1
            // }
            //
            // The expression being evaluated is: <map-literal>.a[-1][b] -> 42
            InterpretableConst op = Interpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
            Attribute attr = attrs.RelativeAttribute(1, op);
            Qualifier qualA = attrs.NewQualifier(null, 2, "a");
            Qualifier qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
            attr.AddQualifier(qualA);
            attr.AddQualifier(qualNeg1);
            attr.AddQualifier(attrs.AbsoluteAttribute(4, "b"));
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesRelativeAttrOneOf()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            Container cont = Container.NewContainer(Container.Name("acme.ns"));
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            IDictionary<object, object>
                data = TestUtil.MapOf("a", TestUtil.MapOf(-1, new int[] { 2, 42 }), "acme.b", 1);
            Activation vars = Activation.NewActivation(data);

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
            InterpretableConst op = Interpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
            Attribute attr = attrs.RelativeAttribute(1, op);
            Qualifier qualA = attrs.NewQualifier(null, 2, "a");
            Qualifier qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
            attr.AddQualifier(qualA);
            attr.AddQualifier(qualNeg1);
            attr.AddQualifier(attrs.MaybeAttribute(4, "b"));
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesRelativeAttrConditional()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a", TestUtil.MapOf(-1, new int[] { 2, 42 }), "b",
                new int[] { 0, 1 }, "c", new object[] { 1, 0 });
            Activation vars = Activation.NewActivation(data);

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
            InterpretableConst cond = Interpretable.NewConstValue(2, BoolT.False);
            Attribute condAttr = attrs.ConditionalAttribute(4, cond, attrs.AbsoluteAttribute(5, "b"),
                attrs.AbsoluteAttribute(6, "c"));
            Qualifier qual0 = attrs.NewQualifier(null, 7, 0);
            condAttr.AddQualifier(qual0);

            InterpretableConst obj = Interpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
            Attribute attr = attrs.RelativeAttribute(1, obj);
            Qualifier qualA = attrs.NewQualifier(null, 2, "a");
            Qualifier qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
            attr.AddQualifier(qualA);
            attr.AddQualifier(qualNeg1);
            attr.AddQualifier(condAttr);
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo(IntT.IntOf(42)));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesRelativeAttrRelative()
        {
            Container cont = Container.NewContainer(Container.Name("acme.ns"));
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a",
                TestUtil.MapOf(-1, TestUtil.MapOf("first", 1, "second", 2, "third", 3)), "b", 2L);
            Activation vars = Activation.NewActivation(data);

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
            InterpretableConst obj = Interpretable.NewConstValue(1, reg.ToTypeAdapter()(data));
            InterpretableConst mp = Interpretable.NewConstValue(1,
                reg.ToTypeAdapter()(TestUtil.MapOf(1, "first", 2, "second", 3, "third")));
            Attribute relAttr = attrs.RelativeAttribute(4, mp);
            Qualifier qualB = attrs.NewQualifier(null, 5, attrs.AbsoluteAttribute(5, "b"));
            relAttr.AddQualifier(qualB);
            Attribute attr = attrs.RelativeAttribute(1, obj);
            Qualifier qualA = attrs.NewQualifier(null, 2, "a");
            Qualifier qualNeg1 = attrs.NewQualifier(null, 3, IntT.IntOf(-1));
            attr.AddQualifier(qualA);
            attr.AddQualifier(qualNeg1);
            attr.AddQualifier(relAttr);

            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo(IntT.IntOf(2)));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesOneofAttr()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            Container cont = Container.NewContainer(Container.Name("acme.ns"));
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a", TestUtil.MapOf("b", new int[] { 2, 42 }), "acme.a.b",
                1, "acme.ns.a.b", "found");
            Activation vars = Activation.NewActivation(data);

            // a.b -> should resolve to acme.ns.a.b per namespace resolution rules.
            Attribute attr = attrs.MaybeAttribute(1, "a");
            Qualifier qualB = attrs.NewQualifier(null, 2, "b");
            attr.AddQualifier(qualB);
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo("found"));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesConditionalAttrTrueBranch()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a", TestUtil.MapOf(-1, new int[] { 2, 42 }), "b",
                TestUtil.MapOf("c", TestUtil.MapOf(-1, new int[] { 2, 42 })));
            Activation vars = Activation.NewActivation(data);

            // (true ? a : b.c)[-1][1]
            NamespacedAttribute tv = attrs.AbsoluteAttribute(2, "a");
            Attribute fv = attrs.MaybeAttribute(3, "b");
            Qualifier qualC = attrs.NewQualifier(null, 4, "c");
            fv.AddQualifier(qualC);
            Attribute cond = attrs.ConditionalAttribute(1, Interpretable.NewConstValue(0, BoolT.True), tv, fv);
            Qualifier qualNeg1 = attrs.NewQualifier(null, 5, IntT.IntOf(-1));
            Qualifier qual1 = attrs.NewQualifier(null, 6, IntT.IntOf(1));
            cond.AddQualifier(qualNeg1);
            cond.AddQualifier(qual1);
            object @out = cond.Resolve(vars);
            Assert.That(@out, Is.EqualTo(42));
            Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
            // Note: migrated to JMH
        }

[Test]
        public virtual void AttributesConditionalAttrFalseBranch()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            IDictionary<object, object> data = TestUtil.MapOf("a", TestUtil.MapOf(-1, new int[] { 2, 42 }), "b",
                TestUtil.MapOf("c", TestUtil.MapOf(-1, new int[] { 2, 42 })));
            Activation vars = Activation.NewActivation(data);

            // (false ? a : b.c)[-1][1]
            NamespacedAttribute tv = attrs.AbsoluteAttribute(2, "a");
            Attribute fv = attrs.MaybeAttribute(3, "b");
            Qualifier qualC = attrs.NewQualifier(null, 4, "c");
            fv.AddQualifier(qualC);
            Attribute cond = attrs.ConditionalAttribute(1, Interpretable.NewConstValue(0, BoolT.False), tv, fv);
            Qualifier qualNeg1 = attrs.NewQualifier(null, 5, IntT.IntOf(-1));
            Qualifier qual1 = attrs.NewQualifier(null, 6, IntT.IntOf(1));
            cond.AddQualifier(qualNeg1);
            cond.AddQualifier(qual1);
            object @out = cond.Resolve(vars);
            Assert.That(@out, Is.EqualTo(42));
            Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
            // Note: migrated to JMH
        }

[Test]
        public virtual void AttributesConditionalAttrErrorUnknown()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);

            // err ? a : b
            NamespacedAttribute tv = attrs.AbsoluteAttribute(2, "a");
            Attribute fv = attrs.MaybeAttribute(3, "b");
            Attribute cond =
                attrs.ConditionalAttribute(1, Interpretable.NewConstValue(0, Err.NewErr("test error")), tv, fv);
            object @out = cond.Resolve(Activation.EmptyActivation());
            Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));

            // unk ? a : b
            Attribute condUnk =
                attrs.ConditionalAttribute(1, Interpretable.NewConstValue(0, UnknownT.UnknownOf(1)), tv, fv);
            @out = condUnk.Resolve(Activation.EmptyActivation());
            Assert.That(@out, Is.InstanceOf(typeof(UnknownT)));
            Assert.That(Cost.EstimateCost(fv).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(fv).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void BenchmarkResolverFieldQualifier()
        {
            NestedMessage nestedMsg = new NestedMessage();
            nestedMsg.Bb = 123;
            TestAllTypes msg = new TestAllTypes();
                msg.SingleNestedMessage = nestedMsg;
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(msg);
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            Activation vars = Activation.NewActivation(TestUtil.MapOf("msg", msg));
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "msg");
            Type opType = reg.FindType("google.api.expr.test.v1.proto3.TestAllTypes");
            Assert.That(opType, Is.Not.Null);
            Type fieldType = reg.FindType("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage");
            Assert.That(fieldType, Is.Not.Null);
            attr.AddQualifier(MakeQualifier(attrs, opType.Type_, 2, "single_nested_message"));
            attr.AddQualifier(MakeQualifier(attrs, fieldType.Type_, 3, "bb"));
            // Note: migrated to JMH
        }

[Test]
        public virtual void ResolverCustomQualifier()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs =
                new CustAttrFactory(AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg));
            NestedMessage msg = new NestedMessage();
            msg.Bb = 123;
            Activation vars = Activation.NewActivation(TestUtil.MapOf("msg", msg));
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "msg");
            Type type = new Type();
            type.MessageType = "google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage";
            Qualifier qualBB =
                attrs.NewQualifier(type, 2, "bb");
            attr.AddQualifier(qualBB);
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.EqualTo(123));
            Assert.That(Cost.EstimateCost(attr).min, Is.EqualTo(1L));
            Assert.That(Cost.EstimateCost(attr).max, Is.EqualTo(1L));
        }

[Test]
        public virtual void AttributesMissingMsg()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            Any any = Any.Pack(new TestAllTypes());
            Activation vars = Activation.NewActivation(TestUtil.MapOf("missing_msg", any));

            // missing_msg.field
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "missing_msg");
            Qualifier field = attrs.NewQualifier(null, 2, "field");
            attr.AddQualifier(field);
            Assert.That(() => attr.Resolve(vars), Throws.Exception.TypeOf(typeof(System.InvalidOperationException)));
        }

[Test]
        public virtual void AttributeMissingMsgUnknownField()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs = AttributePattern.NewPartialAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
            Any any = Any.Pack(new TestAllTypes());
            Activation vars = Activation.NewPartialActivation(TestUtil.MapOf("missing_msg", any),
                AttributePattern.NewAttributePattern("missing_msg").QualString("field"));

            // missing_msg.field
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "missing_msg");
            Qualifier field = attrs.NewQualifier(null, 2, "field");
            attr.AddQualifier(field);
            object @out = attr.Resolve(vars);
            Assert.That(@out, Is.TypeOf(typeof(UnknownT)));
        }

        internal class TestDef
        {
            internal readonly string expr;
            internal IList<Decl> env;
            internal IDictionary<object, object> @in;
            internal Val @out;
            internal IDictionary<object, object> state;

            internal TestDef(string expr)
            {
                this.expr = expr;
            }

            internal virtual TestDef Env(params Decl[] env)
            {
                this.env = new List<Decl> ( env );
                return this;
            }

            internal virtual TestDef In(IDictionary<object, object> @in)
            {
                this.@in = @in;
                return this;
            }

            internal virtual TestDef Out(Val @out)
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

        internal static TestDef[] AttributeStateTrackingTests()
        {
            return new TestDef[]
            {
                (new TestDef("[{\"field\": true}][0].field")).Env().In(TestUtil.MapOf()).Out(BoolT.True).State(
                    TestUtil.MapOf(1L, true, 6L, TestUtil.MapOf(StringT.StringOf("field"), BoolT.True), 8L, true)),
                (new TestDef("a[1]['two']"))
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.String, Decls.Bool))))
                .In(TestUtil.MapOf("a", TestUtil.MapOf(1L, TestUtil.MapOf("two", true)))).Out(BoolT.True)
                .State(TestUtil.MapOf(1L, true, 2L, TestUtil.MapOf("two", true), 4L, true)),
                (new TestDef("a[1][2][3]"))
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.Dyn, Decls.Dyn))))
                .In(TestUtil.MapOf("a",
                    TestUtil.MapOf(1, TestUtil.MapOf(1L, 0L, 2L, new string[] { "index", "middex", "outdex", "dex" }))))
                .Out(StringT.StringOf("dex")).State(TestUtil.MapOf(1L, "dex", 2L,
                    TestUtil.MapOf(1L, 0L, 2L, new string[] { "index", "middex", "outdex", "dex" }), 4L,
                    new string[] { "index", "middex", "outdex", "dex" }, 6L, "dex")),
                (new TestDef("a[1][2][a[1][1]]"))
                .Env(Decls.NewVar("a", Decls.NewMapType(Decls.Int, Decls.NewMapType(Decls.Dyn, Decls.Dyn))))
                .In(TestUtil.MapOf("a",
                    TestUtil.MapOf(1L,
                        TestUtil.MapOf(1L, 0L, 2L, new string[] { "index", "middex", "outdex", "dex" }))))
                .Out(StringT.StringOf("index")).State(TestUtil.MapOf(1L, "index", 2L,
                    TestUtil.MapOf(1L, 0L, 2L, new string[] { "index", "middex", "outdex", "dex" }), 4L,
                    new string[] { "index", "middex", "outdex", "dex" }, 6L, "index", 8L,
                    TestUtil.MapOf(1L, 0L, 2L, new string[] { "index", "middex", "outdex", "dex" }), 10L,
                    IntT.IntOf(0)))
            };
        }

        [TestCaseSource(nameof(AttributeStateTrackingTests))]
        public virtual void AttributeStateTracking(TestDef tc)
        {
            Source src = Source.NewTextSource(tc.expr);
            Parser.Parser.ParseResult parsed = Parser.Parser.ParseAllMacros(src);
            Assert.That(parsed.HasErrors(), Is.False);
            Container cont = Container.DefaultContainer;
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            CheckerEnv env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
            if (tc.env != null)
            {
                env.Add(tc.env);
            }

            Checker.Checker.CheckResult checkResult = Checker.Checker.Check(parsed, src, env);
            if (parsed.HasErrors())
            {
                throw new System.ArgumentException(parsed.Errors.ToDisplayString());
            }

            AttributeFactory attrs = AttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            Interpreter interp = Interpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
            // Show that program planning will now produce an error.
            EvalState st = EvalState.NewEvalState();
            Interpretable i = interp.NewInterpretable(checkResult.CheckedExpr, Interpreter.Optimize(),
                Interpreter.TrackState(st));
            Activation @in = Activation.NewActivation(tc.@in);
            Val @out = i.Eval(@in);
            Assert.That(@out, Is.EqualTo(tc.@out));
            foreach (KeyValuePair<object, object> iv in tc.state)
            {
                long id = Convert.ToInt64((iv.Key));
                object val = iv.Value;
                Val stVal = st.Value(id);
                Assert.That(stVal, Is.Not.Null);
                Assert.That(stVal, Is.EqualTo(DefaultTypeAdapter.Instance.NativeToValue(val)));
                TestUtil.DeepEquals(string.Format("id({0:D})", id), stVal.Value(), val);
            }
        }

[Test]
        public virtual void BenchmarkResolverCustomQualifier()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            AttributeFactory attrs =
                new CustAttrFactory(AttributeFactory.NewAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg));
            TestAllTypes.Types.NestedMessage msg = new NestedMessage();
            msg.Bb = 123;
            Activation vars = Activation.NewActivation(TestUtil.MapOf("msg", msg));
            NamespacedAttribute attr = attrs.AbsoluteAttribute(1, "msg");
            Type type = new Type();
            type.MessageType = "google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage";
            Qualifier qualBB = attrs.NewQualifier(type, 2, "bb");
            attr.AddQualifier(qualBB);
            // Note: Migrated to JMH
        }

        internal class CustAttrFactory : AttributeFactory
        {
            internal readonly AttributeFactory af;

            public CustAttrFactory(AttributeFactory af)
            {
                this.af = af;
            }

            public virtual Qualifier NewQualifier(Type objType, long qualID, object val)
            {
                if (objType.MessageType.Equals("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage"))
                {
                    return new NestedMsgQualifier(qualID, (string)val);
                }

                return af.NewQualifier(objType, qualID, val);
            }

            public virtual NamespacedAttribute AbsoluteAttribute(long id, params string[] names)
            {
                return af.AbsoluteAttribute(id, names);
            }

            public virtual Attribute ConditionalAttribute(long id, Interpretable expr, Attribute t, Attribute f)
            {
                return af.ConditionalAttribute(id, expr, t, f);
            }

            public virtual Attribute MaybeAttribute(long id, string name)
            {
                return af.MaybeAttribute(id, name);
            }

            public virtual Attribute RelativeAttribute(long id, Interpretable operand)
            {
                return af.RelativeAttribute(id, operand);
            }
        }

        internal class NestedMsgQualifier : Coster, Qualifier
        {
            internal readonly long id;
            internal readonly string field;

            internal NestedMsgQualifier(long id, string field)
            {
                this.id = id;
                this.field = field;
            }

            public virtual long Id()
            {
                return id;
            }

            public virtual object Qualify(Activation vars, object obj)
            {
                return ((TestAllTypes.Types.NestedMessage)obj).Bb;
            }

            /// <summary>
            /// Cost implements the Coster interface method. It returns zero for testing purposes. </summary>
            public virtual Cost Cost()
            {
                return global::Cel.Interpreter.Cost.None;
            }
        }

        internal static Qualifier MakeQualifier(AttributeFactory attrs, Type typ, long qualID, object val)
        {
            Qualifier qual = attrs.NewQualifier(typ, qualID, val);
            return qual;
        }
    }
}