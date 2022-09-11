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

namespace Cel.Common.Types.Ref;

public abstract class BaseVal : IVal
{
    public abstract object Value();
    public abstract IType Type();
    public abstract IVal Equal(IVal other);
    public abstract IVal ConvertToType(IType typeValue);
    public abstract object? ConvertToNative(System.Type typeDesc);

    public virtual bool BooleanValue()
    {
        return ConvertToType(BoolT.BoolType).BooleanValue();
    }

    public virtual long IntValue()
    {
        return ConvertToType(IntT.IntType).IntValue();
    }

    public virtual ulong UintValue()
    {
        return ConvertToType(UintT.UintType).UintValue();
    }

    public override int GetHashCode()
    {
        return Value().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is IVal) return Equal((IVal)obj) == BoolT.True;

        return Value().Equals(obj);
    }

    public override string ToString()
    {
        return string.Format("{0}{{{1}}}", Type().TypeName(), Value());
    }
}