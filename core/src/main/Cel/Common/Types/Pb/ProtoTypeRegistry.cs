using System;
using System.Collections;
using System.Collections.Generic;
using Cel.Common.Types.Ref;
using Google.Protobuf;

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
namespace Cel.Common.Types.Pb
{
    using Type = Google.Api.Expr.V1Alpha1.Type;
    using Any = Google.Protobuf.WellKnownTypes.Any;
    using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
    using BytesValue = Google.Protobuf.WellKnownTypes.BytesValue;
    using Descriptor = Google.Protobuf.Reflection.MessageDescriptor;
    using EnumDescriptor = Google.Protobuf.Reflection.EnumDescriptor;
    using EnumValueDescriptor = Google.Protobuf.Reflection.EnumValueDescriptor;
    using FieldDescriptor = Google.Protobuf.Reflection.FieldDescriptor;
    using FileDescriptor = Google.Protobuf.Reflection.FileDescriptor;
    using DoubleValue = Google.Protobuf.WellKnownTypes.DoubleValue;
    using Duration = Google.Protobuf.WellKnownTypes.Duration;
    using Empty = Google.Protobuf.WellKnownTypes.Empty;
    using FloatValue = Google.Protobuf.WellKnownTypes.FloatValue;
    using Int32Value = Google.Protobuf.WellKnownTypes.Int32Value;
    using Int64Value = Google.Protobuf.WellKnownTypes.Int64Value;
    using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
    using Message = Google.Protobuf.IMessage;
    using StringValue = Google.Protobuf.WellKnownTypes.StringValue;
    using Struct = Google.Protobuf.WellKnownTypes.Struct;
    using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
    using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
    using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using WireFormat = Google.Protobuf.WireFormat;
    using TypeT = global::Cel.Common.Types.TypeT;
    using FieldType = global::Cel.Common.Types.Ref.FieldType;
    using TypeRegistry = global::Cel.Common.Types.Ref.TypeRegistry;
    using Val = global::Cel.Common.Types.Ref.Val;

    public sealed class ProtoTypeRegistry : TypeRegistry
    {
        private readonly IDictionary<string, global::Cel.Common.Types.Ref.Type> revTypeMap;
        private readonly Db pbdb;

        private ProtoTypeRegistry(IDictionary<string, global::Cel.Common.Types.Ref.Type> revTypeMap, Db pbdb)
        {
            this.revTypeMap = revTypeMap;
            this.pbdb = pbdb;
        }

        /// <summary>
        /// NewRegistry accepts a list of proto message instances and returns a type provider which can
        /// create new instances of the provided message or any message that proto depends upon in its
        /// FileDescriptor.
        /// </summary>
        public static ProtoTypeRegistry NewRegistry(params Message[] types)
        {
            ProtoTypeRegistry p =
                new ProtoTypeRegistry(new Dictionary<string, global::Cel.Common.Types.Ref.Type>(), Db.NewDb());
            p.RegisterType(
                BoolT.BoolType,
                BytesT.BytesType,
                DoubleT.DoubleType,
                DurationT.DurationType,
                IntT.IntType,
                ListT.ListType,
                MapT.MapType,
                NullT.NullType,
                StringT.StringType,
                TimestampT.TimestampType,
                TypeT.TypeType,
                UintT.UintType);

            FileDescriptor[] fds =
            {
                DoubleValue.Descriptor.File,
                Empty.Descriptor.File,
                Timestamp.Descriptor.File,
                UInt64Value.Descriptor.File,
                Any.Descriptor.File,
                /*
                Google.Protobuf.WellKnownTypes.NullValue
                */
                Struct.Descriptor.File,
                StringValue.Descriptor.File,
                ListValue.Descriptor.File,
                BytesValue.Descriptor.File,
                Value.Descriptor.File,
                Int32Value.Descriptor.File,
                UInt32Value.Descriptor.File,
                Duration.Descriptor.File,
                FloatValue.Descriptor.File,
                BoolValue.Descriptor.File,
                Int64Value.Descriptor.File
            };
            ISet<FileDescriptor> pbDescriptors = new HashSet<FileDescriptor>(fds);
            foreach (FileDescriptor fDesc in pbDescriptors)
            {
                FileDescription fd = FileDescription.NewFileDescription(fDesc);
                p.RegisterAllTypes(fd);
            }

            // This block ensures that the well-known protobuf types are registered by default.
            foreach (FileDescription fd in p.pbdb.FileDescriptions())
            {
                p.RegisterAllTypes(fd);
            }

            foreach (Message msgType in types)
            {
                p.RegisterMessage(msgType);
            }

            return p;
        }

        /// <summary>
        /// NewEmptyRegistry returns a registry which is completely unconfigured. </summary>
        public static ProtoTypeRegistry NewEmptyRegistry()
        {
            return new ProtoTypeRegistry(new Dictionary<string, global::Cel.Common.Types.Ref.Type>(), Db.NewDb());
        }

