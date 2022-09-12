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
    public static readonly Type CheckedDyn;

    // Wrapper and primitive types.
    public static readonly Type CheckedBool = CheckedPrimitive(Type.Types.PrimitiveType.Bool);
    public static readonly Type CheckedBytes = CheckedPrimitive(Type.Types.PrimitiveType.Bytes);
    public static readonly Type CheckedDouble = CheckedPrimitive(Type.Types.PrimitiveType.Double);
    public static readonly Type CheckedInt = CheckedPrimitive(Type.Types.PrimitiveType.Int64);
    public static readonly Type CheckedString = CheckedPrimitive(Type.Types.PrimitiveType.String);

    public static readonly Type CheckedUint = CheckedPrimitive(Type.Types.PrimitiveType.Uint64);

    // Well-known type equivalents.
    public static readonly Type CheckedAny = CheckedWellKnown(Type.Types.WellKnownType.Any);
    public static readonly Type CheckedDuration = CheckedWellKnown(Type.Types.WellKnownType.Duration);

    public static readonly Type CheckedTimestamp = CheckedWellKnown(Type.Types.WellKnownType.Timestamp);

    // Json-based type equivalents.
    public static readonly Type CheckedNull;
    public static readonly Type CheckedListDyn;
    public static readonly Type CheckedMapStringDyn;

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
        CheckedDyn = type;

        type = new Type();
        type.Null = NullValue.NullValue;
        CheckedNull = type;

        type = new Type();
        var list = new Type.Types.ListType();
        list.ElemType = CheckedDyn;
        type.ListType = list;
        CheckedListDyn = type;

        type = new Type();
        var map = new Type.Types.MapType();
        map.KeyType = CheckedString;
        map.ValueType = CheckedDyn;
        type.MapType = map;
        CheckedMapStringDyn = type;

        CheckedPrimitives[Field.Types.Kind.TypeBool] = CheckedBool;
        CheckedPrimitives[Field.Types.Kind.TypeBytes] = CheckedBytes;
        CheckedPrimitives[Field.Types.Kind.TypeDouble] = CheckedDouble;
        CheckedPrimitives[Field.Types.Kind.TypeFloat] = CheckedDouble;
        CheckedPrimitives[Field.Types.Kind.TypeInt32] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeInt64] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSint32] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSint64] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeUint32] = CheckedUint;
        CheckedPrimitives[Field.Types.Kind.TypeUint64] = CheckedUint;
        CheckedPrimitives[Field.Types.Kind.TypeFixed32] = CheckedUint;
        CheckedPrimitives[Field.Types.Kind.TypeFixed64] = CheckedUint;
        CheckedPrimitives[Field.Types.Kind.TypeSfixed32] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeSfixed64] = CheckedInt;
        CheckedPrimitives[Field.Types.Kind.TypeString] = CheckedString;
        // Wrapper types.
        CheckedWellKnowns["google.protobuf.BoolValue"] = CheckedWrap(CheckedBool);
        CheckedWellKnowns["google.protobuf.BytesValue"] = CheckedWrap(CheckedBytes);
        CheckedWellKnowns["google.protobuf.DoubleValue"] = CheckedWrap(CheckedDouble);
        CheckedWellKnowns["google.protobuf.FloatValue"] = CheckedWrap(CheckedDouble);
        CheckedWellKnowns["google.protobuf.Int64Value"] = CheckedWrap(CheckedInt);
        CheckedWellKnowns["google.protobuf.Int32Value"] = CheckedWrap(CheckedInt);
        CheckedWellKnowns["google.protobuf.UInt64Value"] = CheckedWrap(CheckedUint);
        CheckedWellKnowns["google.protobuf.UInt32Value"] = CheckedWrap(CheckedUint);
        CheckedWellKnowns["google.protobuf.StringValue"] = CheckedWrap(CheckedString);
        // Well-known types.
        CheckedWellKnowns["google.protobuf.Any"] = CheckedAny;
        CheckedWellKnowns["google.protobuf.Duration"] = CheckedDuration;
        CheckedWellKnowns["google.protobuf.Timestamp"] = CheckedTimestamp;
        // Json types.
        CheckedWellKnowns["google.protobuf.ListValue"] = CheckedListDyn;
        CheckedWellKnowns["google.protobuf.NullValue"] = CheckedNull;
        CheckedWellKnowns["google.protobuf.Struct"] = CheckedMapStringDyn;
        CheckedWellKnowns["google.protobuf.Value"] = CheckedDyn;
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