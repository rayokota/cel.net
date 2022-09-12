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

namespace Cel.Common;

public interface ILocation : IComparable<ILocation>
{
    public static readonly ILocation NoLocation = NewLocation(-1, -1);

    // NewLocation creates a new location.
    public static ILocation NewLocation(int line, int column)
    {
        return new SourceLocation(line, column);
    }

    /// <summary>
    ///     1-based line number within source.
    /// </summary>
    int Line();

    /// <summary>
    ///     0-based column number within source.
    /// </summary>
    int Column();
}

internal sealed class SourceLocation : ILocation
{
    private readonly int column;

    private readonly int line;

    public SourceLocation(int line, int column)
    {
        this.line = line;
        this.column = column;
    }

    public int CompareTo(ILocation? o)
    {
        var r = line.CompareTo(o.Line());
        if (r == 0) r = column.CompareTo(o.Column());

        return r;
    }

    public int Line()
    {
        return line;
    }

    public int Column()
    {
        return column;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (SourceLocation)o;
        return line == that.line && column == that.column;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(line, column);
    }

    public override string ToString()
    {
        return "line=" + line + ", column=" + column;
    }
}