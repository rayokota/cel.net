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
using Cel.Common.Types.Traits;

namespace Cel.Common.Types;

public abstract class ObjectT : BaseVal, IFieldTester, IIndexer, ITypeAdapterProvider
{
    internal readonly TypeAdapter adapter;
    internal readonly ITypeDescription typeDesc;
    internal readonly IType typeValue;
    internal readonly object value;

    internal ObjectT(TypeAdapter adapter, object value, ITypeDescription typeDesc, IType typeValue)
    {
        this.adapter = adapter;
        this.value = value;
        this.typeDesc = typeDesc;
        this.typeValue = typeValue;
    }

    public abstract IVal IsSet(IVal field);
    public abstract IVal Get(IVal index);

    public TypeAdapter ToTypeAdapter()
    {
        return NativeToValue;
    }

    public override IVal ConvertToType(IType typeVal)
    {
        switch (typeVal.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Type:
                return typeValue;
            case TypeEnum.InnerEnum.Object:
                if (Type().TypeName().Equals(typeVal.TypeName())) return this;

                break;
        }

        return Err.NewTypeConversionError(typeDesc.Name(), typeVal);
    }

    public override IVal Equal(IVal other)
    {
        if (!typeDesc.Name().Equals(other.Type().TypeName())) return Err.NoSuchOverload(this, "equal", other);

        return Types.BoolOf(value.Equals(other.Value()));
    }

    public override IType Type()
    {
        return typeValue;
    }

    public override object Value()
    {
        return value;
    }

    public virtual IVal NativeToValue(object value)
    {
        return adapter(value);
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var objectT = (ObjectT)o;
        return Equals(value, objectT.value) && Equals(typeDesc, objectT.typeDesc) &&
               Equals(typeValue, objectT.typeValue);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), value, typeDesc, typeValue);
    }
}