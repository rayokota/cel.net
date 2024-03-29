﻿using Cel.Common.Types.Ref;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

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
///     Null type implementation.
/// </summary>
public sealed class NullT : BaseVal
{
    /// <summary>
    ///     NullType singleton.
    /// </summary>
    public static readonly IType NullType = TypeT.NewTypeValue(TypeEnum.Null);

    /// <summary>
    ///     NullValue singleton.
    /// </summary>
    public static readonly NullT NullValue = new();

    private static readonly Value PbValue;
    private static readonly Any PbAny;

    static NullT()
    {
        var value = new Value();
        value.NullValue = Google.Protobuf.WellKnownTypes.NullValue.NullValue;
        PbValue = value;

        PbAny = Any.Pack(PbValue);
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(Type typeDesc)
    {
        if (typeDesc == typeof(int)) return (int?)0;

        if (typeDesc == typeof(Any)) return PbAny;

        if (typeDesc == typeof(Value)) return PbValue;

        if (typeDesc == typeof(NullValue)) return Google.Protobuf.WellKnownTypes.NullValue.NullValue;

        if (typeDesc == typeof(IVal) || typeDesc == typeof(NullT)) return this;

        if (typeDesc == typeof(object)) return null;

        //		switch typeDesc.Kind() {
        //		case reflect.Interface:
        //			nv := n.Value()
        //			if reflect.TypeOf(nv).Implements(typeDesc) {
        //				return nv, nil
        //			}
        //			if reflect.TypeOf(n).Implements(typeDesc) {
        //				return n, nil
        //			}
        //		}
        // If the type conversion isn't supported return an error.
        throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", NullType,
            typeDesc.FullName));
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeValue)
    {
        switch (typeValue.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.String:
                return StringT.StringOf("null");
            case TypeEnum.InnerEnum.Null:
                return this;
            case TypeEnum.InnerEnum.Type:
                return NullType;
        }

        return Err.NewTypeConversionError(NullType, typeValue);
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        if (NullType != other.Type()) return Err.NoSuchOverload(this, "equal", other);

        return BoolT.True;
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return NullType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return Google.Protobuf.WellKnownTypes.NullValue.NullValue;
    }

    public override string ToString()
    {
        return "null";
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        return obj.GetType() == typeof(NullT);
    }
}