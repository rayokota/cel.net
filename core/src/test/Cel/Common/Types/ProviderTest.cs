using System;
using System.Collections;
using System.Text;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Api.Expr.Test.V1.Proto2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;
using Duration = Google.Protobuf.WellKnownTypes.Duration;

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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Google.Protobuf.WellKnownTypes.NullValue.NullValue.NullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.That;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Util.TestUtil.MapOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BytesT.BytesT.BytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.DoubleT.DoubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DurationT.DurationT.DurationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntT.IntOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.ListT.ListT.NewGenericArrayList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.MapT.MapT.NewMaybeWrappedMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.NullT.NullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.StringT.StringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.ZoneIdZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TimestampT.TimestampT.TimestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.UintT.UintOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.pb.ProtoTypeRegistry.ProtoTypeRegistry.NewEmptyRegistry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.pb.ProtoTypeRegistry.ProtoTypeRegistry.NewRegistry;

    using GlobalEnum = Google.Api.Expr.Test.V1.Proto3.GlobalEnum;
    using TestAllTypes = Google.Api.Expr.Test.V1.Proto3.TestAllTypes;
    using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
    using Constant = Google.Api.Expr.V1Alpha1.Constant;
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using ParsedExpr = Google.Api.Expr.V1Alpha1.ParsedExpr;
    using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
    using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
    using ByteString = Google.Protobuf.ByteString;
    using BytesValue = Google.Protobuf.WellKnownTypes.BytesValue;
    using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
    using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
    using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
    using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
    using Message = Google.Protobuf.IMessage;
    using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;

    public class ProviderTest
    {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryCopy()
/*
[Test]
        public virtual void TypeRegistryCopy()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry();
            TypeRegistry reg2 = reg.Copy();
            Assert.That(reg, Is.EqualTo(reg2));

            reg = ProtoTypeRegistry.NewRegistry();
            reg2 = reg.Copy();
            Assert.That(reg, Is.EqualTo(reg2));
        }
        */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryEnumValue()
/*
[Test]
        public virtual void TypeRegistryEnumValue()
        {
            ProtoTypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
            reg.RegisterDescriptor(TestAllTypesReflection.Descriptor);
            reg.RegisterDescriptor(OutOfOrderEnumOuterClass.getDescriptor().getFile());

            Val enumVal = reg.EnumValue("google.api.expr.test.v1.proto3.GlobalEnum.GOO");
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(enumVal).extracting(Val::intValue).isEqualTo((long)GlobalEnum.GOO.getNumber());

            Val enumVal2 = reg.FindIdent("google.api.expr.test.v1.proto3.GlobalEnum.GOO");
            Assert.That(enumVal2.Equal(enumVal), Is.SameAs(BoolT.True));

            // Previously, we checked `getIndex` on the `EnumValueDescriptor`, which is the same as the
            // `ordinal` value on the enum.
            // Test the case where the protobuf-defined value for the enum differs from the generated Java
            // enum's ordinal() function.
            Assert.That(OutOfOrderEnumOuterClass.OutOfOrderEnum.TWO.ordinal())
                .isEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.TWO.getValueDescriptor().getIndex())
                .isNotEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.TWO.getNumber());
            // Check that we correctly get the protobuf-defined number.
            Val enumVal3 = reg.EnumValue("org.projectnessie.cel.test.proto3.OutOfOrderEnum.TWO");
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(enumVal3).extracting(Val::intValue)
                .isEqualTo((long)OutOfOrderEnumOuterClass.OutOfOrderEnum.TWO.getNumber());

            // Test also with the case where there's a gap in the enum (FOUR is not defined).
            Assert.That(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.ordinal())
                .isEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getValueDescriptor().getIndex())
                .isNotEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getNumber());
            // Check that we correctly get the protobuf-defined number.
            Val enumVal4 = reg.EnumValue("org.projectnessie.cel.test.proto3.OutOfOrderEnum.FIVE");
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(enumVal4).extracting(Val::intValue)
                .isEqualTo((long)OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getNumber());
        }
        */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryFindType()
[Test]
        public virtual void TypeRegistryFindType()
        {
            ProtoTypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
            reg.RegisterDescriptor(TestAllTypes.Descriptor.File);

            string msgTypeName = "google.api.expr.test.v1.proto3.TestAllTypes";
            Assert.That(reg.FindType(msgTypeName), Is.Not.Null);
            // Assert.That(reg.findType(msgTypeName + "Undefined")).isNotNull(); ... this doesn't exist in
            // protobuf-java
            Assert.That(reg.FindFieldType(msgTypeName, "single_bool"), Is.Not.Null);
            Assert.That(reg.FindFieldType(msgTypeName, "double_bool"), Is.Null);
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryNewValue()
[Test]
        public virtual void TypeRegistryNewValue()
        {
            IDictionary dict = new Dictionary<object, object>();
            dict[1L] = 2L;
            dict[3L] = 4L;
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
            Val sourceInfo = reg.NewValue("google.api.expr.v1alpha1.SourceInfo",
                TestUtil.MappingOf("location", StringT.StringOf("TestTypeRegistryNewValue"), "line_offsets",
                    ListT.NewGenericArrayList(reg.ToTypeAdapter(), new long?[] { 0L, 2L }), "positions",
                    MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)));
            Assert.That(Err.IsError(sourceInfo), Is.False);
            Message info = (Message)sourceInfo.Value();
            SourceInfo srcInfo = new SourceInfo();
            srcInfo.MergeFrom(info.ToByteArray());
            Assert.That(srcInfo.Location, Is.EqualTo("TestTypeRegistryNewValue"));
            Assert.That(srcInfo.LineOffsets, Is.EquivalentTo(new List<int> { 0, 2 }));
            Assert.That(srcInfo.Positions, Is.EquivalentTo(TestUtil.MapOf(1L, 2L, 3L, 4L)));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryNewValue_OneofFields()
[Test]
        public virtual void TypeRegistryNewValueOneofFields()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new CheckedExpr(), new ParsedExpr());
            Val exp = reg.NewValue("google.api.expr.v1alpha1.CheckedExpr",
                TestUtil.MappingOf("expr",
                    reg.NewValue("google.api.expr.v1alpha1.Expr", 
                        TestUtil.MappingOf("const_expr",
                            reg.NewValue("google.api.expr.v1alpha1.Constant",
                                TestUtil.MappingOf("string_value", StringT.StringOf("oneof")))))));

            Assert.That(Err.IsError(exp), Is.False);
            CheckedExpr ce = (CheckedExpr) exp.ConvertToNative(typeof(CheckedExpr));
            Assert.That(ce.Expr.ConstExpr.StringValue, Is.EqualTo("oneof"));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryNewValue_WrapperFields()
[Test]
        public virtual void TypeRegistryNewValueWrapperFields()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new TestAllTypes());
            Val exp = reg.NewValue("google.api.expr.test.v1.proto3.TestAllTypes",
                TestUtil.MappingOf("single_int32_wrapper", IntT.IntOf(123)));
            Assert.That(Err.IsError(exp), Is.False);
            TestAllTypes ce = (TestAllTypes) exp.ConvertToNative(typeof(TestAllTypes));
            Assert.That(ce.SingleInt32Wrapper.Value, Is.EqualTo(123));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void typeRegistryGetters()
