using System.Collections;
using System.Text;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Api.Expr.Test.V1.Proto3;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NUnit.Framework;
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
namespace Cel.Common.Types;

using Message = IMessage;

public class ProviderTest
{
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

/*
[Test]
        public virtual void TypeRegistryEnumValue()
        {
            ProtoTypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
            reg.RegisterDescriptor(TestAllTypesReflection.Descriptor);
            reg.RegisterDescriptor(OutOfOrderEnumOuterClass.getDescriptor().getFile());

            Val enumVal = reg.EnumValue("google.api.expr.test.v1.proto3.GlobalEnum.GOO");
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
            Val enumVal3 = reg.EnumValue("Cel.Test.Proto3.OutOfOrderEnum.TWO");
            Assert.That(enumVal3).extracting(Val::intValue)
                .isEqualTo((long)OutOfOrderEnumOuterClass.OutOfOrderEnum.TWO.getNumber());

            // Test also with the case where there's a gap in the enum (FOUR is not defined).
            Assert.That(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.ordinal())
                .isEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getValueDescriptor().getIndex())
                .isNotEqualTo(OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getNumber());
            // Check that we correctly get the protobuf-defined number.
            Val enumVal4 = reg.EnumValue("Cel.Test.Proto3.OutOfOrderEnum.FIVE");
            Assert.That(enumVal4).extracting(Val::intValue)
                .isEqualTo((long)OutOfOrderEnumOuterClass.OutOfOrderEnum.FIVE.getNumber());
        }
        */

    [Test]
    public virtual void TypeRegistryFindType()
    {
        var reg = ProtoTypeRegistry.NewEmptyRegistry();
        reg.RegisterDescriptor(TestAllTypes.Descriptor.File);

        var msgTypeName = "google.api.expr.test.v1.proto3.TestAllTypes";
        Assert.That(reg.FindType(msgTypeName), Is.Not.Null);
        // Assert.That(reg.findType(msgTypeName + "Undefined")).isNotNull(); ... this doesn't exist in
        // protobuf-java
        Assert.That(reg.FindFieldType(msgTypeName, "single_bool"), Is.Not.Null);
        Assert.That(reg.FindFieldType(msgTypeName, "double_bool"), Is.Null);
    }

