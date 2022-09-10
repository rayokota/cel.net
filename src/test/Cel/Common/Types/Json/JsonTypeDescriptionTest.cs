using System;
using System.Collections.Generic;
using Cel.Common.Types;
using Cel.Common.Types.Json;
using Cel.Common.Types.Json.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;
using Duration = NodaTime.Duration;
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
namespace Cel.Types.Json
{
    using ListType = Google.Api.Expr.V1Alpha1.Type.Types.ListType;
    using MapType = Google.Api.Expr.V1Alpha1.Type.Types.MapType;
    using TypeKindCase = Google.Api.Expr.V1Alpha1.Type.TypeKindOneofCase;
    using ByteString = Google.Protobuf.ByteString;
    using Duration = Google.Protobuf.WellKnownTypes.Duration;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

    internal class JsonTypeDescriptionTest
    {
[Test]
        public virtual void Basics()
        {
            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();

            reg.Register(typeof(CollectionsObject));
            Google.Api.Expr.V1Alpha1.Type t = reg.FindType(typeof(CollectionsObject).FullName);
            Assert.That(t.MessageType, Is.EqualTo(typeof(CollectionsObject).FullName));
            Assert.That(t.TypeKindCase, Is.EqualTo(TypeKindCase.MessageType));

            JsonTypeDescription td = reg.TypeDescription(typeof(CollectionsObject));
            Assert.That(td.PbType(), Is.EqualTo(t));
            Assert.That(td.ReflectType(), Is.EqualTo(typeof(CollectionsObject)));
            Assert.That(td.Name(), Is.EqualTo(typeof(CollectionsObject).FullName));
            Assert.That(td.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName)));

            // check that the nested-class `InnerType` has been implicitly registered

            JsonTypeDescription tdInner = reg.TypeDescription(typeof(InnerType));
            Assert.That(tdInner.PbType(), Is.EqualTo(new Google.Api.Expr.V1Alpha1.Type(){ MessageType = typeof(InnerType).FullName}));
            Assert.That(tdInner.ReflectType(), Is.EqualTo(typeof(InnerType)));
            Assert.That(tdInner.Name(), Is.EqualTo(typeof(InnerType).FullName));
            Assert.That(tdInner.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(InnerType).FullName)));

            //

            Assert.That(reg.FindIdent(typeof(CollectionsObject).FullName),
                Is.EqualTo(TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName)));
            Assert.That(reg.FindIdent(typeof(InnerType).FullName),
                Is.EqualTo(TypeT.NewObjectTypeValue(typeof(InnerType).FullName)));
            Assert.That(reg.FindIdent(typeof(AnEnum).FullName + '.' + AnEnum.ENUM_VALUE_2.ToString()),
                Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_1)));

            Assert.That(() => reg.TypeDescription(typeof(AnEnum)),
                Throws.Exception.InstanceOf(typeof(ArgumentException)));
            Assert.That(() => reg.EnumDescription(typeof(InnerType)),
                Throws.Exception.InstanceOf(typeof(ArgumentException)));
        }

