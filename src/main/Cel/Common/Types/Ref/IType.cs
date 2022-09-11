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

using Cel.Common.Types.Traits;

namespace Cel.Common.Types.Ref;

/// <summary>
///     Type interface indicate the name of a given type.
/// </summary>
public interface IType : IVal
{
    /// <summary>
    ///     HasTrait returns whether the type has a given trait associated with it.
    ///     <para>
    ///         See common/types/traits/traits.go for a list of supported traits.
    ///     </para>
    /// </summary>
    bool HasTrait(Trait trait);

    /// <summary>
    ///     TypeName returns the qualified type name of the type.
    ///     <para>
    ///         The type name is also used as the type's identifier name at type-check and interpretation
    ///         time.
    ///     </para>
    /// </summary>
    string TypeName();

    /// <summary>
    ///     Get the type enum.
    /// </summary>
    TypeEnum TypeEnum();
}