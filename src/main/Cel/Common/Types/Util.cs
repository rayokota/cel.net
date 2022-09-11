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

using Cel.Common.Types.Ref;

namespace Cel.Common.Types;

public sealed class Util
{
    /// <summary>
    ///     IsUnknownOrError returns whether the input element ref.Val is an ErrType or UnknonwType.
    /// </summary>
    public static bool IsUnknownOrError(IVal val)
    {
        switch (val.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Unknown:
            case TypeEnum.InnerEnum.Err:
                return true;
        }

        return false;
    }

    /// <summary>
    ///     IsPrimitiveType returns whether the input element ref.Val is a primitive type. Note, primitive
    ///     types do not include well-known types such as Duration and Timestamp.
    /// </summary>
    public static bool IsPrimitiveType(IVal val)
    {
        switch (val.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Bool:
            case TypeEnum.InnerEnum.Bytes:
            case TypeEnum.InnerEnum.Double:
            case TypeEnum.InnerEnum.Int:
            case TypeEnum.InnerEnum.String:
            case TypeEnum.InnerEnum.Uint:
                return true;
        }

        return false;
    }
}