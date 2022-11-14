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
using System.Collections;
using Cel.Common.Types;
using Cel.Common.Types.Json;
using Cel.Common.Types.Json.Types;
using Cel.Common.Types.Pb;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Types.Json;

using TypeKindCase = Type.TypeKindOneofCase;

internal class JsonTypeDescriptionTest
{
    [Test]
    public virtual void Basics()
    {
        var reg = (JsonRegistry)JsonRegistry.NewRegistry();

        reg.Register(typeof(CollectionsObject));
        var t = reg.FindType(typeof(CollectionsObject).FullName);
        Assert.That(t.MessageType, Is.EqualTo(typeof(CollectionsObject).FullName));
        Assert.That(t.TypeKindCase, Is.EqualTo(TypeKindCase.MessageType));

        var td = reg.TypeDescription(typeof(CollectionsObject));
        Assert.That(td.PbType(), Is.EqualTo(t));
        Assert.That(td.ReflectType(), Is.EqualTo(typeof(CollectionsObject)));
        Assert.That(td.Name(), Is.EqualTo(typeof(CollectionsObject).FullName));
        Assert.That(td.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName)));

        // check that the nested-class `InnerType` has been implicitly registered

        var tdInner = reg.TypeDescription(typeof(InnerType));
        Assert.That(tdInner.PbType(), Is.EqualTo(new Type { MessageType = typeof(InnerType).FullName }));
        Assert.That(tdInner.ReflectType(), Is.EqualTo(typeof(InnerType)));
        Assert.That(tdInner.Name(), Is.EqualTo(typeof(InnerType).FullName));
        Assert.That(tdInner.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(InnerType).FullName)));

        //

        Assert.That(reg.FindIdent(typeof(CollectionsObject).FullName),
            Is.EqualTo(TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName)));
        Assert.That(reg.FindIdent(typeof(InnerType).FullName),
            Is.EqualTo(TypeT.NewObjectTypeValue(typeof(InnerType).FullName)));
        Assert.That(reg.FindIdent(typeof(AnEnum).FullName + '.' + AnEnum.ENUM_VALUE_2),
            Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));

        Assert.That(() => reg.TypeDescription(typeof(AnEnum)),
            Throws.Exception.InstanceOf(typeof(ArgumentException)));
        Assert.That(() => reg.EnumDescription(typeof(InnerType)),
            Throws.Exception.InstanceOf(typeof(ArgumentException)));
    }

    [Test]
    public virtual void Types()
    {
        var reg = (JsonRegistry)JsonRegistry.NewRegistry();
        reg.Register(typeof(CollectionsObject));

        // verify the map-type-fields

        CheckMapType(reg, "stringBooleanMap", typeof(string), Checked.CheckedString, typeof(bool),
            Checked.CheckedBool);
        CheckMapType(reg, "byteShortMap", typeof(byte), Checked.CheckedInt, typeof(short), Checked.CheckedInt);
        CheckMapType(reg, "intLongMap", typeof(int), Checked.CheckedInt, typeof(long), Checked.CheckedInt);
        CheckMapType(reg, "ulongTimestampMap", typeof(ulong), Checked.CheckedUint, typeof(Timestamp),
            Checked.CheckedTimestamp);
        CheckMapType(reg, "ulongZonedDateTimeMap", typeof(ulong), Checked.CheckedUint, typeof(ZonedDateTime),
            Checked.CheckedTimestamp);
        CheckMapType(reg, "stringProtoDurationMap", typeof(string), Checked.CheckedString, typeof(Duration),
            Checked.CheckedDuration);
        CheckMapType(reg, "stringPeriodMap", typeof(string), Checked.CheckedString,
            typeof(Period), Checked.CheckedDuration);
        CheckMapType(reg, "stringBytesMap", typeof(string), Checked.CheckedString, typeof(ByteString),
            Checked.CheckedBytes);
        CheckMapType(reg, "floatDoubleMap", typeof(float), Checked.CheckedDouble, typeof(double),
            Checked.CheckedDouble);

        // verify the list-type-fields

        CheckListType(reg, "stringList", typeof(string), Checked.CheckedString);
        CheckListType(reg, "booleanList", typeof(bool), Checked.CheckedBool);
        CheckListType(reg, "byteList", typeof(byte), Checked.CheckedInt);
        CheckListType(reg, "shortList", typeof(short), Checked.CheckedInt);
        CheckListType(reg, "intList", typeof(int), Checked.CheckedInt);
        CheckListType(reg, "longList", typeof(long), Checked.CheckedInt);
        CheckListType(reg, "ulongList", typeof(ulong), Checked.CheckedUint);
        CheckListType(reg, "timestampList", typeof(Timestamp), Checked.CheckedTimestamp);
        CheckListType(reg, "zonedDateTimeList", typeof(ZonedDateTime), Checked.CheckedTimestamp);
        CheckListType(reg, "durationList", typeof(Duration), Checked.CheckedDuration);
        CheckListType(reg, "periodList", typeof(Period), Checked.CheckedDuration);
        CheckListType(reg, "bytesList", typeof(ByteString), Checked.CheckedBytes);
        CheckListType(reg, "floatList", typeof(float), Checked.CheckedDouble);
        CheckListType(reg, "doubleList", typeof(double), Checked.CheckedDouble);
    }

    private void CheckListType(JsonRegistry reg, string prop, System.Type valueClass,
        Type valueType)
    {
        var ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
        Assert.That(ft, Is.Not.Null);

        Assert.That(ft.Type.ListType.ElemType, Is.SameAs(valueType));
    }

    private void CheckMapType(JsonRegistry reg, string prop, System.Type keyClass,
        Type keyType, System.Type valueClass, Type valueType)
    {
        var ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
        Assert.That(ft, Is.Not.Null);

        Assert.That(ft.Type.MapType.KeyType, Is.SameAs(keyType));
        Assert.That(ft.Type.MapType.ValueType, Is.SameAs(valueType));
    }

    [Test]
    public virtual void UnknownProperties()
    {
        var collectionsObject = new CollectionsObject();

        var reg = (JsonRegistry)JsonRegistry.NewRegistry();
        reg.Register(typeof(CollectionsObject));

        var collectionsVal = reg.NativeToValue(collectionsObject);
        Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
        var obj = (ObjectT)collectionsVal;

        var x = obj.IsSet(StringT.StringOf("bart"));
        Assert.That(x, Is.InstanceOf(typeof(Err)));

        x = obj.Get(StringT.StringOf("bart"));
        Assert.That(x, Is.InstanceOf(typeof(Err)));
    }

    [Test]
    public virtual void CollectionsObjectEmpty()
    {
        var collectionsObject = new CollectionsObject();

        var reg = (JsonRegistry)JsonRegistry.NewRegistry();
        reg.Register(typeof(CollectionsObject));

        var collectionsVal = reg.NativeToValue(collectionsObject);
        Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
        var obj = (ObjectT)collectionsVal;

        foreach (var field in CollectionsObject.AllProperties)
        {
            Assert.That(obj.IsSet(StringT.StringOf(field)), Is.SameAs(BoolT.False));
            Assert.That(obj.Get(StringT.StringOf(field)), Is.SameAs(NullT.NullValue));
        }
    }

    [Test]
    public virtual void CollectionsObjectTypeTest()
    {
        var collectionsObject = new CollectionsObject();

        // populate (primitive) map types

        collectionsObject.stringBooleanMap = new Dictionary<string, bool> { { "a", true } };
        collectionsObject.byteShortMap = new Dictionary<byte, short> { { 1, 2 } };
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
            { { "a", ByteString.CopyFrom(1) } };
        collectionsObject.floatDoubleMap = new Dictionary<float, double> { { 1f, 2d } };

        // populate (primitive) list types

        collectionsObject.stringList = new List<string> { "a", "b", "c" };
        collectionsObject.booleanList = new List<bool> { true, true, false, false };
        collectionsObject.byteList = new List<byte> { 1, 2, 3 };
        collectionsObject.shortList = new List<short> { 4, 5, 6 };
        collectionsObject.intList = new List<int> { 7, 8, 9 };
        collectionsObject.longList = new List<long> { 10L, 11L, 12L };
        collectionsObject.ulongList = new List<ulong> { 1, 2, 3 };
        collectionsObject.timestampList = new List<Timestamp>
        {
            new() { Seconds = 1 },
            new() { Seconds = 2 },
            new() { Seconds = 3 }
        };
        collectionsObject.zonedDateTimeList = new List<ZonedDateTime>
        {
            new(Instant.FromUnixTimeSeconds(1), DateTimeZone.Utc),
            new(Instant.FromUnixTimeSeconds(2), DateTimeZone.Utc),
            new(Instant.FromUnixTimeSeconds(3), DateTimeZone.Utc)
        };
        collectionsObject.durationList = new List<Duration>
        {
            new() { Seconds = 1 },
            new() { Seconds = 2 },
            new() { Seconds = 3 }
        };
        collectionsObject.periodList = new List<Period>
        {
            Period.FromSeconds(1),
            Period.FromSeconds(2),
            Period.FromSeconds(3)
        };
        collectionsObject.bytesList = new List<ByteString>
        {
            ByteString.CopyFrom(1), ByteString.CopyFrom(2),
            ByteString.CopyFrom(3)
        };
        collectionsObject.floatList = new List<float> { 1f, 2f, 3f };
        collectionsObject.doubleList = new List<double> { 1d, 2d, 3d };

        // populate inner/nested type list/map

        var inner1 = new InnerType();
        inner1.intProp = 1;
        inner1.wrappedIntProp = 2;
        collectionsObject.stringInnerMap = new Dictionary<string, InnerType> { { "a", inner1 } };

        var inner2 = new InnerType();
        inner2.intProp = 3;
        inner2.wrappedIntProp = 4;
        collectionsObject.innerTypes = new List<InnerType> { inner1, inner2 };

        // populate enum-related fields

        collectionsObject.anEnum = AnEnum.ENUM_VALUE_2;
        collectionsObject.anEnumList = new List<AnEnum> { AnEnum.ENUM_VALUE_2, AnEnum.ENUM_VALUE_3 };
        collectionsObject.anEnumStringMap = new Dictionary<AnEnum, string> { { AnEnum.ENUM_VALUE_2, "a" } };
        collectionsObject.stringAnEnumMap = new Dictionary<string, AnEnum> { { "a", AnEnum.ENUM_VALUE_2 } };

        // prepare registry

        var reg = (JsonRegistry)JsonRegistry.NewRegistry();
        reg.Register(typeof(CollectionsObject));

        var collectionsVal = reg.NativeToValue(collectionsObject);
        Assert.That(collectionsVal, Is.InstanceOf(typeof(ObjectT)));
        var obj = (ObjectT)collectionsVal;

        // briefly verify all fields

        foreach (var field in CollectionsObject.AllProperties)
        {
            Assert.That(obj.IsSet(StringT.StringOf(field)), Is.SameAs(BoolT.True));
            Assert.That(obj.Get(StringT.StringOf(field)), Is.Not.Null);

            var fieldVal = obj.Get(StringT.StringOf(field));
            var fieldObj = typeof(CollectionsObject).GetField(field).GetValue(collectionsObject);
            if (fieldObj is IDictionary)
                Assert.That(fieldVal, Is.InstanceOf(typeof(MapT)));
            else if (fieldObj is IList) Assert.That(fieldVal, Is.InstanceOf(typeof(ListT)));

            Assert.That(fieldVal.Equal(reg.NativeToValue(fieldObj)), Is.SameAs(BoolT.True));
        }

        // check a few properties manually/explicitly

        var mapVal = (MapT)obj.Get(StringT.StringOf("intLongMap"));
        Assert.That(mapVal.Size(), Is.EqualTo(IntT.IntOf(1)));
        Assert.That(mapVal.Contains(IntT.IntOf(42)), Is.EqualTo(BoolT.False));
        Assert.That(mapVal.Contains(IntT.IntOf(1)), Is.EqualTo(BoolT.True));
        Assert.That(mapVal.Contains(IntT.IntOf(2)), Is.EqualTo(BoolT.False));
        Assert.That(mapVal.Get(IntT.IntOf(1)), Is.EqualTo(IntT.IntOf(2)));

        var listVal = (ListT)obj.Get(StringT.StringOf("ulongList"));
        Assert.That(listVal.Size(), Is.EqualTo(IntT.IntOf(3)));
        Assert.That(listVal.Contains(UintT.UintOf(42)), Is.EqualTo(BoolT.False));
        Assert.That(listVal.Contains(UintT.UintOf(1)), Is.EqualTo(BoolT.True));
        Assert.That(listVal.Contains(UintT.UintOf(2)), Is.EqualTo(BoolT.True));
        Assert.That(listVal.Contains(UintT.UintOf(3)), Is.EqualTo(BoolT.True));
        Assert.That(listVal.Get(IntT.IntOf(0)), Is.EqualTo(UintT.UintOf(1)));
        Assert.That(listVal.Get(IntT.IntOf(1)), Is.EqualTo(UintT.UintOf(2)));
        Assert.That(listVal.Get(IntT.IntOf(2)), Is.EqualTo(UintT.UintOf(3)));

        mapVal = (MapT)obj.Get(StringT.StringOf("stringInnerMap"));
        Assert.That(mapVal.Size(), Is.EqualTo(IntT.IntOf(1)));
        Assert.That(mapVal.Contains(StringT.StringOf("42")), Is.EqualTo(BoolT.False));
        Assert.That(mapVal.Contains(StringT.StringOf("a")), Is.EqualTo(BoolT.True));
        var i = (ObjectT)mapVal.Get(StringT.StringOf("a"));
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

        var x = obj.Get(StringT.StringOf("anEnum"));
        Assert.That(x, Is.InstanceOf(typeof(IntT)));
        Assert.That(x, Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
        listVal = (ListT)obj.Get(StringT.StringOf("anEnumList"));
        Assert.That(listVal.Get(IntT.IntOf(0)), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
        Assert.That(listVal.Get(IntT.IntOf(1)), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_3)));
        mapVal = (MapT)obj.Get(StringT.StringOf("anEnumStringMap"));
        Assert.That(mapVal.Get(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)), Is.EqualTo(StringT.StringOf("a")));
        mapVal = (MapT)obj.Get(StringT.StringOf("stringAnEnumMap"));
        Assert.That(mapVal.Get(StringT.StringOf("a")), Is.EqualTo(IntT.IntOf((int)AnEnum.ENUM_VALUE_2)));
    }
}