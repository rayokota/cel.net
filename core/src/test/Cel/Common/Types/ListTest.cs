using System.Collections;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
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
namespace Cel.Common.Types;

public abstract class ListTest<CONSTRUCT>
{
    internal abstract Val ConstructList(TypeAdapter typeAdapter, CONSTRUCT[] input);

    // Traits: Val, Adder, Container, Indexer, IterableT, Sizer

    public virtual void ListConstruction(TestData tc)
    {
        var val = ConstructList(tc.typeAdapter, tc.input);

        var list = CheckList(tc, val);

        // add list to itself
        var doubleTc = tc.CopyAndAdd(tc.input, tc.validate);
        var doubleListVal = list.Add(list);
        Assert.That(doubleListVal, Is.InstanceOf(typeof(Lister)));

        CheckList(doubleTc, doubleListVal);
    }

    internal virtual Lister CheckList(TestData tc, Val listVal)
    {
        Assert.That(listVal, Is.InstanceOf(typeof(Lister)));
        Assert.That(listVal, Is.InstanceOf(typeof(Sizer)));
        Assert.That(listVal, Is.InstanceOf(typeof(Indexer)));
        Assert.That(listVal, Is.InstanceOf(typeof(Container)));
        Assert.That(listVal, Is.InstanceOf(typeof(IterableT)));
        Assert.That(listVal, Is.InstanceOf(typeof(Adder)));
        Assert.That(listVal, Is.InstanceOf(typeof(Val)));

        var list = (Lister)listVal;

        Assert.That(list.ConvertToType(ListT.ListType), Is.SameAs(list));
        Assert.That(list.ConvertToType(TypeT.TypeType), Is.SameAs(ListT.ListType));
        Assert.That(Err.IsError(list.ConvertToType(IntT.IntType)), Is.True);

        // Sizer.size()
        var size = tc.SourceSize();
        var size2 = list.Size();
        Assert.That(list.Size(), Is.EqualTo(IntT.IntOf(tc.SourceSize())));

        for (var i = 0; i < size; i++)
        {
            var src = tc.SourceGet(i);
            var srcVal = tc.typeAdapter(src);

            // Indexer.get()
            var elem = list.Get(IntT.IntOf(i));
            var nat = elem.ConvertToNative(src is Val ? typeof(Val) : src.GetType());

            Assert.That(src, Is.InstanceOf(nat.GetType()));
            Assert.That(srcVal.Type(), Is.SameAs(elem.Type()));
            Assert.That(src, Is.EqualTo(nat));
            Assert.That(nat, Is.EqualTo(src));
            Assert.That(srcVal, Is.EqualTo(elem));
            Assert.That(elem, Is.EqualTo(srcVal));
            Assert.That(srcVal.Equal(elem), Is.SameAs(BoolT.True));
            Assert.That(elem.Equal(srcVal), Is.SameAs(BoolT.True));

            // Container.contains()
            Assert.That(list.Contains(elem), Is.SameAs(BoolT.True));
            Assert.That(list.Contains(srcVal), Is.SameAs(BoolT.True));
        }

        // non-existence checks
        // NOTE: a .contains() that does *not* return BoolT.True on a list having different types does
        // return an error. So the assertion here must be `!= BoolT.True`.
        Assert.That(list.Contains(IntT.IntOf(987654321)), Is.Not.SameAs(BoolT.True));
        Assert.That(list.Contains(StringT.StringOf("this-is-not-in-the-list")), Is.Not.SameAs(BoolT.True));

        // IterableT.iterate()
        var iter = list.Iterator();
        Assert.That(iter, Is.Not.Null);
        IList<Val> collected = new List<Val>();
        for (var index = 0; iter.HasNext() == BoolT.True; index++)
        {
            var next = iter.Next();
            collected.Add(next);

            // compare n-th element from iterator with element at index 'n' in list
            Assert.That(next.Equal(list.Get(IntT.IntOf(index))), Is.SameAs(BoolT.True));
        }

        Assert.That(collected.Count, Is.EqualTo(size));
        for (var i = 0; i < size; i++) Assert.That(list.Get(IntT.IntOf(i)).Equal(collected[i]), Is.SameAs(BoolT.True));

        for (var i = 0; i < 3; i++)
        {
            Assert.That(iter.HasNext(), Is.SameAs(BoolT.False));
            Assert.That(Err.IsError(iter.Next()), Is.True);
        }

        // Adder.add()
        Assert.That(Err.IsError(list.Add(NullT.NullValue)), Is.True);
        Assert.That(Err.IsError(list.Add(StringT.StringOf("foo"))), Is.True);

        return list;
    }

    public class TestData
    {
        internal readonly string name;
        internal CONSTRUCT[] input;
        internal TypeAdapter typeAdapter = DefaultTypeAdapter.Instance.ToTypeAdapter();
        internal Val[] validate;

        internal TestData(string name)
        {
            this.name = name;
        }

        internal virtual TestData Copy()
        {
            return new TestData(name).Input(input).Validate(validate).TypeAdapter(typeAdapter);
        }

