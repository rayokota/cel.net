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

using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using NUnit.Framework;

namespace Cel.Interpreter;

internal class AttributePatternsTest
{
    internal static PatternTest[] AttributePatternsTestCases()
    {
        return new[]
        {
            new PatternTest("var", AttributePattern.NewAttributePattern("var"))
                .Matches(new Attr("var"), new Attr("var").Quals("field")).Misses(new Attr("ns.var")),
            new PatternTest("var_namespace", AttributePattern.NewAttributePattern("ns.app.var"))
                .Matches(new Attr("ns.app.var"), new Attr("ns.app.var").Quals(0L),
                    new Attr("ns").Quals("app", "var", "foo").Container("ns.app").Unchecked(true))
                .Misses(new Attr("ns.var"), new Attr("ns").Quals("var").Container("ns.app").Unchecked(true)),
            new PatternTest("var_field", AttributePattern.NewAttributePattern("var").QualString("field"))
                .Matches(new Attr("var"), new Attr("var").Quals("field"),
                    new Attr("var").Quals("field").Unchecked(true), new Attr("var").Quals("field", 1L))
                .Misses(new Attr("var").Quals("other")),
            new PatternTest("var_index", AttributePattern.NewAttributePattern("var").QualInt(0))
                .Matches(new Attr("var"), new Attr("var").Quals(0L), new Attr("var").Quals(0L, false))
                .Misses(new Attr("var").Quals((ulong)0L), new Attr("var").Quals(1L, false)),
            new PatternTest("var_index_uint", AttributePattern.NewAttributePattern("var").QualUint(1))
                .Matches(new Attr("var"), new Attr("var").Quals((ulong)1L),
                    new Attr("var").Quals((ulong)1L, true))
                .Misses(new Attr("var").Quals((ulong)0L), new Attr("var").Quals(1L, false)),
            new PatternTest("var_index_bool", AttributePattern.NewAttributePattern("var").QualBool(true))
                .Matches(new Attr("var"), new Attr("var").Quals(true), new Attr("var").Quals(true, "name"))
                .Misses(new Attr("var").Quals(false), new Attr("none")),
            new PatternTest("var_wildcard", AttributePattern.NewAttributePattern("ns.var").Wildcard()).Matches(
                    new Attr("ns.var"), new Attr("var").Quals(true).Container("ns").Unchecked(true),
                    new Attr("var").Quals("name").Container("ns").Unchecked(true),
                    new Attr("var").Quals("name").Container("ns").Unchecked(true))
                .Misses(new Attr("var").Quals(false), new Attr("none")),
            new PatternTest("var_wildcard_field",
                    AttributePattern.NewAttributePattern("var").Wildcard().QualString("field"))
                .Matches(new Attr("var"), new Attr("var").Quals(true), new Attr("var").Quals(10L, "field"))
                .Misses(new Attr("var").Quals(10L, "other")),
            new PatternTest("var_wildcard_wildcard",
                AttributePattern.NewAttributePattern("var").Wildcard().Wildcard()).Matches(new Attr("var"),
                new Attr("var").Quals(true), new Attr("var").Quals(10L, "field")).Misses(new Attr("none"))
        };
    }

    [TestCaseSource(nameof(AttributePatternsTestCases))]
    public virtual void UnknownResolution(PatternTest tst)
    {
        if (tst.disabled != null) return;

        TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        for (var i = 0; i < tst.matches.Length; i++)
        {
            var m = tst.matches[i];
            var cont = Container.DefaultContainer;
            if (m.@unchecked) cont = Container.NewContainer(Container.Name(m.container));

            var fac = AttributePattern.NewPartialAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            var attr = GenAttr(fac, m);
            var partVars =
                Activation.NewPartialActivation(Activation.EmptyActivation(), tst.pattern);
            var val = attr.Resolve(partVars);
            Assert.That(val, Is.InstanceOf(typeof(UnknownT)));
        }

        for (var i = 0; i < tst.misses.Length; i++)
        {
            var m = tst.misses[i];
            var cont = Container.DefaultContainer;
            if (m.@unchecked) cont = Container.NewContainer(Container.Name(m.container));

            var fac = AttributePattern.NewPartialAttributeFactory(cont, reg.ToTypeAdapter(), reg);
            var attr = GenAttr(fac, m);
            var partVars =
                Activation.NewPartialActivation(Activation.EmptyActivation(), tst.pattern);
            Assert.That(() => attr.Resolve(partVars), Throws.Exception.TypeOf(typeof(Err.ErrException)));
        }
    }

