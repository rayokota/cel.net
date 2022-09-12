using System.Collections;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Enum = System.Enum;
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
///     FieldDescription holds metadata related to fields declared within a type.
/// </summary>
public sealed class FieldDescription : Description
{
    private readonly FieldDescriptor desc;

    /// <summary>
    ///     KeyType holds the key FieldDescription for map fields.
    /// </summary>
    internal readonly FieldDescription keyType;

    private readonly Type reflectType;

    /// <summary>
    ///     ValueType holds the value FieldDescription for map fields.
    /// </summary>
    internal readonly FieldDescription valueType;

    private readonly Message zeroMsg;

    private FieldDescription(FieldDescription keyType, FieldDescription valueType, FieldDescriptor desc,
        Type reflectType, Message zeroMsg)
    {
        this.keyType = keyType;
        this.valueType = valueType;
        this.desc = desc;
        this.reflectType = reflectType;
        this.zeroMsg = zeroMsg;
    }

    /// <summary>
    ///     IsEnum returns true if the field type refers to an enum value.
    /// </summary>
    public bool Enum => desc.FieldType == FieldType.Enum;

    /// <summary>
    ///     IsMap returns true if the field is of map type.
    /// </summary>
    public bool Map => desc.IsMap;

    /// <summary>
    ///     IsMessage returns true if the field is of message type.
    /// </summary>
    public bool Message => desc.FieldType == FieldType.Message;

    /// <summary>
    ///     IsOneof returns true if the field is declared within a oneof block.
    /// </summary>
    public bool Oneof => desc.ContainingOneof != null;

    /// <summary>
    ///     IsList returns true if the field is a repeated value.
    ///     <para>
    ///         This method will also return true for map values, so check whether the field is also a map.
    ///     </para>
    /// </summary>
    public bool List => desc.IsRepeated && !desc.IsMap;

    /// <summary>
    ///     NewFieldDescription creates a new field description from a protoreflect.FieldDescriptor.
    /// </summary>
    public static FieldDescription NewFieldDescription(FieldDescriptor fieldDesc)
    {
        Type? reflectType = null;
        Message? zeroMsg = null;
        switch (fieldDesc.FieldType)
        {
            case FieldType.Enum:
                reflectType = typeof(Enum);
                break;
            case FieldType.Message:
                if (!fieldDesc.IsMap)
                {
                    var type = fieldDesc.MessageType.ClrType;
                    zeroMsg = (Message)Activator.CreateInstance(type)!;
                    reflectType = PbTypeDescription.ReflectTypeOf(zeroMsg);
                }

                break;
            default:
                reflectType = ReflectTypeOfField(fieldDesc);
                if (fieldDesc.IsRepeated && !fieldDesc.IsMap)
                {
                    var t = fieldDesc.FieldType;
                    switch (t)
                    {
                        case FieldType.Enum:
                            reflectType = typeof(Enum);
                            break;
                        case FieldType.Message:
                            zeroMsg = (Message)Activator.CreateInstance(fieldDesc.MessageType.ClrType)!;
                            reflectType = zeroMsg.GetType();
                            break;
                        case FieldType.Bool:
                            reflectType = typeof(bool);
                            break;
                        case FieldType.Bytes:
                            reflectType = typeof(byte[]);
                            break;
                        case FieldType.Double:
                            reflectType = typeof(double);
                            break;
                        case FieldType.Float:
                            reflectType = typeof(float);
                            break;
                        case FieldType.Int32:
                        case FieldType.SInt32:
                        case FieldType.SFixed32:
                            reflectType = typeof(int);
                            break;
                        case FieldType.Int64:
                        case FieldType.SInt64:
                        case FieldType.SFixed64:
                            reflectType = typeof(long);
                            break;
                        case FieldType.UInt32:
                        case FieldType.Fixed32:
                            reflectType = typeof(uint);
                            break;
                        case FieldType.UInt64:
                        case FieldType.Fixed64:
                            reflectType = typeof(ulong);
                            break;
                        case FieldType.String:
                            reflectType = typeof(string);
                            break;
                    }
                }

                break;
        }

        // Ensure the list type is appropriately reflected as a Go-native list.
        if (fieldDesc.IsRepeated && !fieldDesc.IsMap)
            // IsList()
            // TODO j.u.List or array???
            reflectType = Array.CreateInstance(reflectType, 0).GetType();

        FieldDescription? keyType = null;
        FieldDescription? valType = null;
        if (fieldDesc.IsMap)
        {
            keyType = NewFieldDescription(fieldDesc.MessageType.FindFieldByNumber(1));
            valType = NewFieldDescription(fieldDesc.MessageType.FindFieldByNumber(2));
        }

        return new FieldDescription(keyType, valType, fieldDesc, reflectType, zeroMsg);
    }

