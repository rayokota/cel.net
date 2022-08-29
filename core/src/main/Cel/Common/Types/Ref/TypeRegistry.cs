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

namespace Cel.Common.Types.Ref
{
    /// <summary>
    /// TypeRegistry allows third-parties to add custom types to CEL. Not all `TypeProvider`
    /// implementations support type-customization, so these features are optional. However, a
    /// `TypeRegistry` should be a `TypeProvider` and a `TypeAdapter` to ensure that types which are
    /// registered can be converted to CEL representations.
    /// </summary>
    public interface TypeRegistry : TypeAdapterProvider, TypeProvider
    {
        /// <summary>
        /// Copy the TypeRegistry and return a new registry whose mutable state is isolated. </summary>
        TypeRegistry Copy();

        /// <summary>
        /// Register a type via a materialized object, which the provider can turn into a type. </summary>
        void Register(object t);

        /// <summary>
        /// RegisterType registers a type value with the provider which ensures the provider is aware of
        /// how to map the type to an identifier.
        /// 
        /// <para>If a type is provided more than once with an alternative definition, the call will result in
        /// an error.
        /// </para>
        /// </summary>
        void RegisterType(params Type[] types);
    }
}