using System;
using System.Collections.Generic;
using Google.Protobuf;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.anyWithEmptyType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.pb.FieldDescription.newFieldDescription;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
    using ByteString = Google.Protobuf.ByteString;
    using BytesValue = Google.Protobuf.WellKnownTypes.BytesValue;
    using Descriptor = Google.Protobuf.Reflection.MessageDescriptor;
    using FieldDescriptor = Google.Protobuf.Reflection.FieldDescriptor;
    using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
    using Duration = Google.Protobuf.WellKnownTypes.Duration;
    using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
    using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
    using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
    using InvalidProtocolBufferException = Google.Protobuf.InvalidProtocolBufferException;
    using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
    using Message = Google.Protobuf.IMessage;
    using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
    using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
    using Struct = Google.Protobuf.WellKnownTypes.Struct;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using TimestampT = global::Cel.Common.Types.TimestampT;
    using TypeDescription = global::Cel.Common.Types.Ref.TypeDescription;

    /// <summary>
    /// TypeDescription is a collection of type metadata relevant to expression checking and evaluation.
    /// </summary>
    public sealed class PbTypeDescription : Description, TypeDescription
    {
        private readonly string typeName;
        private readonly Descriptor desc;
        private readonly IDictionary<string, FieldDescription> fieldMap;
        private Type reflectType;
        private Message zeroMsg;

        private PbTypeDescription(string typeName, Descriptor desc, IDictionary<string, FieldDescription> fieldMap,
            Type reflectType, Message zeroMsg)
        {
            this.typeName = typeName;
            this.desc = desc;
            this.fieldMap = fieldMap;
            this.reflectType = reflectType;
            this.zeroMsg = zeroMsg;
        }

        internal void UpdateReflectType(Message zeroMsg)
        {
            this.zeroMsg = zeroMsg;
            this.reflectType = zeroMsg.GetType();
        }

        /// <summary>
        /// NewTypeDescription produces a TypeDescription value for the fully-qualified proto type name
        /// with a given descriptor.
        /// </summary>
        public static PbTypeDescription NewTypeDescription(string typeName, Descriptor desc)
        {
            Message msgZero = (Message)Activator.CreateInstance(desc.ClrType);
            IDictionary<string, FieldDescription> fieldMap = new Dictionary<string, FieldDescription>();
            IList<FieldDescriptor> fields = desc.Fields.InDeclarationOrder();
            foreach (FieldDescriptor f in fields)
            {
                fieldMap[f.Name] = FieldDescription.NewFieldDescription(f);
            }

            return new PbTypeDescription(typeName, desc, fieldMap, ReflectTypeOf(msgZero), ZeroValueOf(msgZero));
        }

        /// <summary>
        /// FieldMap returns a string field name to FieldDescription map. </summary>
        public IDictionary<string, FieldDescription> FieldMap()
        {
            return fieldMap;
        }

        /// <summary>
        /// FieldByName returns (FieldDescription, true) if the field name is declared within the type. </summary>
        public FieldDescription FieldByName(string name)
        {
            return fieldMap[name];
        }

        /// <summary>
        /// MaybeUnwrap accepts a proto message as input and unwraps it to a primitive CEL type if
        /// possible.
        /// 
        /// <para>This method returns the unwrapped value and 'true', else the original value and 'false'.
        /// </para>
        /// </summary>
        public object MaybeUnwrap(Db db, object m)
        {
            Message msg = (Message)m;
            try
            {
                if (this.reflectType == typeof(Any))
                {
                    string realTypeUrl;
                    ByteString realValue;
                    if (msg is Any)
                    {
                        Any any = (Any)msg;
                        realTypeUrl = any.TypeUrl;
                        realValue = any.Value;
                    }
                    else
                    {
                        return Err.AnyWithEmptyType();
                    }

                    string realTypeName = TypeNameFromUrl(realTypeUrl);
                    if (realTypeName.Length == 0 || realTypeName.Equals(typeName))
                    {
                        return Err.AnyWithEmptyType();
                    }

                    PbTypeDescription realTypeDescriptor = db.DescribeType(realTypeName);
                    Message realMsg = realTypeDescriptor.zeroMsg.Descriptor.Parser.ParseFrom(realValue);
                    return realTypeDescriptor.MaybeUnwrap(db, realMsg);
                }

                if (msg is Any)
                {
                    Any any = (Any)msg;
                    msg = zeroMsg.Descriptor.Parser.ParseFrom(any.Value);
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new Exception("Invalid protocol buffer", e);
            }

            return Unwrap(db, this, msg);
        }

        /// <summary>
        /// Name returns the fully-qualified name of the type. </summary>
        public string Name()
        {
            return desc.FullName;
        }

        /// <summary>
        /// New returns a mutable proto message </summary>
        public Message NewMessageBuilder()
        {
            return (Message)Activator.CreateInstance(zeroMsg.Descriptor.ClrType);
        }

        public Descriptor Descriptor
        {
            get { return desc; }
        }

        /// <summary>
        /// ReflectType returns the Golang reflect.Type for this type. </summary>
        public Type ReflectType()
        {
            return reflectType;
        }

        /// <summary>
        /// Zero returns the zero proto.Message value for this type. </summary>
        public override Message Zero()
        {
            return zeroMsg;
        }

        public override string ToString()
        {
            return "PbTypeDescription{name: '" + typeName + '\'' + ", fieldMap: " + fieldMap + ", reflectType: " +
                   reflectType + '}';
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }

            PbTypeDescription that = (PbTypeDescription)o;
            return Object.Equals(typeName, that.typeName) && Object.Equals(desc, that.desc);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(typeName, desc);
        }

        private static readonly IDictionary<Type, System.Func<Message, object>> MessageToObjectExact =
            new Dictionary<Type, Func<Message, object>>(ReferenceEqualityComparer.Instance);

        static PbTypeDescription()
        {
            MessageToObjectExact[typeof(BoolValue)] = msg => ((BoolValue)msg).Value;
            MessageToObjectExact[typeof(BytesValue)] = msg => ((BytesValue)msg).Value;
            MessageToObjectExact[typeof(DoubleValue)] = msg => ((DoubleValue)msg).Value;
            MessageToObjectExact[typeof(FloatValue)] = msg => ((FloatValue)msg).Value;
            MessageToObjectExact[typeof(Int32Value)] = msg => ((Int32Value)msg).Value;
            MessageToObjectExact[typeof(Int64Value)] = msg => ((Int64Value)msg).Value;
            MessageToObjectExact[typeof(StringValue)] = msg => ((StringValue)msg).Value;
            MessageToObjectExact[typeof(UInt32Value)] = msg => ((UInt32Value)msg).Value;
            MessageToObjectExact[typeof(UInt64Value)] = msg => ((UInt64Value)msg).Value;
            MessageToObjectExact[typeof(Duration)] = msg => AsJavaDuration((Duration)msg);
            MessageToObjectExact[typeof(Timestamp)] = msg => AsJavaTimestamp((Timestamp)msg);
            zeroValueMap["google.protobuf.Any"] = (Message)Activator.CreateInstance(typeof(Any));
            zeroValueMap["google.protobuf.Duration"] = (Message)Activator.CreateInstance(typeof(Duration));
            zeroValueMap["google.protobuf.ListValue"] = (Message)Activator.CreateInstance(typeof(ListValue));
            zeroValueMap["google.protobuf.Struct"] = (Message)Activator.CreateInstance(typeof(Struct));
            zeroValueMap["google.protobuf.Value"] = (Message)Activator.CreateInstance(typeof(Value));
            zeroValueMap["google.protobuf.Timestamp"] = (Message)Activator.CreateInstance(typeof(Timestamp));
            zeroValueMap["google.protobuf.BoolValue"] = (Message)Activator.CreateInstance(typeof(BoolValue));
            zeroValueMap["google.protobuf.BytesValue"] = (Message)Activator.CreateInstance(typeof(BytesValue));
            zeroValueMap["google.protobuf.DoubleValue"] = (Message)Activator.CreateInstance(typeof(DoubleValue));
            zeroValueMap["google.protobuf.FloatValue"] = (Message)Activator.CreateInstance(typeof(FloatValue));
            zeroValueMap["google.protobuf.Int32Value"] = (Message)Activator.CreateInstance(typeof(Int32Value));
            zeroValueMap["google.protobuf.Int64Value"] = (Message)Activator.CreateInstance(typeof(Int64Value));
            zeroValueMap["google.protobuf.StringValue"] = (Message)Activator.CreateInstance(typeof(StringValue));
            zeroValueMap["google.protobuf.UInt32Value"] = (Message)Activator.CreateInstance(typeof(UInt32Value));
            zeroValueMap["google.protobuf.UInt64Value"] = (Message)Activator.CreateInstance(typeof(UInt64Value));
        }

        /// <summary>
        /// unwrap unwraps the provided proto.Message value, potentially based on the description if the
        /// input message is a *dynamicpb.Message which obscures the typing information from Go.
        /// 
        /// <para>Returns the unwrapped value and 'true' if unwrapped, otherwise the input value and 'false'.
        /// </para>
        /// </summary>
        internal static object Unwrap(Db db, Description desc, Message msg)
        {
            System.Func<Message, object> conv = MessageToObjectExact[msg.GetType()];
            if (conv != null)
            {
                return conv(msg);
            }

            if (msg is Any)
            {
                Any v = (Any)msg;
                // TODO check
                throw new NotImplementedException();
                /*
                Message dyn = DynamicMessage.newBuilder(v).build();
                return UnwrapDynamic(db, desc, dyn);
                */
            }

            if (msg is Value)
            {
                Value v = (Value)msg;
                switch (v.KindCase)
                {
                    case Value.KindOneofCase.BoolValue:
                        return v.BoolValue;
                    case Value.KindOneofCase.ListValue:
                        return v.ListValue;
                    case Value.KindOneofCase.NullValue:
                        return v.NullValue;
                    case Value.KindOneofCase.NumberValue:
                        return v.NumberValue;
                    case Value.KindOneofCase.StringValue:
                        return v.StringValue;
                    case Value.KindOneofCase.StructValue:
                        return v.StructValue;
                    default:
                        return NullValue.NullValue;
                }
            }

            return msg;
        }

        private static NodaTime.Duration AsJavaDuration(Duration d)
        {
            return NodaTime.Duration.FromNanoseconds(d.Seconds * 1000000000 + d.Nanos);
        }

        private static NodaTime.ZonedDateTime AsJavaTimestamp(Timestamp t)
        {
            NodaTime.Instant instant = NodaTime.Instant.FromUnixTimeSeconds(t.Seconds);
            instant.PlusNanoseconds(t.Nanos);
            return new NodaTime.ZonedDateTime(instant, TimestampT.ZoneIdZ);
        }

        /// <summary>
        /// unwrapDynamic unwraps a reflected protobuf Message value.
        /// 
        /// <para>Returns the unwrapped value and 'true' if unwrapped, otherwise the input value and 'false'.
        /// </para>
        /// </summary>
        internal static object UnwrapDynamic(Db db, Description desc, Message refMsg)
        {
            Message msg = refMsg;
            if (!msg.IsInitialized())
            {
                msg = desc.Zero();
            }

            // In order to ensure that these wrapped types match the expectations of the CEL type system
            // the dynamicpb.Message must be merged with an protobuf instance of the well-known type
            // value.
            FieldDescriptor valueField;
            string typeName = refMsg.Descriptor.FullName;
            switch (typeName)
            {
                case "google.protobuf.Any":
                    // TODO check
                    throw new NotImplementedException();
                //return UnwrapDynamicAny(db, desc, refMsg);
                case "google.protobuf.BoolValue":
                    if (Object.Equals(msg, new BoolValue()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.BytesValue":
                    if (Object.Equals(msg, new BytesValue()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.DoubleValue":
                    if (Object.Equals(msg, new DoubleValue()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.FloatValue":
                    if (Object.Equals(msg, new FloatValue()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.Int32Value":
                    if (Object.Equals(msg, new Int32Value()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.Int64Value":
                    if (Object.Equals(msg, new Int64Value()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.StringValue":
                    // The msg value is ignored when dealing with wrapper types as they have a null or value
                    // behavior, rather than the standard zero value behavior of other proto message types.
                    if (Object.Equals(msg, new StringValue()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return valueField.Accessor.GetValue(msg);
                case "google.protobuf.UInt32Value":
                    if (Object.Equals(msg, new UInt32Value()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return (ulong)valueField.Accessor.GetValue(msg);
                case "google.protobuf.UInt64Value":
                    // The msg value is ignored when dealing with wrapper types as they have a null or value
                    // behavior, rather than the standard zero value behavior of other proto message types.
                    if (Object.Equals(msg, new UInt64Value()))
                    {
                        return NullValue.NullValue;
                    }

                    valueField = msg.Descriptor.FindFieldByName("value");
                    return (ulong)valueField.Accessor.GetValue(msg);
                case "google.protobuf.Duration":
                    Duration duration = new Duration();
                    MessageExtensions.MergeFrom(duration, msg.ToByteArray());
                    return AsJavaDuration(duration);
                case "google.protobuf.ListValue":
                    ListValue listValue = new ListValue();
                    MessageExtensions.MergeFrom(listValue, msg.ToByteArray());
                    return listValue;
                case "google.protobuf.NullValue":
                    return NullValue.NullValue;
                case "google.protobuf.Struct":
                    Struct structValue = new Struct();
                    MessageExtensions.MergeFrom(structValue, msg.ToByteArray());
                    return structValue;
                case "google.protobuf.Timestamp":
                    Timestamp timestamp = new Timestamp();
                    MessageExtensions.MergeFrom(timestamp, msg.ToByteArray());
                    return AsJavaTimestamp(timestamp);
                case "google.protobuf.Value":
                    Value value = new Value();
                    MessageExtensions.MergeFrom(value, msg.ToByteArray());
                    return Unwrap(db, desc, value);
            }

            return msg;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static Object unwrapDynamicAny(Db db, Description desc, Google.Protobuf.WellKnownTypes.Message refMsg)
        /*
        private static object UnwrapDynamicAny(Db db, Description desc, Message refMsg)
        {
          // Note, Any values require further unwrapping; however, this unwrapping may or may not
          // be to a well-known type. If the unwrapped value is a well-known type it will be further
          // unwrapped before being returned to the caller. Otherwise, the dynamic protobuf object
          // represented by the Any will be returned.
          DynamicMessage dyn = (DynamicMessage) refMsg;
          Any any = Any.newBuilder().mergeFrom(dyn).build();
          string typeUrl = any.getTypeUrl();
          if (typeUrl.Length == 0)
          {
            return Err.AnyWithEmptyType();
          }
          string innerTypeName = TypeNameFromUrl(typeUrl);
          PbTypeDescription innerType = db.DescribeType(innerTypeName);
          if (innerType == null)
          {
            return refMsg;
          }
          try
          {
            Type msgClass = (Type) innerType.ReflectType();
            Message unwrapped = any.unpack(msgClass);
            return UnwrapDynamic(db, desc, unwrapped);
          }
          catch (Exception)
          {
            return refMsg;
          }
        }
        */

        public static string TypeNameFromMessage(Message message)
        {
            if (message is Any)
            {
                Any any = (Any)message;
                string typeUrl = any.TypeUrl;
                return TypeNameFromUrl(typeUrl);
            }

            return message.Descriptor.FullName;
        }

        public static string TypeNameFromUrl(string typeUrl)
        {
            return typeUrl.Substring(typeUrl.IndexOf('/') + 1);
        }

        /// <summary>
        /// reflectTypeOf intercepts the reflect.Type call to ensure that dynamicpb.Message types preserve
        /// well-known protobuf reflected types expected by the CEL type system.
        /// </summary>
        internal static Type ReflectTypeOf(object val)
        {
            if (val is Message)
            {
                val = ZeroValueOf((Message)val);
            }

            return val.GetType();
        }

        /// <summary>
        /// zeroValueOf will return the strongest possible proto.Message representing the default protobuf
        /// message value of the input msg type.
        /// </summary>
        internal static Message ZeroValueOf(Message msg)
        {
            if (msg == null)
            {
                return null;
            }

            string typeName = msg.Descriptor.FullName;
            Message result;
            return zeroValueMap.TryGetValue(typeName, out result) ? result : msg;
        }

        private static readonly IDictionary<string, Message> zeroValueMap = new Dictionary<string, Message>();
    }
}