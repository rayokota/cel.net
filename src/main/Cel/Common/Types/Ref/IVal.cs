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

namespace Cel.Common.Types.Ref;

/// <summary>
///     IVal interface defines the functions supported by all expression values. Val implementations may
///     specialize the behavior of the value through the addition of traits.
/// </summary>
public interface IVal
{
    /// <summary>
    ///     ConvertToNative converts the Value to a native Go struct according to the reflected type
    ///     description, or error if the conversion is not feasible.
    /// </summary>
    object? ConvertToNative(Type typeDesc);

    /// <summary>
    ///     ConvertToType supports type conversions between value types supported by the expression
    ///     language.
    /// </summary>
    IVal ConvertToType(IType typeValue);

    /// <summary>
    ///     Equal returns true if the `other` value has the same type and content as the implementing
    ///     struct.
    /// </summary>
    IVal Equal(IVal other);

    /// <summary>
    ///     Type returns the TypeValue of the value.
    /// </summary>
    IType Type();

    /// <summary>
    ///     Value returns the raw value of the instance which may not be directly compatible with the
    ///     expression language types.
    /// </summary>
    object Value();

    bool BooleanValue();

    long IntValue();

    ulong UintValue();
}