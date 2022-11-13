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

using Avro;
using Cel.Common.Types.Pb;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types.Avro;

public sealed class AvroEnumDescription
{
    private readonly string fullName;
    private readonly Type pbType;
    private readonly IList<string> enumValues;

    public AvroEnumDescription(EnumSchema schema)
    {
        fullName = schema.Fullname;
        enumValues = schema.Symbols;
        pbType = Checked.CheckedString;
    }

    public string FullName()
    {
        return fullName;
    }

    public Type PbType()
    {
        return pbType;
    }

    public IEnumerable<AvroEnumValue> BuildValues()
    {
        return enumValues.Select(v => new AvroEnumValue(this, v));
    }
}