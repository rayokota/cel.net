using System;
using System.Collections.Generic;
using System.Text;

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
	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
	using Val = Cel.Common.Types.Ref.Val;
	using Adder = Cel.Common.Types.Traits.Adder;
	using Comparer = Cel.Common.Types.Traits.Comparer;
	using Matcher = Cel.Common.Types.Traits.Matcher;
	using Receiver = Cel.Common.Types.Traits.Receiver;
	using Sizer = Cel.Common.Types.Traits.Sizer;
	using Trait = Cel.Common.Types.Traits.Trait;

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
		stringOneArgOverloads = new Dictionary<string, BiFunction<string, Val, Val>>();
		stringOneArgOverloads[Overloads.Contains] = StringT.stringContains;
		stringOneArgOverloads[Overloads.EndsWith] = StringT.stringEndsWith;
		stringOneArgOverloads[Overloads.StartsWith] = StringT.stringStartsWith;
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
		  return noSuchOverload(this, "add", other);
		}
		return new StringT(s + ((StringT) other).s);
	  }

	  /// <summary>
	  /// Compare implements traits.Comparer.Compare. </summary>
	  public Val Compare(Val other)
	  {
		if (!(other is StringT))
		{
		  return noSuchOverload(this, "compare", other);
		}

		return intOfCompare(string.CompareOrdinal(s, ((StringT) other).s));
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		if (typeDesc == typeof(string) || typeDesc == typeof(object))
		{
		  return (T) s;
		}
		if (typeDesc == typeof(sbyte[]))
		{
		  return (T) s.GetBytes(Encoding.UTF8);
		}
		if (typeDesc == typeof(Any))
		{
		  return (T) Any.pack(StringValue.of(s));
		}
		if (typeDesc == typeof(StringValue))
		{
		  return (T) StringValue.of(s);
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(StringT))
		{
		  return (T) this;
		}
		if (typeDesc == typeof(Value))
		{
		  return (T) Value.newBuilder().setStringValue(s).build();
		}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", StringType, typeDesc.FullName));
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
	  public override Val ConvertToType(Type typeVal)
	  {
		try
		{
		  switch (typeVal.TypeEnum().innerEnumValue)
		  {
			case TypeEnum.InnerEnum.Int:
			  return intOf(long.Parse(s));
			case TypeEnum.InnerEnum.Uint:
			  return uintOf(Long.parseUnsignedLong(s));
			case TypeEnum.InnerEnum.double:
			  return doubleOf(double.Parse(s));
			case TypeEnum.InnerEnum.Bool:
			  if ("true".Equals(s, StringComparison.OrdinalIgnoreCase))
			  {
				return True;
			  }
			  if ("false".Equals(s, StringComparison.OrdinalIgnoreCase))
			  {
				return False;
			  }
			  break;
			case TypeEnum.InnerEnum.Bytes:
			  return bytesOf(s.GetBytes(Encoding.UTF8));
			case TypeEnum.InnerEnum.Duration:
			  return durationOf(s).rangeCheck();
			case TypeEnum.InnerEnum.Timestamp:
			  return timestampOf(s).rangeCheck();
			case TypeEnum.InnerEnum.String:
			  return this;
			case Type:
			  return StringType;
		  }
		  return newTypeConversionError(StringType, typeVal);
		}
		catch (Exception e)
		{
		  return newErr(e, "error during type conversion from '%s' to %s: %s", StringType, typeVal, e.ToString());
		}
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		if (!(other is StringT))
		{
		  return noSuchOverload(this, "equal", other);
		}
		return boolOf(s.Equals(((StringT) other).s));
	  }

	  /// <summary>
	  /// Match implements traits.Matcher.Match. </summary>
	  public Val Match(Val pattern)
	  {
		if (!(pattern is StringT))
		{
		  return noSuchOverload(this, "match", pattern);
		}
		try
		{
		  Pattern p = Pattern.compile(((StringT) pattern).s);
		  java.util.regex.Matcher m = p.matcher(s);
		  return boolOf(m.find());
		}
		catch (Exception e)
		{
		  return newErr(e, "%s", e.Message);
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
		return noSuchOverload(this, function, overload, args);
	  }

	  /// <summary>
	  /// Size implements traits.Sizer.Size. </summary>
	  public Val Size()
	  {
		return intOf(s.Length);
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
		return Objects.equals(s, stringT.s);
	  }

	  public override int GetHashCode()
	  {
		return Objects.hash(base.GetHashCode(), s);
	  }

	  internal static Val StringContains(string s, Val sub)
	  {
		if (!(sub is StringT))
		{
		  return noSuchOverload(StringType, "contains", sub);
		}
		return boolOf(s.Contains(((StringT) sub).s));
	  }

	  internal static Val StringEndsWith(string s, Val suf)
	  {
		if (!(suf is StringT))
		{
		  return noSuchOverload(StringType, "endsWith", suf);
		}
		return boolOf(s.EndsWith(((StringT) suf).s, StringComparison.Ordinal));
	  }

	  internal static Val StringStartsWith(string s, Val pre)
	  {
		if (!(pre is StringT))
		{
		  return noSuchOverload(StringType, "startsWith", pre);
		}
		return boolOf(s.StartsWith(((StringT) pre).s, StringComparison.Ordinal));
	  }
	}

}