[Test]
        public virtual void Types()
        {
            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            // verify the map-type-fields

            CheckMapType(reg, "stringBooleanMap", typeof(string), Checked.checkedString, typeof(bool),
                Checked.checkedBool);
            CheckMapType(reg, "byteShortMap", typeof(byte), Checked.checkedInt, typeof(short), Checked.checkedInt);
            CheckMapType(reg, "intLongMap", typeof(int), Checked.checkedInt, typeof(long), Checked.checkedInt);
            CheckMapType(reg, "ulongTimestampMap", typeof(ulong), Checked.checkedUint, typeof(Timestamp),
                Checked.checkedTimestamp);
            CheckMapType(reg, "ulongZonedDateTimeMap", typeof(ulong), Checked.checkedUint, typeof(ZonedDateTime),
                Checked.checkedTimestamp);
            CheckMapType(reg, "stringProtoDurationMap", typeof(string), Checked.checkedString, typeof(Duration),
                Checked.checkedDuration);
            CheckMapType(reg, "stringPeriodMap", typeof(string), Checked.checkedString,
                typeof(Period), Checked.checkedDuration);
            CheckMapType(reg, "stringBytesMap", typeof(string), Checked.checkedString, typeof(ByteString),
                Checked.checkedBytes);
            CheckMapType(reg, "floatDoubleMap", typeof(float), Checked.checkedDouble, typeof(double),
                Checked.checkedDouble);

            // verify the list-type-fields

            CheckListType(reg, "stringList", typeof(string), Checked.checkedString);
            CheckListType(reg, "booleanList", typeof(bool), Checked.checkedBool);
            CheckListType(reg, "byteList", typeof(byte), Checked.checkedInt);
            CheckListType(reg, "shortList", typeof(short), Checked.checkedInt);
            CheckListType(reg, "intList", typeof(int), Checked.checkedInt);
            CheckListType(reg, "longList", typeof(long), Checked.checkedInt);
            CheckListType(reg, "ulongList", typeof(ulong), Checked.checkedUint);
            CheckListType(reg, "timestampList", typeof(Timestamp), Checked.checkedTimestamp);
            CheckListType(reg, "zonedDateTimeList", typeof(ZonedDateTime), Checked.checkedTimestamp);
            CheckListType(reg, "durationList", typeof(Duration), Checked.checkedDuration);
            CheckListType(reg, "periodList", typeof(Period), Checked.checkedDuration);
            CheckListType(reg, "bytesList", typeof(ByteString), Checked.checkedBytes);
            CheckListType(reg, "floatList", typeof(float), Checked.checkedDouble);
            CheckListType(reg, "doubleList", typeof(double), Checked.checkedDouble);
        }

        private void CheckListType(JsonRegistry reg, string prop, Type valueClass,
            Google.Api.Expr.V1Alpha1.Type valueType)
        {
            JsonFieldType ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
            Assert.That(ft, Is.Not.Null);
            Type type = ft.PropertyWriter().GetType();

            Assert.That(type.IsGenericType, Is.True);
            Assert.That(type.GetGenericTypeDefinition(), Is.EqualTo(typeof(List<>)));
            Type itemType = type.GetGenericArguments()[0];
            Assert.That(itemType, Is.SameAs(valueClass));
            Assert.That(ft.type.ListType.ElemType, Is.SameAs(valueType));
        }

        private void CheckMapType(JsonRegistry reg, string prop, Type keyClass,
            Google.Api.Expr.V1Alpha1.Type keyType, Type valueClass, Google.Api.Expr.V1Alpha1.Type valueType)
        {
            JsonFieldType ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
            Assert.That(ft, Is.Not.Null);
            Type type = ft.PropertyWriter().GetType();

            Assert.That(type.IsGenericType, Is.True);
            Assert.That(type.GetGenericTypeDefinition(), Is.EqualTo(typeof(Dictionary<,>)));
            Type keyT = type.GetGenericArguments()[0];
            Type valueT = type.GetGenericArguments()[0];
            Assert.That(keyT, Is.SameAs(keyClass));
            Assert.That(valueT, Is.SameAs(valueClass));
            Assert.That(ft.type.MapType.KeyType, Is.SameAs(keyType));
            Assert.That(ft.type.MapType.ValueType, Is.SameAs(valueType));
        }

[Test]
        public virtual void UnknownProperties()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
            ObjectT obj = (ObjectT)collectionsVal;

            Val x = obj.IsSet(StringT.StringOf("bart"));
            Assert.That(x, Is.InstanceOf(typeof(Err)));

            x = obj.Get(StringT.StringOf("bart"));
            Assert.That(x, Is.InstanceOf(typeof(Err)));
        }

[Test]
        public virtual void CollectionsObjectEmpty()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
            ObjectT obj = (ObjectT)collectionsVal;

            foreach (string field in CollectionsObject.ALL_PROPERTIES)
            {
                Assert.That(obj.IsSet(StringT.StringOf(field)), Is.SameAs(BoolT.False));
                Assert.That(obj.Get(StringT.StringOf(field)), Is.SameAs(NullT.NullValue));
            }
        }

