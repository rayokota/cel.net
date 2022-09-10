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
 * Copyright (C) 2021 The Authors of CEL-Java
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void basics()
        internal virtual void Basics()
        {
            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();

            reg.Register(typeof(CollectionsObject));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            Google.Api.Expr.V1Alpha1.Type t = reg.FindType(typeof(CollectionsObject).FullName);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            Assert.That(t.MessageType, Is.EqualTo(typeof(CollectionsObject).FullName));
            Assert.That(t.TypeKindCase, Is.EqualTo(TypeKindCase.MessageType);

            JsonTypeDescription td = reg.TypeDescription(typeof(CollectionsObject));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(td.PbType(), Is.EqualTo(t));
            Assert.That(td.ReflectType(), Is.EqualTo(typeof(CollectionsObject)));
            Assert.That(td.Name(), Is.EqualTo(typeof(CollectionsObject).FullName));
            Assert.That(td.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName)));

            // check that the nested-class `InnerType` has been implicitly registered

            JsonTypeDescription tdInner = reg.TypeDescription(typeof(InnerType));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(tdInner.PbType(), Is.EqualTo(new Google.Api.Expr.V1Alpha1.Type(){ MessageType = typeof(InnerType).FullName}));
            Assert.That(tdInner.ReflectType(), Is.EqualTo(typeof(InnerType)));
            Assert.That(tdInner.Name(), Is.EqualTo(typeof(InnerType).FullName));
            Assert.That(tdInner.Type(), Is.EqualTo(TypeT.NewObjectTypeValue(typeof(InnerType).FullName)));
            
            //

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            Assert.That(reg)
                .extracting(r => r.findIdent(typeof(CollectionsObject).FullName),
                    r => r.findIdent(typeof(InnerType).FullName),
                    r => r.findIdent(typeof(AnEnum).FullName + '.' + AnEnum.ENUM_VALUE_2.ToString())).containsExactly(
                    TypeT.NewObjectTypeValue(typeof(CollectionsObject).FullName),
                    TypeT.NewObjectTypeValue(typeof(InnerType).FullName), IntT.IntOf(AnEnum.ENUM_VALUE_2.ordinal()));

            Assert.ThatThrownBy(() => reg.TypeDescription(typeof(AnEnum)))
                .isInstanceOf(typeof(System.ArgumentException));

            Assert.ThatThrownBy(() => reg.EnumDescription(typeof(InnerType)))
                .isInstanceOf(typeof(System.ArgumentException));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void types()
        internal virtual void Types()
        {
            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            // verify the map-type-fields

            CheckMapType(reg, "stringBooleanMap", typeof(string), Checked.checkedString, typeof(Boolean),
                Checked.checkedBool);
            CheckMapType(reg, "byteShortMap", typeof(Byte), Checked.checkedInt, typeof(Short), Checked.checkedInt);
            CheckMapType(reg, "intLongMap", typeof(Integer), Checked.checkedInt, typeof(Long), Checked.checkedInt);
            CheckMapType(reg, "ulongTimestampMap", typeof(ULong), Checked.checkedUint, typeof(Timestamp),
                Checked.checkedTimestamp);
            CheckMapType(reg, "ulongZonedDateTimeMap", typeof(ULong), Checked.checkedUint, typeof(ZonedDateTime),
                Checked.checkedTimestamp);
            CheckMapType(reg, "stringProtoDurationMap", typeof(string), Checked.checkedString, typeof(Duration),
                Checked.checkedDuration);
            CheckMapType(reg, "stringJavaDurationMap", typeof(string), Checked.checkedString,
                typeof(java.time.Duration), Checked.checkedDuration);
            CheckMapType(reg, "stringBytesMap", typeof(string), Checked.checkedString, typeof(ByteString),
                Checked.checkedBytes);
            CheckMapType(reg, "floatDoubleMap", typeof(Float), Checked.checkedDouble, typeof(Double),
                Checked.checkedDouble);

            // verify the list-type-fields

            CheckListType(reg, "stringList", typeof(string), Checked.checkedString);
            CheckListType(reg, "booleanList", typeof(Boolean), Checked.checkedBool);
            CheckListType(reg, "byteList", typeof(Byte), Checked.checkedInt);
            CheckListType(reg, "shortList", typeof(Short), Checked.checkedInt);
            CheckListType(reg, "intList", typeof(Integer), Checked.checkedInt);
            CheckListType(reg, "longList", typeof(Long), Checked.checkedInt);
            CheckListType(reg, "ulongList", typeof(ULong), Checked.checkedUint);
            CheckListType(reg, "timestampList", typeof(Timestamp), Checked.checkedTimestamp);
            CheckListType(reg, "zonedDateTimeList", typeof(ZonedDateTime), Checked.checkedTimestamp);
            CheckListType(reg, "durationList", typeof(Duration), Checked.checkedDuration);
            CheckListType(reg, "javaDurationList", typeof(java.time.Duration), Checked.checkedDuration);
            CheckListType(reg, "bytesList", typeof(ByteString), Checked.checkedBytes);
            CheckListType(reg, "floatList", typeof(Float), Checked.checkedDouble);
            CheckListType(reg, "doubleList", typeof(Double), Checked.checkedDouble);
        }

        private void CheckListType(JsonRegistry reg, string prop, Type valueClass,
            com.google.api.expr.v1alpha1.Type valueType)
        {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            JsonFieldType ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
            Assert.That(ft).isNotNull();
            JavaType javaType = ft.PropertyWriter().getType();

            Assert.That(javaType).extracting(JavaType.isCollectionLikeType).isEqualTo(true);
            Assert.That(javaType.getContentType()).extracting(JavaType.getRawClass).isSameAs(valueClass);

            Assert.That(ft.type).extracting(com.google.api.expr.v1alpha1.Type.getListType)
                .extracting(Google.Api.Expr.V1Alpha1.Type.Types.ListType.getElemType).isSameAs(valueType);
        }

        private void CheckMapType(JsonRegistry reg, string prop, Type keyClass,
            com.google.api.expr.v1alpha1.Type keyType, Type valueClass, com.google.api.expr.v1alpha1.Type valueType)
        {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            JsonFieldType ft = (JsonFieldType)reg.FindFieldType(typeof(CollectionsObject).FullName, prop);
            Assert.That(ft).isNotNull();
            JavaType javaType = ft.PropertyWriter().getType();

            Assert.That(javaType).extracting(JavaType.isMapLikeType).isEqualTo(true);
            Assert.That(javaType.getKeyType()).extracting(JavaType.getRawClass).isSameAs(keyClass);
            Assert.That(javaType.getContentType()).extracting(JavaType.getRawClass).isSameAs(valueClass);

            Assert.That(ft.type).extracting(com.google.api.expr.v1alpha1.Type.getMapType)
                .extracting(Google.Api.Expr.V1Alpha1.Type.Types.MapType.getKeyType,
                    Google.Api.Expr.V1Alpha1.Type.Types.MapType.getValueType).containsExactly(keyType, valueType);
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void unknownProperties()
        internal virtual void UnknownProperties()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal).isInstanceOf(typeof(ObjectT));
            ObjectT obj = (ObjectT)collectionsVal;

            Val x = obj.IsSet(StringT.StringOf("bart"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(x).isInstanceOf(typeof(Err)).extracting(e => (Err)e).extracting(Err::value)
                .isEqualTo("no such field 'bart'");

            x = obj.Get(StringT.StringOf("bart"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(x).isInstanceOf(typeof(Err)).extracting(e => (Err)e).extracting(Err::value)
                .isEqualTo("no such field 'bart'");
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void collectionsObjectEmpty()
        internal virtual void CollectionsObjectEmpty()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal).isInstanceOf(typeof(ObjectT));
            ObjectT obj = (ObjectT)collectionsVal;

            foreach (string field in CollectionsObject.ALL_PROPERTIES)
            {
                Assert.That(obj.IsSet(StringT.StringOf(field))).isSameAs(BoolT.False);
                Assert.That(obj.Get(StringT.StringOf(field))).isSameAs(NullT.NullValue);
            }
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void collectionsObjectTypeTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        internal virtual void CollectionsObjectTypeTest()
        {
            CollectionsObject collectionsObject = new CollectionsObject();

            // populate (primitive) map types

            collectionsObject.stringBooleanMap = singletonMap("a", true);
            collectionsObject.byteShortMap = singletonMap((sbyte)1, (short)2);
            collectionsObject.intLongMap = singletonMap(1, 2L);
            collectionsObject.ulongTimestampMap =
                singletonMap(ULong.ValueOf(1), Timestamp.newBuilder().setSeconds(1).build());
            collectionsObject.ulongZonedDateTimeMap = singletonMap(ULong.ValueOf(1),
                ZonedDateTime.of(LocalDateTime.ofEpochSecond(1, 0, ZoneOffset.UTC), ZoneId.of("UTC")));
            collectionsObject.stringProtoDurationMap = singletonMap("a", Duration.newBuilder().setSeconds(1).build());
            collectionsObject.stringJavaDurationMap = singletonMap("a", java.time.Duration.ofSeconds(1));
            collectionsObject.stringBytesMap = singletonMap("a", ByteString.copyFrom(new sbyte[] { (sbyte)1 }));
            collectionsObject.floatDoubleMap = singletonMap(1f, 2d);

            // populate (primitive) list types

            collectionsObject.stringList = new List<string> { "a", "b", "c" };
            collectionsObject.booleanList = new List<bool> { true, true, false, false };
            collectionsObject.byteList = new List<sbyte> { (sbyte)1, (sbyte)2, (sbyte)3 };
            collectionsObject.shortList = new List<short> { (short)4, (short)5, (short)6 };
            collectionsObject.intList = new List<int> { 7, 8, 9 };
            collectionsObject.longList = new List<long> { 10L, 11L, 12L };
            collectionsObject.ulongList = new List<ULong> { ULong.ValueOf(1), ULong.ValueOf(2), ULong.ValueOf(3) };
            collectionsObject.timestampList = new List<Timestamp>
            {
                Timestamp.newBuilder().setSeconds(1).build(), Timestamp.newBuilder().setSeconds(2).build(),
                Timestamp.newBuilder().setSeconds(3).build()
            };
            collectionsObject.zonedDateTimeList = new List<ZonedDateTime>
            {
                ZonedDateTime.of(LocalDateTime.ofEpochSecond(1, 0, ZoneOffset.UTC), ZoneId.of("UTC")),
                ZonedDateTime.of(LocalDateTime.ofEpochSecond(2, 0, ZoneOffset.UTC), ZoneId.of("UTC")),
                ZonedDateTime.of(LocalDateTime.ofEpochSecond(3, 0, ZoneOffset.UTC), ZoneId.of("UTC"))
            };
            collectionsObject.durationList = new List<Duration>
            {
                Duration.newBuilder().setSeconds(1).build(), Duration.newBuilder().setSeconds(2).build(),
                Duration.newBuilder().setSeconds(3).build()
            };
            collectionsObject.javaDurationList = new List<java.time.Duration>
                { java.time.Duration.ofSeconds(1), java.time.Duration.ofSeconds(2), java.time.Duration.ofSeconds(3) };
            collectionsObject.bytesList = new List<ByteString>
            {
                ByteString.copyFrom(new sbyte[] { (sbyte)1 }), ByteString.copyFrom(new sbyte[] { (sbyte)2 }),
                ByteString.copyFrom(new sbyte[] { (sbyte)3 })
            };
            collectionsObject.floatList = new List<float> { 1f, 2f, 3f };
            collectionsObject.doubleList = new List<double> { 1d, 2d, 3d };

            // populate inner/nested type list/map

            InnerType inner1 = new InnerType();
            inner1.intProp = 1;
            inner1.wrappedIntProp = 2;
            collectionsObject.stringInnerMap = singletonMap("a", inner1);

            InnerType inner2 = new InnerType();
            inner2.intProp = 3;
            inner2.wrappedIntProp = 4;
            collectionsObject.innerTypes = new List<InnerType> { inner1, inner2 };

            // populate enum-related fields

            collectionsObject.anEnum = AnEnum.ENUM_VALUE_2;
            collectionsObject.anEnumList = new List<AnEnum> { AnEnum.ENUM_VALUE_2, AnEnum.ENUM_VALUE_3 };
            collectionsObject.anEnumStringMap = singletonMap(AnEnum.ENUM_VALUE_2, "a");
            collectionsObject.stringAnEnumMap = singletonMap("a", AnEnum.ENUM_VALUE_2);

            // prepare registry

            JsonRegistry reg = (JsonRegistry)JsonRegistry.NewRegistry();
            reg.Register(typeof(CollectionsObject));

            Val collectionsVal = reg.NativeToValue(collectionsObject);
            Assert.That(collectionsVal).isInstanceOf(typeof(ObjectT));
            ObjectT obj = (ObjectT)collectionsVal;

            // briefly verify all fields

            foreach (string field in CollectionsObject.ALL_PROPERTIES)
            {
                Assert.That(obj.IsSet(StringT.StringOf(field))).isSameAs(BoolT.True);
                Assert.That(obj.Get(StringT.StringOf(field))).isNotNull();

                Val fieldVal = obj.Get(StringT.StringOf(field));
                object fieldObj = typeof(CollectionsObject).getDeclaredField(field).get(collectionsObject);
                if (fieldObj is System.Collections.IDictionary)
                {
                    Assert.That(fieldVal).isInstanceOf(typeof(MapT));
                }
                else if (fieldObj is System.Collections.IList)
                {
                    Assert.That(fieldVal).isInstanceOf(typeof(ListT));
                }

                Assert.That(fieldVal.Equal(reg.NativeToValue(fieldObj))).isSameAs(BoolT.True);
            }

            // check a few properties manually/explicitly

            MapT mapVal = (MapT)obj.Get(StringT.StringOf("intLongMap"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(mapVal)
                .extracting(MapT::size, m => m.contains(IntT.IntOf(42)), m => m.contains(IntT.IntOf(1)),
                    m => m.contains(IntT.IntOf(2)), m => m.contains(IntT.IntOf(3)), m => m.get(IntT.IntOf(1)))
                .containsExactly(IntT.IntOf(1), BoolT.False, BoolT.True, BoolT.False, BoolT.False, IntT.IntOf(2));

            ListT listVal = (ListT)obj.Get(StringT.StringOf("ulongList"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(listVal)
                .extracting(ListT::size, l => l.contains(UintT.UintOf(42)), l => l.contains(UintT.UintOf(1)),
                    l => l.contains(UintT.UintOf(2)), l => l.contains(UintT.UintOf(3)), l => l.get(IntT.IntOf(0)),
                    l => l.get(IntT.IntOf(1)), l => l.get(IntT.IntOf(2))).containsExactly(IntT.IntOf(3), BoolT.False,
                    BoolT.True, BoolT.True, BoolT.True, UintT.UintOf(1), UintT.UintOf(2), UintT.UintOf(3));

            mapVal = (MapT)obj.Get(StringT.StringOf("stringInnerMap"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(mapVal)
                .extracting(MapT::size, m => m.contains(StringT.StringOf("42")), m => m.contains(StringT.StringOf("a")))
                .containsExactly(IntT.IntOf(1), BoolT.False, BoolT.True);
            ObjectT i = (ObjectT)mapVal.Get(StringT.StringOf("a"));
            Assert.That(i)
                .extracting(o => o.get(StringT.StringOf("intProp")), o => o.get(StringT.StringOf("wrappedIntProp")))
                .containsExactly(IntT.IntOf(1), IntT.IntOf(2));

            listVal = (ListT)obj.Get(StringT.StringOf("innerTypes"));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(listVal).extracting(ListT::size).isEqualTo(IntT.IntOf(2));
            i = (ObjectT)listVal.Get(IntT.IntOf(0));
            Assert.That(i)
                .extracting(o => o.get(StringT.StringOf("intProp")), o => o.get(StringT.StringOf("wrappedIntProp")))
                .containsExactly(IntT.IntOf(1), IntT.IntOf(2));
            i = (ObjectT)listVal.Get(IntT.IntOf(1));
            Assert.That(i)
                .extracting(o => o.get(StringT.StringOf("intProp")), o => o.get(StringT.StringOf("wrappedIntProp")))
                .containsExactly(IntT.IntOf(3), IntT.IntOf(4));

            // verify enums

            Val x = obj.Get(StringT.StringOf("anEnum"));
            Assert.That(x).isInstanceOf(typeof(IntT)).isEqualTo(IntT.IntOf(AnEnum.ENUM_VALUE_2.ordinal()));
            listVal = (ListT)obj.Get(StringT.StringOf("anEnumList"));
            Assert.That(listVal).extracting(l => l.get(IntT.IntOf(0)), l => l.get(IntT.IntOf(1)))
                .containsExactly(IntT.IntOf(AnEnum.ENUM_VALUE_2.ordinal()), IntT.IntOf(AnEnum.ENUM_VALUE_3.ordinal()));
            mapVal = (MapT)obj.Get(StringT.StringOf("anEnumStringMap"));
            Assert.That(mapVal).extracting(l => l.get(IntT.IntOf(AnEnum.ENUM_VALUE_2.ordinal())))
                .isEqualTo(StringT.StringOf("a"));
            mapVal = (MapT)obj.Get(StringT.StringOf("stringAnEnumMap"));
            Assert.That(mapVal).extracting(l => l.get(StringT.StringOf("a")))
                .isEqualTo(IntT.IntOf(AnEnum.ENUM_VALUE_2.ordinal()));
        }
    }
}