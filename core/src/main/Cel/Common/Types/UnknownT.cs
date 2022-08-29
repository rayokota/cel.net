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
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
    using Val = global::Cel.Common.Types.Ref.Val;

    /// <summary>
    /// Unknown type implementation which collects expression ids which caused the current value to
    /// become unknown.
    /// </summary>
    public sealed class UnknownT : BaseVal
    {
        /// <summary>
        /// UnknownType singleton. </summary>
        public static readonly Type UnknownType = TypeT.NewTypeValue(TypeEnum.Unknown);

        public static UnknownT UnknownOf(long value)
        {
            return new UnknownT(value);
        }

        private readonly long value;

        private UnknownT(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc.IsAssignableFrom(typeof(long)) || typeDesc.IsAssignableFrom(typeof(long)) ||
                typeDesc == typeof(object))
            {
                return value;
            }

            if (typeDesc == typeof(Val) || typeDesc == typeof(UnknownT))
            {
                return this;
            }

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new Exception(String.Format("native type conversion error from '{0}' to '{1}'", UnknownType,
                typeDesc.FullName));
        }

        public override long IntValue()
        {
            return value;
        }

        /// <summary>
        /// ConvertToType implements ref.Val.ConvertToType. </summary>
        public override Val ConvertToType(Type typeVal)
        {
            return this;
        }

        /// <summary>
        /// Equal implements ref.Val.Equal. </summary>
        public override Val Equal(Val other)
        {
            return this;
        }

        /// <summary>
        /// Type implements ref.Val.Type. </summary>
        public override Type Type()
        {
            return UnknownType;
        }

        /// <summary>
        /// Value implements ref.Val.Value. </summary>
        public override object Value()
        {
            return value;
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

            UnknownT unknownT = (UnknownT)o;
            return value == unknownT.value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), value);
        }

        /// <summary>
        /// IsUnknown returns whether the element ref.Type or ref.Val is equal to the UnknownType
        /// singleton.
        /// </summary>
        public static bool IsUnknown(object val)
        {
            return val != null && val.GetType() == typeof(UnknownT);
        }
    }
}