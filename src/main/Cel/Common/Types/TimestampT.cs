using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Text;
using Type = Cel.Common.Types.Ref.Type;

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

/// <summary>
///     Timestamp type implementation which supports add, compare, and subtract operations. Timestamps
///     are also capable of participating in dynamic function dispatch to instance methods.
/// </summary>
public sealed class TimestampT : BaseVal, Adder, Comparer, Receiver, Subtractor
{
    /// <summary>
    ///     Number of seconds between `0001-01-01T00:00:00Z` and the Unix epoch.
    /// </summary>
    public const long minUnixTime = -62135596800L;

    /// <summary>
    ///     Number of seconds between `9999-12-31T23:59:59.999999999Z` and the Unix epoch.
    /// </summary>
    public const long maxUnixTime = 253402300799L;

    /// <summary>
    ///     TimestampType singleton.
    /// </summary>
    public static readonly Type TimestampType = TypeT.NewTypeValue(TypeEnum.Timestamp, Trait.AdderType,
        Trait.ComparerType, Trait.ReceiverType, Trait.SubtractorType);

    public static readonly DateTimeZone ZoneIdZ = DateTimeZone.Utc;

    private static readonly IDictionary<string, Func<ZonedDateTime, Val>> timestampZeroArgOverloads;
    private static readonly IDictionary<string, Func<ZonedDateTime, Val, Val>> timestampOneArgOverloads;

    private readonly ZonedDateTime t;

    static TimestampT()
    {
        timestampZeroArgOverloads = new Dictionary<string, Func<ZonedDateTime, Val>>();
        timestampZeroArgOverloads[Overloads.TimeGetFullYear] = TimestampGetFullYear;
        timestampZeroArgOverloads[Overloads.TimeGetMonth] = TimestampGetMonth;
        timestampZeroArgOverloads[Overloads.TimeGetDayOfYear] = TimestampGetDayOfYear;
        /*
        timestampZeroArgOverloads[Overloads.TimeGetDate] = TimestampT.TimestampGetDayOfMonthOneBased;
        timestampZeroArgOverloads[Overloads.TimeGetDayOfMonth] = TimestampT.TimestampGetDayOfMonthZeroBased;
        */
        timestampZeroArgOverloads[Overloads.TimeGetDayOfWeek] = TimestampGetDayOfWeek;
        timestampZeroArgOverloads[Overloads.TimeGetHours] = TimestampGetHours;
        timestampZeroArgOverloads[Overloads.TimeGetMinutes] = TimestampGetMinutes;
        timestampZeroArgOverloads[Overloads.TimeGetSeconds] = TimestampGetSeconds;
        timestampZeroArgOverloads[Overloads.TimeGetMilliseconds] = TimestampGetMilliseconds;

        timestampOneArgOverloads = new Dictionary<string, Func<ZonedDateTime, Val, Val>>();
        timestampOneArgOverloads[Overloads.TimeGetFullYear] = TimestampGetFullYearWithTz;
        timestampOneArgOverloads[Overloads.TimeGetMonth] = TimestampGetMonthWithTz;
        timestampOneArgOverloads[Overloads.TimeGetDayOfYear] = TimestampGetDayOfYearWithTz;
        /*
        timestampOneArgOverloads[Overloads.TimeGetDate] = TimestampT.TimestampGetDayOfMonthOneBasedWithTz;
        timestampOneArgOverloads[Overloads.TimeGetDayOfMonth] = TimestampT.TimestampGetDayOfMonthZeroBasedWithTz;
        */
        timestampOneArgOverloads[Overloads.TimeGetDayOfWeek] = TimestampGetDayOfWeekWithTz;
        timestampOneArgOverloads[Overloads.TimeGetHours] = TimestampGetHoursWithTz;
        timestampOneArgOverloads[Overloads.TimeGetMinutes] = TimestampGetMinutesWithTz;
        timestampOneArgOverloads[Overloads.TimeGetSeconds] = TimestampGetSecondsWithTz;
        timestampOneArgOverloads[Overloads.TimeGetMilliseconds] = TimestampGetMillisecondsWithTz;
    }

