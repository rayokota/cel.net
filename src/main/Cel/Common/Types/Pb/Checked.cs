using Google.Protobuf.WellKnownTypes;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Common.Types.Pb;

public sealed class Checked
{
    // common types
    public static readonly Type checkedDyn;

    // Wrapper and primitive types.
    public static readonly Type checkedBool = CheckedPrimitive(Type.Types.PrimitiveType.Bool);
    public static readonly Type checkedBytes = CheckedPrimitive(Type.Types.PrimitiveType.Bytes);
    public static readonly Type checkedDouble = CheckedPrimitive(Type.Types.PrimitiveType.Double);
    public static readonly Type checkedInt = CheckedPrimitive(Type.Types.PrimitiveType.Int64);
    public static readonly Type checkedString = CheckedPrimitive(Type.Types.PrimitiveType.String);

    public static readonly Type checkedUint = CheckedPrimitive(Type.Types.PrimitiveType.Uint64);

    // Well-known type equivalents.
    public static readonly Type checkedAny = CheckedWellKnown(Type.Types.WellKnownType.Any);
    public static readonly Type checkedDuration = CheckedWellKnown(Type.Types.WellKnownType.Duration);

    public static readonly Type checkedTimestamp = CheckedWellKnown(Type.Types.WellKnownType.Timestamp);

    // Json-based type equivalents.
    public static readonly Type checkedNull;
    public static readonly Type checkedListDyn;
    public static readonly Type checkedMapStringDyn;

    /// <summary>
    ///     CheckedPrimitives map from proto field descriptor type to expr.Type.
    /// </summary>
    public static readonly IDictionary<Field.Types.Kind, Type> CheckedPrimitives =
        new Dictionary<Field.Types.Kind, Type>();

    /// <summary>
    ///     CheckedWellKnowns map from qualified proto type name to expr.Type for well-known proto types.
    /// </summary>
    public static readonly IDictionary<string, Type> CheckedWellKnowns = new Dictionary<string, Type>();

    static Checked()
    {
        var type = new Type();
        type.Dyn = new Empty();
        checkedDyn = type;

        type = new Type();
        type.Null = NullValue.NullValue;
        checkedNull = type;

        type = new Type();
        var list = new Type.Types.ListType();
        list.ElemType = checkedDyn;
        type.ListType = list;
        checkedListDyn = type;

        type = new Type();
        var map = new Type.Types.MapType();
        map.KeyType = checkedString;
        map.ValueType = checkedDyn;
        type.MapType = map;
        checkedMapStringDyn = type;

        CheckedPrimitives[Field.Types.Kind.TypeBool] = checkedBool;
        CheckedPrimitives[Field.Types.Kind.TypeBytes] = checkedBytes;
        CheckedPrimitives[Field.Types.Kind.TypeDouble] = checkedDouble;
        CheckedPrimitives[Field.Types.Kind.TypeFloat] = checkedDouble;
        CheckedPrimitives[Field.Types.Kind.TypeInt32] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeInt64] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSint32] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSint64] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeUint32] = checkedUint;
        CheckedPrimitives[Field.Types.Kind.TypeUint64] = checkedUint;
        CheckedPrimitives[Field.Types.Kind.TypeFixed32] = checkedUint;
        CheckedPrimitives[Field.Types.Kind.TypeFixed64] = checkedUint;
        CheckedPrimitives[Field.Types.Kind.TypeSfixed32] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSfixed64] = checkedInt;
        CheckedPrimitives[Field.Types.Kind.TypeString] = checkedString;
        // Wrapper types.
        CheckedWellKnowns["google.protobuf.BoolValue"] = CheckedWrap(checkedBool);
        CheckedWellKnowns["google.protobuf.BytesValue"] = CheckedWrap(checkedBytes);
        CheckedWellKnowns["google.protobuf.DoubleValue"] = CheckedWrap(checkedDouble);
        CheckedWellKnowns["google.protobuf.FloatValue"] = CheckedWrap(checkedDouble);
        CheckedWellKnowns["google.protobuf.Int64Value"] = CheckedWrap(checkedInt);
        CheckedWellKnowns["google.protobuf.Int32Value"] = CheckedWrap(checkedInt);
        CheckedWellKnowns["google.protobuf.UInt64Value"] = CheckedWrap(checkedUint);
        CheckedWellKnowns["google.protobuf.UInt32Value"] = CheckedWrap(checkedUint);
        CheckedWellKnowns["google.protobuf.StringValue"] = CheckedWrap(checkedString);
        // Well-known types.
        CheckedWellKnowns["google.protobuf.Any"] = checkedAny;
        CheckedWellKnowns["google.protobuf.Duration"] = checkedDuration;
        CheckedWellKnowns["google.protobuf.Timestamp"] = checkedTimestamp;
        // Json types.
        CheckedWellKnowns["google.protobuf.ListValue"] = checkedListDyn;
        CheckedWellKnowns["google.protobuf.NullValue"] = checkedNull;
        CheckedWellKnowns["google.protobuf.Struct"] = checkedMapStringDyn;
        CheckedWellKnowns["google.protobuf.Value"] = checkedDyn;
    }


    public static Type CheckedMessageType(string name)
    {
        var type = new Type();
        type.MessageType = name;
        return type;
    }

    internal static Type CheckedPrimitive(Type.Types.PrimitiveType primitive)
    {
        var type = new Type();
        type.Primitive = primitive;
        return type;
    }

    internal static Type CheckedWellKnown(Type.Types.WellKnownType wellKnown)
    {
        var type = new Type();
        type.WellKnown = wellKnown;
        return type;
    }

    internal static Type CheckedWrap(Type t)
    {
        var type = new Type();
        type.Wrapper = t.Primitive;
        return type;
    }
}