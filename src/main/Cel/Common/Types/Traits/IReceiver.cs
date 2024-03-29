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

namespace Cel.Common.Types.Traits;

/// <summary>
///     IReceiver interface for routing instance method calls within a value.
/// </summary>
public interface IReceiver
{
    /// <summary>
    ///     Receive accepts a function name, overload id, and arguments and returns a value.
    /// </summary>
    IVal Receive(string function, string overload, params IVal[] args);
}