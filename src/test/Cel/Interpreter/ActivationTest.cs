using Antlr4.Runtime.Sharpen;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using NodaTime;
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

public class ActivationTest
{
    [Test]
    public virtual void Activation()
    {
        var act = global::Cel.Interpreter.Activation.NewActivation(TestUtil.BindingsOf("a", BoolT.True));
        Assert.That(act, Is.Not.Null);
        Assert.That(global::Cel.Interpreter.Activation.NewActivation(act), Is.Not.Null);
        Assert.That(() => global::Cel.Interpreter.Activation.NewActivation(""),
            Throws.Exception.TypeOf(typeof(ArgumentException)));
    }

    [Test]
    public virtual void Resolve()
    {
        var activation = global::Cel.Interpreter.Activation.NewActivation(TestUtil.BindingsOf("a", BoolT.True));
        Assert.That(activation.ResolveName("a"), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void ResolveLazy()
    {
        var v = new AtomicReference<Val>();
        var now = () =>
        {
            if (v.Get() == null) v.Set(DefaultTypeAdapter.Instance.NativeToValue(new ZonedDateTime()));
            return v.Get();
        };
        IDictionary<string, object> map = new Dictionary<string, object>();
        map["now"] = now;
        var a = global::Cel.Interpreter.Activation.NewActivation(map);
        var first = a.ResolveName("now");
        var second = a.ResolveName("now");
        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public virtual void HierarchicalActivation()
    {
        // compose a parent with more properties than the child
        IDictionary<string, object> parentMap = new Dictionary<string, object>();
        parentMap["a"] = StringT.StringOf("world");
        parentMap["b"] = IntT.IntOf(-42);
        var parent = global::Cel.Interpreter.Activation.NewActivation(parentMap);
        // compose the child such that it shadows the parent
        IDictionary<string, object> childMap = new Dictionary<string, object>();
        childMap["a"] = BoolT.True;
        childMap["c"] = StringT.StringOf("universe");
        var child = global::Cel.Interpreter.Activation.NewActivation(childMap);
        var combined = global::Cel.Interpreter.Activation.NewHierarchicalActivation(parent, child);

        // Resolve the shadowed child value.
        Assert.That(combined.ResolveName("a"), Is.SameAs(BoolT.True));
        // Resolve the parent only value.
        Assert.That(combined.ResolveName("b"), Is.EqualTo(IntT.IntOf(-42)));
        // Resolve the child only value.
        Assert.That(combined.ResolveName("c"), Is.EqualTo(StringT.StringOf("universe")));
    }
}