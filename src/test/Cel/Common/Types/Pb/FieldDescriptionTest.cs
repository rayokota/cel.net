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

using Google.Api.Expr.Test.V1.Proto3;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types.Pb;

using Message = IMessage;

public class FieldDescriptionTest
{
    internal static GetFromTestCase[] FromTestCases
    {
        get
        {
            var msg = new TestAllTypes.Types.NestedMessage();
            msg.Bb = 123;
            var nullValue = new Value();
            nullValue.NullValue = NullValue.NullValue;
            var s = new Struct();
            s.Fields["null"] = nullValue;

            return new[]
            {
                new GetFromTestCase().Field("single_uint64").Want((ulong)12L),
                new GetFromTestCase().Field("single_duration").Want(Period.FromSeconds(1234)),
                new GetFromTestCase().Field("single_timestamp")
                    .Want(Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ)),
                new GetFromTestCase().Field("single_bool_wrapper").Want(false),
                new GetFromTestCase().Field("single_int32_wrapper").Want(42),
                new GetFromTestCase().Field("single_int64_wrapper").Want(NullValue.NullValue),
                new GetFromTestCase().Field("single_nested_message")
                    .Want(msg),
                new GetFromTestCase().Field("standalone_enum")
                    .Want(TestAllTypes.Types.NestedEnum.Bar),
                new GetFromTestCase().Field("single_value").Want("hello world"),
                new GetFromTestCase().Field("single_struct").Want(s)
            };
        }
    }

    public static TestCase[] SetTestCases
    {
        get
        {
            var t = new TestAllTypes();
            t.SingleBool = true;

            var f = new TestAllTypes();
            f.SingleBool = false;

            return new[]
            {
                new TestCase().Msg(t).Field("single_bool").IsSet(true),
                new TestCase().Msg(new TestAllTypes()).Field("single_bool").IsSet(false),
                new TestCase().Msg(f).Field("single_bool").IsSet(false),
                new TestCase().Msg(new TestAllTypes()).Field("single_bool").IsSet(false),
                new TestCase().Msg(new TestAllTypes()).Field("single_bool").IsSet(false),
                new TestCase().Msg(null).Field("single_any").IsSet(false)
            };
        }
    }

    [Test]
    public virtual void FieldDescription()
    {
        var pbdb = Db.NewDb();
        var msg = new NestedTestAllTypes();
        var msgName = NestedTestAllTypes.Descriptor.FullName;
        pbdb.RegisterMessage(msg);
        var td = pbdb.DescribeType(msgName);
        Assert.That(td, Is.Not.Null);

        var fd = td.FieldByName("payload");
        Assert.That(fd, Is.Not.Null);
        Assert.That(fd.Name, Is.EqualTo("payload"));
        Assert.That(fd.Oneof, Is.EqualTo(false));
        Assert.That(fd.Map, Is.EqualTo(false));
        Assert.That(fd.Message, Is.EqualTo(true));
        Assert.That(fd.Enum, Is.EqualTo(false));
        Assert.That(fd.List, Is.EqualTo(false));
        // Access the field by its Go struct name and check to see that it's index
        // matches the one determined by the TypeDescription utils.
        var got = fd.CheckedType();
        var wanted = new Type();
        wanted.MessageType = "google.api.expr.test.v1.proto3.TestAllTypes";
        Assert.That(got, Is.EqualTo(wanted));
    }

    [TestCaseSource(nameof(FromTestCases))]
    public virtual void GetFrom(GetFromTestCase tc)
    {
        var d = new Duration();
        d.Seconds = 1234;
        var ts = new Timestamp();
        ts.Seconds = 12345;
        ts.Nanos = 0;
        var nestedMsg = new TestAllTypes.Types.NestedMessage();
        nestedMsg.Bb = 123;

        var pbdb = Db.NewDb();
        var msg = new TestAllTypes();
        msg.SingleUint64 = 12;
        msg.SingleDuration = d;
        msg.SingleTimestamp = ts;
        msg.SingleBoolWrapper = false;
        msg.SingleInt32Wrapper = 42;
        msg.StandaloneEnum = TestAllTypes.Types.NestedEnum.Bar;
        msg.SingleNestedMessage = nestedMsg;
        var v = new Value();
        v.StringValue = "hello world";
        msg.SingleValue = v;
        var nullValue = new Value();
        nullValue.NullValue = NullValue.NullValue;
        var s = new Struct();
        s.Fields["null"] = nullValue;
        msg.SingleStruct = s;
        var msgName = TestAllTypes.Descriptor.FullName;
        pbdb.RegisterMessage(msg);
        var td = pbdb.DescribeType(msgName);
        Assert.That(td, Is.Not.Null);

        var f = td.FieldByName(tc.field);
        Assert.That(f, Is.Not.Null);
        var got = f.GetFrom(pbdb, msg);
        Assert.That(got, Is.EqualTo(tc.want));
    }

    [TestCaseSource(nameof(SetTestCases))]
    public virtual void IsSet(TestCase tc)
    {
        var pbdb = Db.NewDb();
        var msg = new TestAllTypes();
        var msgName = TestAllTypes.Descriptor.FullName;
        pbdb.RegisterMessage(msg);
        var td = pbdb.DescribeType(msgName);
        Assert.That(td, Is.Not.Null);

        var f = td.FieldByName(tc.field);
        Assert.That(f, Is.Not.Null);

        Assert.That(f.IsSet(tc.msg), Is.EqualTo(tc.isSet));
    }

    public class GetFromTestCase
    {
        internal string? field;
        internal object? want;

        internal virtual GetFromTestCase Field(string field)
        {
            this.field = field;
            return this;
        }

        internal virtual GetFromTestCase Want(object want)
        {
            this.want = want;
            return this;
        }

        public override string ToString()
        {
            return field!;
        }
    }

    public class TestCase
    {
        internal string? field;
        internal bool isSet;
        internal Message? msg;

        internal virtual TestCase Msg(Message? msg)
        {
            this.msg = msg;
            return this;
        }

        internal virtual TestCase Field(string field)
        {
            this.field = field;
            return this;
        }

        internal virtual TestCase IsSet(bool set)
        {
            isSet = set;
            return this;
        }

        public override string ToString()
        {
            return (msg != null ? msg.Descriptor.ToString() + '.' : "null") + field + " " + isSet;
        }
    }
}