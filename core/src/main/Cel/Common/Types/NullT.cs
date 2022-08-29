using System;

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
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;

    using Any = Google.Protobuf.WellKnownTypes.Any;
    using Value = Google.Protobuf.WellKnownTypes.Value;
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
    using Val = global::Cel.Common.Types.Ref.Val;

    /// <summary>
    /// Null type implementation. </summary>
    public sealed class NullT : BaseVal
    {
        /// <summary>
        /// NullType singleton. </summary>
        public static readonly Type NullType = TypeT.NewTypeValue(TypeEnum.Null);

        /// <summary>
        /// NullValue singleton. </summary>
        public static readonly NullT NullValue = new NullT();

        private static readonly Value PbValue;
        private static readonly Any PbAny;

        static NullT()
        {
            Value value = new Value();
            value.NullValue = Google.Protobuf.WellKnownTypes.NullValue.NullValue;
            PbValue = value;

            PbAny = Any.Pack(PbValue);
        }

        /// <summary>
        /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc == typeof(int))
            {
                return (int?)0;
            }

            if (typeDesc == typeof(Any))
            {
                return PbAny;
            }

            if (typeDesc == typeof(Value))
            {
                return PbValue;
            }

            if (typeDesc == typeof(Google.Protobuf.WellKnownTypes.NullValue))
            {
                return Google.Protobuf.WellKnownTypes.NullValue.NullValue;
            }

            if (typeDesc == typeof(Val) || typeDesc == typeof(NullT))
            {
                return this;
            }

            if (typeDesc == typeof(object))
            {
                return null;
            }

            //		switch typeDesc.Kind() {
            //		case reflect.Interface:
            //			nv := n.Value()
            //			if reflect.TypeOf(nv).Implements(typeDesc) {
            //				return nv, nil
            //			}
            //			if reflect.TypeOf(n).Implements(typeDesc) {
            //				return n, nil
            //			}
            //		}
            // If the type conversion isn't supported return an error.
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", NullType,
                typeDesc.FullName));
        }

        /// <summary>
        /// ConvertToType implements ref.Val.ConvertToType. </summary>
        public override Val ConvertToType(Type typeValue)
        {
            switch (typeValue.TypeEnum().InnerEnumValue)
            {
                case TypeEnum.InnerEnum.String:
                    return StringT.StringOf("null");
                case TypeEnum.InnerEnum.Null:
                    return this;
                case TypeEnum.InnerEnum.Type:
                    return NullType;
            }

            return Err.NewTypeConversionError(NullType, typeValue);
        }

        /// <summary>
        /// Equal implements ref.Val.Equal. </summary>
        public override Val Equal(Val other)
        {
            if (NullType != other.Type())
            {
                return Err.NoSuchOverload(this, "equal", other);
            }

            return BoolT.True;
        }

        /// <summary>
        /// Type implements ref.Val.Type. </summary>
        public override Type Type()
        {
            return NullType;
        }

        /// <summary>
        /// Value implements ref.Val.Value. </summary>
        public override object Value()
        {
            return Google.Protobuf.WellKnownTypes.NullValue.NullValue;
        }

        public override string ToString()
        {
            return "null";
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(NullT);
        }
    }
}