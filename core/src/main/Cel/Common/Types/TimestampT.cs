using NodaTime;
using System;
using System.Collections.Generic;
using NodaTime.Text;

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
//	import static Cel.Common.Types.DurationT.durationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errDurationOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errTimestampOutOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errTimestampOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntNegOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
	using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using OverflowException = global::Cel.Common.Types.Overflow.OverflowException;
	using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
	using Type = global::Cel.Common.Types.Ref.Type;
	using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
	using Val = global::Cel.Common.Types.Ref.Val;
	using Adder = global::Cel.Common.Types.Traits.Adder;
	using Comparer = global::Cel.Common.Types.Traits.Comparer;
	using Receiver = global::Cel.Common.Types.Traits.Receiver;
	using Subtractor = global::Cel.Common.Types.Traits.Subtractor;
	using Trait = global::Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// Timestamp type implementation which supports add, compare, and subtract operations. Timestamps
	/// are also capable of participating in dynamic function dispatch to instance methods.
	/// </summary>
	public sealed class TimestampT : BaseVal, Adder, Comparer, Receiver, Subtractor
	{

	  /// <summary>
	  /// TimestampType singleton. </summary>
	  public static readonly Type TimestampType = TypeT.NewTypeValue(TypeEnum.Timestamp, Trait.AdderType, Trait.ComparerType, Trait.ReceiverType, Trait.SubtractorType);

	  /// <summary>
	  /// Number of seconds between `0001-01-01T00:00:00Z` and the Unix epoch. </summary>
	  public const long minUnixTime = -62135596800L;
	  /// <summary>
	  /// Number of seconds between `9999-12-31T23:59:59.999999999Z` and the Unix epoch. </summary>
	  public const long maxUnixTime = 253402300799L;

	  public static readonly DateTimeZone ZoneIdZ = DateTimeZone.Utc;

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
		  Instant ldt = Instant.FromUnixTimeSeconds(t.Seconds);
		  ldt.PlusNanoseconds(t.Nanos);
		ZonedDateTime zdt = new ZonedDateTime(ldt, ZoneIdZ);
		return new TimestampT(zdt);
	  }

	  public static TimestampT TimestampOf(ZonedDateTime t)
	  {
		// Note that this function does not validate that time.Time is in our supported range.
		return new TimestampT(t);
	  }

	  private static readonly IDictionary<string, System.Func<ZonedDateTime, Val>> timestampZeroArgOverloads;
	  private static readonly IDictionary<string, System.Func<ZonedDateTime, Val, Val>> timestampOneArgOverloads;

	  static TimestampT()
	  {
		timestampZeroArgOverloads = new Dictionary<string, Func<ZonedDateTime, Val>>();
		timestampZeroArgOverloads[Overloads.TimeGetFullYear] = TimestampT.TimestampGetFullYear;
		timestampZeroArgOverloads[Overloads.TimeGetMonth] = TimestampT.TimestampGetMonth;
		timestampZeroArgOverloads[Overloads.TimeGetDayOfYear] = TimestampT.TimestampGetDayOfYear;
		/*
		timestampZeroArgOverloads[Overloads.TimeGetDate] = TimestampT.TimestampGetDayOfMonthOneBased;
		timestampZeroArgOverloads[Overloads.TimeGetDayOfMonth] = TimestampT.TimestampGetDayOfMonthZeroBased;
		*/
		timestampZeroArgOverloads[Overloads.TimeGetDayOfWeek] = TimestampT.TimestampGetDayOfWeek;
		timestampZeroArgOverloads[Overloads.TimeGetHours] = TimestampT.TimestampGetHours;
		timestampZeroArgOverloads[Overloads.TimeGetMinutes] = TimestampT.TimestampGetMinutes;
		timestampZeroArgOverloads[Overloads.TimeGetSeconds] = TimestampT.TimestampGetSeconds;
		timestampZeroArgOverloads[Overloads.TimeGetMilliseconds] = TimestampT.TimestampGetMilliseconds;

		timestampOneArgOverloads = new Dictionary<string, Func<ZonedDateTime, Val, Val>>();
		timestampOneArgOverloads[Overloads.TimeGetFullYear] = TimestampT.TimestampGetFullYearWithTz;
		timestampOneArgOverloads[Overloads.TimeGetMonth] = TimestampT.TimestampGetMonthWithTz;
		timestampOneArgOverloads[Overloads.TimeGetDayOfYear] = TimestampT.TimestampGetDayOfYearWithTz;
		/*
		timestampOneArgOverloads[Overloads.TimeGetDate] = TimestampT.TimestampGetDayOfMonthOneBasedWithTz;
		timestampOneArgOverloads[Overloads.TimeGetDayOfMonth] = TimestampT.TimestampGetDayOfMonthZeroBasedWithTz;
		*/
		timestampOneArgOverloads[Overloads.TimeGetDayOfWeek] = TimestampT.TimestampGetDayOfWeekWithTz;
		timestampOneArgOverloads[Overloads.TimeGetHours] = TimestampT.TimestampGetHoursWithTz;
		timestampOneArgOverloads[Overloads.TimeGetMinutes] = TimestampT.TimestampGetMinutesWithTz;
		timestampOneArgOverloads[Overloads.TimeGetSeconds] = TimestampT.TimestampGetSecondsWithTz;
		timestampOneArgOverloads[Overloads.TimeGetMilliseconds] = TimestampT.TimestampGetMillisecondsWithTz;
	  }

	  private readonly ZonedDateTime t;

	  private TimestampT(ZonedDateTime t)
	  {
		this.t = t;
	  }

	  public Val RangeCheck()
	  {
		long unitTime = t.Second;
		if (unitTime < minUnixTime || unitTime > maxUnixTime)
		{
		  return Err.ErrTimestampOutOfRange;
		}
		return this;
	  }

	  /// <summary>
	  /// Add implements traits.Adder.Add. </summary>
	  public Val Add(Val other)
	  {
		if (other.Type().TypeEnum() == TypeEnum.Duration)
		{
		  return ((DurationT) other).Add(this);
		}
		return Err.NoSuchOverload(this, "add", other);
	  }

	  /// <summary>
	  /// Compare implements traits.Comparer.Compare. </summary>
	  public Val Compare(Val other)
	  {
		if (TimestampType != other.Type())
		{
		  return Err.NoSuchOverload(this, "compare", other);
		}
		ZonedDateTime ts1 = t;
		ZonedDateTime ts2 = ((TimestampT) other).t;
		int cmp = ZonedDateTime.Comparer.Instant.Compare(ts1, ts2);
		if (cmp < 0)
		{
		  return IntT.IntNegOne;
		}
		if (cmp > 0)
		{
		  return IntT.IntOne;
		}
		return IntT.IntZero;
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		if (typeDesc == typeof(ZonedDateTime))
		{
		  return t;
		}
		if (typeDesc == typeof(DateTime))
		{
		  return new DateTime(ToEpochMillis());
		}
		if (typeDesc == typeof(OffsetDateTime))
		{
		  return t.ToOffsetDateTime();
		}
		if (typeDesc == typeof(Instant))
		{
		  return t.ToInstant();
		}
		if (typeDesc == typeof(Any))
		{
		  return Any.Pack(ToPbTimestamp());
		}
		if (typeDesc == typeof(Timestamp) || typeDesc == typeof(object))
		{
		  return ToPbTimestamp();
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(TimestampT))
		{
		  return this;
		}
		if (typeDesc == typeof(Value))
		{
		  // CEL follows the proto3 to JSON conversion which formats as an RFC 3339 encoded JSON string.
		  StringValue value = new StringValue();
		  value.Value = LocalDateTimePattern.GeneralIso.Format(t.LocalDateTime);
		  return value;
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", TimestampType, typeDesc.FullName));
	  }

	  private long ToEpochMillis()
	  {
		  return t.ToInstant().ToUnixTimeMilliseconds() + t.NanosecondOfSecond / 1000000;
	  }

	  private Timestamp ToPbTimestamp()
	  {
		  Timestamp ts = new Timestamp();
		  ts.Seconds = t.ToInstant().ToUnixTimeSeconds();
		  ts.Nanos = t.NanosecondOfSecond;
		  return ts;
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
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
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		if (TimestampType != other.Type())
		{
		  return Err.NoSuchOverload(this, "equal", other);
		}
		return Types.BoolOf(t.Equals(((TimestampT) other).t));
	  }

	  /// <summary>
	  /// Receive implements traits.Reciever.Receive. </summary>
	  public Val Receive(string function, string overload, params Val[] args)
	  {
		switch (args.Length)
		{
		  case 0:
			System.Func<ZonedDateTime, Val> f0 = timestampZeroArgOverloads[function];
			if (f0 != null)
			{
			  return f0(t);
			}
			break;
		  case 1:
			System.Func<ZonedDateTime, Val, Val> f1 = timestampOneArgOverloads[function];
			if (f1 != null)
			{
			  return f1(t, args[0]);
			}
			break;
		}
		return Err.NoSuchOverload(this, function, overload, args);
	  }

	  /// <summary>
	  /// Subtract implements traits.Subtractor.Subtract. </summary>
	  public Val Subtract(Val other)
	  {
		switch (other.Type().TypeEnum().InnerEnumValue)
		{
		  case TypeEnum.InnerEnum.Duration:
			Period d = (Period) other.Value();
			try
			{
			  return TimestampOf(Overflow.SubtractTimeDurationChecked(t, d));
			}
			catch (OverflowException)
			{
			  return Err.ErrTimestampOverflow;
			}
		  case TypeEnum.InnerEnum.Timestamp:
			ZonedDateTime o = (ZonedDateTime) other.Value();
			try
			{
			  return DurationT.DurationOf(Overflow.SubtractTimeChecked(t, o)).RangeCheck();
			}
			catch (OverflowException)
			{
			  return Err.ErrDurationOverflow;
			}
		}
		return Err.NoSuchOverload(this, "subtract", other);
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public override Type Type()
	  {
		return TimestampType;
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public override object Value()
	  {
		return t;
	  }

	  public override bool Equals(object o)
	  {
		if (this == o)
		{
		  return true;
		}
		if (o == null || this.GetType() != o.GetType())
		{
		  return false;
		}
		TimestampT that = (TimestampT) o;
		return Object.Equals(t, that.t);
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
		return TimeZone(tz, TimestampT.TimestampGetFullYear, t);
	  }

	  internal static Val TimestampGetMonthWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetMonth, t);
	  }

	  internal static Val TimestampGetDayOfYearWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetDayOfYear, t);
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
		return TimeZone(tz, TimestampT.TimestampGetDayOfWeek, t);
	  }

	  internal static Val TimestampGetHoursWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetHours, t);
	  }

	  internal static Val TimestampGetMinutesWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetMinutes, t);
	  }

	  internal static Val TimestampGetSecondsWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetSeconds, t);
	  }

	  internal static Val TimestampGetMillisecondsWithTz(ZonedDateTime t, Val tz)
	  {
		return TimeZone(tz, TimestampT.TimestampGetMilliseconds, t);
	  }

	  private static Val TimeZone(Val tz, System.Func<ZonedDateTime, Val> funct, ZonedDateTime t)
	  {
		if (tz.Type().TypeEnum() != TypeEnum.String)
		{
		  return Err.NoSuchOverload(TimestampType, "_op_with_timezone", tz);
		}
		string val = (string) tz.Value();
		try
		{
		  DateTimeZone zoneId = ParseTz(val);
		  ZonedDateTime z = new ZonedDateTime(t.ToInstant(), zoneId);
		  return funct(z);
		}
		catch (Exception e)
		{
		  return Err.NewErr(e, "no conversion of '%s' to time-zone '%s': %s", t, val, e);
		}
	  }

	  /// <summary>
	  /// Parses a string to a valid <seealso cref="ZoneId"/>.
	  /// 
	  /// <para>The input can be a
	  /// 
	  /// <ul>
	  ///   <li>numerical representation {@code ( '+' | '-' ) digit ( digit ) ( ':' digit ( digit ) ( ':'
	  ///       digit ( digit ) ) )}, which is more flexible than {@link ZoneOffset#of(String)
	  ///       ZoneOffset.of(String)}, or a
	  ///   <li>zone ID, as returned by <seealso cref="ZoneId.of(String) ZoneId.of(String)"/>, or a
	  ///   <li>time zone, as returned by {@link TimeZone#getTimeZone(String)
	  ///       TimeZone.getTimeZone(String).toZoneId()}.
	  /// </ul>
	  /// </para>
	  /// </summary>
	  internal static DateTimeZone ParseTz(string tz)
	  {
		if (tz.Length == 0)
		{
		  throw new Exception("time-zone must not be empty");
		}

		char first = tz[0];
		if (first == '-' || first == '+' || (first >= '0' && first <= '9'))
		{
		  bool negate = false;

		  int[] i = new int[] {0};

		  if (first == '-')
		  {
			negate = true;
			i[0]++;
		  }
		  else if (first == '+')
		  {
			i[0]++;
		  }

		  int hours = ParseNumber(tz, i, false);
		  int minutes = ParseNumber(tz, i, true);
		  int seconds = ParseNumber(tz, i, true);

		  if (hours > 18 || minutes > 59 || seconds > 59)
		  {
			throw new Exception(String.Format("invalid hour/minute/second value in time zone: '{0}'", tz));
		  }

		  Offset offset;
		  if (negate)
		  {
			  offset = Offset.FromSeconds(-hours * 3600 + -minutes * 60 + -seconds);
		  }
		  else
		  {
			  offset = Offset.FromSeconds(hours * 3600 + minutes * 60 + seconds);
		  }
		  return DateTimeZone.ForOffset(offset);
		}

		return DateTimeZoneProviders.Tzdb[tz];
	  }

	  private static int ParseNumber(string tz, int[] i, bool skipColon)
	  {
		if (skipColon)
		{
		  if (i[0] < tz.Length)
		  {
			char c = tz[i[0]];
			if (c == ':')
			{
			  i[0]++;
			}
		  }
		}

		if (i[0] < tz.Length)
		{
		  char c = tz[i[0]];
		  if (c >= '0' && c <= '9')
		  {
			int dig1 = c - '0';
			i[0]++;

			if (i[0] < tz.Length)
			{
			  c = tz[i[0]];
			  if (c >= '0' && c <= '9')
			  {
				i[0]++;
				int dig2 = c - '0';
				return dig1 * 10 + dig2;
			  }
			  else if (c != ':')
			  {
				throw new Exception(String.Format("unexpected character '{0}' at index {1:D}", c, i[0]));
			  }
			}

			return dig1;
		  }
		  else
		  {
			throw new Exception(String.Format("unexpected character '{0}' at index {1:D}", c, i[0]));
		  }
		}

		return 0;
	  }
	}

}