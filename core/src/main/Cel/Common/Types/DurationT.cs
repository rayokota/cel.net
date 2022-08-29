using System;
using System.Collections.Generic;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using NodaTime;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
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
//	import static Cel.Common.Types.Err.errDurationOutOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errDurationOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOfCompare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.timestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using OverflowException = global::Cel.Common.Types.Overflow.OverflowException;
	using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
	using Type = global::Cel.Common.Types.Ref.Type;
	using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
	using Val = global::Cel.Common.Types.Ref.Val;
	using Adder = global::Cel.Common.Types.Traits.Adder;
	using Comparer = global::Cel.Common.Types.Traits.Comparer;
	using Negater = global::Cel.Common.Types.Traits.Negater;
	using Receiver = global::Cel.Common.Types.Traits.Receiver;
	using Subtractor = global::Cel.Common.Types.Traits.Subtractor;
	using Trait = global::Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// Duration type that implements ref.Val and supports add, compare, negate, and subtract operators.
	/// This type is also a receiver which means it can participate in dispatch to receiver functions.
	/// </summary>
	public sealed class DurationT : BaseVal, Adder, Comparer, Negater, Receiver, Subtractor
	{

	  /// <summary>
	  /// DurationType singleton. </summary>
	  public static readonly Type DurationType = TypeT.NewTypeValue(TypeEnum.Duration, Trait.AdderType, Trait.ComparerType, Trait.NegatorType, Trait.ReceiverType, Trait.SubtractorType);

	  // Go's Duration represents the number of nanoseconds as an int64
	  // 	minDuration Duration = -1 << 63
	  //	maxDuration Duration = 1<<63 - 1
	  // This equates to:
	  //  minDuration in seconds:
	  public const long minDurationSeconds = -9223372036L;
	  public const long maxDurationSeconds = 9223372035L;

	  public static DurationT DurationOf(string s)
	  {
		  Period dur = PeriodPattern.Roundtrip.Parse(s).Value;
		return DurationOf(dur);
	  }

	  public static DurationT DurationOf(Duration d)
	  {
		  // TODO nanos
		return new DurationT(Period.FromSeconds(d.Seconds));
	  }

	  public static DurationT DurationOf(Period d)
	  {
		return new DurationT(d);
	  }

	  private static readonly IDictionary<string, System.Func<Period, Val>> durationZeroArgOverloads;

	  static DurationT()
	  {
		durationZeroArgOverloads = new Dictionary<string, System.Func<Period, Val>>();
		durationZeroArgOverloads[Overloads.TimeGetHours] = DurationT.TimeGetHours;
		durationZeroArgOverloads[Overloads.TimeGetMinutes] = DurationT.TimeGetMinutes;
		durationZeroArgOverloads[Overloads.TimeGetSeconds] = DurationT.TimeGetSeconds;
		durationZeroArgOverloads[Overloads.TimeGetMilliseconds] = DurationT.TimeGetMilliseconds;
	  }

	  private readonly Period d;

	  private DurationT(Period d)
	  {
		this.d = d;
	  }

	  /// <summary>
	  /// Verifies that the range of this duration conforms to Go's constraints, see above code comment.
	  /// </summary>
	  public Val RangeCheck()
	  {
		if (d.Seconds < minDurationSeconds || d.Seconds > maxDurationSeconds)
		{
		  return Err.ErrDurationOutOfRange;
		}
		return this;
	  }

	  /// <summary>
	  /// Add implements traits.Adder.Add. </summary>
	  public Val Add(Val other)
	  {
		switch (other.Type().TypeEnum().InnerEnumValue)
		{
		  case TypeEnum.InnerEnum.Duration:
			try
			{
			  return DurationOf(Overflow.AddDurationChecked(d, ((DurationT) other).d));
			}
			catch (OverflowException)
			{
			  return Err.ErrDurationOverflow;
			}
		  case TypeEnum.InnerEnum.Timestamp:
			try
			{
			  return TimestampT.TimestampOf(Overflow.AddTimeDurationChecked((ZonedDateTime) other.Value(), d));
			}
			catch (OverflowException)
			{
			  return Err.ErrDurationOverflow;
			}
		}
		return Err.NoSuchOverload(this, "add", other);
	  }

	  /// <summary>
	  /// Compare implements traits.Comparer.Compare. </summary>
	  public Val Compare(Val other)
	  {
		if (!(other is DurationT))
		{
		  return Err.NoSuchOverload(this, "compare", other);
		}
		Period o = ((DurationT) other).d;
		IComparer<Period> cmp = Period.CreateComparer(LocalDateTime.MinIsoValue);
		return IntT.IntOfCompare(cmp.Compare(d, o));
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		if (typeDesc.IsAssignableFrom(typeof(Duration)))
		{
		  return d;
		}
		if (typeof(Duration) == typeDesc || typeDesc == typeof(object))
		{
		  return PbVal();
		}
		if (typeof(Any) == typeDesc)
		{
		  return Any.Pack(PbVal());
		}
		if (typeof(long) == typeDesc)
		{
		  return Convert.ToInt64(ToJavaLong());
		}
		if (typeof(string) == typeDesc)
		{
		  // CEL follows the proto3 to JSON conversion.
		  return ToPbString();
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(DurationT))
		{
		  return this;
		}
		if (typeDesc == typeof(Value))
		{
		  // CEL follows the proto3 to JSON conversion.
		  Value value = new Value();
		  value.StringValue = ToPbString();
		  return value;
		}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", DurationType, typeDesc.FullName));
	  }

	  private Duration PbVal()
	  {
		  Duration duration = new Duration();
		  duration.Seconds = d.Seconds;
		  duration.Nanos = (int)d.Nanoseconds;
		  return duration;
	  }

	  private long ToJavaLong()
	  {

		  return d.Seconds * 1000000000 + d.Nanoseconds;
	  }

	  private string ToPbString()
	  {
		// 7506.000001s
		long micros = d.Nanoseconds / 1000;
		if (micros == 0L)
		{
		  return String.Format("{0:D}s", d.Seconds);
		}

		return String.Format("{0:D}.{1:D6}s", d.Seconds, micros);
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
	  public override Val ConvertToType(Type typeValue)
	  {
		switch (typeValue.TypeEnum().InnerEnumValue)
		{
		  case TypeEnum.InnerEnum.String:
			return StringT.StringOf(ToPbString());
		  case TypeEnum.InnerEnum.Int:
			return IntT.IntOf(ToJavaLong());
		  case TypeEnum.InnerEnum.Duration:
			return this;
		  case TypeEnum.InnerEnum.Type:
			return DurationType;
		}
		return Err.NewTypeConversionError(DurationType, typeValue);
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		if (!(other is DurationT))
		{
		  return Err.NoSuchOverload(this, "equal", other);
		}
		return Types.BoolOf(d.Equals(((DurationT) other).d));
	  }

	  /// <summary>
	  /// Negate implements traits.Negater.Negate. </summary>
	  public Val Negate()
	  {
		try
		{
		  return DurationOf(Overflow.NegateDurationChecked(d));
		}
		catch (OverflowException)
		{
		  return Err.ErrDurationOverflow;
		}
	  }

	  /// <summary>
	  /// Receive implements traits.Receiver.Receive. </summary>
	  public Val Receive(string function, string overload, params Val[] args)
	  {
		if (args.Length == 0)
		{
		  System.Func<Period, Val> f = durationZeroArgOverloads[function];
		  if (f != null)
		  {
			return f(d);
		  }
		}
		return Err.NoSuchOverload(this, function, overload, args);
	  }

	  /// <summary>
	  /// Subtract implements traits.Subtractor.Subtract. </summary>
	  public Val Subtract(Val other)
	  {
		if (!(other is DurationT))
		{
		  return Err.NoSuchOverload(this, "subtract", other);
		}
		try
		{
		  return DurationOf(Overflow.SubtractDurationChecked(d, ((DurationT) other).d));
		}
		catch (OverflowException)
		{
		  return Err.ErrDurationOverflow;
		}
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public override Type Type()
	  {
		return DurationType;
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public override object Value()
	  {
		return d;
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
		DurationT durationT = (DurationT) o;
		return Object.Equals(d, durationT.d);
	  }

	  public override int GetHashCode()
	  {
		return HashCode.Combine(base.GetHashCode(), d);
	  }

	  public static Val TimeGetHours(Period duration)
	  {
		return IntT.IntOf((int)duration.ToDuration().TotalHours);
	  }

	  public static Val TimeGetMinutes(Period duration)
	  {
		return IntT.IntOf((int)duration.ToDuration().TotalMinutes);
	  }

	  public static Val TimeGetSeconds(Period duration)
	  {
		return IntT.IntOf((int)duration.ToDuration().TotalSeconds);
	  }

	  public static Val TimeGetMilliseconds(Period duration)
	  {
		return IntT.IntOf((int)duration.ToDuration().TotalMilliseconds);
	  }
	}

}