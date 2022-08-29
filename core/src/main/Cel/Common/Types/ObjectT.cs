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

using Cel.Common.Types.Ref;

namespace Cel.Common.Types
{
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeAdapter = global::Cel.Common.Types.Ref.TypeAdapter;
    using TypeDescription = global::Cel.Common.Types.Ref.TypeDescription;
    using Val = global::Cel.Common.Types.Ref.Val;
    using FieldTester = global::Cel.Common.Types.Traits.FieldTester;
    using Indexer = global::Cel.Common.Types.Traits.Indexer;

    public abstract class ObjectT : BaseVal, FieldTester, Indexer, TypeAdapterProvider
    {
        public abstract Val Get(Ref.Val index);
        public abstract Val IsSet(Ref.Val field);
        protected internal readonly TypeAdapter adapter;
        protected internal readonly object value;
        protected internal readonly TypeDescription typeDesc;
        protected internal readonly Type typeValue;

        protected internal ObjectT(TypeAdapter adapter, object value, TypeDescription typeDesc, Type typeValue)
        {
            this.adapter = adapter;
            this.value = value;
            this.typeDesc = typeDesc;
            this.typeValue = typeValue;
        }

        public override Val ConvertToType(Type typeVal)
        {
            switch (typeVal.TypeEnum().InnerEnumValue)
            {
                case TypeEnum.InnerEnum.Type:
                    return typeValue;
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Object:
                    if (Type().TypeName().Equals(typeVal.TypeName()))
                    {
                        return this;
                    }

                    break;
            }

            return Err.NewTypeConversionError(typeDesc.Name(), typeVal);
        }

        public override Val Equal(Val other)
        {
            if (!typeDesc.Name().Equals(other.Type().TypeName()))
            {
                return Err.NoSuchOverload(this, "equal", other);
            }

            return Types.BoolOf(this.value.Equals(other.Value()));
        }

        public override Type Type()
        {
            return typeValue;
        }

        public override object Value()
        {
            return value;
        }

        public TypeAdapter ToTypeAdapter()
        {
            return NativeToValue;
        }

        public virtual Val NativeToValue(object value)
        {
            return adapter(value);
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

            ObjectT objectT = (ObjectT)o;
            return Object.Equals(value, objectT.value) && Object.Equals(typeDesc, objectT.typeDesc) &&
                   Object.Equals(typeValue, objectT.typeValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), value, typeDesc, typeValue);
        }
    }
}