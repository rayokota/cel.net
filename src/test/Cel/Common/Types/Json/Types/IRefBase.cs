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

using JsonSubTypes;
using Newtonsoft.Json;

namespace Cel.Common.Types.Json.Types;

[JsonConverter(typeof(JsonSubtypes), "Type")]
[JsonSubtypes.KnownSubTypeAttribute(typeof(RefVariantA), "A")]
[JsonSubtypes.KnownSubTypeAttribute(typeof(RefVariantB), "B")]
[JsonSubtypes.KnownSubTypeAttribute(typeof(RefVariantC), "C")]
public interface IRefBase
{
    string Type { get; }

    string Name { get; }

    string Hash { get; }
}