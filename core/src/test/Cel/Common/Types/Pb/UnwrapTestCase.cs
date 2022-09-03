using System.Collections.Generic;

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
namespace Cel.Common.Types.Pb
{
    using TestAllTypes = Google.Api.Expr.Test.V1.Proto3.TestAllTypes;
    using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
    using ByteString = Google.Protobuf.ByteString;
    using BytesValue = Google.Protobuf.WellKnownTypes.BytesValue;
    using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
    using Duration = Google.Protobuf.WellKnownTypes.Duration;
    using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
    using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
    using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
    using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
    using Message = Google.Protobuf.IMessage;
    using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
    using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
    using Value = Google.Protobuf.WellKnownTypes.Value;

    public delegate Message messageSupplier();

    /// <summary>
    /// Test cases for {@link
    /// PbTypeDescriptionTest#benchmarkTypeDescriptionMaybeUnwrap(org.projectnessie.cel.common.types.pb.UnwrapTestCase)}
    /// and {@code TypeDescriptorBnch} JMH benchmark, latter requires this class to be a top-level public
    /// enum.
    /// </summary>
    public sealed class UnwrapTestCase
    {
        private static readonly List<UnwrapTestCase> valueList = new List<UnwrapTestCase>();

        private static readonly BoolValue t;
        private static readonly BoolValue f;
        private static readonly Value nullValue;
        private static readonly Value numValue;
        private static readonly Value strValue1;
        private static readonly Value strValue2;
        private static readonly BytesValue bytesValue;
        private static readonly DoubleValue doubleValue;
        private static readonly FloatValue floatValue;
        private static readonly Int32Value int32Value;
        private static readonly Int64Value int64Value;
        private static readonly UInt32Value uint32Value;
        private static readonly UInt64Value uint64Value;
        private static readonly Timestamp ts;
        private static readonly Duration d;

        static UnwrapTestCase()
        {
            valueList.Add(MsgDesc_zero);
            valueList.Add(Structpb_NewBoolValue_true);
            valueList.Add(Structpb_NewBoolValue_false);
            valueList.Add(Structpb_NewNullValue);
            valueList.Add(Structpb_Value);
            valueList.Add(Structpb_NewNumberValue);
            valueList.Add(Structpb_NewStringValue);
            valueList.Add(Wrapperspb_Bool_false);
            valueList.Add(Wrapperspb_Bool_true);
            valueList.Add(Wrapperspb_Bytes);
            valueList.Add(Wrapperspb_Double);
            valueList.Add(Wrapperspb_Float);
            valueList.Add(Wrapperspb_Int32);
            valueList.Add(Wrapperspb_Int64);
            valueList.Add(Wrapperspb_String);
            valueList.Add(Wrapperspb_UInt32);
            valueList.Add(Wrapperspb_UInt64);
            valueList.Add(Timestamp);
            valueList.Add(Duration);
            valueList.Add(Proto3pb_TestAllTypes);

            t = new BoolValue();
            t.Value = true;
            f = new BoolValue();
            f.Value = false;
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
            ts = new Timestamp();
            ts.Seconds = 12345;
            ts.Nanos = 0;
            d = new Duration();
            d.Seconds = 345;
        }

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

        public static readonly UnwrapTestCase MsgDesc_zero =
            new("MsgDesc_zero", InnerEnum.MsgDesc_zero, () => UnwrapContext.Get().msgDesc.Zero());

        public static readonly UnwrapTestCase Structpb_NewBoolValue_true = new("Structpb_NewBoolValue_true",
            InnerEnum.Structpb_Value, () => t);

        public static readonly UnwrapTestCase Structpb_NewBoolValue_false = new("structpb_NewBoolValuefalse",
            InnerEnum.Structpb_NewBoolValue_false, () => f);

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
            InnerEnum.Wrapperspb_Bool_false, () => f);

        public static readonly UnwrapTestCase Wrapperspb_Bool_true = new("Wrapperspb_Bool_true",
            InnerEnum.Wrapperspb_Bool_true, () => t);

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
            () => ts);

        public static readonly UnwrapTestCase Duration = new("Duration", InnerEnum.Duration,
            () => d);

        public static readonly UnwrapTestCase Proto3pb_TestAllTypes = new("Proto3pb_TestAllTypes",
            InnerEnum.Proto3pb_TestAllTypes, () => new TestAllTypes());


        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        internal UnwrapTestCase(string name, InnerEnum innerEnum, System.Func<Message> message)
        {
            this.message = message;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        private readonly System.Func<Message> message;

        public Message Message()
        {
            return message();
        }

        public static UnwrapTestCase[] Values()
        {
            return valueList.ToArray();
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
            foreach (UnwrapTestCase enumInstance in UnwrapTestCase.valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }

            throw new System.ArgumentException(name);
        }
    }
}