    private static Type ReflectTypeOfField(FieldDescriptor fieldDesc)
    {
        switch (fieldDesc.FieldType)
        {
            case FieldType.Double:
                return typeof(double);
            case FieldType.Float:
                return typeof(float);
            case FieldType.String:
                return typeof(string);
            case FieldType.Bool:
                return typeof(bool);
            case FieldType.Bytes:
                return typeof(ByteString);
            case FieldType.Int32:
            case FieldType.SFixed32:
            case FieldType.SInt32:
                return typeof(int);
            case FieldType.Int64:
            case FieldType.SFixed64:
            case FieldType.SInt64:
                return typeof(long);
            case FieldType.UInt32:
            case FieldType.Fixed32:
                return typeof(uint);
            case FieldType.UInt64:
            case FieldType.Fixed64:
                return typeof(ulong);
            case FieldType.Enum:
                return typeof(Enum);
        }

        throw new NotSupportedException("Unknown type " + fieldDesc.FieldType);
    }

    /// <summary>
    ///     CheckedType returns the type-definition used at type-check time.
    /// </summary>
    public Google.Api.Expr.V1Alpha1.Type CheckedType()
    {
        if (desc.IsMap)
        {
            var mapType = new Google.Api.Expr.V1Alpha1.Type.Types.MapType();
            mapType.KeyType = keyType.TypeDefToType();
            mapType.ValueType = valueType.TypeDefToType();
            var type = new Google.Api.Expr.V1Alpha1.Type();
            type.MapType = mapType;
            return type;
        }

        if (desc.IsRepeated)
        {
            // "isListField()"
            var listType = new Google.Api.Expr.V1Alpha1.Type.Types.ListType();
            listType.ElemType = TypeDefToType();
            var type = new Google.Api.Expr.V1Alpha1.Type();
            type.ListType = listType;
            return type;
        }

        return TypeDefToType();
    }

    /// <summary>
    ///     Descriptor returns the protoreflect.FieldDescriptor for this type.
    /// </summary>
    public FieldDescriptor Descriptor()
    {
        return desc;
    }

    /// <summary>
    ///     IsSet returns whether the field is set on the target value, per the proto presence conventions
    ///     of proto2 or proto3 accordingly.
    ///     <para>
    ///         This function implements the FieldType.IsSet function contract which can be used to operate
    ///         on more than just protobuf field accesses; however, the target here must be a protobuf.Message.
    ///     </para>
    /// </summary>
    public bool IsSet(object target)
    {
        if (target is Message)
        {
            var v = (Message)target;
            // pbRef = v.ProtoReflect()
            var pbDesc = v.Descriptor;
            if (pbDesc == desc.ContainingType)
                // When the target protobuf shares the same message descriptor instance as the field
                // descriptor, use the cached field descriptor value.
                return HasValueForField(desc, v);

            // Otherwise, fallback to a dynamic lookup of the field descriptor from the target
            // instance as an attempt to use the cached field descriptor will result in a panic.
            return HasValueForField(pbDesc.FindFieldByName(Name()), v);
        }

        return false;
    }