    [Test]
    public virtual void TypeRegistryNewValue()
    {
        IDictionary dict = new Dictionary<object, object>();
        dict[1L] = 2L;
        dict[3L] = 4L;
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
        var sourceInfo = reg.NewValue("google.api.expr.v1alpha1.SourceInfo",
            TestUtil.ValMapOf("location", StringT.StringOf("TestTypeRegistryNewValue"), "line_offsets",
                ListT.NewGenericArrayList(reg.ToTypeAdapter(), new long?[] { 0L, 2L }), "positions",
                MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)));
        Assert.That(Err.IsError(sourceInfo), Is.False);
        var info = (Message)sourceInfo.Value();
        var srcInfo = new SourceInfo();
        srcInfo.MergeFrom(info.ToByteArray());
        Assert.That(srcInfo.Location, Is.EqualTo("TestTypeRegistryNewValue"));
        Assert.That(srcInfo.LineOffsets, Is.EquivalentTo(new List<int> { 0, 2 }));
        Assert.That(srcInfo.Positions, Is.EquivalentTo(TestUtil.MapOf(1L, 2L, 3L, 4L)));
    }

    [Test]
    public virtual void TypeRegistryNewValueOneofFields()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new CheckedExpr(), new ParsedExpr());
        var exp = reg.NewValue("google.api.expr.v1alpha1.CheckedExpr",
            TestUtil.ValMapOf("expr",
                reg.NewValue("google.api.expr.v1alpha1.Expr",
                    TestUtil.ValMapOf("const_expr",
                        reg.NewValue("google.api.expr.v1alpha1.Constant",
                            TestUtil.ValMapOf("string_value", StringT.StringOf("oneof")))))));

        Assert.That(Err.IsError(exp), Is.False);
        var ce = (CheckedExpr)exp.ConvertToNative(typeof(CheckedExpr));
        Assert.That(ce.Expr.ConstExpr.StringValue, Is.EqualTo("oneof"));
    }

    [Test]
    public virtual void TypeRegistryNewValueNotWrapperFields()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new TestAllTypes());
        var exp = reg.NewValue("google.api.expr.test.v1.proto3.TestAllTypes",
            TestUtil.ValMapOf("single_int32", IntT.IntOf(123)));
        Assert.That(Err.IsError(exp), Is.False);
        var ce = (TestAllTypes)exp.ConvertToNative(typeof(TestAllTypes));
        Assert.That(ce.SingleInt32, Is.EqualTo(123));
    }

    [Test]
    public virtual void TypeRegistryNewValueWrapperFields()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new TestAllTypes());
        var exp = reg.NewValue("google.api.expr.test.v1.proto3.TestAllTypes",
            TestUtil.ValMapOf("single_int32_wrapper", IntT.IntOf(123)));
        Assert.That(Err.IsError(exp), Is.False);
        var ce = (TestAllTypes)exp.ConvertToNative(typeof(TestAllTypes));
        Assert.That(ce.SingleInt32Wrapper.Value, Is.EqualTo(123));
    }

    [Test]
    public virtual void TypeRegistryGetters()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
        IDictionary dict = new Dictionary<long, int>();
        dict[1L] = 2;
        dict[3L] = 4;
        var sourceInfo = reg.NewValue("google.api.expr.v1alpha1.SourceInfo",
            TestUtil.ValMapOf("location", StringT.StringOf("TestTypeRegistryGetFieldValue"), "line_offsets",
                ListT.NewGenericArrayList(reg.ToTypeAdapter(), new long?[] { 0L, 2L }), "positions",
                MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)));
        Assert.That(Err.IsError(sourceInfo), Is.False);
        var si = (IIndexer)sourceInfo;

        var loc = si.Get(StringT.StringOf("location"));
        Assert.That(loc.Equal(StringT.StringOf("TestTypeRegistryGetFieldValue")), Is.SameAs(BoolT.True));

        var pos = si.Get(StringT.StringOf("positions"));
        Assert.That(pos.Equal(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict)), Is.SameAs(BoolT.True));

        var posKeyVal = ((IIndexer)pos).Get(IntT.IntOf(1));
        Assert.That(posKeyVal.IntValue(), Is.EqualTo(2));

        var offsets = si.Get(StringT.StringOf("line_offsets"));
        Assert.That(Err.IsError(offsets), Is.False);
        var offset1 = ((ILister)offsets).Get(IntT.IntOf(1));
        Assert.That(offset1, Is.EqualTo(IntT.IntOf(2)));
    }

    [Test]
    public virtual void ConvertToNative()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());

        // Core type conversion tests.
        ExpectValueToNative(BoolT.True, true);
        ExpectValueToNative(BoolT.True, BoolT.True);
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { BoolT.True, BoolT.False }),
            new object[] { true, false });
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { BoolT.True, BoolT.False }),
            new IVal[] { BoolT.True, BoolT.False });
        ExpectValueToNative(IntT.IntOf(-1), -1);
        ExpectValueToNative(IntT.IntOf(2), 2L);
        ExpectValueToNative(IntT.IntOf(-1), -1);
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { IntT.IntOf(4) }),
            new object[] { 4L });
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { IntT.IntOf(5) }),
            new IVal[] { IntT.IntOf(5) });
        ExpectValueToNative(UintT.UintOf(3), (ulong)3);
        ExpectValueToNative(UintT.UintOf(4), (ulong)4);
        ExpectValueToNative(UintT.UintOf(5), 5);
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { UintT.UintOf(4) }),
            new object[] { 4L }); // loses "ULong" here
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { UintT.UintOf(5) }),
            new IVal[] { UintT.UintOf(5) });
        ExpectValueToNative(DoubleT.DoubleOf(5.5d), 5.5f);
        ExpectValueToNative(DoubleT.DoubleOf(-5.5d), -5.5d);
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { DoubleT.DoubleOf(-5.5) }),
            new object[] { -5.5 });
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { DoubleT.DoubleOf(-5.5) }),
            new IVal[] { DoubleT.DoubleOf(-5.5) });
        ExpectValueToNative(DoubleT.DoubleOf(-5.5), DoubleT.DoubleOf(-5.5));
        ExpectValueToNative(StringT.StringOf("hello"), "hello");
        ExpectValueToNative(StringT.StringOf("hello"), StringT.StringOf("hello"));
        ExpectValueToNative(NullT.NullValue, NullValue.NullValue);
        ExpectValueToNative(NullT.NullValue, NullT.NullValue);
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { NullT.NullValue }),
            new object[] { null });
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { NullT.NullValue }),
            new IVal[] { NullT.NullValue });
        ExpectValueToNative(BytesT.BytesOf("world"), Encoding.UTF8.GetBytes("world"));
        ExpectValueToNative(BytesT.BytesOf("world"), Encoding.UTF8.GetBytes("world"));
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { BytesT.BytesOf("hello") }),
            new object[] { ByteString.CopyFromUtf8("hello") });
        ExpectValueToNative(ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { BytesT.BytesOf("hello") }),
            new IVal[] { BytesT.BytesOf("hello") });
        ExpectValueToNative(
            ListT.NewGenericArrayList(reg.ToTypeAdapter(), new IVal[] { IntT.IntOf(1), IntT.IntOf(2), IntT.IntOf(3) }),
            new object[] { 1L, 2L, 3L });
        ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)), Period.FromSeconds(500));
        var d = new Duration();
        d.Seconds = 500;
        ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)), d);
        ExpectValueToNative(DurationT.DurationOf(Period.FromSeconds(500)),
            DurationT.DurationOf(Period.FromSeconds(500)));
        var ts = new Timestamp();
        ts.Seconds = 12345;
        ExpectValueToNative(TimestampT.TimestampOf(ts), Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ));
        ExpectValueToNative(TimestampT.TimestampOf(ts), TimestampT.TimestampOf(ts));
        ExpectValueToNative(TimestampT.TimestampOf(ts), ts);
        IDictionary dict = new Dictionary<object, object>();
        dict[1L] = 1L;
        dict[2L] = 1L;
        dict[3L] = 1L;
        ExpectValueToNative(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict), dict);

        // Null conversion tests.
        ExpectValueToNative(NullT.NullValue, NullValue.NullValue);

        // Proto conversion tests.
        var parsedExpr = new ParsedExpr();
        ExpectValueToNative(reg.ToTypeAdapter()(parsedExpr), parsedExpr);
    }

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

    [Test]
    public virtual void NativeToValueWrappers()
    {
        var bv = new BoolValue();
        bv.Value = true;
        var bytesValue = new BytesValue();
        bytesValue.Value = ByteString.CopyFromUtf8("hi");
        var dv = new DoubleValue();
        dv.Value = 6.4;
        var fv = new FloatValue();
        fv.Value = 3.0f;
        var iv = new Int32Value();
        iv.Value = -32;
        var lv = new Int64Value();
        lv.Value = -64;
        var sv = new StringValue();
        sv.Value = "hello";
        var uiv = new UInt32Value();
        uiv.Value = 32;
        var ulv = new UInt64Value();
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

    [Test]
    public virtual void NativeToValuePrimitive()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();

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
        var ts = new Timestamp();
        ts.Seconds = 12345;
        var cal = ZonedDateTime.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(12345));
        ExpectNativeToValue(cal, TimestampT.TimestampOf(ts));
        ExpectNativeToValue(Instant.FromUnixTimeSeconds(12345).InZone(TimestampT.ZoneIdZ),
            TimestampT.TimestampOf(ts));
        ExpectNativeToValue(Period.FromSeconds(500), DurationT.DurationOf(Period.FromSeconds(500)));
        ExpectNativeToValue(new[] { 1, 2, 3 },
            ListT.NewGenericArrayList(reg.ToTypeAdapter(), new object[] { 1, 2, 3 }));
        IDictionary dict = new Dictionary<object, object>();
        dict[1L] = 1L;
        dict[2L] = 1L;
        dict[3L] = 1L;
        ExpectNativeToValue(TestUtil.MapOf(1, 1, 2, 1, 3, 1), MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), dict));

        // Null conversion test.
        ExpectNativeToValue(null, NullT.NullValue);
        ExpectNativeToValue(NullValue.NullValue, NullT.NullValue);
    }

    [Test]
    public virtual void UnsupportedConversion()
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
        var val = reg.ToTypeAdapter()(new nonConvertible());
        Assert.That(Err.IsError(val), Is.True);
    }

    internal static void ExpectValueToNative(IVal @in, object @out)
    {
        var val = @in.ConvertToNative(@out.GetType());
        Assert.That(val, Is.Not.Null);

        if (val is byte[])
            Assert.That((byte[])val, Is.EqualTo((byte[])@in.Value()));
        else
            Assert.That(val, Is.EqualTo(@out));
    }

    internal static void ExpectNativeToValue(object? @in, IVal @out)
    {
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
        var val = reg.ToTypeAdapter()(@in);
        Assert.That(Err.IsError(val), Is.False);
        Assert.That(val.Equal(@out), Is.SameAs(BoolT.True));
    }

    internal class nonConvertible
    {
    }
}