        /// <summary>
        /// Copy implements the ref.TypeRegistry interface method which copies the current state of the
        /// registry into its own memory space.
        /// </summary>
        public TypeRegistry Copy()
        {
            return new ProtoTypeRegistry(new Dictionary<string, global::Cel.Common.Types.Ref.Type>(this.revTypeMap),
                pbdb.Copy());
        }

        public void Register(object t)
        {
            if (t is Message)
            {
                ISet<FileDescriptor> fds = Db.CollectFileDescriptorSet((Message)t);
                foreach (FileDescriptor fd in fds)
                {
                    RegisterDescriptor(fd);
                }

                RegisterMessage((Message)t);
            }
            else if (t is Type)
            {
                RegisterType((global::Cel.Common.Types.Ref.Type)t);
            }
            else
            {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw new Exception(String.Format("unsupported type: {0}", t.GetType().FullName));
            }
        }

        public Val EnumValue(string enumName)
        {
            EnumValueDescription enumVal = pbdb.DescribeEnum(enumName);
            if (enumVal == null)
            {
                return Err.NewErr("unknown enum name '{0}'", enumName);
            }

            return IntT.IntOf(enumVal.Value());
        }

        public FieldType FindFieldType(string messageType, string fieldName)
        {
            PbTypeDescription msgType = pbdb.DescribeType(messageType);
            if (msgType == null)
            {
                return null;
            }

            FieldDescription field = msgType.FieldByName(fieldName);
            if (field == null)
            {
                return null;
            }

            return new FieldType(field.CheckedType(), field.HasField, field.GetField);
        }

        public Val FindIdent(string identName)
        {
            global::Cel.Common.Types.Ref.Type t = revTypeMap[identName];
            if (t != null)
            {
                return t;
            }

            EnumValueDescription enumVal = pbdb.DescribeEnum(identName);
            if (enumVal != null)
            {
                return IntT.IntOf(enumVal.Value());
            }

            return null;
        }

        public Type FindType(string typeName)
        {
            if (pbdb.DescribeType(typeName) == null)
            {
                return null;
            }

            if (typeName.Length > 0 && typeName[0] == '.')
            {
                typeName = typeName.Substring(1);
            }

            Type type = new Type();
            type.MessageType = typeName;
            Type result = new Type();
            result.Type_ = type;
            return result;
        }

        public Val NewValue(string typeName, IDictionary<string, Val> fields)
        {
            PbTypeDescription td = pbdb.DescribeType(typeName);
            if (td == null)
            {
                return Err.UnknownType(typeName);
            }

            Message msg = td.NewMessageBuilder();
            Val err = NewValueSetFields(fields, td, msg);
            if (err != null)
            {
                return err;
            }

            return NativeToValue(msg);
        }

        private Val NewValueSetFields(IDictionary<string, Val> fields, PbTypeDescription td, Message builder)
        {
            IDictionary<string, FieldDescription> fieldMap = td.FieldMap();
            foreach (KeyValuePair<string, Val> nv in fields)
            {
                string name = nv.Key;
                FieldDescription field;
                fieldMap.TryGetValue(name, out field);
                if (field == null)
                {
                    return Err.NoSuchField(name);
                }

                // TODO resolve inefficiency for maps: first converted from a MapT to a native Java map and
                //  then to a protobuf struct. The intermediate step (the Java map) could be omitted.

                object value = nv.Value.ConvertToNative(field.ReflectType());
                if (value.GetType().IsArray)
                {
                    value = new List<object>((object[])value);
                }

                FieldDescriptor pbDesc = field.Descriptor();

                if (pbDesc.FieldType == Google.Protobuf.Reflection.FieldType.Enum)
                {
                    value = IntToProtoEnumValues(field, value);
                }

                // TODO
                /*
                if (pbDesc.IsMap)
                {
                  value = ToProtoMapStructure(pbDesc, value);
                }
                */


                pbDesc.Accessor.SetValue(builder, value);
            }

            return null;
        }

        /// <summary>
        /// Converts {@code value}, of the map-field {@code fieldDesc} from its Java <seealso cref="System.Collections.IDictionary"/>
        /// representation to the protobuf-y {@code <seealso cref="System.Collections.IList"/><<seealso cref="MapEntry"/>>} representation.
        /// </summary>
        /*
        private object ToProtoMapStructure(FieldDescriptor fieldDesc, object value)
        {
          Descriptor mesgType = fieldDesc.MessageType;
          FieldDescriptor keyType = mesgType.FindFieldByNumber(1);
          FieldDescriptor valueType = mesgType.FindFieldByNumber(2);
          WireFormat.FieldType keyFieldType = WireFormat.FieldType.valueOf(keyType.getType().name());
          WireFormat.FieldType valueFieldType = WireFormat.FieldType.valueOf(valueType.getType().name());
          if (value is System.Collections.IDictionary)
          {
            System.Collections.IList newList = new ArrayList();
  //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
  //ORIGINAL LINE: for (java.util.Map.Entry e : ((java.util.Map<?, ?>) value).entrySet())
            foreach (DictionaryEntry e in ((IDictionary<object, object>) value).SetOfKeyValuePairs())
            {
              object v = e.Value;
              object k = e.Key;
  
              // TODO improve the type-A-to-B-conversion
              // if (!(k instanceof String)) {
              //   return Err.newTypeConversionError(k.getClass().getName(), String.class.getName());
              // }
              if (valueFieldType == WireFormat.FieldType.MESSAGE && !(v is Message))
              {
                v = NativeToValue(v).ConvertToNative(typeof(Value));
              }
  
              MapEntry newEntry = MapEntry.newDefaultInstance(mesgType, keyFieldType, k, valueFieldType, v);
              newList.Add(newEntry);
            }
            value = newList;
          }
  
          return value;
        }
        */

