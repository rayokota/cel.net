﻿using Google.Api.Expr.Test.V1.Proto3;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

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

using Message = IMessage;

public delegate Message messageSupplier();

/// <summary>
///     Test cases for {@link
///     PbTypeDescriptionTest#benchmarkTypeDescriptionMaybeUnwrap(Cel.Common.Types.Pb.UnwrapTestCase)}
///     and {@code TypeDescriptorBnch} JMH benchmark, latter requires this class to be a top-level public
///     enum.
/// </summary>
public sealed class UnwrapTestCase
{
    public enum InnerEnum
    {
        MsgDesc_zero,
        Structpb_NewBoolValue_true,
        Structpb_NewBoolValue_false,
        Structpb_NewNullValue,
        Structpb_Value,
        Structpb_NewNumberValue,
        Structpb_NewStringValue,
        Wrapperspb_Bool_false,
        Wrapperspb_Bool_true,
        Wrapperspb_Bytes,
        Wrapperspb_Double,
        Wrapperspb_Float,
        Wrapperspb_Int32,
        Wrapperspb_Int64,
        Wrapperspb_String,
        Wrapperspb_UInt32,
        Wrapperspb_UInt64,
        Timestamp,
        Duration,
        Proto3pb_TestAllTypes
    }

    private static readonly List<UnwrapTestCase> ValueList = new();

    internal static readonly BoolValue trueBool;
    internal static readonly BoolValue falseBool;
    internal static readonly Value trueValue;
    internal static readonly Value falseValue;
    internal static readonly Value nullValue;
    internal static readonly Value numValue;
    internal static readonly Value strValue1;
    internal static readonly Value strValue2;
    internal static readonly BytesValue bytesValue;
    internal static readonly DoubleValue doubleValue;
    internal static readonly FloatValue floatValue;
    internal static readonly Int32Value int32Value;
    internal static readonly Int64Value int64Value;
    internal static readonly UInt32Value uint32Value;
    internal static readonly UInt64Value uint64Value;
    internal static readonly Timestamp timestampValue;
    internal static readonly Duration durationValue;

    public static readonly UnwrapTestCase MsgDesc_zero =
        new("MsgDesc_zero", InnerEnum.MsgDesc_zero, () => UnwrapContext.Get().msgDesc.Zero());

    public static readonly UnwrapTestCase Structpb_NewBoolValue_true = new("Structpb_NewBoolValue_true",
        InnerEnum.Structpb_Value, () => trueBool);

    public static readonly UnwrapTestCase Structpb_NewBoolValue_false = new("structpb_NewBoolValuefalse",
        InnerEnum.Structpb_NewBoolValue_false, () => falseBool);

    public static readonly UnwrapTestCase Structpb_NewNullValue = new("structPb_NewNullValue",
        InnerEnum.Structpb_NewNullValue,
        () => nullValue);

    public static readonly UnwrapTestCase Structpb_Value =
        new("Structpb_Value", InnerEnum.Structpb_Value, () => new Value());

    public static readonly UnwrapTestCase Structpb_NewNumberValue = new("Structpb_NewNumberValue",
        InnerEnum.Structpb_NewNumberValue,
        () => numValue);

    public static readonly UnwrapTestCase Structpb_NewStringValue = new("Structpb_NewStringValue",
        InnerEnum.Structpb_NewStringValue, () => strValue1);

    public static readonly UnwrapTestCase Wrapperspb_Bool_false = new("Wrapperspb_Bool_False",
        InnerEnum.Wrapperspb_Bool_false, () => falseBool);

    public static readonly UnwrapTestCase Wrapperspb_Bool_true = new("Wrapperspb_Bool_true",
        InnerEnum.Wrapperspb_Bool_true, () => trueBool);

    public static readonly UnwrapTestCase Wrapperspb_Bytes = new("Wrapperspb_Bytes", InnerEnum.Wrapperspb_Bytes,
        () => bytesValue);

    public static readonly UnwrapTestCase Wrapperspb_Double = new("Wrapperspb_Double", InnerEnum.Wrapperspb_Double,
        () => doubleValue);

    public static readonly UnwrapTestCase Wrapperspb_Float = new("Wrapperspb_Float", InnerEnum.Wrapperspb_Float,
        () => floatValue);

    public static readonly UnwrapTestCase Wrapperspb_Int32 = new("Wrapperspb_Int32", InnerEnum.Wrapperspb_Int32,
        () => int32Value);

