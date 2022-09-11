using Cel.Common.Types.Ref;
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

public sealed class PbObjectT : ObjectT
{
    private PbObjectT(TypeAdapter adapter, Message value, PbTypeDescription typeDesc, IType typeValue) : base(
        adapter, value, typeDesc, typeValue)
    {
    }

    /// <summary>
    ///     NewObject returns an object based on a proto.Message value which handles conversion between
    ///     protobuf type values and expression type values. Objects support indexing and iteration.
    ///     <para>
    ///         Note: the type value is pulled from the list of registered types within the type provider.
    ///         If the proto type is not registered within the type provider, then this will result in an error
    ///         within the type adapter / provider.
    ///     </para>
    /// </summary>
    public static IVal NewObject(TypeAdapter adapter, PbTypeDescription typeDesc, IType typeValue, Message value)
    {
        return new PbObjectT(adapter, value, typeDesc, typeValue);
    }

    /// <summary>
    ///     IsSet tests whether a field which is defined is set to a non-default value.
    /// </summary>
    public override IVal IsSet(IVal field)
    {
        if (!(field is StringT)) return Err.NoSuchOverload(this, "isSet", field);

        var protoFieldStr = (string)field.Value();
        var fd = TypeDesc().FieldByName(protoFieldStr);
        if (fd == null) return Err.NoSuchField(protoFieldStr);

        return Types.BoolOf(fd.HasField(value));
    }

    public override IVal Get(IVal index)
    {
        if (!(index is StringT)) return Err.NoSuchOverload(this, "get", index);

        var protoFieldStr = (string)index.Value();
        var fd = TypeDesc().FieldByName(protoFieldStr);
        if (fd == null) return Err.NoSuchField(protoFieldStr);

        return NativeToValue(fd.GetField(value));
    }

    public override object? ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc.IsAssignableFrom(value.GetType())) return value;

        if (typeDesc.IsAssignableFrom(GetType())) return this;

        if (typeDesc.IsAssignableFrom(value.GetType())) return value;

        if (typeDesc == typeof(Any))
        {
            // anyValueType
            if (value is Any) return value;

            return Any.Pack(Message());
        }

        if (typeDesc == typeof(Value))
            // jsonValueType
            throw new NotSupportedException("IMPLEMENT proto-to-json");
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

        if (typeDesc.IsAssignableFrom(this.typeDesc.ReflectType()) || typeDesc == typeof(object))
        {
            if (value is Any) return BuildFrom(typeDesc);

            return value;
        }

        if (typeDesc.IsAssignableFrom(typeof(Message))) return BuildFrom(typeDesc);

        if (typeDesc == typeof(IVal) || typeDesc == typeof(PbObjectT)) return this;

        // impossible cast
        throw new ArgumentException(
            Err.NewTypeConversionError(value.GetType().FullName!, typeDesc).ToString());
    }

    private Message Message()
    {
        return (Message)value;
    }

    private PbTypeDescription TypeDesc()
    {
        return (PbTypeDescription)typeDesc;
    }

    private Message BuildFrom(System.Type typeDesc)
    {
        try
        {
            var builder = (Message)Activator.CreateInstance(typeDesc)!;
            builder.MergeFrom(Message().ToByteArray());
            return builder;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("{0}: {1}",
                Err.NewTypeConversionError(value.GetType().FullName!, typeDesc), e));
        }
    }
}