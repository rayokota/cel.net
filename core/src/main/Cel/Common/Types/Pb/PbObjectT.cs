using System;
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
//	import static Cel.common.types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.noSuchField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Types.boolOf;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using Message = Google.Protobuf.IMessage;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using ObjectT = global::Cel.Common.Types.ObjectT;
    using StringT = global::Cel.Common.Types.StringT;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeAdapter = global::Cel.Common.Types.Ref.TypeAdapter;
    using Val = global::Cel.Common.Types.Ref.Val;

    public sealed class PbObjectT : ObjectT
    {
        private PbObjectT(TypeAdapter adapter, Message value, PbTypeDescription typeDesc, Type typeValue) : base(
            adapter, value, typeDesc, typeValue)
        {
        }

        /// <summary>
        /// NewObject returns an object based on a proto.Message value which handles conversion between
        /// protobuf type values and expression type values. Objects support indexing and iteration.
        /// 
        /// <para>Note: the type value is pulled from the list of registered types within the type provider.
        /// If the proto type is not registered within the type provider, then this will result in an error
        /// within the type adapter / provider.
        /// </para>
        /// </summary>
        public static Val NewObject(TypeAdapter adapter, PbTypeDescription typeDesc, Type typeValue, Message value)
        {
            return new PbObjectT(adapter, value, typeDesc, typeValue);
        }

        /// <summary>
        /// IsSet tests whether a field which is defined is set to a non-default value. </summary>
        public override Val IsSet(Val field)
        {
            if (!(field is StringT))
            {
                return Err.NoSuchOverload(this, "isSet", field);
            }

            string protoFieldStr = (string)field.Value();
            FieldDescription fd = TypeDesc().FieldByName(protoFieldStr);
            if (fd == null)
            {
                return Err.NoSuchField(protoFieldStr);
            }

            return Types.BoolOf(fd.HasField(value));
        }

        public override Val Get(Val index)
        {
            if (!(index is StringT))
            {
                return Err.NoSuchOverload(this, "get", index);
            }

            string protoFieldStr = (string)index.Value();
            FieldDescription fd = TypeDesc().FieldByName(protoFieldStr);
            if (fd == null)
            {
                return Err.NoSuchField(protoFieldStr);
            }

            return NativeToValue(fd.GetField(value));
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (typeDesc.IsAssignableFrom(this.GetType()))
            {
                return this;
            }

            if (typeDesc.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (typeDesc == typeof(Any))
            {
                // anyValueType
                if (value is Any)
                {
                    return value;
                }

                return Any.Pack(Message());
            }

            if (typeDesc == typeof(Value))
            {
                // jsonValueType
                throw new System.NotSupportedException("IMPLEMENT proto-to-json");
                // TODO proto-to-json
                //		// Marshal the proto to JSON first, and then rehydrate as protobuf.Value as there is no
                //		// support for direct conversion from proto.Message to protobuf.Value.
                //		bytes, err := protojson.Marshal(pb)
                //		if err != nil {
                //			return nil, err
                //		}
                //		json := &structpb.Value{}
                //		err = protojson.Unmarshal(bytes, json)
                //		if err != nil {
                //			return nil, err
                //		}
                //		return json, nil
            }

            if (typeDesc.IsAssignableFrom(this.typeDesc.ReflectType()) || typeDesc == typeof(object))
            {
                if (value is Any)
                {
                    return BuildFrom(typeDesc);
                }

                return value;
            }

            if (typeDesc.IsAssignableFrom(typeof(Message)))
            {
                return BuildFrom(typeDesc);
            }

            if (typeDesc == typeof(Val) || typeDesc == typeof(PbObjectT))
            {
                return this;
            }

            // impossible cast
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new System.ArgumentException(
                Err.NewTypeConversionError(value.GetType().FullName, typeDesc).ToString());
        }

        private Message Message()
        {
            return (Message)value;
        }

        private PbTypeDescription TypeDesc()
        {
            return (PbTypeDescription)typeDesc;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T> T buildFrom(Class<T> typeDesc)
        private Message BuildFrom(System.Type typeDesc)
        {
            try
            {
                Message builder = (Message)Activator.CreateInstance(typeDesc);
                MessageExtensions.MergeFrom(builder, Message().ToByteArray());
                return builder;
            }
            catch (Exception e)
            {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw new Exception(String.Format("{0}: {1}",
                    Err.NewTypeConversionError(value.GetType().FullName, typeDesc), e));
            }
        }
    }
}