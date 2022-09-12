using Cel.Common.Types.Ref;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Extensions;
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

public class TimestampTest
{
    internal const int numTimeZones = 5;
    internal const int numOffsets = 5;
    internal const int numDateTimes = 10;

    [Test]
    public virtual void TimestampAdd()
    {
        var ts = DefaultTS();
        var val = ts.Add(DurationT.DurationOf(Period.FromNanoseconds(3600L * 1000000000 + 1000000)));
        Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(TimestampT.TimestampType));
        var expected =
            TimestampT.TimestampOf(Instant.FromUnixTimeMilliseconds(11106 * 1000 + 1).InZone(TimestampT.ZoneIdZ));
        Assert.That(expected.Compare(val), Is.SameAs(IntT.IntZero));
        Assert.That(ts.Add(expected), Is.InstanceOf(typeof(Err)));

        var i1 = Instant.FromUnixTimeMilliseconds(TimestampT.maxUnixTime);
        var max999 = i1.PlusNanoseconds(999999999).InZone(TimestampT.ZoneIdZ);
        var i2 = Instant.FromUnixTimeMilliseconds(TimestampT.maxUnixTime);
        var max998 = i2.PlusNanoseconds(999999998).InZone(TimestampT.ZoneIdZ);
        var i3 = Instant.FromUnixTimeMilliseconds(TimestampT.minUnixTime);
        var min0 = i3.PlusNanoseconds(0).InZone(TimestampT.ZoneIdZ);
        var i4 = Instant.FromUnixTimeMilliseconds(TimestampT.minUnixTime);
        var min1 = i4.PlusNanoseconds(1).InZone(TimestampT.ZoneIdZ);

        // TODO remove
        /*
        Assert.That(TimestampT.TimestampOf(max999).Add(DurationT.DurationOf(Period.FromNanoseconds(1))),
            Is.SameAs(Err.ErrDurationOverflow));
        Assert.That(TimestampT.TimestampOf(min0).Add(DurationT.DurationOf(Period.FromNanoseconds(-1))),
            Is.SameAs(Err.ErrDurationOverflow));
        */

        Assert.That(
            TimestampT.TimestampOf(max999).Add(DurationT.DurationOf(Period.FromNanoseconds(-1)))
                .Equal(TimestampT.TimestampOf(max998)), Is.SameAs(BoolT.True));
        Assert.That(
            TimestampT.TimestampOf(min0).Add(DurationT.DurationOf(Period.FromNanoseconds(1)))
                .Equal(TimestampT.TimestampOf(min1)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void TimestampConvertToNativeAny()
    {
        // 1970-01-01T02:05:06Z
        var ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
        var val = (Any)ts.ConvertToNative(typeof(Any));
        var ts2 = new Timestamp();
        ts2.Seconds = 7506;
        var want = Any.Pack(ts2);
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void TimestampConvertToNativeJSON()
    {
        var ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));

        // JSON
        var val = ts.ConvertToNative(typeof(Value));
        var want = new StringValue();
        want.Value = "1970-01-01T02:05:06Z";
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void TimestampConvertToNative()
    {
        // 1970-01-01T02:05:06Z
        var ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));

        var val = ts.ConvertToNative(typeof(Timestamp));
        Timestamp ts2;
        ts2 = new Timestamp();
        ts2.Seconds = 7506;
        Assert.That(val, Is.EqualTo(ts2));

        val = ts.ConvertToNative(typeof(Any));
        var any = Any.Pack(ts2);
        Assert.That(val, Is.EqualTo(any));

        val = ts.ConvertToNative(typeof(ZonedDateTime));
        Assert.That(val, Is.EqualTo(ts.Value()));

        val = ts.ConvertToNative(typeof(DateTime));
        var dt = DateTimeOffset.FromUnixTimeSeconds(7506).DateTime;
        Assert.That(val, Is.EqualTo(dt));
    }

    [Test]
    public virtual void TimestampSubtract()
    {
        var ts = DefaultTS();
        var periodBuilder = new PeriodBuilder();
        periodBuilder.Seconds = 3600;
        periodBuilder.Nanoseconds = 1000;
        var val = ts.Subtract(DurationT.DurationOf(periodBuilder.Build()));
        Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(TimestampT.TimestampType));