    public static readonly UnwrapTestCase Wrapperspb_Int64 = new("Wrapperspb_Int64", InnerEnum.Wrapperspb_Int64,
        () => int64Value);

    public static readonly UnwrapTestCase Wrapperspb_String = new("Wrapperspb_String", InnerEnum.Wrapperspb_String,
        () => strValue2);

    public static readonly UnwrapTestCase Wrapperspb_UInt32 = new("Wrapperspb_UInt32", InnerEnum.Wrapperspb_UInt32,
        () => uint32Value);

    public static readonly UnwrapTestCase Wrapperspb_UInt64 = new("Wrapperspb_UInt64", InnerEnum.Wrapperspb_UInt64,
        () => uint64Value);

    public static readonly UnwrapTestCase Timestamp = new("Timestamp", InnerEnum.Timestamp,
        () => timestampValue);

    public static readonly UnwrapTestCase Duration = new("Duration", InnerEnum.Duration,
        () => durationValue);

    public static readonly UnwrapTestCase Proto3pb_TestAllTypes = new("Proto3pb_TestAllTypes",
        InnerEnum.Proto3pb_TestAllTypes, () => new TestAllTypes());

    private static int nextOrdinal;


    private readonly InnerEnum innerEnumValue;

    private readonly Func<Message> message;
    private readonly string nameValue;
    private readonly int ordinalValue;

    static UnwrapTestCase()
    {
        ValueList.Add(MsgDesc_zero);
        ValueList.Add(Structpb_NewBoolValue_true);
        ValueList.Add(Structpb_NewBoolValue_false);
        ValueList.Add(Structpb_NewNullValue);
        ValueList.Add(Structpb_Value);
        ValueList.Add(Structpb_NewNumberValue);
        ValueList.Add(Structpb_NewStringValue);
        ValueList.Add(Wrapperspb_Bool_false);
        ValueList.Add(Wrapperspb_Bool_true);
        ValueList.Add(Wrapperspb_Bytes);
        ValueList.Add(Wrapperspb_Double);
        ValueList.Add(Wrapperspb_Float);
        ValueList.Add(Wrapperspb_Int32);
        ValueList.Add(Wrapperspb_Int64);
        ValueList.Add(Wrapperspb_String);
        ValueList.Add(Wrapperspb_UInt32);
        ValueList.Add(Wrapperspb_UInt64);
        ValueList.Add(Timestamp);
        ValueList.Add(Duration);
        ValueList.Add(Proto3pb_TestAllTypes);

        trueBool = new BoolValue();
        trueBool.Value = true;
        falseBool = new BoolValue();
        falseBool.Value = false;
        trueValue = new Value();
        trueValue.BoolValue = true;
        falseValue = new Value();
        falseValue.BoolValue = false;
        nullValue = new Value();
        nullValue.NullValue = NullValue.NullValue;
        numValue = new Value();
        numValue.NumberValue = 1.5;
        strValue1 = new Value();
        strValue1.StringValue = "hello world";
        strValue2 = new Value();
        strValue2.StringValue = "goodbye";
        bytesValue = new BytesValue();
        bytesValue.Value = ByteString.CopyFromUtf8("hello");
        doubleValue = new DoubleValue();
        doubleValue.Value = -4.2;
        floatValue = new FloatValue();
        floatValue.Value = 4.5f;
        int32Value = new Int32Value();
        int32Value.Value = 123;
        int64Value = new Int64Value();
        int64Value.Value = 456;
        uint32Value = new UInt32Value();
        uint32Value.Value = 1234;
        uint64Value = new UInt64Value();
        uint64Value.Value = 5678;
        timestampValue = new Timestamp();
        timestampValue.Seconds = 12345;
        timestampValue.Nanos = 0;
        durationValue = new Duration();
        durationValue.Seconds = 345;
    }

    internal UnwrapTestCase(string name, InnerEnum innerEnum, Func<Message> message)
    {
        this.message = message;

        nameValue = name;
        ordinalValue = nextOrdinal++;
        innerEnumValue = innerEnum;
    }

    public Message Message()
    {
        return message();
    }

    public static UnwrapTestCase[] Values()
    {
        return ValueList.ToArray();
    }

    public int Ordinal()
    {
        return ordinalValue;
    }

    public override string ToString()
    {
        return nameValue;
    }

    public static UnwrapTestCase valueOf(string name)
    {
        foreach (var enumInstance in ValueList)
            if (enumInstance.nameValue == name)
                return enumInstance;

        throw new ArgumentException(name);
    }
}