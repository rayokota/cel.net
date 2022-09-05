using System.Text;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using NodaTime;
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
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.BoolType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BytesT.BytesT.BytesType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BytesT.BytesT.BytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntNegOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.MapT.MapT.MapType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.TimestampType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.ZoneIdZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.TimestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TypeT.TypeT.TypeType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class StringTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringAdd()
[Test]
	  public virtual void StringAdd()
	  {
		Assert.That(StringT.StringOf("hello").Add(StringT.StringOf(" world")), Is.EqualTo(StringT.StringOf("hello world")));
		Assert.That(StringT.StringOf("hello").Add(StringT.StringOf(" world")).Equal(StringT.StringOf("hello world")), Is.EqualTo(BoolT.True));

		Assert.That(StringT.StringOf("goodbye").Add(IntT.IntOf(1)), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringCompare()
[Test]
	  public virtual void StringCompare()
	  {
		StringT a = StringT.StringOf("a");
		StringT a2 = StringT.StringOf("a");
		StringT b = StringT.StringOf("bbbb");
		StringT c = StringT.StringOf("c");
		Assert.That(a.Compare(b), Is.SameAs(IntT.IntNegOne));
		Assert.That(a.Compare(a), Is.SameAs(IntT.IntZero));
		Assert.That(a.Compare(a2), Is.SameAs(IntT.IntZero));
		Assert.That(c.Compare(b), Is.SameAs(IntT.IntOne));
		Assert.That(a.Compare(BoolT.True), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_Any()
[Test]
	  public virtual void StringConvertToNativeAny()
	  {
		  StringValue s = new StringValue();
		  s.Value = "hello";
		Any val = (Any) StringT.StringOf("hello").ConvertToNative(typeof(Any));
		Any want = Any.Pack(s);
		Assert.That(val, Is.EqualTo(want));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_Error()
[Test]
	  public virtual void StringConvertToNativeError()
	  {
		Assert.That(() => StringT.StringOf("hello").ConvertToNative(typeof(int)), Throws.Exception);
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_Json()
[Test]
	  public virtual void StringConvertToNativeJson()
	  {
		Value val = (Value) StringT.StringOf("hello").ConvertToNative(typeof(Value));
		Value pbVal = new Value();
		pbVal.StringValue = "hello";
		Assert.That(val, Is.EqualTo(pbVal));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_Ptr()
[Test]
	  public virtual void StringConvertToNativePtr()
	  {
		string val = (string) StringT.StringOf("hello").ConvertToNative(typeof(string));
		Assert.That(val, Is.EqualTo("hello"));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_String()
[Test]
	  public virtual void StringConvertToNativeString()
	  {
		Assert.That(StringT.StringOf("hello").ConvertToNative(typeof(string)), Is.EqualTo("hello"));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToNative_Wrapper()
[Test]
	  public virtual void StringConvertToNativeWrapper()
	  {
		string val = (string) StringT.StringOf("hello").ConvertToNative(typeof(StringValue));
		string want = "hello";
		Assert.That(val, Is.EqualTo(want));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringConvertToType()
[Test]
	  public virtual void StringConvertToType()
	  {
		Assert.That(StringT.StringOf("-1").ConvertToType(IntT.IntType).Equal(IntT.IntNegOne), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("false").ConvertToType(BoolT.BoolType).Equal(BoolT.False), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("1").ConvertToType(UintT.UintType).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("2.5").ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(2.5)), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("hello").ConvertToType(BytesT.BytesType).Equal(BytesT.BytesOf(Encoding.UTF8.GetBytes("hello"))), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("goodbye").ConvertToType(TypeT.TypeType).Equal(StringT.StringType), Is.SameAs(BoolT.True));
		StringT gb = StringT.StringOf("goodbye");
		Assert.That(gb.ConvertToType(StringT.StringType), Is.SameAs(gb));
		Assert.That(StringT.StringOf("goodbye").ConvertToType(StringT.StringType).Equal(StringT.StringOf("goodbye")), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("2017-01-01T00:00:00Z").ConvertToType(TimestampT.TimestampType).Equal(TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(1483228800).InZone(TimestampT.ZoneIdZ))), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("3605s").ConvertToType(DurationT.DurationType).Equal(DurationT.DurationOf(Period.FromSeconds(3605))), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("map{}").ConvertToType(MapT.MapType), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringEqual()
[Test]
	  public virtual void StringEqual()
	  {
		Assert.That(StringT.StringOf("hello").Equal(StringT.StringOf("hello")), Is.SameAs(BoolT.True));
		Assert.That(StringT.StringOf("hello").Equal(StringT.StringOf("hell")), Is.SameAs(BoolT.False));
		Assert.That(StringT.StringOf("c").Equal(IntT.IntOf(99)), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringMatch()
[Test]
	  public virtual void StringMatch()
	  {
		StringT str = StringT.StringOf("hello 1 world");
		Assert.That(str.Match(StringT.StringOf("^hello")), Is.SameAs(BoolT.True));
		Assert.That(str.Match(StringT.StringOf("llo 1 w")), Is.SameAs(BoolT.True));
		Assert.That(str.Match(StringT.StringOf("llo w")), Is.SameAs(BoolT.False));
		Assert.That(str.Match(StringT.StringOf("\\d world$")), Is.SameAs(BoolT.True));
		Assert.That(str.Match(StringT.StringOf("ello 1 worlds")), Is.SameAs(BoolT.False));
		Assert.That(str.Match(IntT.IntOf(1)), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringContains()
[Test]
	  public virtual void StringContains()
	  {
		Val y = StringT.StringOf("goodbye").Receive(Overloads.Contains, Overloads.ContainsString, StringT.StringOf("db"));
		Assert.That(y, Is.SameAs(BoolT.True));

		Val n = StringT.StringOf("goodbye").Receive(Overloads.Contains, Overloads.ContainsString, StringT.StringOf("ggood"));
		Assert.That(n, Is.SameAs(BoolT.False));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringEndsWith()
[Test]
	  public virtual void StringEndsWith()
	  {
		Val y = StringT.StringOf("goodbye").Receive(Overloads.EndsWith, Overloads.EndsWithString, StringT.StringOf("bye"));
		Assert.That(y, Is.SameAs(BoolT.True));

		Val n = StringT.StringOf("goodbye").Receive(Overloads.EndsWith, Overloads.EndsWithString, StringT.StringOf("good"));
		Assert.That(n, Is.SameAs(BoolT.False));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringStartsWith()
[Test]
	  public virtual void StringStartsWith()
	  {
		Val y = StringT.StringOf("goodbye").Receive(Overloads.StartsWith, Overloads.StartsWithString, StringT.StringOf("good"));
		Assert.That(y, Is.SameAs(BoolT.True));

		Val n = StringT.StringOf("goodbye").Receive(Overloads.StartsWith, Overloads.StartsWithString, StringT.StringOf("db"));
		Assert.That(n, Is.SameAs(BoolT.False));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringSize()
[Test]
	  public virtual void StringSize()
	  {
		Assert.That(StringT.StringOf("").Size(), Is.SameAs(IntT.IntZero));
		Assert.That(StringT.StringOf("hello world").Size(), Is.EqualTo(IntT.IntOf(11)));
		Assert.That(StringT.StringOf("\u65e5\u672c\u8a9e").Size(), Is.EqualTo(IntT.IntOf(3)));
	  }
	}

}