    [Test]
    public virtual void CrossReference()
    {
        TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var fac = AttributePattern.NewPartialAttributeFactory(Container.DefaultContainer, reg.ToTypeAdapter(), reg);
        var a = fac.AbsoluteAttribute(1, "a");
        var b = fac.AbsoluteAttribute(2, "b");
        a.AddQualifier(b);

        // Ensure that var a[b], the dynamic index into var 'a' is the unknown value
        // returned from attribute resolution.
        var partVars =
            Activation.NewPartialActivation(TestUtil.BindingsOf("a", new[] { 1L, 2L }),
                AttributePattern.NewAttributePattern("b"));
        var val = a.Resolve(partVars);
        Assert.That(val, Is.EqualTo(UnknownT.UnknownOf(2)));

        // Ensure that a[b], the dynamic index into var 'a' is the unknown value
        // returned from attribute resolution. Note, both 'a' and 'b' have unknown attribute
        // patterns specified. This changes the evaluation behavior slightly, but the end
        // result is the same.
        partVars = Activation.NewPartialActivation(TestUtil.BindingsOf("a", new[] { 1L, 2L }),
            AttributePattern.NewAttributePattern("a").QualInt(0), AttributePattern.NewAttributePattern("b"));
        val = a.Resolve(partVars);
        Assert.That(val, Is.EqualTo(UnknownT.UnknownOf(2)));

        // Note, that only 'a[0].c' will result in an unknown result since both 'a' and 'b'
        // have values. However, since the attribute being pattern matched is just 'a.b',
        // the outcome will indicate that 'a[b]' is unknown.
        partVars = Activation.NewPartialActivation(TestUtil.BindingsOf("a", new long[] { 1, 2 }, "b", 0),
            AttributePattern.NewAttributePattern("a").QualInt(0).QualString("c"));
        val = a.Resolve(partVars);
        Assert.That(val, Is.EqualTo(UnknownT.UnknownOf(2)));

        // Test a positive case that returns a valid value even though the attribugte factory
        // is the partial attribute factory.
        partVars = Activation.NewPartialActivation(TestUtil.BindingsOf("a", new long[] { 1, 2 }, "b", 0));
        val = a.Resolve(partVars);
        Assert.That(val, Is.EqualTo(1L));

        // Ensure the unknown attribute id moves when the attribute becomes more specific.
        partVars = Activation.NewPartialActivation(TestUtil.BindingsOf("a", new long[] { 1, 2 }, "b", 0),
            AttributePattern.NewAttributePattern("a").QualInt(0).QualString("c"));
        // Qualify a[b] with 'c', a[b].c
        var c = fac.NewQualifier(null, 3, "c");
        a.AddQualifier(c);
        // The resolve step should return unknown
        val = a.Resolve(partVars);
        Assert.That(val, Is.EqualTo(UnknownT.UnknownOf(3)));
    }

    internal static Attribute GenAttr(AttributeFactory fac, Attr a)
    {
        var id = 1L;
        Attribute attr;
        if (a.@unchecked)
            attr = fac.MaybeAttribute(1, a.name);
        else
            attr = fac.AbsoluteAttribute(1, a.name);

        if (a.quals != null)
            foreach (var q in a.quals)
            {
                var qual = fac.NewQualifier(null, id, q);
                attr.AddQualifier(qual);
                id++;
            }

        return attr;
    }

    /// <summary>
    ///     attr describes a simplified format for specifying common Attribute and Qualifier values for use
    ///     in pattern matching tests.
    /// </summary>
    internal class Attr
    {
        /// <summary>
        ///     variable name, fully qualified unless the attr is marked as unchecked=true
        /// </summary>
        internal readonly string name;

        /// <summary>
        ///     container simulates the expression container and is only relevant on 'unchecked' test inputs
        ///     + as the container is used to resolve the potential fully qualified variable names
        ///     represented + by an identifier or select expression.
        /// </summary>
        internal string container = "";

        /// <summary>
        ///     quals contains a list of static qualifiers.
        /// </summary>
        internal object[] quals;

        /// <summary>
        ///     unchecked indicates whether the attribute has not been type-checked and thus not gone // the
        ///     variable and function resolution step.
        /// </summary>
        internal bool @unchecked;

        internal Attr(string name)
        {
            this.name = name;
        }

        internal virtual Attr Unchecked(bool @unchecked)
        {
            this.@unchecked = @unchecked;
            return this;
        }

        internal virtual Attr Container(string container)
        {
            this.container = container;
            return this;
        }

        internal virtual Attr Quals(params object[] quals)
        {
            this.quals = quals;
            return this;
        }

        public override string ToString()
        {
            return "Attr{" + "unchecked=" + @unchecked + ", container='" + container + '\'' + ", name='" + name +
                   '\'' + ", quals=" +
                   (quals != null ? string.Join(",\n    ", quals.Select(o => o.ToString())) : null) +
                   '}';
        }
    }

    /// <summary>
    ///     patternTest describes a pattern, and a set of matches and misses for the pattern to highlight +
    ///     what the pattern will and will not match.
    /// </summary>
    internal class PatternTest
    {
        internal readonly string name;
        internal readonly AttributePattern pattern;
        internal string disabled;
        internal Attr[] matches;
        internal Attr[] misses;

        internal PatternTest(string name, AttributePattern pattern)
        {
            this.name = name;
            this.pattern = pattern;
        }

        public override string ToString()
        {
            return name;
        }

        internal virtual PatternTest Disabled(string reason)
        {
            disabled = reason;
            return this;
        }

        internal virtual PatternTest Matches(params Attr[] matches)
        {
            this.matches = matches;
            return this;
        }

        internal virtual PatternTest Misses(params Attr[] misses)
        {
            this.misses = misses;
            return this;
        }
    }
}