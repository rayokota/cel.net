﻿/*
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

namespace Cel.Common.Types.Json;

public sealed class JsonEnumValue
{
    private readonly Enum enumValue;

    private readonly IVal ordinalValue;

    public JsonEnumValue(Enum enumValue)
    {
        ordinalValue = IntT.IntOf(Convert.ToInt32(enumValue));
        this.enumValue = enumValue;
    }

    public static string FullyQualifiedName(Enum value)
    {
        return value.GetType().FullName + '.' + Enum.GetName(value.GetType(), value);
    }

    public string FullyQualifiedName()
    {
        var s = FullyQualifiedName(enumValue);
        return s;
    }

    public IVal OrdinalValue()
    {
        return ordinalValue;
    }
}