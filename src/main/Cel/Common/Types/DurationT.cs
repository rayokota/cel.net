using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Text;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
using Type = System.Type;

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
///     Duration type that implements ref.Val and supports add, compare, negate, and subtract operators.
///     This type is also a receiver which means it can participate in dispatch to receiver functions.
/// </summary>
public sealed class DurationT : BaseVal, IAdder, IComparer, INegater, IReceiver, ISubtractor
{
    // Go's Duration represents the number of nanoseconds as an int64
    // 	minDuration Duration = -1 << 63
    //	maxDuration Duration = 1<<63 - 1
    // This equates to:
    //  minDuration in seconds:
    public const long MinDurationSeconds = -9223372036L;
    public const long MaxDurationSeconds = 9223372035L;

    /// <summary>
    ///     DurationType singleton.
    /// </summary>
    public static readonly IType DurationType = TypeT.NewTypeValue(TypeEnum.Duration, Trait.AdderType,
        Trait.ComparerType, Trait.NegatorType, Trait.ReceiverType, Trait.SubtractorType);

    private static readonly IDictionary<string, Func<Period, IVal>> durationZeroArgOverloads;

    private readonly Period d;

    static DurationT()
    {
        durationZeroArgOverloads = new Dictionary<string, Func<Period, IVal>>();
        durationZeroArgOverloads[Overloads.TimeGetHours] = TimeGetHours;
        durationZeroArgOverloads[Overloads.TimeGetMinutes] = TimeGetMinutes;
        durationZeroArgOverloads[Overloads.TimeGetSeconds] = TimeGetSeconds;
        durationZeroArgOverloads[Overloads.TimeGetMilliseconds] = TimeGetMilliseconds;
    }

    private DurationT(Period d)
    {
        this.d = d;
    }

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public IVal Add(IVal other)
    {
        switch (other.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Duration:
                try
                {
                    return DurationOf(Overflow.AddDurationChecked(d, ((DurationT)other).d));
                }
                catch (Overflow.OverflowException)
                {
                    return Err.ErrDurationOverflow;
                }
            case TypeEnum.InnerEnum.Timestamp:
                try
                {
                    return TimestampT.TimestampOf(Overflow.AddTimeDurationChecked((ZonedDateTime)other.Value(), d));
                }
                catch (Overflow.OverflowException)
                {
                    return Err.ErrDurationOverflow;
                }
        }

        return Err.NoSuchOverload(this, "add", other);
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public IVal Compare(IVal other)
    {
        if (!(other is DurationT)) return Err.NoSuchOverload(this, "compare", other);

        var o = ((DurationT)other).d;
        IComparer<Period> cmp = Period.CreateComparer(LocalDateTime.MinIsoValue);
        return IntT.IntOfCompare(cmp.Compare(d, o));
    }

    /// <summary>
    ///     Negate implements traits.Negater.Negate.
    /// </summary>
    public IVal Negate()
    {
        try
        {
            return DurationOf(Overflow.NegateDurationChecked(d));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrDurationOverflow;
        }
    }

    /// <summary>
    ///     Receive implements traits.Receiver.Receive.
    /// </summary>
    public IVal Receive(string function, string overload, params IVal[] args)
    {
        if (args.Length == 0)
        {
            durationZeroArgOverloads.TryGetValue(function, out var f);
            if (f != null) return f(d);
        }

        return Err.NoSuchOverload(this, function, overload, args);
    }

    /// <summary>
    ///     Subtract implements traits.Subtractor.Subtract.
    /// </summary>
    public IVal Subtract(IVal other)
    {
        if (!(other is DurationT)) return Err.NoSuchOverload(this, "subtract", other);

        try
        {
            return DurationOf(Overflow.SubtractDurationChecked(d, ((DurationT)other).d));
        }
        catch (Overflow.OverflowException)
        {
            return Err.ErrDurationOverflow;
        }
    }

    public static DurationT DurationOf(string s)
    {
        var dur = PeriodPattern.Roundtrip.Parse("PT" + s.ToUpper()).Value;
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

    /// <summary>
    ///     Verifies that the range of this duration conforms to Go's constraints, see above code comment.
    /// </summary>
    public IVal RangeCheck()
    {
        if (d.Seconds < MinDurationSeconds || d.Seconds > MaxDurationSeconds) return Err.ErrDurationOutOfRange;

        return this;
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeof(Period) == typeDesc) return d;

        if (typeof(Duration) == typeDesc || typeDesc == typeof(object)) return PbVal();

        if (typeof(Any) == typeDesc) return Any.Pack(PbVal());

        if (typeof(long) == typeDesc) return ToLong();

        if (typeof(string) == typeDesc)
            // CEL follows the proto3 to JSON conversion.
            return ToPbString();

        if (typeDesc == typeof(IVal) || typeDesc == typeof(DurationT)) return this;

        if (typeDesc == typeof(Value))
        {
            // CEL follows the proto3 to JSON conversion.
            var value = new Value();
            value.StringValue = ToPbString();
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", DurationType,
            typeDesc.FullName));
    }

    private Duration PbVal()
    {
        var duration = new Duration();
        duration.Seconds = d.Seconds;
        duration.Nanos = (int)d.Nanoseconds;
        return duration;
    }

    private long ToLong()
    {
        return d.Seconds * 1000000000 + d.Nanoseconds;
    }

    private string ToPbString()
    {
        // 7506.000001s
        var micros = d.Nanoseconds / 1000;
        if (micros == 0L) return string.Format("{0:D}s", d.Seconds);

        return string.Format("{0:D}.{1:D6}s", d.Seconds, micros);
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeValue)
    {
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(ToPbString());
            case TypeEnum.InnerEnum.Int:
                return IntT.IntOf(ToLong());
            case TypeEnum.InnerEnum.Duration:
                return this;
            case TypeEnum.InnerEnum.Type:
                return DurationType;
        }

        return Err.NewTypeConversionError(DurationType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        if (!(other is DurationT)) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(d.Equals(((DurationT)other).d));
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return DurationType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return d;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var durationT = (DurationT)o;
        return Equals(d, durationT.d);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), d);
    }

    public static IVal TimeGetHours(Period duration)
    {
        return IntT.IntOf((int)duration.ToDuration().TotalHours);
    }

    public static IVal TimeGetMinutes(Period duration)
    {
        return IntT.IntOf((int)duration.ToDuration().TotalMinutes);
    }

    public static IVal TimeGetSeconds(Period duration)
    {
        return IntT.IntOf((int)duration.ToDuration().TotalSeconds);
    }

    public static IVal TimeGetMilliseconds(Period duration)
    {
        return IntT.IntOf((int)duration.ToDuration().TotalMilliseconds);
    }
}