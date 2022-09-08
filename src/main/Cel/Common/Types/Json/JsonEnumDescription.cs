using Cel.Common.Types.Pb;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Common.Types.Json;

internal sealed class JsonEnumDescription
{
    private readonly IEnumerable<Enum> enumValues;

    private readonly string name;
    private readonly Type pbType;

    internal JsonEnumDescription(System.Type type)
    {
        name = type.FullName;
        enumValues = (IEnumerable<Enum>)Enum.GetValues(type);
        pbType = Checked.checkedInt;
    }

    internal Type PbType()
    {
        return pbType;
    }

    internal IEnumerable<JsonEnumValue> BuildValues()
    {
        return enumValues.Select(v => new JsonEnumValue(v));
    }
}