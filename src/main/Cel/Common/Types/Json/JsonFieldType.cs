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
using Newtonsoft.Json.Serialization;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types.Json;

public sealed class JsonFieldType : FieldType
{
    private readonly IValueProvider propertyWriter;

    public JsonFieldType(Type type, FieldTester isSet, FieldGetter getFrom, IValueProvider propertyWriter) : base(
        type, isSet, getFrom)
    {
        this.propertyWriter = propertyWriter;
    }

    public IValueProvider PropertyWriter()
    {
        return propertyWriter;
    }
}