    /// <summary>
    ///     GetFrom returns the accessor method associated with the field on the proto generated struct.
    ///     <para>
    ///         If the field is not set, the proto default value is returned instead.
    ///     </para>
    ///     <para>
    ///         This function implements the FieldType.GetFrom function contract which can be used to
    ///         operate on more than just protobuf field accesses; however, the target here must be a
    ///         protobuf.Message.
    ///     </para>
    /// </summary>
    public object GetFrom(Db db, object target)
    {
        if (!(target is Message))
            throw new ArgumentException(string.Format("unsupported field selection target: ({0}){1}",
                target.GetType().FullName, target));

        var v = (Message)target;
        // pbRef = v.protoReflect();
        var pbDesc = v.Descriptor;
        object fieldVal;

        FieldDescriptor fd;
        if (pbDesc == desc.ContainingType)
            // When the target protobuf shares the same message descriptor instance as the field
            // descriptor, use the cached field descriptor value.
            fd = desc;
        else
            // Otherwise, fallback to a dynamic lookup of the field descriptor from the target
            // instance as an attempt to use the cached field descriptor will result in a panic.
            fd = pbDesc.FindFieldByName(Name());

        fieldVal = GetValueFromField(fd, v);

        var fieldType = fieldVal.GetType();
        if (fd.FieldType != FieldType.Message
            || fieldType.IsPrimitive
            || fieldType.IsEnum
            || fieldType == typeof(byte[])
            || fieldType == typeof(bool)
            || fieldType == typeof(byte)
            || fieldType == typeof(short)
            || fieldType == typeof(int)
            || fieldType == typeof(long)
            || fieldType == typeof(ulong)
            || fieldType == typeof(float)
            || fieldType == typeof(double)
            || fieldType == typeof(string))
            // Fast-path return for primitive types.
            return fieldVal;

        if (fieldVal is EnumValue) return (long)((EnumValue)fieldVal).Number;

        if (fieldVal is Message) return MaybeUnwrapDynamic(db, (Message)fieldVal);

        throw new NotSupportedException("IMPLEMENT ME");
        // TODO implement this
        //    if (field)
        //    switch fv := fieldVal.(type) {
        //    case bool, []byte, float32, float64, int32, int64, string, uint32, uint64,
        // protoreflect.List:
        //      return fv, nil
        //    case protoreflect.Map:
        //      // Return a wrapper around the protobuf-reflected Map types which carries additional
        //      // information about the key and value definitions of the map.
        //      return &Map{Map: fv, KeyType: keyType, ValueType: valueType}, nil
        //    default:
        //      return fv, nil
        //    }
    }

    /// <summary>
    ///     MaybeUnwrapDynamic takes the reflected protoreflect.Message and determines whether the value
    ///     can be unwrapped to a more primitive CEL type.
    ///     <para>
    ///         This function returns the unwrapped value and 'true' on success, or the original value and
    ///         'false' otherwise.
    ///     </para>
    /// </summary>
    public object MaybeUnwrapDynamic(Db db, Message msg)
    {
        return PbTypeDescription.UnwrapDynamic(db, this, msg);
    }

    /// <summary>
    ///     Name returns the CamelCase name of the field within the proto-based struct.
    /// </summary>
    public string Name()
    {
        return desc.Name;
    }

