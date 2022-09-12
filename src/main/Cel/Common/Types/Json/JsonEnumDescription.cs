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

public sealed class JsonEnumDescription
{
    private readonly IList<Enum> enumValues;

    private readonly string name;
    private readonly Type pbType;

    public JsonEnumDescription(System.Type type)
    {
        name = type.FullName!;

        enumValues = new List<Enum>();
        foreach (Enum e in Enum.GetValues(type)) enumValues.Add(e);
        pbType = Checked.CheckedInt;
    }

    public Type PbType()
    {
        return pbType;
    }

    public IEnumerable<JsonEnumValue> BuildValues()
    {
        return enumValues.Select(v => new JsonEnumValue(v));
    }
}