[Test]
        public virtual void TypeRegistryGetters()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
            IDictionary dict = new Dictionary<long, int>();
            dict[1L] = 2;
            dict[3L] = 4;
            Val sourceInfo = reg.NewValue("google.api.expr.v1alpha1.SourceInfo",
                TestUtil.MappingOf("location", StringT.StringOf("TestTypeRegistryGetFieldValue"), "line_offsets",
                    ListT.NewGenericArrayList(reg.ToTypeAdapter(), new long?[] { 0L, 2L }), "positions",
                    MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)));
            Assert.That(Err.IsError(sourceInfo), Is.False);
            Indexer si = (Indexer)sourceInfo;

            Val loc = si.Get(StringT.StringOf("location"));
            Assert.That(loc.Equal(StringT.StringOf("TestTypeRegistryGetFieldValue")), Is.SameAs(BoolT.True));

            Val pos = si.Get(StringT.StringOf("positions"));
            Assert.That(pos.Equal(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)), Is.SameAs(BoolT.True));

            Val posKeyVal = ((Indexer)pos).Get(IntT.IntOf(1));
            Assert.That(posKeyVal.IntValue(), Is.EqualTo(2));

            Val offsets = si.Get(StringT.StringOf("line_offsets"));
            Assert.That(Err.IsError(offsets), Is.False);
            Val offset1 = ((Lister)offsets).Get(IntT.IntOf(1));
            Assert.That(offset1, Is.EqualTo(IntT.IntOf(2)));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void convertToNative()