    /// <summary>
    ///     ReflectType returns the Golang reflect.Type for this field.
    /// </summary>
    public Type ReflectType()
    {
        var r = desc.IsRepeated;
        if (r && desc.IsMap) return typeof(IDictionary);

        switch (desc.FieldType)
        {
            case FieldType.Enum:
            case FieldType.Message:
                return reflectType;
            case FieldType.Bool:
                return r ? typeof(bool[]) : typeof(bool);
            case FieldType.Bytes:
                return r ? typeof(ByteString[]) : typeof(ByteString);
            case FieldType.Double:
                return r ? typeof(double[]) : typeof(double);
            case FieldType.Float:
                return r ? typeof(float[]) : typeof(float);
            case FieldType.Int32:
            case FieldType.SInt32:
            case FieldType.SFixed32:
                return r ? typeof(int[]) : typeof(int);
            case FieldType.Int64:
            case FieldType.SInt64:
            case FieldType.SFixed64:
                return r ? typeof(long[]) : typeof(long);
            case FieldType.UInt32:
            case FieldType.Fixed32:
                return r ? typeof(uint[]) : typeof(uint);
            case FieldType.UInt64:
            case FieldType.Fixed64:
                return r ? typeof(ulong[]) : typeof(ulong);
            case FieldType.String:
                return r ? typeof(string[]) : typeof(string);
        }

        return reflectType;
    }

    /// <summary>
    ///     String returns the fully qualified name of the field within its type as well as whether the
    ///     field occurs within a oneof. func (fd *FieldDescription) String() string { return
    ///     fmt.Sprintf("%v.{0} `oneof=%t`", desc.ContainingMessage().FullName(), name(), isOneof()) }
    ///     <para>
    ///         /** Zero returns the zero value for the protobuf message represented by this field.
    ///     </para>
    ///     <para>
    ///         If the field is not a proto.Message type, the zero value is nil.
    ///     </para>
    /// </summary>
    public override Message Zero()
    {
        return zeroMsg;
    }

    public Google.Api.Expr.V1Alpha1.Type TypeDefToType()
    {
        switch (desc.FieldType)
        {
            case FieldType.Message:
                var msgType = desc.MessageType.FullName;
                Checked.CheckedWellKnowns.TryGetValue(msgType, out var wk);
                if (wk != null) return wk;

                return Checked.CheckedMessageType(msgType);
            case FieldType.Enum:
                return Checked.CheckedInt;
            case FieldType.Bool:
                return Checked.CheckedBool;
            case FieldType.Bytes:
                return Checked.CheckedBytes;
            case FieldType.Double:
            case FieldType.Float:
                return Checked.CheckedDouble;
            case FieldType.Int32:
            case FieldType.SInt32:
            case FieldType.SFixed32:
                return Checked.CheckedInt;
            case FieldType.UInt32:
            case FieldType.Fixed32:
                return Checked.CheckedUint;
            case FieldType.Int64:
            case FieldType.SInt64:
            case FieldType.SFixed64:
                return Checked.CheckedInt;
            case FieldType.UInt64:
            case FieldType.Fixed64:
                return Checked.CheckedUint;
            case FieldType.String:
                return Checked.CheckedString;
        }

        throw new NotSupportedException("Unknown type " + desc.FieldType);
    }

