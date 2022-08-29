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

namespace Cel.Common;

public interface Location : IComparable<Location>
{
    public static Location NoLocation = NewLocation(-1, -1);

    // NewLocation creates a new location.
    public static Location NewLocation(int line, int column)
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

internal sealed class SourceLocation : Location
{
    private readonly int column_Conflict;

    private readonly int line_Conflict;

    public SourceLocation(int line, int column)
    {
        line_Conflict = line;
        column_Conflict = column;
    }

    public int CompareTo(Location o)
    {
        var r = line_Conflict.CompareTo(o.Line());
        if (r == 0) r = column_Conflict.CompareTo(o.Column());

        return r;
    }

    public int Line()
    {
        return line_Conflict;
    }

    public int Column()
    {
        return column_Conflict;
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var that = (SourceLocation)o;
        return line_Conflict == that.line_Conflict && column_Conflict == that.column_Conflict;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(line_Conflict, column_Conflict);
    }

    public override string ToString()
    {
        return "line=" + line_Conflict + ", column=" + column_Conflict;
    }
}