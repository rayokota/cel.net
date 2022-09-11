using System.Collections;
using Cel.Common.Types.Pb;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Duration = Google.Protobuf.WellKnownTypes.Duration;

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
namespace Cel.Common.Types.Ref;

/// <summary>
///     Helper class for <seealso cref="TypeAdapter" /> implementations to convert from a C# type to a CEL type.
/// </summary>
public sealed class TypeAdapterSupport
{
    private static readonly IDictionary<System.Type, Func<TypeAdapter, object, IVal>> NativeToValueExact =
        new Dictionary<System.Type, Func<TypeAdapter, object, IVal>>(ReferenceEqualityComparer.Instance);

    static TypeAdapterSupport()
    {
        NativeToValueExact[typeof(bool)] = (a, value) => Types.BoolOf((bool)value);
        NativeToValueExact[typeof(byte[])] = (a, value) => BytesT.BytesOf((byte[])value);
        NativeToValueExact[typeof(float)] = (a, value) => DoubleT.DoubleOf((float)value);
        NativeToValueExact[typeof(double)] = (a, value) => DoubleT.DoubleOf((double)value);
        NativeToValueExact[typeof(byte)] = (a, value) => IntT.IntOf((byte)value);
        NativeToValueExact[typeof(short)] = (a, value) => IntT.IntOf((short)value);
        NativeToValueExact[typeof(int)] = (a, value) => IntT.IntOf((int)value);
        NativeToValueExact[typeof(long)] = (a, value) => IntT.IntOf((long)value);
        NativeToValueExact[typeof(uint)] = (a, value) => UintT.UintOf(Convert.ToUInt64(value));
        NativeToValueExact[typeof(ulong)] = (a, value) => UintT.UintOf((ulong)value);
        NativeToValueExact[typeof(string)] = (a, value) => StringT.StringOf((string)value);
        NativeToValueExact[typeof(Period)] = (a, value) => DurationT.DurationOf((Period)value);
        NativeToValueExact[typeof(Duration)] = (a, value) =>
            DurationT.DurationOf((Duration)value);
        NativeToValueExact[typeof(Timestamp)] = (a, value) => TimestampT.TimestampOf((Timestamp)value);
        NativeToValueExact[typeof(ZonedDateTime)] = (a, value) => TimestampT.TimestampOf((ZonedDateTime)value);
        NativeToValueExact[typeof(Instant)] = (a, value) => TimestampT.TimestampOf((Instant)value);
        NativeToValueExact[typeof(int[])] = (a, value) =>
            ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                ((int[])value).Select(i => IntT.IntOf(i)).ToArray());
        NativeToValueExact[typeof(long[])] = (a, value) =>
            ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                ((long[])value).Select(i => IntT.IntOf(i)).ToArray());
        NativeToValueExact[typeof(double[])] = (a, value) =>
            ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                ((double[])value).Select(i => DoubleT.DoubleOf(i)).ToArray());
        NativeToValueExact[typeof(string[])] = (a, value) => ListT.NewStringArrayList((string[])value);
        NativeToValueExact[typeof(IVal[])] = (a, value) => ListT.NewValArrayList(a, (IVal[])value);
        NativeToValueExact[typeof(NullValue)] = (a, value) => NullT.NullValue;
        NativeToValueExact[typeof(ListValue)] = (a, value) => ListT.NewJSONList(a, (ListValue)value);
        NativeToValueExact[typeof(UInt32Value)] = (a, value) => UintT.UintOf(((UInt32Value)value).Value);
        NativeToValueExact[typeof(UInt64Value)] = (a, value) => UintT.UintOf(((UInt64Value)value).Value);
        NativeToValueExact[typeof(Struct)] = (a, value) => MapT.NewJSONStruct(a, (Struct)value);
        NativeToValueExact[typeof(EnumValue)] = (a, value) => IntT.IntOf(((EnumValue)value).Number);
        NativeToValueExact[typeof(EnumValueDescriptor)] = (a, value) =>
        {
            var e = (EnumValueDescriptor)value;
            return IntT.IntOf(e.Number);
        };
    }

    private TypeAdapterSupport()
    {
    }

    public static IVal? MaybeNativeToValue(TypeAdapter a, object value)
    {
        if (value == null) return NullT.NullValue;

        NativeToValueExact.TryGetValue(value.GetType(), out var conv);
        if (conv != null) return conv(a, value);

        if (value is Array) return ListT.NewGenericArrayList(a, (Array)value);

        if (value is IList)
        {
            var list = (IList)value;
            var array = new object[list.Count];
            list.CopyTo(array, 0);
            return ListT.NewGenericArrayList(a, array);
        }

        if (value is IDictionary)
            return MapT.NewMaybeWrappedMap(a, (IDictionary)value);

        if (value is ByteString) return BytesT.BytesOf((ByteString)value);

        if (value is Instant) return TimestampT.TimestampOf(((Instant)value).InZone(TimestampT.ZoneIdZ));

        if (value is ZonedDateTime) return TimestampT.TimestampOf((ZonedDateTime)value);

        if (value is DateTime)
            return TimestampT.TimestampOf(Instant.FromDateTimeUtc((DateTime)value).InZone(TimestampT.ZoneIdZ));

        return null;
    }
}