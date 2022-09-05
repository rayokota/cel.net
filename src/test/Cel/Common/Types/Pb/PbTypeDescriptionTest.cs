using Cel.Checker;
using Google.Api.Expr.Test.V1.Proto3;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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
namespace Cel.Common.Types.Pb;

using Message = IMessage;

public class PbTypeDescriptionTest
{
    public static string[] GetAllTypeNames()
    {
        return new[]
        {
            ".google.protobuf.Any", ".google.protobuf.BoolValue", ".google.protobuf.BytesValue",
            ".google.protobuf.DoubleValue", ".google.protobuf.FloatValue", ".google.protobuf.Int32Value",
            ".google.protobuf.Int64Value", ".google.protobuf.ListValue", ".google.protobuf.Struct",
            ".google.protobuf.Value"
        };
    }

    [TestCaseSource(nameof(GetAllTypeNames))]
    public virtual void TypeDescriptor(string typeName)
    {
        var pbdb = Db.NewDb();
        Assert.That(pbdb.DescribeType(typeName), Is.Not.Null);
    }

    [Test]
    public virtual void FieldMap()
    {
        var pbdb = Db.NewDb();
        var msg = new NestedTestAllTypes();
        pbdb.RegisterMessage(msg);
        var td = pbdb.DescribeType(NestedTestAllTypes.Descriptor.FullName);
        Assert.That(td, Is.Not.Null);

        Assert.That(td.FieldMap().Count, Is.EqualTo(2));
    }

