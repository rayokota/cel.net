using System;
using System.Collections.Generic;
using NodaTime;

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
namespace Cel.Common.Types.Ref
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.BytesT.bytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.DurationT.durationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.ListT.newGenericArrayList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.ListT.newJSONList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.ListT.newStringArrayList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.ListT.newValArrayList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.MapT.newJSONStruct;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.MapT.newMaybeWrappedMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.TimestampT.ZoneIdZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.TimestampT.timestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.UintT.uintOf;

    using ByteString = Google.Protobuf.ByteString;
    using EnumValueDescriptor = Google.Protobuf.Reflection.EnumValueDescriptor;
    using EnumValue = Google.Protobuf.WellKnownTypes.EnumValue;
    using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
    using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
    using Struct = Google.Protobuf.WellKnownTypes.Struct;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
    using DoubleT = global::Cel.Common.Types.DoubleT;
    using IntT = global::Cel.Common.Types.IntT;
    using NullT = global::Cel.Common.Types.NullT;
    using DefaultTypeAdapter = global::Cel.Common.Types.Pb.DefaultTypeAdapter;

    /// <summary>
    /// Helper class for <seealso cref="TypeAdapter"/> implementations to convert from a Java type to a CEL type.
    /// </summary>
    public sealed class TypeAdapterSupport
    {
        private TypeAdapterSupport()
        {
        }

        private static readonly IDictionary<System.Type, System.Func<TypeAdapter, object, Val>> NativeToValueExact =
            new Dictionary<System.Type, Func<TypeAdapter, object, Val>>(ReferenceEqualityComparer.Instance);

        static TypeAdapterSupport()
        {
            NativeToValueExact[typeof(bool)] = (a, value) => Types.BoolOf((bool)value);
            NativeToValueExact[typeof(byte[])] = (a, value) => BytesT.BytesOf(((byte[])value));
            NativeToValueExact[typeof(float)] = (a, value) => DoubleT.DoubleOf(((float)value));
            NativeToValueExact[typeof(double)] = (a, value) => DoubleT.DoubleOf((double)value);
            NativeToValueExact[typeof(byte)] = (a, value) => IntT.IntOf((byte)value);
            NativeToValueExact[typeof(short)] = (a, value) => IntT.IntOf((short)value);
            NativeToValueExact[typeof(int)] = (a, value) => IntT.IntOf((int)value);
            NativeToValueExact[typeof(ulong)] = (a, value) => UintT.UintOf((ulong)value);
            NativeToValueExact[typeof(long)] = (a, value) => IntT.IntOf((long)value);
            NativeToValueExact[typeof(string)] = (a, value) => StringT.StringOf((string)value);
            NativeToValueExact[typeof(Duration)] = (a, value) => DurationT.DurationOf((Period)value);
            NativeToValueExact[typeof(Google.Protobuf.WellKnownTypes.Duration)] = (a, value) =>
                DurationT.DurationOf((Google.Protobuf.WellKnownTypes.Duration)value);
            NativeToValueExact[typeof(Timestamp)] = (a, value) => TimestampT.TimestampOf((Timestamp)value);
            NativeToValueExact[typeof(ZonedDateTime)] = (a, value) => TimestampT.TimestampOf((ZonedDateTime)value);
            NativeToValueExact[typeof(Instant)] = (a, value) => TimestampT.TimestampOf((Instant)value);
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
            NativeToValueExact[typeof(int[])] = (a, value) =>
                ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                    ((int[])value).Select(i => IntT.IntOf(i)).ToArray());
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
            NativeToValueExact[typeof(long[])] = (a, value) =>
                ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                    ((long[])value).Select(i => IntT.IntOf(i)).ToArray());
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
            NativeToValueExact[typeof(double[])] = (a, value) =>
                ListT.NewValArrayList(DefaultTypeAdapter.Instance.ToTypeAdapter(),
                    ((double[])value).Select(i => DoubleT.DoubleOf(i)).ToArray());
            NativeToValueExact[typeof(string[])] = (a, value) => ListT.NewStringArrayList((string[])value);
            NativeToValueExact[typeof(Val[])] = (a, value) => ListT.NewValArrayList(a, (Val[])value);
            NativeToValueExact[typeof(NullValue)] = (a, value) => NullT.NullValue;
            NativeToValueExact[typeof(ListValue)] = (a, value) => ListT.NewJSONList(a, (ListValue)value);
            NativeToValueExact[typeof(UInt32Value)] = (a, value) => UintT.UintOf(((UInt32Value)value).Value);
            NativeToValueExact[typeof(UInt64Value)] = (a, value) => UintT.UintOf(((UInt64Value)value).Value);
            NativeToValueExact[typeof(Struct)] = (a, value) => MapT.NewJSONStruct(a, (Struct)value);
            NativeToValueExact[typeof(EnumValue)] = (a, value) => IntT.IntOf(((EnumValue)value).Number);
            NativeToValueExact[typeof(EnumValueDescriptor)] = (a, value) =>
            {
                EnumValueDescriptor e = (EnumValueDescriptor)value;
                return IntT.IntOf(e.Number);
            };
        }

        public static Val MaybeNativeToValue(TypeAdapter a, object value)
        {
            if (value == null)
            {
                return NullT.NullValue;
            }

            System.Func<TypeAdapter, object, Val> conv = NativeToValueExact[value.GetType()];
            if (conv != null)
            {
                return conv(a, value);
            }

            if (value is object[])
            {
                return ListT.NewGenericArrayList(a, (object[])value);
            }

            if (value is System.Collections.IList)
            {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return newGenericArrayList(a, ((java.util.List<?>) value).toArray());
                return ListT.NewGenericArrayList(a, ((IList<object>)value).ToArray());
            }

            if (value is System.Collections.IDictionary)
            {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return newMaybeWrappedMap(a, (java.util.Map<?, ?>) value);
                return MapT.NewMaybeWrappedMap(a, (IDictionary<object, object>)value);
            }

            if (value is ByteString)
            {
                return BytesT.BytesOf((ByteString)value);
            }

            if (value is Instant)
            {
                return TimestampT.TimestampOf(((Instant)value).InZone(TimestampT.ZoneIdZ));
            }

            if (value is ZonedDateTime)
            {
                return TimestampT.TimestampOf((ZonedDateTime)value);
            }
            // TODO
            /*
            if (value is DateTime)
            {
                
              return TimestampT.TimestampOf(((DateTime) value).toInstant().atZone(TimestampT.ZoneIdZ));
            }
            */

            return null;
        }
    }
}