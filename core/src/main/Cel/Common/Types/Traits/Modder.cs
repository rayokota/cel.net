/*
 * Copyright (C) 2021 The Authors of CEL-Java
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

namespace Cel.Common.Types.Traits
{
    using Val = global::Cel.Common.Types.Ref.Val;

    /// <summary>
    /// Modder interface to support '%' operator overloads. </summary>
    public interface Modder
    {
        /// <summary>
        /// Modulo returns the result of taking the modulus of the current value by the denominator.
        /// 
        /// <para>A denominator value of zero results in an error.
        /// </para>
        /// </summary>
        Val Modulo(Val denominator);
    }
}