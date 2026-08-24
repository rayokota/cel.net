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

namespace Cel.Common.Types.Ref;

/// <summary>
///     ITypeAdapter converts native Go values of varying type and complexity to equivalent CEL values.
/// </summary>
public interface ITypeAdapterProvider
{
    TypeAdapter ToTypeAdapter();

    /// <summary>
    ///     NativeToValue converts a native value to its CEL equivalent. The counterpart of cel-go's
    ///     types.Adapter interface: an adapter is an object, not a bare delegate, so that
    ///     Env.Extend can recognize a mutable adapter and copy or alias it the way cel-go does.
    ///     ToTypeAdapter above remains as the delegate view for the interpreter internals.
    /// </summary>
    IVal NativeToValue(object? value);
}