        internal virtual TestData CopyAndAdd(CONSTRUCT[] input, params Val[] inputValidate)
        {
            Assert.That(SizeOf(input), Is.EqualTo(inputValidate.Length));
            var added = SourceAdd(input);
            var valid = new Val[validate.Length + inputValidate.Length];
            Array.Copy(validate, 0, valid, 0, validate.Length);
            Array.Copy(inputValidate, 0, valid, validate.Length, inputValidate.Length);
            return Copy().Input(added).Validate(valid);
        }

        public override string ToString()
        {
            return name;
        }

        internal virtual TestData Input(CONSTRUCT[] input)
        {
            this.input = input;
            return this;
        }

        internal virtual TestData Validate(params Val[] validate)
        {
            this.validate = validate;
            return this;
        }

        internal virtual TestData TypeAdapter(TypeAdapter typeAdapter)
        {
            this.typeAdapter = typeAdapter;
            return this;
        }

        internal virtual CONSTRUCT[] SourceAdd(CONSTRUCT[] add)
        {
            var srcSize = SizeOf(input);
            var addSize = SizeOf(add);

            CONSTRUCT[] newOne;
            if (IsList(input))
            {
                newOne = new CONSTRUCT[srcSize + addSize];
                for (var i = 0; i < srcSize; i++) newOne[i] = (CONSTRUCT)GetAt(input, i);
                for (var i = 0; i < addSize; i++) newOne[srcSize + i] = (CONSTRUCT)GetAt(add, i);
            }
            else
            {
                newOne = new CONSTRUCT[srcSize + addSize];
                Array.Copy(input, 0, newOne, 0, input.Length);
                for (var i = 0; i < addSize; i++) newOne.SetValue(GetAt(add, i), srcSize + i);
            }

            return newOne;
        }

        internal virtual int SourceSize()
        {
            return SizeOf(input);
        }

        internal static int SizeOf(object listOrArray)
        {
            if (IsList(listOrArray)) return ((ICollection)listOrArray).Count;

            return ((CONSTRUCT[])listOrArray).Length;
        }

        internal static bool IsList(object listOrArray)
        {
            return listOrArray is IList;
        }

        internal virtual object SourceGet(int index)
        {
            return GetAt(input, index);
        }

        internal static object GetAt(object listOrArray, int index)
        {
            if (IsList(listOrArray)) return ((IList)listOrArray)[index];

            return ((CONSTRUCT[])listOrArray)[index];
        }
    }

    // TODO JSON to list
    // TODO list to JSON

    // TODO Any to list
    // TODO list to Any
}

[TestFixture]
public class StringArrayListTest : ListTest<string>
{
    [TestCaseSource(nameof(TestDataSets))]
    public override void ListConstruction(TestData tc)
    {
        base.ListConstruction(tc);
    }

    internal override Val ConstructList(TypeAdapter typeAdapter, string[] input)
    {
        return ListT.NewStringArrayList(input);
    }

    private static IList<TestData> TestDataSets()
    {
        return new List<TestData>
        {
            new TestData("empty").Input(new string[0]).Validate(),
            new TestData("one-empty").Input(new[] { "" }).Validate(StringT.StringOf("")),
            new TestData("one").Input(new[] { "one" }).Validate(StringT.StringOf("one")),
            new TestData("three").Input(new[] { "one", "two", "three" })
                .Validate(StringT.StringOf("one"), StringT.StringOf("two"), StringT.StringOf("three"))
        };
    }
}

[TestFixture]
public class GenericArrayListTest : ListTest<object>
{
    [TestCaseSource(nameof(TestDataSets))]
    public override void ListConstruction(TestData tc)
    {
        base.ListConstruction(tc);
    }

    internal override Val ConstructList(TypeAdapter typeAdapter, object[] input)
    {
        return ListT.NewGenericArrayList(typeAdapter, input);
    }

    private static IList<TestData> TestDataSets()
    {
        return new List<TestData>
        {
            new TestData("empty").Input(new object[0]).Validate(),
            new TestData("one-empty").Input(new object[] { "" }).Validate(StringT.StringOf("")),
            new TestData("one").Input(new object[] { "one" }).Validate(StringT.StringOf("one")),
            new TestData("one-int").Input(new object[] { 1 }).Validate(IntT.IntOf(1)),
            new TestData("val-double-string-int")
                .Input(new object[] { DoubleT.DoubleOf(42.42d), StringT.StringOf("string"), IntT.IntOf(42) })
                .Validate(DoubleT.DoubleOf(42.42d), StringT.StringOf("string"), IntT.IntOf(42)),
            new TestData("mixed")
                .Input(new object[] { "one", StringT.StringOf("two"), "three", NullT.NullValue, UintT.UintOf(99) })
                .Validate(StringT.StringOf("one"), StringT.StringOf("two"), StringT.StringOf("three"),
                    NullT.NullValue, UintT.UintOf(99)),
            new TestData("three").Input(new object[] { "one", "two", "three" })
                .Validate(StringT.StringOf("one"), StringT.StringOf("two"), StringT.StringOf("three"))
        };
    }
}