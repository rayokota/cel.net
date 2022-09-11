using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.WellKnownTypes;

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
///     Bool type that implements ref.Val and supports comparison and negation.
/// </summary>
public sealed class BoolT : BaseVal, IComparer, INegater
{
    /// <summary>
    ///     BoolType singleton.
    /// </summary>
    public static readonly IType BoolType = TypeT.NewTypeValue(TypeEnum.Bool, Trait.ComparerType, Trait.NegatorType);

    /// <summary>
    ///     Boolean constants
    /// </summary>
    public static readonly BoolT False = new(false);

    public static readonly BoolT True = new(true);

    private readonly bool b;

    internal BoolT(bool b)
    {
        this.b = b;
    }

    /// <summary>
    ///     Compare implements the traits.Comparer interface method.
    /// </summary>
    public IVal Compare(IVal other)
    {
        if (!(other is BoolT)) return Err.NoSuchOverload(this, "compare", other);

        return IntT.IntOfCompare(b.CompareTo(((BoolT)other).b));
    }

    /// <summary>
    ///     Negate implements the traits.Negater interface method.
    /// </summary>
    public IVal Negate()
    {
        return Types.BoolOf(!b);
    }

    public override bool BooleanValue()
    {
        return b;
    }

    /// <summary>
    ///     ConvertToNative implements the ref.Val interface method.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(bool) || typeDesc == typeof(object))
            return Convert.ToBoolean(b);

        if (typeDesc == typeof(Any))
        {
            var value = new BoolValue();
            value.Value = b;
            return Any.Pack(value);
        }

        if (typeDesc == typeof(BoolValue)) return b;
        /*
            var value = new BoolValue();
            value.Value = b;
            return value;
            */
        if (typeDesc == typeof(IVal) || typeDesc == typeof(BoolT)) return this;

        if (typeDesc == typeof(Value))
        {
            var value = new Value();
            value.BoolValue = b;
            return value;
        }

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", BoolType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements the ref.Val interface method.
    /// </summary>
    public override IVal ConvertToType(IType typeVal)
    {
        switch (typeVal.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf(Convert.ToString(b));
            case TypeEnum.InnerEnum.Bool:
                return this;
            case TypeEnum.InnerEnum.Type:
                return BoolType;
        }

        return Err.NewTypeConversionError(BoolType, typeVal);
    }

    /// <summary>
    ///     Equal implements the ref.Val interface method.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        if (!(other is BoolT)) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(b == ((BoolT)other).b);
    }

    /// <summary>
    ///     Type implements the ref.Val interface method.
    /// </summary>
    public override IType Type()
    {
        return BoolType;
    }

    /// <summary>
    ///     Value implements the ref.Val interface method.
    /// </summary>
    public override object Value()
    {
        return b;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var boolT = (BoolT)o;
        return b == boolT.b;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), b);
    }
}