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

using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.That;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.ThatThrownBy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.Err.ErrUintOverflow;
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
//	import static org.projectnessie.cel.common.types.IntT.IntT.MaxIntJSON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.MapT.MapT.MapType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TypeT.TypeT.TypeType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
	using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
	using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
	using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class UintTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintAdd()
[Test]
	  public virtual void UintAdd()
	  {
		Assert.That(UintT.UintOf(4).Add(UintT.UintOf(3)).Equal(UintT.UintOf(7)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(1).Add(StringT.StringOf("-1")), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(ulong.MaxValue).Add(UintT.UintOf(1)), Is.SameAs(Err.ErrUintOverflow));
		Assert.That(UintT.UintOf(ulong.MaxValue-1).Add(UintT.UintOf(1)).Equal(UintT.UintOf(ulong.MaxValue)), Is.SameAs(BoolT.True));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintCompare()
[Test]
	  public virtual void UintCompare()
	  {
		UintT lt = UintT.UintOf(204);
		UintT gt = UintT.UintOf(1300);
		Assert.That(lt.Compare(gt).Equal(IntT.IntNegOne), Is.SameAs(BoolT.True));
		Assert.That(gt.Compare(lt).Equal(IntT.IntOne), Is.SameAs(BoolT.True));
		Assert.That(gt.Compare(gt).Equal(IntT.IntZero), Is.SameAs(BoolT.True));
		Assert.That(gt.Compare(TypeT.TypeType), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToNative_Any()
[Test]
	  public virtual void UintConvertToNativeAny()
	  {
		Any val = (Any)UintT.UintOf(ulong.MaxValue).ConvertToNative(typeof(Any));
		UInt64Value v = new UInt64Value();
		v.Value = ulong.MaxValue;
		Any want = Any.Pack(v);
		Assert.That(val, Is.EqualTo(want));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Disabled("CANNOT IMPLEMENT - JAVA VS GO - SIGNED VS UNSIGNED") void uintConvertToNative_Error()
	  public virtual void UintConvertToNativeError()
	  {
		Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(int)), Throws.Exception);
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToWrapper_Error()
[Test]
	  public virtual void UintConvertToWrapperError()
	  {
		  Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(Int32Value)), Throws.Exception);
		Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(Int64Value)), Throws.Exception);
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToNative_Json()
[Test]
	  public virtual void UintConvertToNativeJson()
	  {
		// Value can be represented accurately as a JSON number.
		Value val = (Value)UintT.UintOf((ulong)IntT.MaxIntJSON).ConvertToNative(typeof(Value));
		Value val2 = new Value();
		val2.NumberValue = 9007199254740991.0d;
		Assert.That(val, Is.EqualTo(val2));

		// Value converts to a JSON decimal string
		val = (Value)IntT.IntOf(IntT.MaxIntJSON + 1).ConvertToNative(typeof(Value));
		val2 = new Value();
		val2.StringValue = "9007199254740992";
		Assert.That(val, Is.EqualTo(val2));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToNative_Ptr_Uint32()
[Test]
	  public virtual void UintConvertToNativePtrUint32()
	  {
		int val = (int)UintT.UintOf(10000).ConvertToNative(typeof(int));
		Assert.That(val, Is.EqualTo(10000));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToNative_Ptr_Uint64()
[Test]
	  public virtual void UintConvertToNativePtrUint64()
	  {
		// 18446744073709551612 --> -4L
		ulong val = (ulong)UintT.UintOf(ulong.MaxValue - 3).ConvertToNative(typeof(ulong));
		Assert.That(val, Is.EqualTo(ulong.MaxValue - 3));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToNative_Wrapper()
[Test]
	  public virtual void UintConvertToNativeWrapper()
	  {
		UInt32Value val = (UInt32Value)UintT.UintOf(uint.MaxValue).ConvertToNative(typeof(UInt32Value));
		UInt32Value want = new UInt32Value();
		want.Value = uint.MaxValue;
		Assert.That(val, Is.EqualTo(want));

		UInt64Value val2 = (UInt64Value)UintT.UintOf(ulong.MaxValue).ConvertToNative(typeof(UInt64Value));
		UInt64Value want2 = new UInt64Value();
		want2.Value = ulong.MaxValue;
		Assert.That(val2, Is.EqualTo(want2));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintConvertToType()
[Test]
	  public virtual void UintConvertToType()
	  {
		// 18446744073709551612L
		// --> 0xFFFFFFFFFFFFFFFCL
		Assert.That(UintT.UintOf(0xFFFFFFFFFFFFFFFCL).ConvertToType(IntT.IntType), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(4).ConvertToType(IntT.IntType).Equal(IntT.IntOf(4)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(4).ConvertToType(UintT.UintType).Equal(UintT.UintOf(4)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(4).ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(4)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(4).ConvertToType(StringT.StringType).Equal(StringT.StringOf("4")), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(4).ConvertToType(TypeT.TypeType).Equal(UintT.UintType), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(4).ConvertToType(MapT.MapType), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintDivide()
[Test]
	  public virtual void UintDivide()
	  {
		Assert.That(UintT.UintOf(3).Divide(UintT.UintOf(2)).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintZero.Divide(UintT.UintZero), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(1).Divide(DoubleT.DoubleOf(-1)), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintEqual()
[Test]
	  public virtual void UintEqual()
	  {
		  Assert.That(UintT.UintOf(0).Equal(BoolT.False), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintModulo()
[Test]
	  public virtual void UintModulo()
	  {
		Assert.That(UintT.UintOf(21).Modulo(UintT.UintOf(2)).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(21).Modulo(UintT.UintZero), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(21).Modulo(IntT.IntOne), Is.InstanceOf(typeof(Err)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintMultiply()
[Test]
	  public virtual void UintMultiply()
	  {
		Assert.That(UintT.UintOf(2).Multiply(UintT.UintOf(2)).Equal(UintT.UintOf(4)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(1).Multiply(DoubleT.DoubleOf(-4.0)), Is.InstanceOf(typeof(Err)));
		// maxUInt64 / 2 --> 0x7fffffffffffffffL
		Assert.That(UintT.UintOf(0x7fffffffffffffffL).Multiply(UintT.UintOf(3)), Is.SameAs(Err.ErrUintOverflow));
		Assert.That(UintT.UintOf(0x7fffffffffffffffL).Multiply(UintT.UintOf(2)).Equal(UintT.UintOf(0xfffffffffffffffeL)), Is.SameAs(BoolT.True));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uintSubtract()
[Test]
	  public virtual void UintSubtract()
	  {
		Assert.That(UintT.UintOf(4).Subtract(UintT.UintOf(3)).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(1).Subtract(IntT.IntOf(1)), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(0xfffffffffffffffeL).Subtract(UintT.UintOf(0xffffffffffffffffL)), Is.SameAs(Err.ErrUintOverflow));
		Assert.That(UintT.UintOf(0xffffffffffffffffL).Subtract(UintT.UintOf(0xffffffffffffffffL)).Equal(UintT.UintOf(0)), Is.SameAs(BoolT.True));
	  }
	}

}