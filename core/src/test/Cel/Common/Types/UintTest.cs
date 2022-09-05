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

using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types
{
	using Any = Google.Protobuf.WellKnownTypes.Any;
	using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
	using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
	using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
	using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class UintTest
	{

[Test]
	  public virtual void UintAdd()
	  {
		Assert.That(UintT.UintOf(4).Add(UintT.UintOf(3)).Equal(UintT.UintOf(7)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(1).Add(StringT.StringOf("-1")), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(ulong.MaxValue).Add(UintT.UintOf(1)), Is.SameAs(Err.ErrUintOverflow));
		Assert.That(UintT.UintOf(ulong.MaxValue-1).Add(UintT.UintOf(1)).Equal(UintT.UintOf(ulong.MaxValue)), Is.SameAs(BoolT.True));
	  }

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

[Test]
	  public virtual void UintConvertToNativeAny()
	  {
		Any val = (Any)UintT.UintOf(ulong.MaxValue).ConvertToNative(typeof(Any));
		UInt64Value v = new UInt64Value();
		v.Value = ulong.MaxValue;
		Any want = Any.Pack(v);
		Assert.That(val, Is.EqualTo(want));
	  }

	  public virtual void UintConvertToNativeError()
	  {
		Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(int)), Throws.Exception);
	  }

[Test]
	  public virtual void UintConvertToWrapperError()
	  {
		  Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(Int32Value)), Throws.Exception);
		Assert.That(() => UintT.UintOf(10000).ConvertToNative(typeof(Int64Value)), Throws.Exception);
	  }

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

[Test]
	  public virtual void UintConvertToNativePtrUint32()
	  {
		int val = (int)UintT.UintOf(10000).ConvertToNative(typeof(int));
		Assert.That(val, Is.EqualTo(10000));
	  }

[Test]
	  public virtual void UintConvertToNativePtrUint64()
	  {
		// 18446744073709551612 --> -4L
		ulong val = (ulong)UintT.UintOf(ulong.MaxValue - 3).ConvertToNative(typeof(ulong));
		Assert.That(val, Is.EqualTo(ulong.MaxValue - 3));
	  }

[Test]
	  public virtual void UintConvertToNativeWrapper()
	  {
		uint val = (uint)UintT.UintOf(uint.MaxValue).ConvertToNative(typeof(UInt32Value));
		Assert.That(val, Is.EqualTo(uint.MaxValue));

		ulong val2 = (ulong)UintT.UintOf(ulong.MaxValue).ConvertToNative(typeof(UInt64Value));
		Assert.That(val2, Is.EqualTo(ulong.MaxValue));
	  }

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

[Test]
	  public virtual void UintDivide()
	  {
		Assert.That(UintT.UintOf(3).Divide(UintT.UintOf(2)).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintZero.Divide(UintT.UintZero), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(1).Divide(DoubleT.DoubleOf(-1)), Is.InstanceOf(typeof(Err)));
	  }

[Test]
	  public virtual void UintEqual()
	  {
		  Assert.That(UintT.UintOf(0).Equal(BoolT.False), Is.InstanceOf(typeof(Err)));
	  }

[Test]
	  public virtual void UintModulo()
	  {
		Assert.That(UintT.UintOf(21).Modulo(UintT.UintOf(2)).Equal(UintT.UintOf(1)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(21).Modulo(UintT.UintZero), Is.InstanceOf(typeof(Err)));
		Assert.That(UintT.UintOf(21).Modulo(IntT.IntOne), Is.InstanceOf(typeof(Err)));
	  }

[Test]
	  public virtual void UintMultiply()
	  {
		Assert.That(UintT.UintOf(2).Multiply(UintT.UintOf(2)).Equal(UintT.UintOf(4)), Is.SameAs(BoolT.True));
		Assert.That(UintT.UintOf(1).Multiply(DoubleT.DoubleOf(-4.0)), Is.InstanceOf(typeof(Err)));
		// maxUInt64 / 2 --> 0x7fffffffffffffffL
		Assert.That(UintT.UintOf(0x7fffffffffffffffL).Multiply(UintT.UintOf(3)), Is.SameAs(Err.ErrUintOverflow));
		Assert.That(UintT.UintOf(0x7fffffffffffffffL).Multiply(UintT.UintOf(2)).Equal(UintT.UintOf(0xfffffffffffffffeL)), Is.SameAs(BoolT.True));
	  }

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