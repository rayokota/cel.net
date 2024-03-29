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

using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types.Ref;

/// <summary>
///     ITypeProvider specifies functions for creating new object instances and for resolving enum values
///     by name.
/// </summary>
public interface ITypeProvider
{
    /// <summary>
    ///     EnumValue returns the numeric value of the given enum value name.
    /// </summary>
    IVal EnumValue(string enumName);

    /// <summary>
    ///     FindIdent takes a qualified identifier name and returns a Value if one exists.
    /// </summary>
    IVal? FindIdent(string identName);

    /// <summary>
    ///     FindType looks up the Type given a qualified typeName. Returns false if not found.
    ///     <para>
    ///         Used during type-checking only.
    ///     </para>
    /// </summary>
    Type? FindType(string typeName);

    /// <summary>
    ///     FieldFieldType returns the field type for a checked type value. Returns false if the field
    ///     could not be found.
    ///     <para>
    ///         Used during type-checking only.
    ///     </para>
    /// </summary>
    FieldType? FindFieldType(string messageType, string fieldName);

    /// <summary>
    ///     NewValue creates a new type value from a qualified name and map of field name to value.
    ///     <para>
    ///         Note, for each value, the Val.ConvertToNative function will be invoked to convert the Val to
    ///         the field's native type. If an error occurs during conversion, the NewValue will be a
    ///         types.Err.
    ///     </para>
    /// </summary>
    IVal NewValue(string typeName, IDictionary<string, IVal> fields);
}