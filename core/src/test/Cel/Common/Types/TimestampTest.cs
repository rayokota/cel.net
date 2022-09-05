using System;
using System.Collections.Generic;
using Cel.Common.Types.Ref;
using NodaTime;
using NodaTime.Extensions;
using NUnit.Framework;

/*
 * Copyright (C) 2021 The Authors of CEL-Java
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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.That;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.ThatThrownBy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.Err.ErrDurationOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.TimestampType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.ZoneIdZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.MaxUnixTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.MinUnixTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.ParseTz;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.TimestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TypeT.TypeT.TypeType;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using Value = Google.Protobuf.WellKnownTypes.Value;

    public class TimestampTest
    {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampAdd()
[Test]
        public virtual void TimestampAdd()
        {
            TimestampT ts = DefaultTS();
            Ref.Val val = ts.Add(DurationT.DurationOf(Period.FromNanoseconds(3600L * 1000000000 + 1000000)));
            Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(TimestampT.TimestampType));
            TimestampT expected =
                TimestampT.TimestampOf(Instant.FromUnixTimeMilliseconds(11106 * 1000 + 1).InZone(TimestampT.ZoneIdZ));
            Assert.That(expected.Compare(val), Is.SameAs(IntT.IntZero));
            Assert.That(ts.Add(expected), Is.InstanceOf(typeof(Err)));

            Instant i1 = Instant.FromUnixTimeMilliseconds(TimestampT.maxUnixTime);
            ZonedDateTime max999 = i1.PlusNanoseconds(999999999).InZone(TimestampT.ZoneIdZ);
            Instant i2 = Instant.FromUnixTimeMilliseconds(TimestampT.maxUnixTime);
            ZonedDateTime max998 = i2.PlusNanoseconds(999999998).InZone(TimestampT.ZoneIdZ);
            Instant i3 = Instant.FromUnixTimeMilliseconds(TimestampT.minUnixTime);
            ZonedDateTime min0 = i3.PlusNanoseconds(0).InZone(TimestampT.ZoneIdZ);
            Instant i4 = Instant.FromUnixTimeMilliseconds(TimestampT.minUnixTime);
            ZonedDateTime min1 = i4.PlusNanoseconds(1).InZone(TimestampT.ZoneIdZ);

            // TODO ?
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampConvertToNative_Any()
[Test]
        public virtual void TimestampConvertToNativeAny()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
            Any val = (Any)ts.ConvertToNative(typeof(Any));
            Timestamp ts2 = new Timestamp();
            ts2.Seconds = 7506;
            Any want = Any.Pack(ts2);
            Assert.That(val, Is.EqualTo(want));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampConvertToNative_JSON()
[Test]
        public virtual void TimestampConvertToNativeJSON()
        {
            TimestampT ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));

            // JSON
            object val = ts.ConvertToNative(typeof(Value));
            StringValue want = new StringValue();
            want.Value = "1970-01-01T02:05:06Z";
            Assert.That(val, Is.EqualTo(want));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampConvertToNative()
[Test]
        public virtual void TimestampConvertToNative()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));

            object val = ts.ConvertToNative(typeof(Timestamp));
            Timestamp ts2;
            ts2 = new Timestamp();
            ts2.Seconds = 7506;
            Assert.That(val, Is.EqualTo(ts2));

            val = ts.ConvertToNative(typeof(Any));
            Any any = Any.Pack(ts2);
            Assert.That(val, Is.EqualTo(any));

            val = ts.ConvertToNative(typeof(ZonedDateTime));
            Assert.That(val, Is.EqualTo(ts.Value()));

            val = ts.ConvertToNative(typeof(DateTime));
            DateTime dt = DateTimeOffset.FromUnixTimeSeconds(7506).DateTime;
            Assert.That(val, Is.EqualTo(dt));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampSubtract()
[Test]
        public virtual void TimestampSubtract()
        {
            TimestampT ts = DefaultTS();
            PeriodBuilder periodBuilder = new PeriodBuilder();
            periodBuilder.Seconds = 3600;
            periodBuilder.Nanoseconds = 1000;
            Val val = ts.Subtract(DurationT.DurationOf(periodBuilder.Build()));
            Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(TimestampT.TimestampType));

            Instant i1 = Instant.FromUnixTimeSeconds(3905);
            i1 = i1.PlusNanoseconds(999999000);
            Instant i2 = Instant.FromUnixTimeSeconds(6506);
            TimestampT expected =
                TimestampT.TimestampOf(i1.InZone(TimestampT.ZoneIdZ));
            Assert.That(expected.Compare(val), Is.SameAs(IntT.IntZero));
            TimestampT ts2 = TimestampT.TimestampOf(i2.InZone(TimestampT.ZoneIdZ));
            val = ts.Subtract(ts2);
            Assert.That(val.ConvertToType(TypeT.TypeType), Is.SameAs(DurationT.DurationType));

            DurationT expectedDur = DurationT.DurationOf(Period.FromNanoseconds(1000000000000L));
            Assert.That(expectedDur.Compare(val), Is.SameAs(IntT.IntZero));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampGetDayOfYear()
[Test]
        public virtual void TimestampGetDayOfYear()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
            Val hr = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYear);
            Assert.That(hr, Is.SameAs(IntT.IntZero));
            // 1969-12-31T19:05:06Z
            Val hrTz = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYearWithTz,
                StringT.StringOf("America/Phoenix"));
            Assert.That(hrTz.Equal(IntT.IntOf(364)), Is.SameAs(BoolT.True));
            hrTz = ts.Receive(Overloads.TimeGetDayOfYear, Overloads.TimestampToDayOfYearWithTz,
                StringT.StringOf("-07:00"));
            Assert.That(hrTz.Equal(IntT.IntOf(364)), Is.SameAs(BoolT.True));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampGetMonth()
[Test]
        public virtual void TimestampGetMonth()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = DefaultTS();
            Val hr = ts.Receive(Overloads.TimeGetMonth, Overloads.TimestampToMonth);
            Assert.That(hr.Value(), Is.EqualTo(0L));
            // 1969-12-31T19:05:06Z
            Val hrTz = ts.Receive(Overloads.TimeGetMonth, Overloads.TimestampToMonthWithTz,
                StringT.StringOf("America/Phoenix"));
            Assert.That(hrTz.Value(), Is.EqualTo(11L));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampGetHours()
[Test]
        public virtual void TimestampGetHours()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = DefaultTS();
            Val hr = ts.Receive(Overloads.TimeGetHours, Overloads.TimestampToHours);
            Assert.That(hr.Value(), Is.EqualTo(2L));
            // 1969-12-31T19:05:06Z
            Val hrTz = ts.Receive(Overloads.TimeGetHours, Overloads.TimestampToHoursWithTz,
                StringT.StringOf("America/Phoenix"));
            Assert.That(hrTz.Value(), Is.EqualTo(19L));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampGetMinutes()
[Test]
        public virtual void TimestampGetMinutes()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = DefaultTS();
            Val min = ts.Receive(Overloads.TimeGetMinutes, Overloads.TimestampToMinutes);
            Assert.That(min.Equal(IntT.IntOf(5)), Is.SameAs(BoolT.True));
            // 1969-12-31T19:05:06Z
            Val minTz = ts.Receive(Overloads.TimeGetMinutes, Overloads.TimestampToMinutesWithTz,
                StringT.StringOf("America/Phoenix"));
            Assert.That(minTz.Equal(IntT.IntOf(5)), Is.SameAs(BoolT.True));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void timestampGetSeconds()
[Test]
        public virtual void TimestampGetSeconds()
        {
            // 1970-01-01T02:05:06Z
            TimestampT ts = DefaultTS();
            Val sec = ts.Receive(Overloads.TimeGetSeconds, Overloads.TimestampToSeconds);
            Assert.That(sec.Equal(IntT.IntOf(6)), Is.SameAs(BoolT.True));
            // 1969-12-31T19:05:06Z
            Val secTz = ts.Receive(Overloads.TimeGetSeconds, Overloads.TimestampToSecondsWithTz,
                StringT.StringOf("America/Phoenix"));
            Assert.That(secTz.Equal(IntT.IntOf(6)), Is.SameAs(BoolT.True));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parseTimezone()
[Test]
        public virtual void ParseTimezone()
        {
            DateTimeZone zoneUTC = DateTimeZone.Utc;

            Assert.That(new List<DateTimeZone>{TimestampT.ParseTz("-0"), TimestampT.ParseTz("+0"), TimestampT.ParseTz("0"),
                    TimestampT.ParseTz("-00"), TimestampT.ParseTz("+00"), TimestampT.ParseTz("00"),
                    TimestampT.ParseTz("-0:0"), TimestampT.ParseTz("+0:0:0"), TimestampT.ParseTz("0:0:0"),
                    TimestampT.ParseTz("-00:0:0"), TimestampT.ParseTz("+0:00:0"), TimestampT.ParseTz("+00:00:0")}, 
                Is.EquivalentTo(new List<DateTimeZone>{zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC, zoneUTC,
                    zoneUTC,
                    zoneUTC, zoneUTC}));

            Assert.That(() => TimestampT.ParseTz("+19"), Throws.Exception);
            Assert.That(() => TimestampT.ParseTz("+1:61"), Throws.Exception);
            Assert.That(() => TimestampT.ParseTz("+1:60:0"), Throws.Exception);
            Assert.That(() => TimestampT.ParseTz("+1:1:60"), Throws.Exception);

            Offset o1 = Offset.FromHoursAndMinutes(-1, -2);
            o1 = o1.Plus(Offset.FromSeconds(-30));
            DateTimeZone z1 = DateTimeZone.ForOffset(o1);
            Offset o2 = Offset.FromHoursAndMinutes(0, 1);
            o2 = o2.Plus(Offset.FromSeconds(3));
            DateTimeZone z2 = DateTimeZone.ForOffset(o2);
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

        public class ParseTestCase
        {
            internal string tz;
            internal readonly string timestamp;
            internal readonly int[] tuple;
            internal readonly DateTime ldt;

            internal ParseTestCase(string timestamp, int[] tuple)
            {
                this.timestamp = timestamp;
                this.tuple = tuple;
                this.ldt = new DateTime(tuple[0], tuple[1], tuple[2], tuple[3], tuple[4], tuple[5]);
            }

            internal virtual ParseTestCase WithTZ(string tz)
            {
                ParseTestCase copy = new ParseTestCase(timestamp, tuple);
                copy.tz = tz;
                return copy;
            }

            public override string ToString()
            {
                return "timestamp='" + timestamp + '\'' + ", tz='" + tz + '\'';
            }
        }

        internal const int numTimeZones = 5;
        internal const int numOffsets = 5;
        internal const int numDateTimes = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") static java.util.List<ParseTestCase> timestampParsingTestCases()
        internal static IList<ParseTestCase> TimestampParsingTestCases()
        {
            Random rand = new Random();

            IList<ParseTestCase> testCases = new List<ParseTestCase>(numDateTimes * numTimeZones);

            testCases.Add(
                (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ(
                    "Australia/Sydney"));
            testCases.Add(
                (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ("+11:00"));
            // time-zones unknown to ZoneId
            testCases.Add(
                (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ("CST"));
            testCases.Add(
                (new ParseTestCase("2009-02-13T23:31:30Z", new int[] { 2009, 2, 13, 23, 31, 30 })).WithTZ("MIT"));

            // Collect a couple of random time zones and date-times.
            IList<DateTimeZone> availableTimeZones =
                DateTimeZoneProviderExtensions.GetAllZones(DateTimeZoneProviders.Tzdb).ToList();

            for (int j = 0; j < numDateTimes; j++)
            {
                int year = rand.Next(1970, 2200);
                int month = rand.Next(1, 13);
                int day = rand.Next(1, 28);
                int hour = rand.Next(0, 24);
                int minute = rand.Next(0, 60);
                int second = rand.Next(0, 60);
                string dateTime = string.Format("{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}Z", year, month, day, hour,
                    minute, second);
                int[] tuple = new int[] { year, month, day, hour, minute, second };

                ParseTestCase noTzTestCase = new ParseTestCase(dateTime, tuple);

                for (int i = 0; i < numTimeZones; i++)
                {
                    int index = rand.Next(0, availableTimeZones.Count);
                    DateTimeZone tz = availableTimeZones[index];
                    availableTimeZones.RemoveAt(index);
                    testCases.Add(noTzTestCase.WithTZ(tz.Id));
                }

                for (int i = 0; i < numOffsets; i++)
                {
                    int offsetHours = rand.Next(-18, 19);
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
                    string id = string.Format("%+d:%02d", offsetHours, 0);
                    testCases.Add(noTzTestCase.WithTZ(id));
                }
            }

            return testCases;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("timestampParsingTestCases") void timestampParsing(ParseTestCase tc)
        public virtual void TimestampParsing(ParseTestCase tc)
        {
            Val ts = StringT.StringOf(tc.timestamp).ConvertToType(TimestampT.TimestampType);

            ZonedDateTime value = (ZonedDateTime)ts.Value();
            DateTime dtZlocal = value.ToDateTimeUnspecified();

            // Verify that the values in ParseTestCase are fine

            Assert.That(tc.tuple, Is.EquivalentTo(new List<int>{dtZlocal.Year, dtZlocal.Month, dtZlocal.Day, dtZlocal.Hour,
                dtZlocal.Minute, dtZlocal.Second}));

            // check the timestampGetXyz methods (without a time-zone), (assuming UTC)

            Assert.That(new List<Val>{TimestampT.TimestampGetFullYear(value), TimestampT.TimestampGetMonth(value),
                /*
                TimestampT.TimestampGetDayOfMonthOneBased(value), 
                TimestampT.TimestampGetDayOfMonthZeroBased(value),
                */
                TimestampT.TimestampGetHours(value), TimestampT.TimestampGetMinutes(value),
                TimestampT.TimestampGetSeconds(value), TimestampT.TimestampGetDayOfWeek(value),
                TimestampT.TimestampGetDayOfYear(value)}, Is.EquivalentTo(new List<Val>{
                IntT.IntOf(dtZlocal.Year),
                IntT.IntOf(dtZlocal.Month - 1),
                /*
                IntT.IntOf(dtZlocal.Day), IntT.IntOf(dtZlocal.Day - 1),
                */
                IntT.IntOf(dtZlocal.Hour), IntT.IntOf(dtZlocal.Minute), IntT.IntOf(dtZlocal.Second),
                IntT.IntOf((int)dtZlocal.DayOfWeek), IntT.IntOf(dtZlocal.DayOfYear - 1)
            }));

            // check the timestampGetXyzWithTu methods (with a time-zone)

            DateTimeZone zoneId = TimestampT.ParseTz(tc.tz);

            ZonedDateTime atZone = new ZonedDateTime(value.ToInstant(), zoneId);
            Val tzVal = StringT.StringOf(tc.tz);

            Assert.That(new List<Val>{TimestampT.TimestampGetFullYearWithTz(value, tzVal),
                TimestampT.TimestampGetMonthWithTz(value, tzVal),
                /*
                TimestampT.TimestampGetDayOfMonthOneBasedWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfMonthZeroBasedWithTz(value, tzVal),
                */
                TimestampT.TimestampGetHoursWithTz(value, tzVal), TimestampT.TimestampGetMinutesWithTz(value, tzVal),
                TimestampT.TimestampGetSecondsWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfWeekWithTz(value, tzVal),
                TimestampT.TimestampGetDayOfYearWithTz(value, tzVal)},
                Is.EquivalentTo(new List<Val>{IntT.IntOf(atZone.Year),
                IntT.IntOf(atZone.Month - 1), 
                /*
                IntT.IntOf(atZone.getDayOfMonth()),
                IntT.IntOf(atZone.getDayOfMonth() - 1), 
                */
                IntT.IntOf(atZone.Hour), IntT.IntOf(atZone.Minute),
                IntT.IntOf(atZone.Second), IntT.IntOf((int)atZone.DayOfWeek),
                IntT.IntOf(atZone.DayOfYear - 1)}));
        }

        private TimestampT DefaultTS()
        {
            return TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(7506).InZone(TimestampT.ZoneIdZ));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nanoMicroMilliPrecision()
[Test]
        public virtual void NanoMicroMilliPrecision()
        {
            long secondsEpoch = 1624006650L;
            string ts0 = "2021-06-18T08:57:30Z";
            int nano3 = 123000000;
            string ts3 = "2021-06-18T08:57:30.123Z";
            int nano4 = 123400000;
            string ts4 = "2021-06-18T08:57:30.1234Z";
            int nano6 = 123456000;
            string ts6 = "2021-06-18T08:57:30.123456Z";
            int nano9 = 123456789;
            string ts9 = "2021-06-18T08:57:30.123456789Z";

            object z = TimestampT.TimestampOf(ts0).Value();
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
    }
}