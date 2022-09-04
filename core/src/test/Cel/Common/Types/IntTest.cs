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

using NodaTime;
using NUnit.Framework;

namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.That;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.Err.ErrIntOverflow;
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
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintOf;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
    using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
    using Value = Google.Protobuf.WellKnownTypes.Value;

    public class IntTest
    {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intAdd()
[Test]
        public virtual void IntAdd()
        {
            Assert.That(IntT.IntOf(4).Add(IntT.IntOf(-3)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(-1).Add(StringT.StringOf("-1")), Is.InstanceOf(typeof(Err)));
            for (int i = 1; i <= 10; i++)
            {
                Assert.That(IntT.IntOf(long.MaxValue).Add(IntT.IntOf(i)), Is.SameAs(Err.ErrIntOverflow));
                Assert.That(IntT.IntOf(long.MinValue).Add(IntT.IntOf(-i)), Is.SameAs(Err.ErrIntOverflow));
                Assert.That(IntT.IntOf(long.MaxValue - i).Add(IntT.IntOf(i)), Is.EqualTo(IntT.IntOf(long.MaxValue)));
                Assert.That(IntT.IntOf(long.MinValue + i).Add(IntT.IntOf(-i)), Is.EqualTo(IntT.IntOf(long.MinValue)));
            }
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intCompare()
[Test]
        public virtual void IntCompare()
        {
            IntT lt = IntT.IntOf(-1300);
            IntT gt = IntT.IntOf(204);
            Assert.That(lt.Compare(gt), Is.SameAs(IntT.IntNegOne));
            Assert.That(gt.Compare(lt), Is.SameAs(IntT.IntOne));
            Assert.That(gt.Compare(gt), Is.SameAs(IntT.IntZero));
            Assert.That(gt.Compare(TypeT.TypeType), Is.InstanceOf(typeof(Err)));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Any()
[Test]
        public virtual void IntConvertToNativeAny()
        {
            Any val = (Any) IntT.IntOf(long.MaxValue).ConvertToNative(typeof(Any));
            Int64Value v = new Int64Value();
            v.Value = long.MaxValue;
            Any want = Any.Pack(v);
            Assert.That(val, Is.EqualTo(want));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Error()
[Test]
        public virtual void IntConvertToNativeError()
        {
            Value val = (Value) IntT.IntOf(1).ConvertToNative(typeof(Value));
            //          		if err == nil {
            //          			t.Errorf("Got '%v', expected error", val)
            //          		}
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Int32()
[Test]
        public virtual void IntConvertToNativeInt32()
        {
            int val = (int)IntT.IntOf(20050).ConvertToNative(typeof(int));
            Assert.That(val, Is.EqualTo(20050));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Int64()
[Test]
        public virtual void IntConvertToNativeInt64()
        {
            // Value greater than max int32.
            long val = (long)IntT.IntOf(4147483648L).ConvertToNative(typeof(long));
            Assert.That(val, Is.EqualTo(4147483648L));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Json()
[Test]
        public virtual void IntConvertToNativeJson()
        {
            // Value can be represented accurately as a JSON number.
            Value val = (Value)IntT.IntOf(IntT.MaxIntJSON).ConvertToNative(typeof(Value));
            Value v = new Value();
            v.NumberValue = 9007199254740991.0;
            Assert.That(val, Is.EqualTo(v));

            // Value converts to a JSON decimal string.
            val = (Value)IntT.IntOf(IntT.MaxIntJSON + 1).ConvertToNative(typeof(Value));
            v = new Value();
            v.StringValue = "9007199254740992";
            Assert.That(val, Is.EqualTo(v));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Ptr_Int32()
[Test]
        public virtual void IntConvertToNativePtrInt32()
        {
            int val = (int) IntT.IntOf(20050).ConvertToNative(typeof(int));
            Assert.That(val, Is.EqualTo(20050));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Ptr_Int64()
[Test]
        public virtual void IntConvertToNativePtrInt64()
        {
            // Value greater than max int32.
            long val = (long)IntT.IntOf(1L + int.MaxValue).ConvertToNative(typeof(long));
            Assert.That(val, Is.EqualTo(1L + int.MaxValue));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToNative_Wrapper()
[Test]
        public virtual void IntConvertToNativeWrapper()
        {
            Int32Value val = (Int32Value)IntT.IntOf(int.MaxValue).ConvertToNative(typeof(Int32Value));
            Int32Value want = new Int32Value();
            want.Value = int.MaxValue;
            Assert.That(val, Is.EqualTo(want));

            Int64Value val2 = (Int64Value)IntT.IntOf(long.MinValue).ConvertToNative(typeof(Int64Value));
            Int64Value want2 = new Int64Value();
            want2.Value = long.MinValue;
            Assert.That(val2, Is.EqualTo(want2));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intConvertToType()
[Test]
        public virtual void IntConvertToType()
        {
            Assert.That(IntT.IntOf(-4).ConvertToType(IntT.IntType).Equal(IntT.IntOf(-4)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(-1).ConvertToType(UintT.UintType), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(-4).ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(-4)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(-4).ConvertToType(StringT.StringType).Equal(StringT.StringOf("-4")), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(-4).ConvertToType(TypeT.TypeType), Is.SameAs(IntT.IntType));
            Assert.That(IntT.IntOf(-4).ConvertToType(DurationT.DurationType), Is.InstanceOf(typeof(Err)));
            int celtsSecs = 946684800;
            TimestampT celts = TimestampT.TimestampOf(Instant.FromUnixTimeSeconds(celtsSecs).InZone(TimestampT.ZoneIdZ));
            Assert.That(IntT.IntOf(celtsSecs).ConvertToType(TimestampT.TimestampType).Equal(celts), Is.SameAs(BoolT.True));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intDivide()
[Test]
        public virtual void IntDivide()
        {
            Assert.That(IntT.IntOf(3).Divide(IntT.IntOf(2)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntZero.Divide(IntT.IntZero), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(1).Divide(DoubleT.DoubleOf(-1)), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(long.MinValue).Divide(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intEqual()
[Test]
        public virtual void IntEqual()
        {
            Assert.That(IntT.IntOf(0).Equal(BoolT.False), Is.InstanceOf(typeof(Err)));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intModulo()
[Test]
        public virtual void IntModulo()
        {
            Assert.That(IntT.IntOf(21).Modulo(IntT.IntOf(2)).Equal(IntT.IntOf(1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(21).Modulo(IntT.IntZero), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(21).Modulo(UintT.UintZero), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(long.MinValue).Modulo(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intMultiply()
[Test]
        public virtual void IntMultiply()
        {
            Assert.That(IntT.IntOf(2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(-4)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(1).Multiply(DoubleT.DoubleOf(-4.0)), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(3)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MinValue / 2).Multiply(IntT.IntOf(3)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(2)).Equal(IntT.IntOf(long.MaxValue - 1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(long.MinValue / 2).Multiply(IntT.IntOf(2)).Equal(IntT.IntOf(long.MinValue)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(long.MaxValue / 2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(long.MinValue + 2)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf((long.MinValue + 2) / 2).Multiply(IntT.IntOf(-2)).Equal(IntT.IntOf(long.MaxValue - 1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(long.MinValue).Multiply(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intNegate()
[Test]
        public virtual void IntNegate()
        {
            Assert.That(IntT.IntOf(1).Negate().Equal(IntT.IntOf(-1)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(long.MinValue).Negate(), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MaxValue).Negate().Equal(IntT.IntOf(long.MinValue + 1)), Is.SameAs(BoolT.True));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intSubtract()
[Test]
        public virtual void IntSubtract()
        {
            Assert.That(IntT.IntOf(4).Subtract(IntT.IntOf(-3)).Equal(IntT.IntOf(7)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(1).Subtract(UintT.UintOf(1)), Is.InstanceOf(typeof(Err)));
            Assert.That(IntT.IntOf(long.MaxValue).Subtract(IntT.IntOf(-1)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MinValue).Subtract(IntT.IntOf(1)), Is.SameAs(Err.ErrIntOverflow));
            Assert.That(IntT.IntOf(long.MaxValue - 1).Subtract(IntT.IntOf(-1)).Equal(IntT.IntOf(long.MaxValue)), Is.SameAs(BoolT.True));
            Assert.That(IntT.IntOf(long.MinValue + 1).Subtract(IntT.IntOf(1)).Equal(IntT.IntOf(long.MinValue)), Is.SameAs(BoolT.True));
        }
    }
}