        var i1 = Instant.FromUnixTimeSeconds(3905);
        i1 = i1.PlusNanoseconds(999999000);
        var i2 = Instant.FromUnixTimeSeconds(6506);
        var expected =
            TimestampT.TimestampOf(i1.InZone(TimestampT.ZoneIdZ));
        Assert.That(expected.Compare(val), Is.SameAs(IntT.IntZero));
        var ts2 = TimestampT.TimestampOf(i2.InZone(TimestampT.ZoneIdZ));
        val = ts.Subtract(ts2);
        Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(DurationT.DurationType));

        var expectedDur = DurationT.DurationOf(Period.FromNanoseconds(1000000000000L));
        Assert.That(expectedDur.Compare(val), Is.SameAs(IntT.IntZero));
    }

    [Test]
    public virtual void TimestampGetDayOfYear()
    {
        // 1970-01-01T02:05:06Z
        var ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
        var hr = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYear);
        Assert.That(hr, Is.SameAs(IntT.IntZero));
        // 1969-12-31T19:05:06Z
        var hrTz = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYearWithTz,
            StringT.StringOf("America/Phoenix"));
        Assert.That(hrTz.Equal(IntT.IntOf(364)), Is.SameAs(BoolT.True));
        hrTz = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYearWithTz,
            StringT.StringOf("-07:00"));
        Assert.That(hrTz.Equal(IntT.IntOf(364)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void TimestampGetMonth()
    {
        // 1970-01-01T02:05:06Z
        var ts = DefaultTS();
        var hr = ts.Receive(Overloads.TimeGetMonth, Overloads.TimestampToMonth);
        Assert.That(hr.Value(), Is.EqualTo(0L));
        // 1969-12-31T19:05:06Z
        var hrTz = ts.Receive(Overloads.TimeGetMonth, Overloads.TimestampToMonthWithTz,
            StringT.StringOf("America/Phoenix"));
        Assert.That(hrTz.Value(), Is.EqualTo(11L));
    }

    [Test]
    public virtual void TimestampGetHours()
    {
        // 1970-01-01T02:05:06Z
        var ts = DefaultTS();
        var hr = ts.Receive(Overloads.TimeGetHours, Overloads.TimestampToHours);
        Assert.That(hr.Value(), Is.EqualTo(2L));
        // 1969-12-31T19:05:06Z
        var hrTz = ts.Receive(Overloads.TimeGetHours, Overloads.TimestampToHoursWithTz,
            StringT.StringOf("America/Phoenix"));
        Assert.That(hrTz.Value(), Is.EqualTo(19L));
    }

    [Test]
    public virtual void TimestampGetMinutes()
    {
        // 1970-01-01T02:05:06Z
        var ts = DefaultTS();
        var min = ts.Receive(Overloads.TimeGetMinutes, Overloads.TimestampToMinutes);
        Assert.That(min.Equal(IntT.IntOf(5)), Is.SameAs(BoolT.True));
        // 1969-12-31T19:05:06Z
        var minTz = ts.Receive(Overloads.TimeGetMinutes, Overloads.TimestampToMinutesWithTz,
            StringT.StringOf("America/Phoenix"));
        Assert.That(minTz.Equal(IntT.IntOf(5)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void TimestampGetSeconds()
    {
        // 1970-01-01T02:05:06Z
        var ts = DefaultTS();
        var sec = ts.Receive(Overloads.TimeGetSeconds, Overloads.TimestampToSeconds);
        Assert.That(sec.Equal(IntT.IntOf(6)), Is.SameAs(BoolT.True));
        // 1969-12-31T19:05:06Z
        var secTz = ts.Receive(Overloads.TimeGetSeconds, Overloads.TimestampToSecondsWithTz,
            StringT.StringOf("America/Phoenix"));
        Assert.That(secTz.Equal(IntT.IntOf(6)), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void ParseTimezone()
    {
        var zoneUTC = DateTimeZone.Utc;

        Assert.That(new List<DateTimeZone>
            {
                TimestampT.ParseTz("-0"), TimestampT.ParseTz("+0"), TimestampT.ParseTz("0"),
                TimestampT.ParseTz("-00"), TimestampT.ParseTz("+00"), TimestampT.ParseTz("00"),
                TimestampT.ParseTz("-0:0"), TimestampT.ParseTz("+0:0:0"), TimestampT.ParseTz("0:0:0"),
                TimestampT.ParseTz("-00:0:0"), TimestampT.ParseTz("+0:00:0"), TimestampT.ParseTz("+00:00:0")
            },
            Is.EquivalentTo(new List<DateTimeZone>
            {
                zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC,
                zoneUTC,
                zoneUTC, zoneUTC
            }));

        Assert.That(() => TimestampT.ParseTz("+19"), Throws.Exception);
        Assert.That(() => TimestampT.ParseTz("+1:61"), Throws.Exception);
        Assert.That(() => TimestampT.ParseTz("+1:60:0"), Throws.Exception);
        Assert.That(() => TimestampT.ParseTz("+1:1:60"), Throws.Exception);

        var o1 = Offset.FromHoursAndMinutes(-1, -2);
        o1 = o1.Plus(Offset.FromSeconds(-30));
        var z1 = DateTimeZone.ForOffset(o1);
        var o2 = Offset.FromHoursAndMinutes(0, 1);
        o2 = o2.Plus(Offset.FromSeconds(3));
        var z2 = DateTimeZone.ForOffset(o2);
        Assert
            .That(new List<DateTimeZone>
                {
                    TimestampT.ParseTz("-1"), TimestampT.ParseTz("+1"), TimestampT.ParseTz("1"),
                    TimestampT.ParseTz("-01:2:30"), TimestampT.ParseTz("+02:30"), TimestampT.ParseTz("0:1:3")
                },
                Is.EquivalentTo(new List<DateTimeZone>
                {
                    DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(-1, 0)),
                    DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(1, 0)),
                    DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(1, 0)),
                    z1,
                    DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(2, 30)),
                    z2
                }));
    }

    internal static IList<ParseTestCase> TimestampParsingTestCases()
    {
        var rand = new Random();

        IList<ParseTestCase> testCases = new List<ParseTestCase>(numDateTimes * numTimeZones);

        testCases.Add(
            new ParseTestCase("2009-02-13T23:31:30Z", new[] { 2009, 2, 13, 23, 31, 30 }).WithTZ(
                "Australia/Sydney"));
        testCases.Add(
            new ParseTestCase("2009-02-13T23:31:30Z", new[] { 2009, 2, 13, 23, 31, 30 }).WithTZ("+11:00"));
        // TODO remove
        // time-zones unknown to ZoneId
        /*
        testCases.Add(
            (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ("CST"));
        testCases.Add(
            (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ("MIT"));
        */

        // Collect a couple of random time zones and date-times.
        IList<DateTimeZone> availableTimeZones =
            DateTimeZoneProviders.Tzdb.GetAllZones().ToList();

        for (var j = 0; j < numDateTimes; j++)
        {
            var year = rand.Next(1970, 2200);
            var month = rand.Next(1, 13);
            var day = rand.Next(1, 28);
            var hour = rand.Next(0, 24);
            var minute = rand.Next(0, 60);
            var second = rand.Next(0, 60);
            var dateTime = string.Format("{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}Z", year, month, day, hour,
                minute, second);
            int[] tuple = { year, month, day, hour, minute, second };

            var noTzTestCase = new ParseTestCase(dateTime, tuple);

            for (var i = 0; i < numTimeZones; i++)
            {
                var index = rand.Next(0, availableTimeZones.Count);
                var tz = availableTimeZones[index];
                availableTimeZones.RemoveAt(index);
                testCases.Add(noTzTestCase.WithTZ(tz.Id));
            }

            for (var i = 0; i < numOffsets; i++)
            {
                var offsetHours = rand.Next(-18, 19);
                var id = string.Format("{0:+#;-#;+0}:{1:D2}", offsetHours, 0);
                testCases.Add(noTzTestCase.WithTZ(id));
            }
        }

        return testCases;
    }

    [TestCaseSource(nameof(TimestampParsingTestCases))]
    public virtual void TimestampParsing(ParseTestCase tc)
    {
        var ts = StringT.StringOf(tc.timestamp).ConvertToType(TimestampT.TimestampType);

        var value = (ZonedDateTime)ts.Value();
        var dtZlocal = value.ToDateTimeUnspecified();

        // Verify that the values in ParseTestCase are fine

        Assert.That(tc.tuple, Is.EquivalentTo(new List<int>
        {
            dtZlocal.Year, dtZlocal.Month, dtZlocal.Day, dtZlocal.Hour,
            dtZlocal.Minute, dtZlocal.Second
        }));

        // check the timestampGetXyz methods (without a time-zone), (assuming UTC)

        Assert.That(new List<IVal>
        {
            TimestampT.TimestampGetFullYear(value), TimestampT.TimestampGetMonth(value),
            TimestampT.TimestampGetDayOfMonthOneBased(value),
            TimestampT.TimestampGetDayOfMonthZeroBased(value),
            TimestampT.TimestampGetHours(value), TimestampT.TimestampGetMinutes(value),
            TimestampT.TimestampGetSeconds(value), TimestampT.TimestampGetDayOfWeek(value),
            TimestampT.TimestampGetDayOfYear(value)
        }, Is.EquivalentTo(new List<IVal>
        {
            IntT.IntOf(dtZlocal.Year),
            IntT.IntOf(dtZlocal.Month - 1),
            IntT.IntOf(dtZlocal.Day), IntT.IntOf(dtZlocal.Day - 1),
            IntT.IntOf(dtZlocal.Hour), IntT.IntOf(dtZlocal.Minute), IntT.IntOf(dtZlocal.Second),
            IntT.IntOf(dayOfWeekToIso(dtZlocal.DayOfWeek)), IntT.IntOf(dtZlocal.DayOfYear - 1)
        }));

        // check the timestampGetXyzWithTu methods (with a time-zone)

        var zoneId = TimestampT.ParseTz(tc.tz);

        var atZone = new ZonedDateTime(value.ToInstant(), zoneId);
        IVal tzVal = StringT.StringOf(tc.tz);

        Assert.That(new List<IVal>
            {
                TimestampT.TimestampGetFullYearWithTz(value, tzVal),
                TimestampT.TimestampGetMonthWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfMonthOneBasedWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfMonthZeroBasedWithTz(value, tzVal),
                TimestampT.TimestampGetHoursWithTz(value, tzVal), TimestampT.TimestampGetMinutesWithTz(value, tzVal),
                TimestampT.TimestampGetSecondsWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfWeekWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfYearWithTz(value, tzVal)
            },
            Is.EquivalentTo(new List<IVal>
            {
                IntT.IntOf(atZone.Year),
                IntT.IntOf(atZone.Month - 1),
                IntT.IntOf(atZone.Day),
                IntT.IntOf(atZone.Day - 1),
                IntT.IntOf(atZone.Hour), IntT.IntOf(atZone.Minute),
                IntT.IntOf(atZone.Second), IntT.IntOf((int)atZone.DayOfWeek),
                IntT.IntOf(atZone.DayOfYear - 1)
            }));
    }

    private int dayOfWeekToIso(DayOfWeek day)
    {
        var i = (int)day;
        if (i == 0) i = 7;
        return i;
    }

    private TimestampT DefaultTS()
    {
        return TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
    }

    [Test]
    public virtual void NanoMicroMilliPrecision()
    {
        var secondsEpoch = 1624006650L;
        var ts0 = "2021-06-18T08:57:30Z";
        var nano3 = 123000000;
        var ts3 = "2021-06-18T08:57:30.123Z";
        var nano4 = 123400000;
        var ts4 = "2021-06-18T08:57:30.1234Z";
        var nano6 = 123456000;
        var ts6 = "2021-06-18T08:57:30.123456Z";
        var nano9 = 123456789;
        var ts9 = "2021-06-18T08:57:30.123456789Z";

        var z = TimestampT.TimestampOf(ts0).Value();
        Assert.That(((ZonedDateTime)z).ToInstant().ToUnixTimeSeconds(), Is.EqualTo(secondsEpoch));
        Assert.That(((ZonedDateTime)z).NanosecondOfSecond, Is.EqualTo(0));

        z = TimestampT.TimestampOf(ts3).Value();
        Assert.That(((ZonedDateTime)z).ToInstant().ToUnixTimeSeconds(), Is.EqualTo(secondsEpoch));
        Assert.That(((ZonedDateTime)z).NanosecondOfSecond, Is.EqualTo(nano3));

        z = TimestampT.TimestampOf(ts4).Value();
        Assert.That(((ZonedDateTime)z).ToInstant().ToUnixTimeSeconds(), Is.EqualTo(secondsEpoch));
        Assert.That(((ZonedDateTime)z).NanosecondOfSecond, Is.EqualTo(nano4));

        z = TimestampT.TimestampOf(ts6).Value();
        Assert.That(((ZonedDateTime)z).ToInstant().ToUnixTimeSeconds(), Is.EqualTo(secondsEpoch));
        Assert.That(((ZonedDateTime)z).NanosecondOfSecond, Is.EqualTo(nano6));

        z = TimestampT.TimestampOf(ts9).Value();
        Assert.That(((ZonedDateTime)z).ToInstant().ToUnixTimeSeconds(), Is.EqualTo(secondsEpoch));
        Assert.That(((ZonedDateTime)z).NanosecondOfSecond, Is.EqualTo(nano9));
    }

    public class ParseTestCase
    {
        internal readonly DateTime ldt;
        internal readonly string timestamp;
        internal readonly int[] tuple;
        internal string? tz;

        internal ParseTestCase(string timestamp, int[] tuple)
        {
            this.timestamp = timestamp;
            this.tuple = tuple;
            ldt = new DateTime(tuple[0], tuple[1], tuple[2], tuple[3], tuple[4], tuple[5]);
        }

        internal virtual ParseTestCase WithTZ(string tz)
        {
            var copy = new ParseTestCase(timestamp, tuple);
            copy.tz = tz;
            return copy;
        }

        public override string ToString()
        {
            return "timestamp='" + timestamp + '\'' + ", tz='" + tz + '\'';
        }
    }
}