    internal static MaybeUnwrapTestCase[] MaybeUnwrapTestCases()
    {
        var listValue = new Value();
        listValue.ListValue = JsonList(UnwrapTestCase.trueValue, UnwrapTestCase.numValue);
        var structValue = new Value();
        structValue.StructValue = JsonStruct(TestUtil.MapOf("hello", UnwrapTestCase.strValue2));

        return new[]
        {
            new MaybeUnwrapTestCase("msgDesc.zero()").In(UnwrapContext.Get().msgDesc.Zero())
                .Out(NullValue.NullValue),
            new MaybeUnwrapTestCase("any(true)").In(AnyMsg(UnwrapTestCase.trueBool)).Out(true),
            new MaybeUnwrapTestCase("any(value(number(4.5)))")
                .In(AnyMsg(UnwrapTestCase.numValue)).Out(1.5),
            /*
            (new MaybeUnwrapTestCase("dyn(any(value(number(4.5)))"))
            .In(DynMsg(AnyMsg(Value.newBuilder().setNumberValue(4.5).build()))).Out(4.5),
            (new MaybeUnwrapTestCase("dyn(any(singleFloat))"))
            .In(DynMsg(AnyMsg(TestAllTypes.newBuilder().setSingleFloat(123.0f).build())))
            .Out(TestAllTypes.newBuilder().setSingleFloat(123.0f).build()),
            (new MaybeUnwrapTestCase("dyn(listValue())")).In(DynMsg(ListValue.getDefaultInstance()))
            .Out(JsonList()),
            */
            new MaybeUnwrapTestCase("value(null)")
                .In(UnwrapTestCase.nullValue).Out(NullValue.NullValue),
            new MaybeUnwrapTestCase("value()").In(new Value()).Out(NullValue.NullValue),
            new MaybeUnwrapTestCase("value(number(1.5))").In(UnwrapTestCase.numValue)
                .Out(1.5d),
            new MaybeUnwrapTestCase("value(list(true, number(1.0)))")
                .In(listValue)
                .Out(JsonList(UnwrapTestCase.trueValue, UnwrapTestCase.numValue)),
            new MaybeUnwrapTestCase("value(struct(hello->world))")
                .In(structValue)
                .Out(JsonStruct(TestUtil.MapOf("hello", UnwrapTestCase.strValue2))),
            new MaybeUnwrapTestCase("b'hello'").In(UnwrapTestCase.bytesValue)
                .Out(ByteString.CopyFromUtf8("hello")),
            new MaybeUnwrapTestCase("true").In(UnwrapTestCase.trueBool).Out(true),
            new MaybeUnwrapTestCase("false").In(UnwrapTestCase.falseBool).Out(false),
            new MaybeUnwrapTestCase("doubleValue(-4.2)").In(UnwrapTestCase.doubleValue).Out(-4.2),
            new MaybeUnwrapTestCase("floatValue(4.5)").In(UnwrapTestCase.floatValue).Out(4.5f),
            new MaybeUnwrapTestCase("int32(123)").In(UnwrapTestCase.int32Value).Out(123),
            new MaybeUnwrapTestCase("int64(456)").In(UnwrapTestCase.int64Value).Out(456L),
            new MaybeUnwrapTestCase("string(goodbye)").In(UnwrapTestCase.strValue2).Out("goodbye"),
            new MaybeUnwrapTestCase("uint32(1234)").In(UnwrapTestCase.uint32Value).Out((ulong)1234),
            new MaybeUnwrapTestCase("uint64(5678)").In(UnwrapTestCase.uint64Value).Out((ulong)5678),
            new MaybeUnwrapTestCase("timestamp(12345,0)")
                .In(UnwrapTestCase.timestampValue)
                .Out(Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ)),
            new MaybeUnwrapTestCase("duration(345)").In(UnwrapTestCase.durationValue)
                .Out(Period.FromSeconds(345))
        };
    }

    [TestCaseSource(nameof(MaybeUnwrapTestCases))]
    public virtual void MaybeUnwrap(MaybeUnwrapTestCase tc)
    {
        var c = UnwrapContext.Get();

        var typeName = tc.@in.Descriptor.FullName;
        var td = c.pbdb.DescribeType(typeName);
        Assert.That(td, Is.Not.Null);
        var val = td.MaybeUnwrap(c.pbdb, tc.@in);
        Assert.That(val, Is.Not.Null);

        Assert.That(val, Is.EqualTo(tc.@out));
    }

    public static UnwrapTestCase[] GetAllUnwrapTestCases()
    {
        return UnwrapTestCase.Values();
    }

    [TestCaseSource(nameof(GetAllUnwrapTestCases))]
    public virtual void BenchmarkTypeDescriptionMaybeUnwrap(UnwrapTestCase tc)
    {
        var c = UnwrapContext.Get();

        var msg = tc.Message();

        var typeName = msg.Descriptor.FullName;
        var td = c.pbdb.DescribeType(typeName);
        Assert.That(td, Is.Not.Null);

        td.MaybeUnwrap(c.pbdb, msg);
    }

    [Test]
    public virtual void CheckedType()
    {
        var pbdb = Db.NewDb();
        var msg = new TestAllTypes();
        var msgName = TestAllTypes.Descriptor.FullName;
        pbdb.RegisterMessage(msg);
        var td = pbdb.DescribeType(msgName);
        Assert.That(td, Is.Not.Null);

        var field = td.FieldByName("map_string_string");
        Assert.That(field, Is.Not.Null);

        var mapType = Decls.NewMapType(Decls.String, Decls.String);
        Assert.That(field.CheckedType(), Is.EqualTo(mapType));

        field = td.FieldByName("repeated_nested_message");
        Assert.That(field, Is.Not.Null);
        var listType =
            Decls.NewListType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage"));
        Assert.That(field.CheckedType(), Is.EqualTo(listType));
    }

    internal static Any AnyMsg(Message msg)
    {
        return Any.Pack(msg);
    }

    internal static ListValue JsonList(params Value[] elems)
    {
        var list = new ListValue();
        list.Values.Add(elems);
        return list;
    }

    internal static Struct JsonStruct(IDictionary<object, object> entries)
    {
        var s = new Struct();
        foreach (var entry in entries) s.Fields[entry.Key.ToString()] = (Value)entry.Value;
        return s;
    }

    public class MaybeUnwrapTestCase
    {
        internal readonly string name;
        internal Message @in;
        internal object @out;

        internal MaybeUnwrapTestCase(string name)
        {
            this.name = name;
        }

        internal virtual MaybeUnwrapTestCase In(Message @in)
        {
            this.@in = @in;
            return this;
        }

        internal virtual MaybeUnwrapTestCase Out(object @out)
        {
            this.@out = @out;
            return this;
        }

        public override string ToString()
        {
            return name;
        }
    }
}