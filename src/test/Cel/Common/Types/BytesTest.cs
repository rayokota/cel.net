using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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
namespace Cel.Common.Types;

public class BytesTest
{
    [Test]
    public virtual void BytesAdd()
    {
        Assert.That(BytesT.BytesOf("hello").Add(BytesT.BytesOf("world")).Equal(BytesT.BytesOf("helloworld")),
            Is.SameAs(BoolT.True));
        Assert.That(Err.IsError(BytesT.BytesOf("hello").Add(StringT.StringOf("world"))), Is.True);
    }

    [Test]
    public virtual void BytesCompare()
    {
        Assert.That(BytesT.BytesOf("1234").Compare(BytesT.BytesOf("2345")).Equal(IntT.IntNegOne),
            Is.SameAs(BoolT.True));
        Assert.That(BytesT.BytesOf("2345").Compare(BytesT.BytesOf("1234")).Equal(IntT.IntOne), Is.SameAs(BoolT.True));
        Assert.That(BytesT.BytesOf("2345").Compare(BytesT.BytesOf("2345")).Equal(IntT.IntZero), Is.SameAs(BoolT.True));
        Assert.That(Err.IsError(BytesT.BytesOf("1").Compare(StringT.StringOf("1"))), Is.True);
    }

    [Test]
    public virtual void BytesConvertToNativeAny()
    {
        var val = (Any)BytesT.BytesOf("123").ConvertToNative(typeof(Any));
        var bytesValue = new BytesValue();
        bytesValue.Value = ByteString.CopyFromUtf8("123");
        var want = Any.Pack(bytesValue);
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void BytesConvertToNativeByteSlice()
    {
        var val = (byte[])BytesT.BytesOf("123").ConvertToNative(typeof(byte[]));
        Assert.That(val, Is.EquivalentTo(new byte[] { 49, 50, 51 }));
    }

    [Test]
    public virtual void BytesConvertToNativeByteString()
    {
        var val = (ByteString)BytesT.BytesOf("123").ConvertToNative(typeof(ByteString));
        Assert.That(val, Is.EqualTo(ByteString.CopyFrom(49, 50, 51)));
    }

    [Test]
    public virtual void BytesConvertToNativeByteBuffer()
    {
        var val = (MemoryStream)BytesT.BytesOf("123").ConvertToNative(typeof(MemoryStream));
        Assert.That(val, Is.EqualTo(new MemoryStream(new byte[] { 49, 50, 51 })));
    }

    [Test]
    public virtual void BytesConvertToNativeError()
    {
        Assert.That(BytesT.BytesOf("123").ConvertToNative(typeof(string)), Is.EqualTo("123"));
    }

    [Test]
    public virtual void BytesConvertToNativeJson()
    {
        var val = (Value)BytesT.BytesOf("123").ConvertToNative(typeof(Value));
        var want = new Value();
        want.StringValue = "MTIz";
        Assert.That(val, Is.EqualTo(want));
    }

    [Test]
    public virtual void BytesConvertToNativeWrapper()
    {
        var val = (byte[])BytesT.BytesOf("123").ConvertToNative(typeof(byte[]));
        var want = Encoding.UTF8.GetBytes("123");
        Assert.That(val, Is.EquivalentTo(want));
    }

    [Test]
    public virtual void BytesConvertToType()
    {
        Assert.That(BytesT.BytesOf("hello world").ConvertToType(BytesT.BytesType).Equal(BytesT.BytesOf("hello world")),
            Is.SameAs(BoolT.True));
        Assert.That(
            BytesT.BytesOf("hello world").ConvertToType(StringT.StringType).Equal(StringT.StringOf("hello world")),
            Is.SameAs(BoolT.True));
        Assert.That(BytesT.BytesOf("hello world").ConvertToType(TypeT.TypeType).Equal(BytesT.BytesType),
            Is.SameAs(BoolT.True));
        Assert.That(Err.IsError(BytesT.BytesOf("hello").ConvertToType(IntT.IntType)), Is.True);
    }

    [Test]
    public virtual void BytesSize()
    {
        Assert.That(BytesT.BytesOf("1234567890").Size().Equal(IntT.IntOf(10)), Is.SameAs(BoolT.True));
    }
}