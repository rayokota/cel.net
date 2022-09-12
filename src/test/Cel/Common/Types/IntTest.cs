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

using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;

namespace Cel.Common.Types;

public class IntTest
{
    [Test]
    public virtual void IntAdd()
    {
        Assert.That(IntT.IntOf(4).Add(IntT.IntOf(-3)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(-1).Add(StringT.StringOf("-1")), Is.InstanceOf(typeof(Err)));
        for (var i = 1; i <= 10; i++)
        {
            Assert.That(IntT.IntOf(long.MaxValue).Add(IntT.IntOf(i)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MinValue).Add(IntT.IntOf(-i)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MaxValue - i).Add(IntT.IntOf(i)), Is.EqualTo(IntT.IntOf(long.MaxValue)));
            Assert.That(IntT.IntOf(long.MinValue + i).Add(IntT.IntOf(-i)), Is.EqualTo(IntT.IntOf(long.MinValue)));
        }
    }

    [Test]
    public virtual void IntCompare()
    {
        var lt = IntT.IntOf(-1300);
        var gt = IntT.IntOf(204);
        Assert.That(lt.Compare(gt), Is.SameAs(IntT.IntNegOne));
        Assert.That(gt.Compare(lt), Is.SameAs(IntT.IntOne));
        Assert.That(gt.Compare(gt), Is.SameAs(IntT.IntZero));
        Assert.That(gt.Compare(TypeT.TypeType), Is.InstanceOf(typeof(Err)));
    }

    [Test]
    public virtual void IntConvertToNativeAny()
    {
        var val = (Any)IntT.IntOf(long.MaxValue).ConvertToNative(typeof(Any));
        var v = new Int64Value();
        v.Value = long.MaxValue;
        var want = Any.Pack(v);
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void IntConvertToNativeError()
    {
        var val = (Value)IntT.IntOf(1).ConvertToNative(typeof(Value));
        //          		if err == nil {
        //          			t.Errorf("Got '%v', expected error", val)
        //          		}
    }

    [Test]
    public virtual void IntConvertToNativeInt32()
    {
        var val = (int)IntT.IntOf(20050).ConvertToNative(typeof(int));
        Assert.That(val, Is.EqualTo(20050));
    }

    [Test]
    public virtual void IntConvertToNativeInt64()
    {
        // Value greater than max int32.
        var val = (long)IntT.IntOf(4147483648L).ConvertToNative(typeof(long));
        Assert.That(val, Is.EqualTo(4147483648L));
    }

    [Test]
    public virtual void IntConvertToNativeJson()
    {
        // Value can be represented accurately as a JSON number.
        var val = (Value)IntT.IntOf(IntT.MaxIntJson).ConvertToNative(typeof(Value));
        var v = new Value();
        v.NumberValue = 9007199254740991.0;
        Assert.That(val, Is.EqualTo(v));

        // Value converts to a JSON decimal string.
        val = (Value)IntT.IntOf(IntT.MaxIntJson + 1).ConvertToNative(typeof(Value));
        v = new Value();
        v.StringValue = "9007199254740992";
        Assert.That(val, Is.EqualTo(v));
    }

    [Test]
    public virtual void IntConvertToNativePtrInt32()
    {
        var val = (int)IntT.IntOf(20050).ConvertToNative(typeof(int));
        Assert.That(val, Is.EqualTo(20050));
    }

    [Test]
    public virtual void IntConvertToNativePtrInt64()
    {
        // Value greater than max int32.
        var val = (long)IntT.IntOf(1L + int.MaxValue).ConvertToNative(typeof(long));
        Assert.That(val, Is.EqualTo(1L + int.MaxValue));
    }

    [Test]
    public virtual void IntConvertToNativeWrapper()
    {
        var val = (int)IntT.IntOf(int.MaxValue).ConvertToNative(typeof(Int32Value));
        var want = int.MaxValue;
        Assert.That(val, Is.EqualTo(want));

        var val2 = (long)IntT.IntOf(long.MinValue).ConvertToNative(typeof(Int64Value));
        var want2 = long.MinValue;
        Assert.That(val2, Is.EqualTo(want2));
    }

    [Test]
    public virtual void IntConvertToType()
    {
        Assert.That(IntT.IntOf(-4).ConvertToType(IntT.IntType).Equal(IntT.IntOf(-4)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(-1).ConvertToType(UintT.UintType), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(-4).ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(-4)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(-4).ConvertToType(StringT.StringType).Equal(StringT.StringOf("-4")),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(-4).ConvertToType(TypeT.TypeType), Is.SameAs(IntT.IntType));
        Assert.That(IntT.IntOf(-4).ConvertToType(DurationT.DurationType), Is.InstanceOf(typeof(Err)));
        var celtsSecs = 946684800;
        var celts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(celtsSecs).InZone(TimestampT.ZoneIdZ));
        Assert.That(IntT.IntOf(celtsSecs).ConvertToType(TimestampT.TimestampType).Equal(celts), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void IntDivide()
    {
        Assert.That(IntT.IntOf(3).Divide(IntT.IntOf(2)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntZero.Divide(IntT.IntZero), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(1).Divide(DoubleT.DoubleOf(-1)), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(long.MinValue).Divide(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
    }

    [Test]
    public virtual void IntEqual()
    {
        Assert.That(IntT.IntOf(0).Equal(BoolT.False), Is.InstanceOf(typeof(Err)));
    }

    [Test]
    public virtual void IntModulo()
    {
        Assert.That(IntT.IntOf(21).Modulo(IntT.IntOf(2)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(21).Modulo(IntT.IntZero), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(21).Modulo(UintT.UintZero), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(long.MinValue).Modulo(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
    }

    [Test]
    public virtual void IntMultiply()
    {
        Assert.That(IntT.IntOf(2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(-4)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(1).Multiply(DoubleT.DoubleOf(-4.0)), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(3)), Is.SameAs(Err.ErrIntOverflow));
        Assert.That(IntT.IntOf(long.MinValue / 2).Multiply(IntT.IntOf(3)), Is.SameAs(Err.ErrIntOverflow));
        Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(2)).Equal(IntT.IntOf(long.MaxValue - 1)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(long.MinValue / 2).Multiply(IntT.IntOf(2)).Equal(IntT.IntOf(long.MinValue)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(long.MinValue + 2)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf((long.MinValue + 2) / 2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(long.MaxValue - 1)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(long.MinValue).Multiply(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
    }

    [Test]
    public virtual void IntNegate()
    {
        Assert.That(IntT.IntOf(1).Negate().Equal(IntT.IntOf(-1)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(long.MinValue).Negate(), Is.SameAs(Err.ErrIntOverflow));
        Assert.That(IntT.IntOf(long.MaxValue).Negate().Equal(IntT.IntOf(long.MinValue + 1)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void IntSubtract()
    {
        Assert.That(IntT.IntOf(4).Subtract(IntT.IntOf(-3)).Equal(IntT.IntOf(7)), Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(1).Subtract(UintT.UintOf(1)), Is.InstanceOf(typeof(Err)));
        Assert.That(IntT.IntOf(long.MaxValue).Subtract(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
        Assert.That(IntT.IntOf(long.MinValue).Subtract(IntT.IntOf(1)), Is.SameAs(Err.ErrIntOverflow));
        Assert.That(IntT.IntOf(long.MaxValue - 1).Subtract(IntT.IntOf(-1)).Equal(IntT.IntOf(long.MaxValue)),
            Is.SameAs(BoolT.True));
        Assert.That(IntT.IntOf(long.MinValue + 1).Subtract(IntT.IntOf(1)).Equal(IntT.IntOf(long.MinValue)),
            Is.SameAs(BoolT.True));
    }
}