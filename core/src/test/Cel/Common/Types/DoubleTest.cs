using System;
using NUnit.Framework;

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
namespace Cel.Common.Types
{
	using Any = Google.Protobuf.WellKnownTypes.Any;
	using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
	using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;

	public class DoubleTest
	{

[Test]
	  public virtual void DoubleAdd()
	  {
		Assert.That(DoubleT.DoubleOf(4).Add(DoubleT.DoubleOf(-3.5)).Equal(DoubleT.DoubleOf(0.5)), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(DoubleT.DoubleOf(-1).Add(StringT.StringOf("-1"))), Is.True);
	  }

[Test]
	  public virtual void DoubleCompare()
	  {
		DoubleT lt = DoubleT.DoubleOf(-1300);
		DoubleT gt = DoubleT.DoubleOf(204);
		Assert.That(lt.Compare(gt).Equal(IntT.IntNegOne), Is.SameAs(BoolT.True));
		Assert.That(gt.Compare(lt).Equal(IntT.IntOne), Is.SameAs(BoolT.True));
		Assert.That(gt.Compare(gt).Equal(IntT.IntZero), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(gt.Compare(TypeT.TypeType)), Is.True);
	  }

[Test]
	  public virtual void DoubleConvertToNativeZeros()
	  {
		DoubleT p0 = DoubleT.DoubleOf(0.0d);
		DoubleT n0 = DoubleT.DoubleOf(-0.0d);
		Assert.That(p0.Equal(n0), Is.SameAs(BoolT.True));
		Assert.That(n0.Compare(p0), Is.SameAs(IntT.IntZero)); // "-0.0 < 0.0" --> BoolT.False
	  }

[Test]
	  public virtual void DoubleConvertToNativeAny()
	  {
		Any val = (Any) DoubleT.DoubleOf(double.MaxValue).ConvertToNative(typeof(Any));
		DoubleValue dv = new DoubleValue();
		dv.Value = 1.7976931348623157e+308;
		Any want = Any.Pack(dv);
		Assert.That(val, Is.EqualTo(want));
	  }

[Test]
	  public virtual void DoubleConvertToNativeError()
	  {
		  Assert.That(() => DoubleT.DoubleOf(-10000).ConvertToNative(typeof(string)), Throws.Exception.InstanceOf(typeof(Exception)));
	  }

[Test]
	  public virtual void DoubleConvertToNativeFloat32()
	  {
		float val = (float) DoubleT.DoubleOf(3.1415).ConvertToNative(typeof(float));
		Assert.That(val, Is.EqualTo(3.1415f));
	  }

[Test]
	  public virtual void DoubleConvertToNativeFloat64()
	  {
		double val = (double) DoubleT.DoubleOf(30000000.1).ConvertToNative(typeof(double));
		Assert.That(val, Is.EqualTo(30000000.1d));
	  }

[Test]
	  public virtual void DoubleConvertToNativeJson()
	  {
		Value val = (Value) DoubleT.DoubleOf(-1.4).ConvertToNative(typeof(Value));

		Value pbVal = new Value();
		pbVal.NumberValue = -1.4;
		Assert.That(val, Is.EqualTo(pbVal));

		val = (Value) DoubleT.DoubleOf(Double.NaN).ConvertToNative(typeof(Value));
		Assert.That(double.IsNaN(val.NumberValue), Is.True);

		val = (Value) DoubleT.DoubleOf(double.NegativeInfinity).ConvertToNative(typeof(Value));
		pbVal = new Value();
		pbVal.NumberValue = double.NegativeInfinity;
		Assert.That(val, Is.EqualTo(pbVal));

		val = (Value) DoubleT.DoubleOf(double.PositiveInfinity).ConvertToNative(typeof(Value));
		pbVal = new Value();
		pbVal.NumberValue = double.PositiveInfinity;
		Assert.That(val, Is.EqualTo(pbVal));
	  }

[Test]
	  public virtual void DoubleConvertToNativePtrFloat32()
	  {
		float val = (float) DoubleT.DoubleOf(3.1415).ConvertToNative(typeof(float));
		Assert.That(val, Is.EqualTo(3.1415f));
	  }

[Test]
	  public virtual void DoubleConvertToNativePtrFloat64()
	  {
		double val = (double) DoubleT.DoubleOf(30000000.1).ConvertToNative(typeof(double));
		Assert.That(val, Is.EqualTo(30000000.1d));
	  }

[Test]
	  public virtual void DoubleConvertToNativeWrapper()
	  {
		float val = (float) DoubleT.DoubleOf(3.1415d).ConvertToNative(typeof(FloatValue));
		float want = 3.1415f;
		Assert.That(val, Is.EqualTo(want));

		double val2 = (double) DoubleT.DoubleOf(double.MaxValue).ConvertToNative(typeof(DoubleValue));
		double want2 = 1.7976931348623157e+308d;
		Assert.That(val2, Is.EqualTo(want2));
	  }

[Test]
	  public virtual void DoubleConvertToType()
	  {
		// NOTE: the original Go test assert on `IntT.IntOf(-5)`, because Go's implementation uses
		// the Go `math.Round(float64)` function. The implementation of Go's `math.Round(float64)`
		// behaves differently to Java's `Math.round(double)` (or `Math.rint()`).
		// Further, the CEL-spec conformance tests assert on a different behavior and therefore those
		// conformance-tests fail against the Go implementation.
		// Even more complicated: the CEL-spec says: "CEL provides no way to control the finer points
		// of floating-point arithmetic, such as expression evaluation, rounding mode, or exception
		// handling. However, any two not-a-number values will compare equal even if their underlying
		// properties are different."
		// (see https://github.com/google/cel-spec/blob/master/doc/langdef.md#numeric-values)
		Assert.That(DoubleT.DoubleOf(-4.5d).ConvertToType(IntT.IntType).Equal(IntT.IntOf(-4)), Is.SameAs(BoolT.True));

		Assert.That(Err.IsError(DoubleT.DoubleOf(-4.5d).ConvertToType(UintT.UintType)), Is.True);
		Assert.That(DoubleT.DoubleOf(-4.5d).ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(-4.5)), Is.SameAs(BoolT.True));
		Assert.That(DoubleT.DoubleOf(-4.5d).ConvertToType(StringT.StringType).Equal(StringT.StringOf("-4.5")), Is.SameAs(BoolT.True));
		Assert.That(DoubleT.DoubleOf(-4.5d).ConvertToType(TypeT.TypeType).Equal(DoubleT.DoubleType), Is.SameAs(BoolT.True));
	  }

[Test]
	  public virtual void DoubleDivide()
	  {
		Assert.That(DoubleT.DoubleOf(3).Divide(DoubleT.DoubleOf(1.5)).Equal(DoubleT.DoubleOf(2)), Is.SameAs(BoolT.True));
		double z = 0.0d; // Avoid 0.0 since const div by zero is an error.
		Assert.That(DoubleT.DoubleOf(1.1).Divide(DoubleT.DoubleOf(0)).Equal(DoubleT.DoubleOf(1.1 / z)), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(DoubleT.DoubleOf(1.1).Divide(IntT.IntNegOne)), Is.True);
	  }

[Test]
	  public virtual void DoubleEqual()
	  {
		  Assert.That(Err.IsError(DoubleT.DoubleOf(0).Equal(BoolT.False)), Is.True);
	  }

[Test]
	  public virtual void DoubleMultiply()
	  {
		Assert.That(DoubleT.DoubleOf(1.1).Multiply(DoubleT.DoubleOf(-1.2)).Equal(DoubleT.DoubleOf(-1.32)), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(DoubleT.DoubleOf(1.1).Multiply(IntT.IntNegOne)), Is.True);
	  }

[Test]
	  public virtual void DoubleNegate()
	  {
		Assert.That(DoubleT.DoubleOf(1.1).Negate().Equal(DoubleT.DoubleOf(-1.1)), Is.SameAs(BoolT.True));
	  }

[Test]
	  public virtual void DoubleSubtract()
	  {
		Assert.That(DoubleT.DoubleOf(4).Subtract(DoubleT.DoubleOf(-3.5)).Equal(DoubleT.DoubleOf(7.5)), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(DoubleT.DoubleOf(1.1).Subtract(IntT.IntNegOne)), Is.True);
	  }
	}

}