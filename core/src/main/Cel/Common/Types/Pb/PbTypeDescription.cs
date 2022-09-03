using Cel.Common.Types.Ref;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Duration = Google.Protobuf.WellKnownTypes.Duration;
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
namespace Cel.Common.Types.Pb;

using Descriptor = MessageDescriptor;
using Message = IMessage;

/// <summary>
///     TypeDescription is a collection of type metadata relevant to expression checking and evaluation.
/// </summary>
public sealed class PbTypeDescription : Description, TypeDescription
{
    private static readonly IDictionary<Type, Func<Message, object>> MessageToObjectExact =
        new Dictionary<Type, Func<Message, object>>(ReferenceEqualityComparer.Instance);

    private static readonly IDictionary<string, Message> zeroValueMap = new Dictionary<string, Message>();
    private readonly IDictionary<string, FieldDescription> fieldMap;
    private readonly string typeName;
    private Type reflectType;
    private Message zeroMsg;

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
        MessageToObjectExact[typeof(Duration)] =
            msg => AsDuration((Duration)msg);
        MessageToObjectExact[typeof(Timestamp)] = msg => AsTimestamp((Timestamp)msg);
        zeroValueMap["google.protobuf.Any"] = (Message)Activator.CreateInstance(typeof(Any));
        zeroValueMap["google.protobuf.Duration"] =
            (Message)Activator.CreateInstance(typeof(Duration));
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

    private PbTypeDescription(string typeName, Descriptor desc, IDictionary<string, FieldDescription> fieldMap,
        Type reflectType, Message zeroMsg)
    {
        this.typeName = typeName;
        Descriptor = desc;
        this.fieldMap = fieldMap;
        this.reflectType = reflectType;
        this.zeroMsg = zeroMsg;
    }

    public Descriptor Descriptor { get; }

    /// <summary>
    ///     Name returns the fully-qualified name of the type.
    /// </summary>
    public string Name()
    {
        return Descriptor.FullName;
    }

    /// <summary>
    ///     ReflectType returns the Golang reflect.Type for this type.
    /// </summary>
    public Type ReflectType()
    {
        return reflectType;
    }

    internal void UpdateReflectType(Message zeroMsg)
    {
        this.zeroMsg = zeroMsg;
        reflectType = zeroMsg.GetType();
    }

    /// <summary>
    ///     NewTypeDescription produces a TypeDescription value for the fully-qualified proto type name
    ///     with a given descriptor.
    /// </summary>
    public static PbTypeDescription NewTypeDescription(string typeName, Descriptor desc)
    {
        var msgZero = (Message)Activator.CreateInstance(desc.ClrType);
        IDictionary<string, FieldDescription> fieldMap = new Dictionary<string, FieldDescription>();
        IList<FieldDescriptor> fields = desc.Fields.InDeclarationOrder();
        foreach (var f in fields) fieldMap[f.Name] = FieldDescription.NewFieldDescription(f);

        return new PbTypeDescription(typeName, desc, fieldMap, ReflectTypeOf(msgZero), ZeroValueOf(msgZero));
    }

    /// <summary>
    ///     FieldMap returns a string field name to FieldDescription map.
    /// </summary>
    public IDictionary<string, FieldDescription> FieldMap()
    {
        return fieldMap;
    }

    /// <summary>
    ///     FieldByName returns (FieldDescription, true) if the field name is declared within the type.
    /// </summary>
    public FieldDescription FieldByName(string name)
    {
        fieldMap.TryGetValue(name, out var fd);
        return fd;
    }