        /// <summary>
        /// Converts a value of type <seealso cref="Number"/> to <seealso cref="EnumValueDescriptor"/>, also works for arrays
        /// and <seealso cref="System.Collections.IList"/>s containing <seealso cref="Number"/>s.
        /// </summary>
        private object IntToProtoEnumValues(FieldDescription field, object value)
        {
            EnumDescriptor enumType = field.Descriptor().EnumType;
            if (value is int)
            {
                int enumValue = (int)value;
                value = enumType.FindValueByNumber(enumValue);
            }
            else if (value is System.Collections.IList)
            {
                System.Collections.IList list = (System.Collections.IList)value;
                System.Collections.IList newList = new ArrayList(list.Count);
                foreach (object o in list)
                {
                    int enumValue = (int)o;
                    newList.Add(enumType.FindValueByNumber(enumValue));
                }

                value = newList;
            }
            else if (value.GetType().IsArray)
            {
                int[] array = (int[])value;
                int l = array.Length;
                EnumValueDescriptor[] newArr = new EnumValueDescriptor[l];
                for (int i = 0; i < l; i++)
                {
                    int enumValue = array[i];
                    newArr[i] = enumType.FindValueByNumber(enumValue);
                }

                value = newArr;
            }

            return value;
        }

        /// <summary>
        /// RegisterDescriptor registers the contents of a protocol buffer `FileDescriptor`. </summary>
        public void RegisterDescriptor(FileDescriptor fileDesc)
        {
            FileDescription fd = pbdb.RegisterDescriptor(fileDesc);
            RegisterAllTypes(fd);
        }

        /// <summary>
        /// RegisterMessage registers a protocol buffer message and its dependencies. </summary>
        public void RegisterMessage(Message message)
        {
            FileDescription fd = pbdb.RegisterMessage(message);
            RegisterAllTypes(fd);
        }

        public void RegisterType(params global::Cel.Common.Types.Ref.Type[] types)
        {
            foreach (global::Cel.Common.Types.Ref.Type t in types)
            {
                revTypeMap[t.TypeName()] = t;
            }
            // TODO: generate an error when the type name is registered more than once.
        }

        public TypeAdapter ToTypeAdapter()
        {
            return NativeToValue;
        }

        /// <summary>
        /// NativeToValue converts various "native" types to ref.Val with this specific implementation
        /// providing support for custom proto-based types.
        /// 
        /// <para>This method should be the inverse of ref.Val.ConvertToNative.
        /// </para>
        /// </summary>
        public Val NativeToValue(object value)
        {
            Val val;
            if (value is Message)
            {
                Message v = (Message)value;
                string typeName = PbTypeDescription.TypeNameFromMessage(v);
                if (typeName.Length == 0)
                {
                    return Err.AnyWithEmptyType();
                }

                PbTypeDescription td = pbdb.DescribeType(typeName);
                if (td == null)
                {
                    return Err.UnknownType(typeName);
                }

                object unwrapped = td.MaybeUnwrap(pbdb, v);
                if (unwrapped != null)
                {
                    object further = DefaultTypeAdapter.MaybeUnwrapValue(unwrapped);
                    if (further != unwrapped)
                    {
                        return NativeToValue(further);
                    }

                    val = TypeAdapterSupport.MaybeNativeToValue(ToTypeAdapter(), unwrapped);
                    if (val != null)
                    {
                        return val;
                    }

                    if (unwrapped is Message)
                    {
                        v = (Message)unwrapped;
                    }
                }

                Val typeVal = FindIdent(typeName);
                if (typeVal == null)
                {
                    return Err.UnknownType(typeName);
                }

                return PbObjectT.NewObject(ToTypeAdapter(), td, (TypeT)typeVal, v);
            }

            val = DefaultTypeAdapter.NativeToValue(pbdb, ToTypeAdapter(), value);
            if (val != null)
            {
                return val;
            }

            return Err.UnsupportedRefValConversionErr(value);
        }

        internal void RegisterAllTypes(FileDescription fd)
        {
            foreach (string typeName in fd.TypeNames)
            {
                RegisterType(TypeT.NewObjectTypeValue(typeName));
            }
        }

        public override string ToString()
        {
            return "ProtoTypeRegistry{" + "revTypeMap.size=" + revTypeMap.Count + ", pbdb=" + pbdb + '}';
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

            ProtoTypeRegistry that = (ProtoTypeRegistry)o;
            return Object.Equals(revTypeMap, that.revTypeMap) && Object.Equals(pbdb, that.pbdb);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(revTypeMap, pbdb);
        }
    }
}