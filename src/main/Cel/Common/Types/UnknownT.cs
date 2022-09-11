using Cel.Common.Types.Ref;

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
///     Unknown type implementation which collects expression ids which caused the current value to
///     become unknown.
/// </summary>
public sealed class UnknownT : BaseVal
{
    /// <summary>
    ///     UnknownType singleton.
    /// </summary>
    public static readonly IType UnknownType = TypeT.NewTypeValue(TypeEnum.Unknown);

    private readonly long value;

    private UnknownT(long value)
    {
        this.value = value;
    }

    public static UnknownT UnknownOf(long value)
    {
        return new UnknownT(value);
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(long) ||
            typeDesc == typeof(object))
            return value;

        if (typeDesc == typeof(IVal) || typeDesc == typeof(UnknownT)) return this;

        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", UnknownType,
            typeDesc.FullName));
    }

    public override long IntValue()
    {
        return value;
    }

    public override ulong UintValue()
    {
        return Convert.ToUInt64(value);
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeVal)
    {
        return this;
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        return this;
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return UnknownType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return value;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var unknownT = (UnknownT)o;
        return value == unknownT.value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), value);
    }

    /// <summary>
    ///     IsUnknown returns whether the element ref.Type or ref.Val is equal to the UnknownType
    ///     singleton.
    /// </summary>
    public static bool IsUnknown(object? val)
    {
        return val != null && val.GetType() == typeof(UnknownT);
    }
}