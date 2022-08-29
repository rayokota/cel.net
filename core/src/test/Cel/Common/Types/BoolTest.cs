using NUnit.Framework;
using System;

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
    using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
    using Value = Google.Protobuf.WellKnownTypes.Value;

    [TestFixture]
    public class BoolTest
    {
        [Test]
        public virtual void BoolCompare()
        {
            Assert.That(BoolT.False.Compare(BoolT.True), Is.EqualTo(IntT.IntNegOne));
            Assert.That(BoolT.True.Compare(BoolT.False), Is.EqualTo(IntT.IntOne));
            Assert.That(BoolT.True.Compare(BoolT.True), Is.EqualTo(IntT.IntZero));
            Assert.That(BoolT.False.Compare(BoolT.False), Is.EqualTo(IntT.IntZero));
            Assert.True(Err.IsError(BoolT.True.Compare(UintT.UintZero)));
        }

        [Test]
        public virtual void BoolConvertToNativeAny()
        {
            object val = BoolT.True.ConvertToNative(typeof(Any));
            BoolValue boolValue = new BoolValue();
            boolValue.Value = true;
            Any pbVal = Any.Pack(boolValue);
            Assert.That(val, Is.EqualTo(pbVal));
        }

        [Test]
        public virtual void BoolConvertToNativeBool()
        {
            bool? val = (bool?)BoolT.True.ConvertToNative(typeof(Boolean));
            Assert.That(val, Is.True);
            val = (bool?)BoolT.False.ConvertToNative(typeof(bool));
            Assert.That(val, Is.False);
        }

        [Test]
        public virtual void BoolConvertToNativeError()
        {
            Assert.That(() => BoolT.True.ConvertToNative(typeof(string)), Throws.Exception.TypeOf<Exception>());
        }

        [Test]
        public virtual void BoolConvertToNativeJson()
        {
            Value val = (Value)BoolT.True.ConvertToNative(typeof(Value));
            Value pbVal = new Value();
            pbVal.BoolValue = true;
            Assert.That(val, Is.EqualTo(pbVal));
        }

        [Test]
        public virtual void BoolConvertToNativePtr()
        {
            bool? val = (bool?)BoolT.True.ConvertToNative(typeof(Boolean));
            Assert.That(val, Is.True);
        }

        [Test]
        public virtual void BoolConvertToNative()
        {
            bool? val = (bool?)BoolT.True.ConvertToNative(typeof(bool));
            Assert.That(val, Is.True);
        }

        [Test]
        public virtual void BoolConvertToNativeWrapper()
        {
            BoolValue val = (BoolValue)BoolT.True.ConvertToNative(typeof(BoolValue));
            BoolValue pbVal = new BoolValue();
            pbVal.Value = true;
            Assert.That(val, Is.EqualTo(pbVal));
        }

        [Test]
        public virtual void BoolConvertToType()
        {
            Assert.That(BoolT.True.ConvertToType(StringT.StringType).Equal(StringT.StringOf("True")),
                Is.EqualTo(BoolT.True));
            Assert.That(BoolT.True.ConvertToType(BoolT.BoolType), Is.EqualTo(BoolT.True));
            Assert.That(BoolT.True.ConvertToType(TypeT.TypeType), Is.SameAs(BoolT.BoolType));
            Assert.That(BoolT.True.ConvertToType(TimestampT.TimestampType), Is.InstanceOf(typeof(Err)));
        }


        [Test]
        public virtual void BoolNegate()
        {
            Assert.That(BoolT.True.Negate(), Is.EqualTo(BoolT.False));
            Assert.That(BoolT.False.Negate(), Is.EqualTo(BoolT.True));
        }

        [Test]
        public virtual void BoolPredefined()
        {
            Assert.That(Types.BoolOf(true), Is.SameAs(BoolT.True));
            Assert.That(Types.BoolOf(false), Is.SameAs(BoolT.False));
            Assert.That(Types.BoolOf(true), Is.SameAs(BoolT.True));
            Assert.That(Types.BoolOf(false), Is.SameAs(BoolT.False));
        }
    }
}