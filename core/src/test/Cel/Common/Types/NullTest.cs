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

using Cel.Common.Types;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;

namespace org.projectnessie.cel.common.types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.NullT.NullType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TypeT.TypeType;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class NullTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullConvertToNative_Json()
[Test]
	  public virtual void NullConvertToNativeJson()
	  {
		  Value expected = new Value();
		  expected.NullValue = Google.Protobuf.WellKnownTypes.NullValue.NullValue;

		// Json Value
		Value val = (Value)NullT.NullValue.ConvertToNative(typeof(Value));
		Assert.That(expected, Is.EqualTo(val));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullConvertToNative() throws Exception
[Test]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void NullConvertToNative()
	  {
		  Value expected = new Value();
		  expected.NullValue = Google.Protobuf.WellKnownTypes.NullValue.NullValue;

		// google.protobuf.Any
		Any val = (Any) NullT.NullValue.ConvertToNative(typeof(Any));

		Value data = val.Unpack<Value>();
		Assert.That(expected, Is.EqualTo(data));

		// NullValue
		NullValue val2 = (NullValue) NullT.NullValue.ConvertToNative(typeof(NullValue));
		Assert.That(val2, Is.EqualTo(Google.Protobuf.WellKnownTypes.NullValue.NullValue));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullConvertToType()
[Test]
	  public virtual void NullConvertToType()
	  {
		  Assert.That(NullT.NullValue.ConvertToType(NullT.NullType).Equal(NullT.NullValue), Is.SameAs(BoolT.True));

		Assert.That(NullT.NullValue.ConvertToType(StringT.StringType).Equal(StringT.StringOf("null")), Is.SameAs(BoolT.True));
		Assert.That(NullT.NullValue.ConvertToType(TypeT.TypeType).Equal(NullT.NullType), Is.SameAs(BoolT.True));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullEqual()
[Test]
	  public virtual void NullEqual()
	  {
		Assert.That(NullT.NullValue.Equal(NullT.NullValue), Is.SameAs(BoolT.True));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullType()
[Test]
	  public virtual void NullType()
	  {
		  Assert.That(NullT.NullValue.Type(), Is.SameAs(NullT.NullType));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullValue()
[Test]
	  public virtual void NullValue()
	  {
		  Assert.That(NullT.NullValue.Value(), Is.EqualTo(Google.Protobuf.WellKnownTypes.NullValue.NullValue));
	  }
	}

}