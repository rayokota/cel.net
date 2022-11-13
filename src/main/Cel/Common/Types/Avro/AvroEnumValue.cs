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

using Avro.Generic;
using Cel.Common.Types.Ref;

namespace Cel.Common.Types.Avro;

public sealed class AvroEnumValue
{
    private readonly AvroEnumDescription enumType;
    private readonly IVal stringValue;

    public AvroEnumValue(AvroEnumDescription enumType, string enumValue)
    {
        this.enumType = enumType;
        this.stringValue = StringT.StringOf(enumValue);
    }

    public static string FullyQualifiedName(GenericEnum value)
    {
        return value.Schema.Fullname + '.' + value;
    }

    public string FullyQualifiedName()
    {
        return enumType.FullName() + "." + stringValue.Value();
    }

    public IVal StringValue()
    {
        return stringValue;
    }
}