    /// <summary>
    ///     MaybeUnwrap accepts a proto message as input and unwraps it to a primitive CEL type if
    ///     possible.
    ///     <para>
    ///         This method returns the unwrapped value and 'true', else the original value and 'false'.
    ///     </para>
    /// </summary>
    public object MaybeUnwrap(Db db, object m)
    {
        var msg = (Message)m;
        try
        {
            if (reflectType == typeof(Any))
            {
                string realTypeUrl;
                ByteString realValue;
                if (msg is Any)
                {
                    var any = (Any)msg;
                    realTypeUrl = any.TypeUrl;
                    realValue = any.Value;
                }
                else
                {
                    return Err.AnyWithEmptyType();
                }

                var realTypeName = TypeNameFromUrl(realTypeUrl);
                if (realTypeName.Length == 0 || realTypeName.Equals(typeName)) return Err.AnyWithEmptyType();

                var realTypeDescriptor = db.DescribeType(realTypeName);
                var realMsg = realTypeDescriptor.zeroMsg.Descriptor.Parser.ParseFrom(realValue);
                return realTypeDescriptor.MaybeUnwrap(db, realMsg);
            }

            if (msg is Any)
            {
                var any = (Any)msg;
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
    ///     New returns a mutable proto message
    /// </summary>
    public Message NewMessageBuilder()
    {
        return (Message)Activator.CreateInstance(zeroMsg.Descriptor.ClrType);
    }

    /// <summary>
    ///     Zero returns the zero proto.Message value for this type.
    /// </summary>
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
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (PbTypeDescription)o;
        return Equals(typeName, that.typeName) && Equals(Descriptor, that.Descriptor);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(typeName, Descriptor);
    }

    /// <summary>
    ///     unwrap unwraps the provided proto.Message value, potentially based on the description if the
    ///     input message is a *dynamicpb.Message which obscures the typing information from Go.
    ///     <para>
    ///         Returns the unwrapped value and 'true' if unwrapped, otherwise the input value and 'false'.
    ///     </para>
    /// </summary>
    internal static object Unwrap(Db db, Description desc, Message msg)
    {
        MessageToObjectExact.TryGetValue(msg.GetType(), out var conv);
        if (conv != null) return conv(msg);

        if (msg is Any)
        {
            var v = (Any)msg;
            // TODO check
            throw new NotImplementedException();
            /*
            Message dyn = DynamicMessage.newBuilder(v).build();
            return UnwrapDynamic(db, desc, dyn);
            */
        }

        if (msg is Value)
        {
            var v = (Value)msg;
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

    private static Period AsDuration(Duration d)
    {
        PeriodBuilder period = new PeriodBuilder();
        period.Seconds = d.Seconds;
        period.Nanoseconds = d.Nanos;
        return period.Build();
    }

    private static ZonedDateTime AsTimestamp(Timestamp t)
    {
        var instant = Instant.FromUnixTimeSeconds(t.Seconds);
        instant.PlusNanoseconds(t.Nanos);
        return new ZonedDateTime(instant, TimestampT.ZoneIdZ);
    }

    /// <summary>
    ///     unwrapDynamic unwraps a reflected protobuf Message value.
    ///     <para>
    ///         Returns the unwrapped value and 'true' if unwrapped, otherwise the input value and 'false'.
    ///     </para>
    /// </summary>
    internal static object UnwrapDynamic(Db db, Description desc, Message refMsg)
    {
        var msg = refMsg;
        if (!msg.IsInitialized()) msg = desc.Zero();

        // In order to ensure that these wrapped types match the expectations of the CEL type system
        // the dynamicpb.Message must be merged with an protobuf instance of the well-known type
        // value.
        FieldDescriptor valueField;
        var typeName = refMsg.Descriptor.FullName;
        switch (typeName)
        {
            case "google.protobuf.Any":
                // TODO check
                throw new NotImplementedException();
            //return UnwrapDynamicAny(db, desc, refMsg);
            case "google.protobuf.BoolValue":
                if (msg == null || Equals(msg, new BoolValue())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.BytesValue":
                if (msg == null || Equals(msg, new BytesValue())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.DoubleValue":
                if (msg == null || Equals(msg, new DoubleValue())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.FloatValue":
                if (msg == null || Equals(msg, new FloatValue())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.Int32Value":
                if (msg == null || Equals(msg, new Int32Value())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.Int64Value":
                if (msg == null || Equals(msg, new Int64Value())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.StringValue":
                // The msg value is ignored when dealing with wrapper types as they have a null or value
                // behavior, rather than the standard zero value behavior of other proto message types.
                if (msg == null || Equals(msg, new StringValue())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return valueField.Accessor.GetValue(msg);
            case "google.protobuf.UInt32Value":
                if (msg == null || Equals(msg, new UInt32Value())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return (ulong)valueField.Accessor.GetValue(msg);
            case "google.protobuf.UInt64Value":
                // The msg value is ignored when dealing with wrapper types as they have a null or value
                // behavior, rather than the standard zero value behavior of other proto message types.
                if (msg == null || Equals(msg, new UInt64Value())) return NullValue.NullValue;

                valueField = msg.Descriptor.FindFieldByName("value");
                return (ulong)valueField.Accessor.GetValue(msg);
            case "google.protobuf.Duration":
                var duration = new Duration();
                duration.MergeFrom(msg.ToByteArray());
                return AsDuration(duration);
            case "google.protobuf.ListValue":
                var listValue = new ListValue();
                listValue.MergeFrom(msg.ToByteArray());
                return listValue;
            case "google.protobuf.NullValue":
                return NullValue.NullValue;
            case "google.protobuf.Struct":
                var structValue = new Struct();
                structValue.MergeFrom(msg.ToByteArray());
                return structValue;
            case "google.protobuf.Timestamp":
                var timestamp = new Timestamp();
                timestamp.MergeFrom(msg.ToByteArray());
                return AsTimestamp(timestamp);
            case "google.protobuf.Value":
                var value = new Value();
                value.MergeFrom(msg.ToByteArray());
                return Unwrap(db, desc, value);
        }

        return msg;
    }

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
            var any = (Any)message;
            var typeUrl = any.TypeUrl;
            return TypeNameFromUrl(typeUrl);
        }

        return message.Descriptor.FullName;
    }

    public static string TypeNameFromUrl(string typeUrl)
    {
        return typeUrl.Substring(typeUrl.IndexOf('/') + 1);
    }

    /// <summary>
    ///     reflectTypeOf intercepts the reflect.Type call to ensure that dynamicpb.Message types preserve
    ///     well-known protobuf reflected types expected by the CEL type system.
    /// </summary>
    internal static Type ReflectTypeOf(object val)
    {
        if (val is Message) val = ZeroValueOf((Message)val);

        return val.GetType();
    }

    /// <summary>
    ///     zeroValueOf will return the strongest possible proto.Message representing the default protobuf
    ///     message value of the input msg type.
    /// </summary>
    internal static Message ZeroValueOf(Message msg)
    {
        if (msg == null) return null;

        var typeName = msg.Descriptor.FullName;
        return zeroValueMap.TryGetValue(typeName, out var result) ? result : msg;
    }
}