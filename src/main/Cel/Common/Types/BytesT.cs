using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Parser;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = Cel.Common.Types.Ref.Type;
using Value = Google.Protobuf.WellKnownTypes.Value;

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

/// <summary>
///     Bytes type that implements ref.Val and supports add, compare, and size operations.
/// </summary>
public sealed class BytesT : BaseVal, Adder, Comparer, Sizer
{
    /// <summary>
    ///     BytesType singleton.
    /// </summary>
    public static readonly Type BytesType =
        TypeT.NewTypeValue(TypeEnum.Bytes, Trait.AdderType, Trait.ComparerType, Trait.SizerType);

    private readonly byte[] b;

    private BytesT(byte[] b)
    {
        this.b = b;
    }

    /// <summary>
    ///     Add implements traits.Adder interface method by concatenating byte sequences.
    /// </summary>
    public Val Add(Val other)
    {
        if (!(other is BytesT)) return Err.NoSuchOverload(this, "add", other);

        var o = ((BytesT)other).b;
        var n = new byte[b.Length + o.Length];
        Array.Copy(b, 0, n, 0, b.Length);
        Array.Copy(o, 0, n, b.Length, o.Length);
        return BytesOf(n);
    }

    /// <summary>
    ///     Compare implments traits.Comparer interface method by lexicographic ordering.
    /// </summary>
    public Val Compare(Val other)
    {
        if (!(other is BytesT)) return Err.NoSuchOverload(this, "compare", other);

        var o = ((BytesT)other).b;
        // unsigned !!!
        var l = b.Length;
        var ol = o.Length;
        var cl = Math.Min(l, ol);
        for (var i = 0; i < cl; i++)
        {
            var b1 = b[i];
            var b2 = o[i];
            var cmpUns = b1 - b2;
            if (cmpUns != 0) return IntT.IntOfCompare(cmpUns);
        }

        return IntT.IntOfCompare(l - ol);
    }

    /// <summary>
    ///     Size implements the traits.Sizer interface method.
    /// </summary>
    public Val Size()
    {
        return IntT.IntOf(b.Length);
    }

    public static BytesT BytesOf(byte[] b)
    {
        return new BytesT(b);
    }

    public static Val BytesOf(ByteString value)
    {
        return BytesOf(value.ToByteArray());
    }

    public static BytesT BytesOf(string s)
    {
        var encoding = StringT.UTF8;
        return new BytesT(encoding.GetBytes(s));
    }

    /// <summary>
    ///     ConvertToNative implements the ref.Val interface method.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(ByteString) || typeDesc == typeof(object)) return ByteString.CopyFrom(b);

        if (typeDesc == typeof(byte[])) return b;

        if (typeDesc == typeof(string))
            try
            {
                return Unescape.ToUtf8(new MemoryStream(b));
            }
            catch (Exception)
            {
                throw new Exception("invalid UTF-8 in bytes, cannot convert to string");
            }

        if (typeDesc == typeof(Any))
        {
            var value = new BytesValue();
            value.Value = ByteString.CopyFrom(b);
            return Any.Pack(value);
        }

        if (typeDesc == typeof(BytesValue)) return ByteString.CopyFrom(b);
        /*
            var value = new BytesValue();
            value.Value = ByteString.CopyFrom(b);
            return value;
            */
        if (typeDesc == typeof(MemoryStream)) return new MemoryStream(b);

        if (typeDesc == typeof(Val) || typeDesc == typeof(BytesT)) return this;

        if (typeDesc == typeof(Value))
        {
            // CEL follows the proto3 to JSON conversion by encoding bytes to a string via base64.
            // The encoding below matches the golang 'encoding/json' behavior during marshaling,
            // which uses base64.StdEncoding.
            var value = new Value();
            value.StringValue = Convert.ToBase64String(b);
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", BytesType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements the ref.Val interface method.
    /// </summary>
    public override Val ConvertToType(Type typeValue)
    {
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.String:
                try
                {
                    return StringT.StringOf(Unescape.ToUtf8(new MemoryStream(b)));
                }
                catch (Exception e)
                {
                    return Err.NewErr(e, "invalid UTF-8 in bytes, cannot convert to string");
                }
            case TypeEnum.InnerEnum.Bytes:
                return this;
            case TypeEnum.InnerEnum.Type:
                return BytesType;
        }

        return Err.NewTypeConversionError(BytesType, typeValue);
    }

    /// <summary>
    ///     Equal implements the ref.Val interface method.
    /// </summary>
    public override Val Equal(Val other)
    {
        if (!(other is BytesT)) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(b.SequenceEqual(((BytesT)other).b));
    }

    /// <summary>
    ///     Type implements the ref.Val interface method.
    /// </summary>
    public override Type Type()
    {
        return BytesType;
    }

    /// <summary>
    ///     Value implements the ref.Val interface method.
    /// </summary>
    public override object Value()
    {
        return b;
    }

    public override string ToString()
    {
        var constant = new Constant();
        constant.BytesValue = ByteString.CopyFrom(b);
        return "bytes{" + Debug.Debug.FormatLiteral(constant) + "}";
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var bytesT = (BytesT)o;
        return b.SequenceEqual(bytesT.b);
    }

    public override int GetHashCode()
    {
        var result = base.GetHashCode();
        result = 31 * result + b.GetHashCode();
        return result;
    }
}