    public override string ToString()
    {
        return CheckedType().ToString();
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (FieldDescription)o;
        return Equals(desc, that.desc) && Equals(reflectType, that.reflectType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(desc, reflectType);
    }

    public bool HasField(object target)
    {
        return HasValueForField(desc, (Message)target);
    }

    public object GetField(object target)
    {
        return GetValueFromField(desc, (Message)target);
    }

    public static object GetValueFromField(FieldDescriptor desc, Message message)
    {
        if (IsWellKnownType(desc) && !desc.Accessor.HasValue(message)) return NullValue.NullValue;

        var v = desc.Accessor.GetValue(message);

        // TODO remove
        /*
        if (!desc.IsMap)
        {
          FieldType type = desc.FieldType;
          if (v != null 
              && (type == FieldType.UInt32 
                  || type == FieldType.UInt64 
                  || type == FieldType.Fixed32 
                  || type == FieldType.Fixed64))
          {
            v = ULong.ValueOf(((Number) v).longValue());
          }
        }
        if (desc.IsMap)
        {
          // TODO protobuf-java inefficiency
          //  protobuf-java does NOT have a generic way to retrieve the underlying map, but instead
          //  getField() returns a list of Google.Protobuf.WellKnownTypes.MapEntry. It's not great that we have
          //  to have this workaround here to re-build a j.u.Map.
          //  I.e. to access a single map entry we *HAVE TO* touch and re-build the whole map. This
          //  is very inefficient.
          //  There is no way to do a "message.getMapField(desc, key)" (aka a "reflective counterpart"
          //  for the generated map accessor methods like 'getXXXTypeOrThrow()'), too.
          if (v is System.Collections.IList)
          {
            IList<object> lst = (IList<object>) v;
            IDictionary<object, object> map = new Dictionary<object, object>(lst.Count * 4 / 3 + 1);
            foreach (object e in lst)
            {
              object key;
              object value;
              if (e is MapEntry)
              {
                key = ((MapEntry<object, object>) e).getKey();
                value = ((MapEntry<object, object>) e).getValue();
              }
              else if (e is DynamicMessage)
              {
                DynamicMessage dynMsg = (DynamicMessage) e;
                IList<FieldDescriptor> fields = dynMsg.getDescriptorForType().getFields();
                if (fields.Count == 2)
                {
                  key = dynMsg.getField(fields[0]);
                  value = dynMsg.getField(fields[1]);
                }
                else
                {
                  throw new System.ArgumentException(String.Format("Unexpected {0} ({1}) in list of map fields, dynamic message with != 2 fields", e.GetType(), e));
                }
              }
              else
              {
                throw new System.ArgumentException(String.Format("Unexpected {0} ({1}) in list of map fields", e.GetType(), e));
              }
              map[key] = value;
            }
            v = map;
          }
        }
        */
        return v;
    }

    private static bool IsWellKnownType(FieldDescriptor desc)
    {
        if (desc.FieldType != FieldType.Message) return false;

        Checked.CheckedWellKnowns.TryGetValue(desc.MessageType.FullName, out var wellKnown);
        if (wellKnown == null) return false;

        return wellKnown.TypeKindCase == Google.Api.Expr.V1Alpha1.Type.TypeKindOneofCase.Wrapper;
    }

    public static bool HasValueForField(FieldDescriptor desc, Message message)
    {
        if (desc.IsMap) return ((IDictionary)desc.Accessor.GetValue(message)).Count > 0;
        if (desc.IsRepeated) return ((IList)desc.Accessor.GetValue(message)).Count > 0;

        if (desc.HasPresence)
            return desc.Accessor.HasValue(message);
        var value = desc.Accessor.GetValue(message);
        return !IsDefaultValue(desc, value);
    }

    private static bool IsDefaultValue(FieldDescriptor descriptor, object value)
    {
        if (descriptor.IsMap)
        {
            var dictionary = (IDictionary)value;
            return dictionary.Count == 0;
        }

        if (descriptor.IsRepeated)
        {
            var list = (IList)value;
            return list.Count == 0;
        }

        switch (descriptor.FieldType)
        {
            case FieldType.Bool:
                return (bool)value == false;
            case FieldType.Bytes:
                return (ByteString)value == ByteString.Empty;
            case FieldType.String:
                return (string)value == "";
            case FieldType.Double:
                return (double)value == 0.0;
            case FieldType.SInt32:
            case FieldType.Int32:
            case FieldType.SFixed32:
            case FieldType.Enum:
                return (int)value == 0;
            case FieldType.Fixed32:
            case FieldType.UInt32:
                return (uint)value == 0;
            case FieldType.Fixed64:
            case FieldType.UInt64:
                return (ulong)value == 0;
            case FieldType.SFixed64:
            case FieldType.Int64:
            case FieldType.SInt64:
                return (long)value == 0;
            case FieldType.Float:
                return (float)value == 0f;
            case FieldType.Message:
            case FieldType.Group: // Never expect to get this, but...
                return value == null;
            default:
                throw new ArgumentException("Invalid field type");
        }
    }
}