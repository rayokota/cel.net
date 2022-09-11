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

using Google.Protobuf.Reflection;

namespace Cel.Common.Types.Pb;

/// <summary>
///     EnumValueDescription maps a fully-qualified enum value name to its numeric value.
/// </summary>
public sealed class EnumValueDescription
{
    private readonly EnumValueDescriptor desc;
    private readonly string enumValueName;

    private EnumValueDescription(string enumValueName, EnumValueDescriptor desc)
    {
        this.enumValueName = enumValueName;
        this.desc = desc;
    }

    /// <summary>
    ///     NewEnumValueDescription produces an enum value description with the fully qualified enum value
    ///     name and the enum value descriptor.
    /// </summary>
    public static EnumValueDescription NewEnumValueDescription(string name, EnumValueDescriptor desc)
    {
        return new EnumValueDescription(name, desc);
    }

    /// <summary>
    ///     Name returns the fully-qualified identifier name for the enum value.
    /// </summary>
    public string Name()
    {
        return enumValueName;
    }

    /// <summary>
    ///     Value returns the (numeric) value of the enum.
    /// </summary>
    public int Value()
    {
        return desc.Number;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (EnumValueDescription)o;
        return Equals(enumValueName, that.enumValueName) && Equals(desc, that.desc);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(enumValueName, desc);
    }
}