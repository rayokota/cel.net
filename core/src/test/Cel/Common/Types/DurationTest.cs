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
using Duration = Google.Protobuf.WellKnownTypes.Duration;

namespace Cel.Common.Types;

public class DurationTest
{
    [Test]
    public virtual void DurationAdd()
    {
        var dur = Period.FromSeconds(7506);
        var d = DurationT.DurationOf(dur);

        Assert.That(d.Add(d).Equal(DurationT.DurationOf(Period.FromSeconds(15012))), Is.SameAs(BoolT.True));

        Assert.That(
            Err.IsError(DurationT.DurationOf(Period.FromSeconds(long.MaxValue))
                .Add(DurationT.DurationOf(Period.FromSeconds(1L)))), Is.True);

        Assert.That(
            Err.IsError(DurationT.DurationOf(Period.FromSeconds(long.MinValue))
                .Add(DurationT.DurationOf(Period.FromSeconds(-1L)))), Is.True);

        Assert.That(
            DurationT.DurationOf(Period.FromSeconds(long.MaxValue - 1))
                .Add(DurationT.DurationOf(Period.FromSeconds(1L)))
                .Equal(DurationT.DurationOf(Period.FromSeconds(long.MaxValue))), Is.SameAs(BoolT.True));

        Assert.That(
            DurationT.DurationOf(Period.FromSeconds(long.MinValue + 1))
                .Add(DurationT.DurationOf(Period.FromSeconds(-1L)))
                .Equal(DurationT.DurationOf(Period.FromSeconds(long.MinValue))), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationCompare()
    {
        var d = DurationT.DurationOf(Period.FromSeconds(7506));
        var lt = DurationT.DurationOf(Period.FromSeconds(-10));
        Assert.That(d.Compare(lt), Is.SameAs(IntT.IntOne));
        Assert.That(lt.Compare(d), Is.SameAs(IntT.IntNegOne));
        Assert.That(d.Compare(d), Is.SameAs(IntT.IntZero));
        Assert.That(Err.IsError(d.Compare(BoolT.False)), Is.True);
    }

    [Test]
    public virtual void DurationConvertToNative()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 1000;
        var dur = DurationT.DurationOf(periodBuilder.Build());

        var val = (Duration)dur.ConvertToNative(typeof(Duration));
        var want = new Duration();
        want.Seconds = 7506;
        want.Nanos = 1000;
        Assert.That(val, Is.EqualTo(want));

        Assert.That(val.Seconds, Is.EqualTo(((Period)dur.Value()).Seconds));
        Assert.That(val.Nanos, Is.EqualTo(((Period)dur.Value()).Nanoseconds));

        var valD = (Period)dur.ConvertToNative(typeof(Period));
        Assert.That(valD, Is.EqualTo(dur.Value()));
    }

    [Test]
    public virtual void DurationConvertToNativeAny()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 1000;
        var d = DurationT.DurationOf(periodBuilder.Build());
        var val = (Any)d.ConvertToNative(typeof(Any));
        var du = new Duration();
        du.Seconds = 7506;
        du.Nanos = 1000;
        var want = Any.Pack(du);
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void DurationConvertToNativeError()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 1000;
        var val = (Value)DurationT.DurationOf(periodBuilder.Build()).ConvertToNative(typeof(Value));
        var want = new Value();
        want.StringValue = "7506.000001s";
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void DurationConvertToNativeJson()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 1000;
        var val = (Value)DurationT.DurationOf(periodBuilder.Build()).ConvertToNative(typeof(Value));
        var want = new Value();
        want.StringValue = "7506.000001s";
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void DurationConvertToTypeIdentity()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 1000;
        var d = DurationT.DurationOf(periodBuilder.Build());

        Assert.That(d.ConvertToType(IntT.IntType).Value(), Is.EqualTo(7506000001000L));
        Assert.That(d.ConvertToType(DurationT.DurationType).Equal(d), Is.SameAs(BoolT.True));
        Assert.That(d.ConvertToType(TypeT.TypeType), Is.SameAs(DurationT.DurationType));
        Assert.That(Err.IsError(d.ConvertToType(UintT.UintType)), Is.True);
        Assert.That(d.ConvertToType(StringT.StringType).Value().ToString(), Is.EqualTo("7506.000001s"));
    }

    [Test]
    public virtual void DurationNegate()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 1234;
        periodBuilder.Nanoseconds = 1;
        var neg = DurationT.DurationOf(periodBuilder.Build()).Negate();
        periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = -1234;
        periodBuilder.Nanoseconds = -1;
        Assert.That(neg.Value(), Is.EqualTo(periodBuilder.Build()));
        Assert.That(Err.IsError(DurationT.DurationOf(Period.FromSeconds(long.MinValue)).Negate()), Is.True);
        Assert.That(
            DurationT.DurationOf(Period.FromSeconds(long.MaxValue)).Negate()
                .Equal(DurationT.DurationOf(Period.FromSeconds(long.MinValue + 1))), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationGetHours()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 0;
        var d = DurationT.DurationOf(periodBuilder.Build());
        var hr = d.Receive(Overloads.TimeGetHours, Overloads.DurationToHours);
        Assert.That(hr.Equal(IntT.IntOf(2)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationGetMinutes()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 0;
        var d = DurationT.DurationOf(periodBuilder.Build());
        var min = d.Receive(Overloads.TimeGetMinutes, Overloads.DurationToMinutes);
        Assert.That(min.Equal(IntT.IntOf(125)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationGetSeconds()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 0;
        var d = DurationT.DurationOf(periodBuilder.Build());
        var sec = d.Receive(Overloads.TimeGetSeconds, Overloads.DurationToSeconds);
        Assert.That(sec.Equal(IntT.IntOf(7506)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationGetMilliseconds()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 0;
        var d = DurationT.DurationOf(periodBuilder.Build());
        var min = d.Receive(Overloads.TimeGetMilliseconds, Overloads.DurationToMilliseconds);
        Assert.That(min.Equal(IntT.IntOf(7506000)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void DurationSubtract()
    {
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 7506;
        periodBuilder.Nanoseconds = 0;
        var d = DurationT.DurationOf(periodBuilder.Build());

        Assert.That(d.Subtract(d).ConvertToType(IntT.IntType).Equal(IntT.IntZero), Is.SameAs(BoolT.True));

        Assert.That(Err.IsError(DurationT.DurationOf(Period.FromSeconds(long.MaxValue))
            .Subtract(DurationT.DurationOf(Period.FromSeconds(-1L)))), Is.True);

        Assert.That(Err.IsError(DurationT.DurationOf(Period.FromSeconds(long.MinValue))
            .Subtract(DurationT.DurationOf(Period.FromSeconds(1L)))), Is.True);

        Assert.That(
            DurationT.DurationOf(Period.FromSeconds(long.MaxValue - 1))
                .Subtract(DurationT.DurationOf(Period.FromSeconds(-1L)))
                .Equal(DurationT.DurationOf(Period.FromSeconds(long.MaxValue))), Is.SameAs(BoolT.True));

        Assert.That(
            DurationT.DurationOf(Period.FromSeconds(long.MinValue + 1))
                .Subtract(DurationT.DurationOf(Period.FromSeconds(1L)))
                .Equal(DurationT.DurationOf(Period.FromSeconds(long.MinValue))), Is.SameAs(BoolT.True));
    }
}