using System.Text;
using System.Text.RegularExpressions;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;
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
///     String type implementation which supports addition, comparison, matching, and size functions.
/// </summary>
public sealed class StringT : BaseVal, Adder, Comparer, Matcher, Receiver, Sizer
{
    /// <summary>
    ///     StringType singleton.
    /// </summary>
    public static readonly Type StringType = TypeT.NewTypeValue(TypeEnum.String, Trait.AdderType,
        Trait.ComparerType, Trait.MatcherType, Trait.ReceiverType, Trait.SizerType);

    public static readonly UTF8Encoding UTF8 = new UTF8Encoding(false, true);

    private static readonly IDictionary<string, Func<string, Val, Val>> stringOneArgOverloads;

    private readonly string s;

    static StringT()
    {
        stringOneArgOverloads = new Dictionary<string, Func<string, Val, Val>>();
        stringOneArgOverloads[Overloads.Contains] = StringContains;
        stringOneArgOverloads[Overloads.EndsWith] = StringEndsWith;
        stringOneArgOverloads[Overloads.StartsWith] = StringStartsWith;
    }

    private StringT(string s)
    {
        this.s = s;
    }

    /// <summary>
    ///     Add implements traits.Adder.Add.
    /// </summary>
    public Val Add(Val other)
    {
        if (!(other is StringT)) return Err.NoSuchOverload(this, "add", other);

        return new StringT(s + ((StringT)other).s);
    }

    /// <summary>
    ///     Compare implements traits.Comparer.Compare.
    /// </summary>
    public Val Compare(Val other)
    {
        if (!(other is StringT)) return Err.NoSuchOverload(this, "compare", other);

        return IntT.IntOfCompare(string.CompareOrdinal(s, ((StringT)other).s));
    }

    /// <summary>
    ///     Match implements traits.Matcher.Match.
    /// </summary>
    public Val Match(Val pattern)
    {
        if (!(pattern is StringT)) return Err.NoSuchOverload(this, "match", pattern);

        try
        {
            var p = new Regex(((StringT)pattern).s);
            return Types.BoolOf(p.IsMatch(s));
        }
        catch (Exception e)
        {
            return Err.NewErr(e, "{0}", e.Message);
        }
    }

    /// <summary>
    ///     Receive implements traits.Reciever.Receive.
    /// </summary>
    public Val Receive(string function, string overload, params Val[] args)
    {
        if (args.Length == 1)
        {
            stringOneArgOverloads.TryGetValue(function, out var f);
            if (f != null) return f(s, args[0]);
        }

        return Err.NoSuchOverload(this, function, overload, args);
    }

    /// <summary>
    ///     Size implements traits.Sizer.Size.
    /// </summary>
    public Val Size()
    {
        return IntT.IntOf(s.Length);
    }

    public static StringT StringOf(string s)
    {
        return new StringT(s);
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(string) || typeDesc == typeof(object)) return s;

        if (typeDesc == typeof(byte[])) return UTF8.GetBytes(s);

        if (typeDesc == typeof(Any))
        {
            var value = new StringValue();
            value.Value = s;
            return Any.Pack(value);
        }

        if (typeDesc == typeof(StringValue)) return s;
        /*
            var value = new StringValue();
            value.Value = s;
            return value;
            */
        if (typeDesc == typeof(Val) || typeDesc == typeof(StringT)) return this;

        if (typeDesc == typeof(Value))
        {
            var value = new Value();
            value.StringValue = s;
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", StringType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override Val ConvertToType(Type typeVal)
    {
        try
        {
            switch (typeVal.TypeEnum().InnerEnumValue)
            {
                case TypeEnum.InnerEnum.Int:
                    return IntT.IntOf(long.Parse(s));
                case TypeEnum.InnerEnum.Uint:
                    return UintT.UintOf(ulong.Parse(s));
                case TypeEnum.InnerEnum.Double:
                    return DoubleT.DoubleOf(double.Parse(s));
                case TypeEnum.InnerEnum.Bool:
                    if ("true".Equals(s, StringComparison.OrdinalIgnoreCase)) return BoolT.True;

                    if ("false".Equals(s, StringComparison.OrdinalIgnoreCase)) return BoolT.False;

                    break;
                case TypeEnum.InnerEnum.Bytes:
                    return BytesT.BytesOf(UTF8.GetBytes(s));
                case TypeEnum.InnerEnum.Duration:
                    return DurationT.DurationOf(s).RangeCheck();
                case TypeEnum.InnerEnum.Timestamp:
                    return TimestampT.TimestampOf(s).RangeCheck();
                case TypeEnum.InnerEnum.String:
                    return this;
                case TypeEnum.InnerEnum.Type:
                    return StringType;
            }

            return Err.NewTypeConversionError(StringType, typeVal);
        }
        catch (Exception e)
        {
            return Err.NewErr(e, "error during type conversion from '{0}' to {1}: {2}", StringType, typeVal,
                e.ToString());
        }
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override Val Equal(Val other)
    {
        if (!(other is StringT)) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(s.Equals(((StringT)other).s));
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override Type Type()
    {
        return StringType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return s;
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var stringT = (StringT)o;
        return Equals(s, stringT.s);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), s);
    }

    internal static Val StringContains(string s, Val sub)
    {
        if (!(sub is StringT)) return Err.NoSuchOverload(StringType, "contains", sub);

        return Types.BoolOf(s.Contains(((StringT)sub).s));
    }

    internal static Val StringEndsWith(string s, Val suf)
    {
        if (!(suf is StringT)) return Err.NoSuchOverload(StringType, "endsWith", suf);

        return Types.BoolOf(s.EndsWith(((StringT)suf).s, StringComparison.Ordinal));
    }

    internal static Val StringStartsWith(string s, Val pre)
    {
        if (!(pre is StringT)) return Err.NoSuchOverload(StringType, "startsWith", pre);

        return Types.BoolOf(s.StartsWith(((StringT)pre).s, StringComparison.Ordinal));
    }
}