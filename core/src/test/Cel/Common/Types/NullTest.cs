﻿/*
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

using Cel.Common.Types;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;

namespace org.projectnessie.cel.common.types
{
	using Any = Google.Protobuf.WellKnownTypes.Any;
	using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class NullTest
	{

[Test]
	  public virtual void NullConvertToNativeJson()
	  {
		  Value expected = new Value();
		  expected.NullValue = Google.Protobuf.WellKnownTypes.NullValue.NullValue;

		// Json Value
		Value val = (Value)NullT.NullValue.ConvertToNative(typeof(Value));
		Assert.That(expected, Is.EqualTo(val));
	  }

[Test]
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

[Test]
	  public virtual void NullConvertToType()
	  {
		  Assert.That(NullT.NullValue.ConvertToType(NullT.NullType).Equal(NullT.NullValue), Is.SameAs(BoolT.True));

		Assert.That(NullT.NullValue.ConvertToType(StringT.StringType).Equal(StringT.StringOf("null")), Is.SameAs(BoolT.True));
		Assert.That(NullT.NullValue.ConvertToType(TypeT.TypeType).Equal(NullT.NullType), Is.SameAs(BoolT.True));
	  }

[Test]
	  public virtual void NullEqual()
	  {
		Assert.That(NullT.NullValue.Equal(NullT.NullValue), Is.SameAs(BoolT.True));
	  }

[Test]
	  public virtual void NullType()
	  {
		  Assert.That(NullT.NullValue.Type(), Is.SameAs(NullT.NullType));
	  }

[Test]
	  public virtual void NullValue()
	  {
		  Assert.That(NullT.NullValue.Value(), Is.EqualTo(Google.Protobuf.WellKnownTypes.NullValue.NullValue));
	  }
	}

}