[Test]
        public virtual void CollectionsObjectTypeTest()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            // populate (primitive) map types

            collectionsObject.stringBooleanMap = new Dictionary<string, bool> { { "a", true } };
            collectionsObject.byteShortMap = new Dictionary<byte, short> { { (byte)1, (short)2 } };
            collectionsObject.intLongMap = new Dictionary<int, long> { { 1, 2L } };
            collectionsObject.ulongTimestampMap =
                new Dictionary<ulong, Timestamp> { { 1, new Timestamp { Seconds = 1 } } };
            collectionsObject.ulongZonedDateTimeMap = new Dictionary<ulong, ZonedDateTime>
            {
                {
                    1,
                    new ZonedDateTime(Instant.FromUnixTimeSeconds(1), DateTimeZone.Utc)
                }
            };
            collectionsObject.stringProtoDurationMap =
                new Dictionary<string, Duration> { { "a", new Duration { Seconds = 1 } } };

        collectionsObject.stringPeriodMap = new Dictionary<string, Period> { { "a", Period.FromSeconds(1) } };
            collectionsObject.stringBytesMap = new Dictionary<string, ByteString>
                { { "a", ByteString.CopyFrom(new byte[] { (byte)1 }) } };
            collectionsObject.floatDoubleMap = new Dictionary<float, double> { { 1f, 2d } };

        // populate (primitive) list types

            collectionsObject.stringList = new List<string> { "a", "b", "c" };
            collectionsObject.booleanList = new List<bool> { true, true, false, false };
            collectionsObject.byteList = new List<byte> { (byte)1, (byte)2, (byte)3 };
            collectionsObject.shortList = new List<short> { (short)4, (short)5, (short)6 };
            collectionsObject.intList = new List<int> { 7, 8, 9 };
            collectionsObject.longList = new List<long> { 10L, 11L, 12L };
            collectionsObject.ulongList = new List<ulong> { 1, 2, 3 };
            collectionsObject.timestampList = new List<Timestamp>
            {
                new Timestamp{ Seconds = 1 }, 
                new Timestamp{ Seconds = 2 },
                new Timestamp{ Seconds = 3 }
            };
            collectionsObject.zonedDateTimeList = new List<ZonedDateTime>
            {
                    new ZonedDateTime(Instant.FromUnixTimeSeconds(1), DateTimeZone.Utc),
                    new ZonedDateTime(Instant.FromUnixTimeSeconds(2), DateTimeZone.Utc),
                    new ZonedDateTime(Instant.FromUnixTimeSeconds(3), DateTimeZone.Utc)
            };
            collectionsObject.durationList = new List<Duration>
            {
                new Duration{ Seconds = 1 }, 
                new Duration{ Seconds = 2 },
                new Duration{ Seconds = 3 }
            };
            collectionsObject.periodList = new List<Period>
            {
                Period.FromSeconds(1),
                Period.FromSeconds(2),
                Period.FromSeconds(3)
            };
            collectionsObject.bytesList = new List<ByteString>
            {
                ByteString.CopyFrom(new byte[] { (byte)1 }), ByteString.CopyFrom(new byte[] { (byte)2 }),
                ByteString.CopyFrom(new byte[] { (byte)3 })
            };
            collectionsObject.floatList = new List<float> { 1f, 2f, 3f };
            collectionsObject.doubleList = new List<double> { 1d, 2d, 3d };

            // populate inner/nested type list/map

            InnerType inner1 = new InnerType();
            inner1.intProp = 1;
            inner1.wrappedIntProp = 2;
            collectionsObject.stringInnerMap = new Dictionary<string, InnerType>{{"a", inner1}};

            InnerType inner2 = new InnerType();
            inner2.intProp = 3;
            inner2.wrappedIntProp = 4;
            collectionsObject.innerTypes = new List<InnerType> { inner1, inner2 };

            // populate enum-related fields

            collectionsObject.anEnum = AnEnum.ENUM_VALUE_2;
            collectionsObject.anEnumList = new List<AnEnum> { AnEnum.ENUM_VALUE_2, AnEnum.ENUM_VALUE_3 };
            collectionsObject.anEnumStringMap = new Dictionary<AnEnum, string>{{AnEnum.ENUM_VALUE_2, "a"}};
            collectionsObject.stringAnEnumMap = new Dictionary<string, AnEnum>{{"a", AnEnum.ENUM_VALUE_2}};

            // prepare registry

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
            ObjectT obj = (ObjectT)collectionsVal;

            // briefly verify all fields

            foreach (string field in CollectionsObject.ALL_PROPERTIES)
            {
                Assert.That(obj.IsSet(StringT.StringOf(field)), Is.SameAs(BoolT.True));
                Assert.That(obj.Get(StringT.StringOf(field)), Is.Not.Null);

                Val fieldVal = obj.Get(StringT.StringOf(field));
                object fieldObj = typeof(CollectionsObject).GetField(field).GetValue(collectionsObject);
                if (fieldObj is System.Collections.IDictionary)
                {
                    Assert.That(fieldVal, Is.InstanceOf(typeof(MapT)));
                }
                else if (fieldObj is System.Collections.IList)
                {
                    Assert.That(fieldVal, Is.InstanceOf(typeof(ListT)));
                }

                Assert.That(fieldVal.Equal(reg.NativeToValue(fieldObj)), Is.SameAs(BoolT.True));
            }

            // check a few properties manually/explicitly

            MapT mapVal = (MapT)obj.Get(StringT.StringOf("intLongMap"));
            Assert.That(mapVal.Size(), Is.EqualTo(IntT.IntOf(1)));
            Assert.That(mapVal.Contains(IntT.IntOf(42)), Is.EqualTo(BoolT.False));
            Assert.That(mapVal.Contains(IntT.IntOf(1)), Is.EqualTo(BoolT.True));
            Assert.That(mapVal.Contains(IntT.IntOf(2)), Is.EqualTo(BoolT.False));
            Assert.That(mapVal.Get(IntT.IntOf(1)), Is.EqualTo(IntT.IntOf(2)));

            ListT listVal = (ListT)obj.Get(StringT.StringOf("ulongList"));
            Assert.That(listVal.Size(), Is.EqualTo(IntT.IntOf(3)));
            Assert.That(listVal.Contains(IntT.IntOf(42)), Is.EqualTo(BoolT.False));
            Assert.That(listVal.Contains(IntT.IntOf(1)), Is.EqualTo(BoolT.True));
            Assert.That(listVal.Contains(IntT.IntOf(2)), Is.EqualTo(BoolT.True));
            Assert.That(listVal.Contains(IntT.IntOf(3)), Is.EqualTo(BoolT.True));
            Assert.That(listVal.Get(IntT.IntOf(0)), Is.EqualTo(IntT.IntOf(1)));
            Assert.That(listVal.Get(IntT.IntOf(1)), Is.EqualTo(IntT.IntOf(2)));
            Assert.That(listVal.Get(IntT.IntOf(2)), Is.EqualTo(IntT.IntOf(3)));

            mapVal = (MapT)obj.Get(StringT.StringOf("stringInnerMap"));
            Assert.That(mapVal.Size(), Is.EqualTo(IntT.IntOf(1)));
            Assert.That(mapVal.Contains(StringT.StringOf("42")), Is.EqualTo(BoolT.False));
            Assert.That(mapVal.Contains(StringT.StringOf("a")), Is.EqualTo(BoolT.True));
            ObjectT i = (ObjectT)mapVal.Get(StringT.StringOf("a"));
            Assert.That(i.Get(StringT.StringOf("intProp")), Is.EqualTo(IntT.IntOf(1)));
            Assert.That(i.Get(StringT.StringOf("wrappedIntProp")), Is.EqualTo(IntT.IntOf(2)));

            listVal = (ListT)obj.Get(StringT.StringOf("innerTypes"));
            Assert.That(listVal.Size(), Is.EqualTo(IntT.IntOf(2)));
            i = (ObjectT)listVal.Get(IntT.IntOf(0));
            Assert.That(i.Get(StringT.StringOf("intProp")), Is.EqualTo(IntT.IntOf(1)));
            Assert.That(i.Get(StringT.StringOf("wrappedIntProp")), Is.EqualTo(IntT.IntOf(2)));
            i = (ObjectT)listVal.Get(IntT.IntOf(1));
            Assert.That(i.Get(StringT.StringOf("intProp")), Is.EqualTo(IntT.IntOf(3)));
            Assert.That(i.Get(StringT.StringOf("wrappedIntProp")), Is.EqualTo(IntT.IntOf(4)));

            // verify enums

            Val x = obj.Get(StringT.StringOf("anEnum"));
            Assert.That(x, Is.InstanceOf(typeof(IntT)));
            Assert.That(x, Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
            listVal = (ListT)obj.Get(StringT.StringOf("anEnumList"));
            Assert.That(listVal.Get(IntT.IntOf(0)), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
            Assert.That(listVal.Get(IntT.IntOf(1)), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_3)));
            mapVal = (MapT)obj.Get(StringT.StringOf("anEnumStringMap"));
            Assert.That(mapVal.Get(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)), Is.EqualTo(StringT.StringOf("a'")));
            mapVal = (MapT)obj.Get(StringT.StringOf("stringAnEnumMap"));
            Assert.That(mapVal.Get(StringT.StringOf("a")), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
        }
    }
}