[Test]
        public virtual void ConvertToNative()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());

            // Core type conversion tests.
            ExpectValueToNative(BoolT.True, true);
            ExpectValueToNative(BoolT.True, BoolT.True);
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { BoolT.True, BoolT.False }), new object[] { true, false });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { BoolT.True, BoolT.False }), new Val[] { BoolT.True, BoolT.False });
            ExpectValueToNative(IntT.IntOf(-1), -1);
            ExpectValueToNative(IntT.IntOf(2), 2L);
            ExpectValueToNative(IntT.IntOf(-1), -1);
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { IntT.IntOf(4) }), new object[] { 4L });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { IntT.IntOf(5) }), new Val[] { IntT.IntOf(5) });
            ExpectValueToNative(UintT.UintOf(3), (ulong)3);
            ExpectValueToNative(UintT.UintOf(4), (ulong)4);
            ExpectValueToNative(UintT.UintOf(5), 5);
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { UintT.UintOf(4) }),
                new object[] { 4L }); // loses "ULong" here
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { UintT.UintOf(5) }), new Val[] { UintT.UintOf(5) });
            ExpectValueToNative(DoubleT.DoubleOf(5.5d), 5.5f);
            ExpectValueToNative(DoubleT.DoubleOf(-5.5d), -5.5d);
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { DoubleT.DoubleOf(-5.5) }), new object[] { -5.5 });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { DoubleT.DoubleOf(-5.5) }), new Val[] { DoubleT.DoubleOf(-5.5) });
            ExpectValueToNative(DoubleT.DoubleOf(-5.5), DoubleT.DoubleOf(-5.5));
            ExpectValueToNative(StringT.StringOf("hello"), "hello");
            ExpectValueToNative(StringT.StringOf("hello"), StringT.StringOf("hello"));
            ExpectValueToNative(NullT.NullValue, NullValue.NullValue);
            ExpectValueToNative(NullT.NullValue, NullT.NullValue);
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { NullT.NullValue }), new object[] { null });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { NullT.NullValue }), new Val[] { NullT.NullValue });
            ExpectValueToNative(BytesT.BytesOf("world"), Encoding.UTF8.GetBytes("world"));
            ExpectValueToNative(BytesT.BytesOf("world"), Encoding.UTF8.GetBytes("world"));
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { BytesT.BytesOf("hello") }),
                new object[] { ByteString.CopyFromUtf8("hello") });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { BytesT.BytesOf("hello") }),
                new Val[] { BytesT.BytesOf("hello") });
            ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new Val[] { IntT.IntOf(1), IntT.IntOf(2), IntT.IntOf(3) }),
                new object[] { 1L, 2L, 3L });
            ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)), Period.FromSeconds(500));
            Duration d = new Duration();
            d.Seconds = 500;
            ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)), d);
            ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)), DurationT.DurationOf(Period.FromSeconds(500)));
            Timestamp ts = new Timestamp();
            ts.Seconds = 12345;
            ExpectValueToNative(TimestampT.TimestampOf(ts), Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ));
            ExpectValueToNative(TimestampT.TimestampOf(ts), TimestampT.TimestampOf(ts)); ExpectValueToNative(TimestampT.TimestampOf(ts), ts);
            IDictionary dict = new Dictionary<object, object>();
            dict[1L] = 1L;
            dict[2L] = 1L;
            dict[3L] = 1L;
            ExpectValueToNative(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict), dict);

            // Null conversion tests.
            ExpectValueToNative(NullT.NullValue, NullValue.NullValue);

            // Proto conversion tests.
            ParsedExpr parsedExpr = new ParsedExpr();
            ExpectValueToNative(reg.ToTypeAdapter()(parsedExpr), parsedExpr);
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void nativeToValue_Any()
[Test]
        public virtual void NativeToValueAny()
        {
            //    		TypeRegistry reg = ProtoTypeRegistry.NewRegistry(ParsedExpr.getDefaultInstance())
            //    		// NullValue
            //    		Any anyValue = NullValue.convertToNative(Any.class);
            //    		expectNativeToValue(anyValue, NullValue);
            //
            //    		// Json Struct
            //    		anyValue = anypb.New(
            //    			structpb.NewStructValue(
            //    				&structpb.Struct{
            //    					Fields: map[string]*structpb.Value{
            //    						"a": structpb.NewStringValue("world"),
            //    						"b": structpb.NewStringValue("five!"),
            //    					},
            //    				},
            //    			),
            //    		);
            //    		expected = newJSONStruct(reg, &structpb.Struct{
            //    			Fields: map[string]*structpb.Value{
            //    				"a": structpb.NewStringValue("world"),
            //    				"b": structpb.NewStringValue("five!"),
            //    			},
            //    		})
            //    		expectNativeToValue(anyValue, expected)
            //
            //    		//Json List
            //    		anyValue = anypb.New(structpb.NewListValue(
            //    			&structpb.ListValue{
            //    				Values: []*structpb.Value{
            //    					structpb.NewStringValue("world"),
            //    					structpb.NewStringValue("five!"),
            //    				},
            //    			},
            //    		));
            //    		expectedList = newJSONList(reg, &structpb.ListValue{
            //    			Values: []*structpb.Value{
            //    				structpb.NewStringValue("world"),
            //    				structpb.NewStringValue("five!"),
            //    			}})
            //    		expectNativeToValue(anyValue, expectedList)
            //
            //    		// Object
            //    		pbMessage = exprpb.ParsedExpr{
            //    			SourceInfo: &exprpb.SourceInfo{
            //    				LineOffsets: []int32{1, 2, 3}}}
            //    		anyValue = anypb.New(&pbMessage);
            //    		expectNativeToValue(anyValue, reg.nativeToValue(&pbMessage))
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void nativeToValue_Json()
[Test]
        public virtual void NativeToValueJson()
        {
            //    		TypeRegistry reg = ProtoTypeRegistry.NewRegistry(ParsedExpr.getDefaultInstance())
            //    		// Json primitive conversion test.
            //    		expectNativeToValue(BoolValue.of(false), BoolT.False);
            //    		expectNativeToValue(Value.newBuilder().setNumberValue(1.1d).build(), DoubleT.DoubleOf(1.1));
            //    		expectNativeToValue(Google.Protobuf.WellKnownTypes.NullValue.forNumber(0), NullValue);
            //    		expectNativeToValue(StringValue.of("hello"), StringT.StringOf("hello"));
            //
            //    		// Json list conversion.
            //    		expectNativeToValue(
            //						ListValue.newBuilder()
            //						.addValues(Value.newBuilder().setStringValue("world"))
            //						.addValues(Value.newBuilder().setStringValue("five!"))
            //						.build(),
            //    			newJSONList(reg, ListValue.newBuilder()
            //							.addValues(Value.newBuilder().setStringValue("world"))
            //							.addValues(Value.newBuilder().setStringValue("five!"))
            //							.build()));
            //
            //    		// Json struct conversion.
            //    		expectNativeToValue(
            //						Struct.newBuilder()
            //							.putFields("a", Value.newBuilder().setStringValue("world").build())
            //							.putFields("b", Value.newBuilder().setStringValue("five!").build())
            //							.build(),
            //    			newJSONStruct(reg, Struct.newBuilder()
            //							.putFields("a", Value.newBuilder().setStringValue("world").build())
            //							.putFields("b", Value.newBuilder().setStringValue("five!").build())
            //							.build()));
            //
            //    		// Proto conversion test.
            //    		ParsedExpr parsedExpr = ParsedExpr.getDefaultInstance();
            //    		expectNativeToValue(parsedExpr, reg.nativeToValue(parsedExpr));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nativeToValue_Wrappers()
[Test]
        public virtual void NativeToValueWrappers()
        {
            BoolValue bv = new BoolValue();
            bv.Value = true;
            BytesValue bytesValue = new BytesValue();
            bytesValue.Value = ByteString.CopyFromUtf8("hi");
            DoubleValue dv = new DoubleValue();
            dv.Value = 6.4;
            FloatValue fv = new FloatValue();
            fv.Value = 3.0f;
            Int32Value iv = new Int32Value();
            iv.Value = -32;
            Int64Value lv = new Int64Value();
            lv.Value = -64;
            StringValue sv = new StringValue();
            sv.Value = "hello";
            UInt32Value uiv = new UInt32Value();
            uiv.Value = 32;
            UInt64Value ulv = new UInt64Value();
            ulv.Value = 64;
                
            // Wrapper conversion test.
            ExpectNativeToValue(bv, BoolT.True);
            ExpectNativeToValue(new BoolValue(), BoolT.False);
            ExpectNativeToValue(new BytesValue(), BytesT.BytesOf(""));
            ExpectNativeToValue(bytesValue, BytesT.BytesOf("hi"));
            ExpectNativeToValue(new DoubleValue(), DoubleT.DoubleOf(0.0));
            ExpectNativeToValue(dv, DoubleT.DoubleOf(6.4));
            ExpectNativeToValue(new FloatValue(), DoubleT.DoubleOf(0.0));
            ExpectNativeToValue(fv, DoubleT.DoubleOf(3.0));
            ExpectNativeToValue(new Int32Value(), IntT.IntZero);
            ExpectNativeToValue(iv, IntT.IntOf(-32));
            ExpectNativeToValue(new Int64Value(), IntT.IntZero);
            ExpectNativeToValue(lv, IntT.IntOf(-64));
            ExpectNativeToValue(new StringValue(), StringT.StringOf(""));
            ExpectNativeToValue(sv, StringT.StringOf("hello"));
            ExpectNativeToValue(new UInt32Value(), UintT.UintOf(0));
            ExpectNativeToValue(uiv, UintT.UintOf(32));
            ExpectNativeToValue(new UInt64Value(), UintT.UintOf(0));
            ExpectNativeToValue(ulv, UintT.UintOf(64));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nativeToValue_Primitive()
[Test]
        public virtual void NativeToValuePrimitive()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();

            // Core type conversions.
            ExpectNativeToValue(true, BoolT.True);
            ExpectNativeToValue(-10, IntT.IntOf(-10));
            ExpectNativeToValue(-1, IntT.IntOf(-1));
            ExpectNativeToValue(2L, IntT.IntOf(2));
            ExpectNativeToValue((ulong)6, UintT.UintOf(6));
            ExpectNativeToValue((ulong)3, UintT.UintOf(3));
            ExpectNativeToValue((ulong)4, UintT.UintOf(4));
            ExpectNativeToValue(5.5f, DoubleT.DoubleOf(5.5));
            ExpectNativeToValue(-5.5d, DoubleT.DoubleOf(-5.5));
            ExpectNativeToValue("hello", StringT.StringOf("hello"));
            ExpectNativeToValue(Encoding.UTF8.GetBytes("world"), BytesT.BytesOf("world"));
            ExpectNativeToValue(Period.FromSeconds(500), DurationT.DurationOf(Period.FromSeconds(500)));
            Timestamp ts = new Timestamp();
            ts.Seconds = 12345;
            ZonedDateTime cal = ZonedDateTime.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(12345));
            ExpectNativeToValue(cal, TimestampT.TimestampOf(ts));
            ExpectNativeToValue(Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ),
                TimestampT.TimestampOf(ts));
            ExpectNativeToValue(Period.FromSeconds(500), DurationT.DurationOf(Period.FromSeconds(500)));
            ExpectNativeToValue(new int[] { 1, 2, 3 }, ListT.NewGenericArrayList(reg.ToTypeAdapter(), new object[] { 1, 2, 3 }));
            IDictionary dict = new Dictionary<object, object>();
            dict[1L] = 1L;
            dict[2L] = 1L;
            dict[3L] = 1L;
            ExpectNativeToValue(TestUtil.MapOf(1, 1, 2, 1, 3, 1), MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict));

            // Null conversion test.
            ExpectNativeToValue(null, NullT.NullValue);
            ExpectNativeToValue(NullValue.NullValue, NullT.NullValue);
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void unsupportedConversion()
[Test]
        public virtual void UnsupportedConversion()
        {
            TypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
            Val val = reg.ToTypeAdapter()(new nonConvertible());
            Assert.That(Err.IsError(val), Is.True);
        }

        internal class nonConvertible
        {
        }

        internal static void ExpectValueToNative(Val @in, object @out)
        {
            object val = @in.ConvertToNative(@out.GetType());
            Assert.That(val, Is.Not.Null);

            if (val is byte[])
            {
                Assert.That((byte[])val, Is.EqualTo((byte[])@in.Value()));
            }
            else
            {
                Assert.That(val, Is.EqualTo(@out));
            }
        }

        internal static void ExpectNativeToValue(object @in, Val @out)
        {
            TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
            Val val = reg.ToTypeAdapter()(@in);
            Assert.That(Err.IsError(val), Is.False);
            Assert.That(val.Equal(@out), Is.SameAs(BoolT.True));
        }
    }
}