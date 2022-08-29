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

namespace Cel.Common.Types
{
    using Val = global::Cel.Common.Types.Ref.Val;

    public sealed class Util
    {
        /// <summary>
        /// IsUnknownOrError returns whether the input element ref.Val is an ErrType or UnknonwType. </summary>
        public static bool IsUnknownOrError(Val val)
        {
            switch (val.Type().TypeEnum().InnerEnumValue)
            {
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Unknown:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Err:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// IsPrimitiveType returns whether the input element ref.Val is a primitive type. Note, primitive
        /// types do not include well-known types such as Duration and Timestamp.
        /// </summary>
        public static bool IsPrimitiveType(Val val)
        {
            switch (val.Type().TypeEnum().InnerEnumValue)
            {
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bool:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bytes:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Double:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Int:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.String:
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Uint:
                    return true;
            }

            return false;
        }
    }
}