    private TimestampT(ZonedDateTime t)
    {
        this.t = t;
    }

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public Val Add(Val other)
    {
        if (other.Type().TypeEnum() == TypeEnum.Duration) return ((DurationT)other).Add(this);

        return Err.NoSuchOverload(this, "add", other);
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public Val Compare(Val other)
    {
        if (TimestampType != other.Type()) return Err.NoSuchOverload(this, "compare", other);

        var ts1 = t;
        var ts2 = ((TimestampT)other).t;
        var cmp = ZonedDateTime.Comparer.Instant.Compare(ts1, ts2);
        if (cmp < 0) return IntT.IntNegOne;

        if (cmp > 0) return IntT.IntOne;

        return IntT.IntZero;
    }

    /// <summary>
    ///     Receive implements traits.Reciever.Receive.
    /// </summary>
    public Val Receive(string function, string overload, params Val[] args)
    {
        switch (args.Length)
        {
            case 0:
                timestampZeroArgOverloads.TryGetValue(function, out var f0);
                if (f0 != null) return f0(t);

                break;
            case 1:
                timestampOneArgOverloads.TryGetValue(function, out var f1);
                if (f1 != null) return f1(t, args[0]);

                break;
        }

        return Err.NoSuchOverload(this, function, overload, args);
    }

    /// <summary>
    ///     Subtract implements traits.Subtractor.Subtract.
    /// </summary>
    public Val Subtract(Val other)
    {
        switch (other.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Duration:
                var d = (Period)other.Value();
                try
                {
                    return TimestampOf(Overflow.SubtractTimeDurationChecked(t, d));
                }
                catch (Overflow.OverflowException)
                {
                    return Err.ErrTimestampOverflow;
                }
            case TypeEnum.InnerEnum.Timestamp:
                var o = (ZonedDateTime)other.Value();
                try
                {
                    return DurationT.DurationOf(Overflow.SubtractTimeChecked(t, o)).RangeCheck();
                }
                catch (Overflow.OverflowException)
                {
                    return Err.ErrDurationOverflow;
                }
        }

        return Err.NoSuchOverload(this, "subtract", other);
    }

    public static TimestampT TimestampOf(string s)
    {
        // String parsing is a bit more complex here.
        // If the fraction of the second is 3 digits long, it's considered as milliseconds.
        // If the fraction of the second is 6 digits long, it's considered as microseconds.
        // If the fraction of the second is 9 digits long, it's considered as nanoseconds.
        // This is mostly to help with Java's behavior across different Java versions with String
        // representations, which can have 3 (fraction as millis), 6 (fraction as micros) or
        // 9 (fraction as nanos) digits. I.e. this implementation accepts these format patterns:
        //  yyyy-mm-ddThh:mm:ssZ
        //  yyyy-mm-ddThh:mm:ss.mmmZ
        //  yyyy-mm-ddThh:mm:ss.uuuuuuZ
        //  yyyy-mm-ddThh:mm:ss.nnnnnnnnnZ
        return TimestampOf(new ZonedDateTime(OffsetDateTimePattern.Rfc3339.Parse(s).Value.ToInstant(), ZoneIdZ));
    }

    public static TimestampT TimestampOf(Instant t)
    {
        return new TimestampT(new ZonedDateTime(t, ZoneIdZ));
    }

    public static TimestampT TimestampOf(Timestamp t)
    {
        var ldt = Instant.FromUnixTimeSeconds(t.Seconds);
        ldt.PlusNanoseconds(t.Nanos);
        var zdt = new ZonedDateTime(ldt, ZoneIdZ);
        return new TimestampT(zdt);
    }

    public static TimestampT TimestampOf(ZonedDateTime t)
    {
        // Note that this function does not validate that time.Time is in our supported range.
        return new TimestampT(t);
    }

    public Val RangeCheck()
    {
        long unitTime = t.Second;
        if (unitTime < minUnixTime || unitTime > maxUnixTime) return Err.ErrTimestampOutOfRange;

        return this;
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(ZonedDateTime)) return t;

        if (typeDesc == typeof(DateTime)) return DateTimeOffset.FromUnixTimeSeconds(ToEpochSeconds()).DateTime;

        if (typeDesc == typeof(OffsetDateTime)) return t.ToOffsetDateTime();

        if (typeDesc == typeof(Instant)) return t.ToInstant();

        if (typeDesc == typeof(Any)) return Any.Pack(ToPbTimestamp());

        if (typeDesc == typeof(Timestamp) || typeDesc == typeof(object)) return ToPbTimestamp();

        if (typeDesc == typeof(Val) || typeDesc == typeof(TimestampT)) return this;

        if (typeDesc == typeof(Value))
        {
            // CEL follows the proto3 to JSON conversion which formats as an RFC 3339 encoded JSON string.
            var value = new StringValue();
            value.Value = LocalDateTimePattern.GeneralIso.Format(t.LocalDateTime) + "Z";
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", TimestampType,
            typeDesc.FullName));
    }

    private long ToEpochMillis()
    {
        return t.ToInstant().ToUnixTimeMilliseconds() + t.NanosecondOfSecond / 1000000;
    }

    private long ToEpochSeconds()
    {
        return t.ToInstant().ToUnixTimeSeconds() + t.NanosecondOfSecond / 1000000000;
    }

    private Timestamp ToPbTimestamp()
    {
        var ts = new Timestamp();
        ts.Seconds = t.ToInstant().ToUnixTimeSeconds();
        ts.Nanos = t.NanosecondOfSecond;
        return ts;
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override Val ConvertToType(Type typeValue)
    {
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(OffsetDateTimePattern.Rfc3339.Format(t.ToOffsetDateTime()));
            case TypeEnum.InnerEnum.Int:
                return IntT.IntOf(t.ToInstant().ToUnixTimeSeconds());
            case TypeEnum.InnerEnum.Timestamp:
                return this;
            case TypeEnum.InnerEnum.Type:
                return TimestampType;
        }

        return Err.NewTypeConversionError(TimestampType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override Val Equal(Val other)
    {
        if (TimestampType != other.Type()) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(t.Equals(((TimestampT)other).t));
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override Type Type()
    {
        return TimestampType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return t;
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (TimestampT)o;
        return Equals(t, that.t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), t);
    }

    internal static Val TimestampGetFullYear(ZonedDateTime t)
    {
        return IntT.IntOf(t.Year);
    }

    internal static Val TimestampGetMonth(ZonedDateTime t)
    {
        // CEL spec indicates that the month should be 0-based, but the Time value
        // for Month() is 1-based. */
        return IntT.IntOf(t.Month - 1);
    }

    internal static Val TimestampGetDayOfYear(ZonedDateTime t)
    {
        return IntT.IntOf(t.DayOfYear - 1);
    }

    // TODO
    /*
    internal static Val TimestampGetDayOfMonthZeroBased(ZonedDateTime t)
    {
      return IntT.IntOf(t.DayOfMonth() - 1);
    }

    internal static Val TimestampGetDayOfMonthOneBased(ZonedDateTime t)
    {
      return intOf(t.getDayOfMonth());
    }
    */

    internal static Val TimestampGetDayOfWeek(ZonedDateTime t)
    {
        return IntT.IntOf((int)t.DayOfWeek);
    }

    internal static Val TimestampGetHours(ZonedDateTime t)
    {
        return IntT.IntOf(t.Hour);
    }

    internal static Val TimestampGetMinutes(ZonedDateTime t)
    {
        return IntT.IntOf(t.Minute);
    }

    internal static Val TimestampGetSeconds(ZonedDateTime t)
    {
        return IntT.IntOf(t.Second);
    }

    internal static Val TimestampGetMilliseconds(ZonedDateTime t)
    {
        return IntT.IntOf(t.ToInstant().ToUnixTimeMilliseconds() + t.NanosecondOfSecond * 1000000);
    }

    internal static Val TimestampGetFullYearWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetFullYear, t);
    }

    internal static Val TimestampGetMonthWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetMonth, t);
    }

    internal static Val TimestampGetDayOfYearWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetDayOfYear, t);
    }

    /*
    internal static Val TimestampGetDayOfMonthZeroBasedWithTz(ZonedDateTime t, Val tz)
    {
      return TimeZone(tz, TimestampT.TimestampGetDayOfMonthZeroBased, t);
    }

    internal static Val TimestampGetDayOfMonthOneBasedWithTz(ZonedDateTime t, Val tz)
    {
      return TimeZone(tz, TimestampT.TimestampGetDayOfMonthOneBased, t);
    }
    */

    internal static Val TimestampGetDayOfWeekWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetDayOfWeek, t);
    }

    internal static Val TimestampGetHoursWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetHours, t);
    }

    internal static Val TimestampGetMinutesWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetMinutes, t);
    }

    internal static Val TimestampGetSecondsWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetSeconds, t);
    }

    internal static Val TimestampGetMillisecondsWithTz(ZonedDateTime t, Val tz)
    {
        return TimeZone(tz, TimestampGetMilliseconds, t);
    }

    private static Val TimeZone(Val tz, Func<ZonedDateTime, Val> funct, ZonedDateTime t)
    {
        if (tz.Type().TypeEnum() != TypeEnum.String) return Err.NoSuchOverload(TimestampType, "_op_with_timezone", tz);

        var val = (string)tz.Value();
        try
        {
            var zoneId = ParseTz(val);
            var z = new ZonedDateTime(t.ToInstant(), zoneId);
            return funct(z);
        }
        catch (Exception e)
        {
            return Err.NewErr(e, "no conversion of '{0}' to time-zone '{0}': {0}", t, val, e);
        }
    }

    /// <summary>
    ///     Parses a string to a valid <seealso cref="ZoneId" />.
    ///     <para>
    ///         The input can be a
    ///         <ul>
    ///             <li>
    ///                 numerical representation {@code ( '+' | '-' ) digit ( digit ) ( ':' digit ( digit ) ( ':'
    ///                 digit ( digit ) ) )}, which is more flexible than {@link ZoneOffset#of(String)
    ///                 ZoneOffset.of(String)}, or a
    ///                 <li>
    ///                     zone ID, as returned by <seealso cref="ZoneId.of(String) ZoneId.of(String)" />, or a
    ///                     <li>
    ///                         time zone, as returned by {@link TimeZone#getTimeZone(String)
    ///                         TimeZone.getTimeZone(String).toZoneId()}.
    ///         </ul>
    ///     </para>
    /// </summary>
    internal static DateTimeZone ParseTz(string tz)
    {
        if (tz.Length == 0) throw new Exception("time-zone must not be empty");

        var first = tz[0];
        if (first == '-' || first == '+' || (first >= '0' && first <= '9'))
        {
            var negate = false;

            int[] i = { 0 };

            if (first == '-')
            {
                negate = true;
                i[0]++;
            }
            else if (first == '+')
            {
                i[0]++;
            }

            var hours = ParseNumber(tz, i, false);
            var minutes = ParseNumber(tz, i, true);
            var seconds = ParseNumber(tz, i, true);

            if (hours > 18 || minutes > 59 || seconds > 59)
                throw new Exception(string.Format("invalid hour/minute/second value in time zone: '{0}'", tz));

            Offset offset;
            if (negate)
                offset = Offset.FromSeconds(-hours * 3600 + -minutes * 60 + -seconds);
            else
                offset = Offset.FromSeconds(hours * 3600 + minutes * 60 + seconds);

            return DateTimeZone.ForOffset(offset);
        }

        return DateTimeZoneProviders.Tzdb[tz];
    }

    private static int ParseNumber(string tz, int[] i, bool skipColon)
    {
        if (skipColon)
            if (i[0] < tz.Length)
            {
                var c = tz[i[0]];
                if (c == ':') i[0]++;
            }

        if (i[0] < tz.Length)
        {
            var c = tz[i[0]];
            if (c >= '0' && c <= '9')
            {
                var dig1 = c - '0';
                i[0]++;

                if (i[0] < tz.Length)
                {
                    c = tz[i[0]];
                    if (c >= '0' && c <= '9')
                    {
                        i[0]++;
                        var dig2 = c - '0';
                        return dig1 * 10 + dig2;
                    }

                    if (c != ':')
                        throw new Exception(string.Format("unexpected character '{0}' at index {1:D}", c, i[0]));
                }

                return dig1;
            }

            throw new Exception(string.Format("unexpected character '{0}' at index {1:D}", c, i[0]));
        }

        return 0;
    }
}