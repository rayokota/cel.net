using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
//	import static Cel.Common.Types.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BytesT.bytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DurationT.durationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOfCompare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.timestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.uintOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
	using Type = global::Cel.Common.Types.Ref.Type;
	using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
	using Val = global::Cel.Common.Types.Ref.Val;
	using Adder = global::Cel.Common.Types.Traits.Adder;
	using Comparer = global::Cel.Common.Types.Traits.Comparer;
	using Matcher = global::Cel.Common.Types.Traits.Matcher;
	using Receiver = global::Cel.Common.Types.Traits.Receiver;
	using Sizer = global::Cel.Common.Types.Traits.Sizer;
	using Trait = global::Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// String type implementation which supports addition, comparison, matching, and size functions. </summary>
	public sealed class StringT : BaseVal, Adder, Comparer, Matcher, Receiver, Sizer
	{

	  /// <summary>
	  /// StringType singleton. </summary>
	  public static readonly Type StringType = TypeT.NewTypeValue(TypeEnum.String, Trait.AdderType, Trait.ComparerType, Trait.MatcherType, Trait.ReceiverType, Trait.SizerType);

	  private static readonly IDictionary<string, System.Func<string, Val, Val>> stringOneArgOverloads;

	  static StringT()
	  {
		stringOneArgOverloads = new Dictionary<string, Func<string, Val, Val>>();
		stringOneArgOverloads[Overloads.Contains] = StringT.StringContains;
		stringOneArgOverloads[Overloads.EndsWith] = StringT.StringEndsWith;
		stringOneArgOverloads[Overloads.StartsWith] = StringT.StringStartsWith;
	  }

	  public static StringT StringOf(string s)
	  {
		return new StringT(s);
	  }

	  private readonly string s;

	  private StringT(string s)
	  {
		this.s = s;
	  }

	  /// <summary>
	  /// Add implements traits.Adder.Add. </summary>
	  public Val Add(Val other)
	  {
		if (!(other is StringT))
		{
		  return Err.NoSuchOverload(this, "add", other);
		}
		return new StringT(s + ((StringT) other).s);
	  }

	  /// <summary>
	  /// Compare implements traits.Comparer.Compare. </summary>
	  public Val Compare(Val other)
	  {
		if (!(other is StringT))
		{
		  return Err.NoSuchOverload(this, "compare", other);
		}

		return IntT.IntOfCompare(string.CompareOrdinal(s, ((StringT) other).s));
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		if (typeDesc == typeof(string) || typeDesc == typeof(object))
		{
		  return s;
		}
		if (typeDesc == typeof(sbyte[]))
		{
			return Encoding.UTF8.GetBytes(s);
		}
		if (typeDesc == typeof(Any))
		{
			StringValue value = new StringValue();
			value.Value = s;
			return Any.Pack(value);
		}
		if (typeDesc == typeof(StringValue))
		{
			StringValue value = new StringValue();
			value.Value = s;
			return value;
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(StringT))
		{
		  return this;
		}
		if (typeDesc == typeof(Value))
		{
			Value value = new Value();
			value.StringValue = s;
			return value;
		}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", StringType, typeDesc.FullName));
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
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
			  if ("true".Equals(s, StringComparison.OrdinalIgnoreCase))
			  {
				return BoolT.True;
			  }
			  if ("false".Equals(s, StringComparison.OrdinalIgnoreCase))
			  {
				return BoolT.False;
			  }
			  break;
			case TypeEnum.InnerEnum.Bytes:
				return BytesT.BytesOf(Encoding.UTF8.GetBytes(s));
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
		  return Err.NewErr(e, "error during type conversion from '%s' to %s: %s", StringType, typeVal, e.ToString());
		}
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		if (!(other is StringT))
		{
		  return Err.NoSuchOverload(this, "equal", other);
		}
		return Types.BoolOf(s.Equals(((StringT) other).s));
	  }

	  /// <summary>
	  /// Match implements traits.Matcher.Match. </summary>
	  public Val Match(Val pattern)
	  {
		if (!(pattern is StringT))
		{
		  return Err.NoSuchOverload(this, "match", pattern);
		}
		try
		{
		  Regex p = new Regex(((StringT)pattern).s);
		  return Types.BoolOf(p.IsMatch(s));
		}
		catch (Exception e)
		{
		  return Err.NewErr(e, "%s", e.Message);
		}
	  }

	  /// <summary>
	  /// Receive implements traits.Reciever.Receive. </summary>
	  public Val Receive(string function, string overload, params Val[] args)
	  {
		if (args.Length == 1)
		{
		  System.Func<string, Val, Val> f = stringOneArgOverloads[function];
		  if (f != null)
		  {
			return f(s, args[0]);
		  }
		}
		return Err.NoSuchOverload(this, function, overload, args);
	  }

	  /// <summary>
	  /// Size implements traits.Sizer.Size. </summary>
	  public Val Size()
	  {
		return IntT.IntOf(s.Length);
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public override Type Type()
	  {
		return StringType;
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public override object Value()
	  {
		return s;
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
		StringT stringT = (StringT) o;
		return Object.Equals(s, stringT.s);
	  }

	  public override int GetHashCode()
	  {
		return HashCode.Combine(base.GetHashCode(), s);
	  }

	  internal static Val StringContains(string s, Val sub)
	  {
		if (!(sub is StringT))
		{
		  return Err.NoSuchOverload(StringType, "contains", sub);
		}
		return Types.BoolOf(s.Contains(((StringT) sub).s));
	  }

	  internal static Val StringEndsWith(string s, Val suf)
	  {
		if (!(suf is StringT))
		{
		  return Err.NoSuchOverload(StringType, "endsWith", suf);
		}
		return Types.BoolOf(s.EndsWith(((StringT) suf).s, StringComparison.Ordinal));
	  }

	  internal static Val StringStartsWith(string s, Val pre)
	  {
		if (!(pre is StringT))
		{
		  return Err.NoSuchOverload(StringType, "startsWith", pre);
		}
		return Types.BoolOf(s.StartsWith(((StringT) pre).s, StringComparison.Ordinal));
	  }
	}

}