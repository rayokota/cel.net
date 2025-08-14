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

using Google.Api.Expr.V1Alpha1;

namespace Cel.Common;

/// <summary>
///     ISource interface for filter source contents.
/// </summary>
public interface ISource
{
    /// <summary>
    ///     NewTextSource creates a new Source from the input text string.
    /// </summary>
    

    /// <summary>
    ///     NewStringSource creates a new Source from the given contents and description.
    /// </summary>
    

    /// <summary>
    ///     NewInfoSource creates a new Source from a SourceInfo.
    /// </summary>
    

    /// <summary>
    ///     Content returns the source content represented as a string. Examples contents are the single
    ///     file contents, textbox field, or url parameter.
    /// </summary>
    string Content();

    /// <summary>
    ///     Description gives a brief description of the source. Example descriptions are a file name or ui
    ///     element.
    /// </summary>
    string Description();

    /// <summary>
    ///     LineOffsets gives the character offsets at which lines occur. The zero-th entry should refer to
    ///     the break between the first and second line, or EOF if there is only one line of source.
    /// </summary>
    IList<int> LineOffsets();

    /// <summary>
    ///     LocationOffset translates a Location to an offset. Given the line and column of the Location
    ///     returns the Location's character offset in the Source, and a bool indicating whether the
    ///     Location was found.
    /// </summary>
    int LocationOffset(ILocation location);

    /// <summary>
    ///     OffsetLocation translates a character offset to a Location, or false if the conversion was not
    ///     feasible.
    /// </summary>
    ILocation OffsetLocation(int offset);

    /// <summary>
    ///     NewLocation takes an input line and column and produces a Location. The default behavior is to
    ///     treat the line and column as absolute, but concrete derivations may use this method to convert
    ///     a relative line and column position into an absolute location.
    /// </summary>
    ILocation NewLocation(int line, int col);

    /// <summary>
    ///     Snippet returns a line of content and whether the line was found.
    /// </summary>
    string? Snippet(int line);
}

internal sealed class SourceImpl : ISource
{
    private readonly string content;

    private readonly string description;
    private readonly IDictionary<long, int> idOffsets;

    private readonly IList<int> lineOffsets;

    internal SourceImpl(string content, string description, IList<int> lineOffsets) : this(content, description,
        lineOffsets, new Dictionary<long, int>())
    {
    }

    internal SourceImpl(string content, string description, IList<int> lineOffsets,
        IDictionary<long, int> idOffsets)
    {
        this.content = content;
        this.description = description;
        this.lineOffsets = lineOffsets;
        this.idOffsets = idOffsets;
    }

    public string Content()
    {
        return content;
    }

    public string Description()
    {
        return description;
    }

    public IList<int> LineOffsets()
    {
        return lineOffsets;
    }

    public int LocationOffset(ILocation location)
    {
        return findLineOffset(location.Line()) + location.Column();
    }

    public ILocation NewLocation(int line, int col)
    {
        return LocationFactory.NewLocation(line, col);
    }

    public ILocation OffsetLocation(int offset)
    {
        // findLine finds the line that contains the given character offset and
        // returns the line number and offset of the beginning of that line.
        // Note that the last line is treated as if it contains all offsets
        // beyond the end of the actual source.
        var line = 1;
        int lineOffset;
        foreach (var lo in lineOffsets)
            if (lo > offset)
                break;
            else
                line++;

        if (line == 1)
            lineOffset = 0;
        else
            lineOffset = lineOffsets[line - 2];

        return LocationFactory.NewLocation(line, offset - lineOffset);
    }

    public string? Snippet(int line)
    {
        var charStart = findLineOffset(line);
        if (charStart < 0) return null;

        var charEnd = findLineOffset(line + 1);
        if (charEnd >= 0) return content.Substring(charStart, charEnd - 1 - charStart);

        return content.Substring(charStart);
    }

    /// <summary>
    ///     findLineOffset returns the offset where the (1-indexed) line begins, or false if line doesn't
    ///     exist.
    /// </summary>
    private int findLineOffset(int line)
    {
        if (line == 1) return 0;

        if (line > 1 && line <= lineOffsets.Count) return lineOffsets[line - 2];

        return -1;
    }
}