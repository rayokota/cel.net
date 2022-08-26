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

namespace Cel.Common
{
    using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;

    /// <summary>
    /// Source interface for filter source contents. </summary>
    public interface Source
    {
        /// <summary>
        /// NewTextSource creates a new Source from the input text string. </summary>
        static Source NewTextSource(string text)
        {
            return NewStringSource(text, "<input>");
        }

        /// <summary>
        /// NewStringSource creates a new Source from the given contents and description. </summary>
        static Source NewStringSource(string contents, string description)
        {
            // Compute line offsets up front as they are referred to frequently.
            IList<int> offsets = new List<int>();
            for (int i = 0; i <= contents.Length;)
            {
                if (i > 0)
                {
                    // don't add '0' for the first line, it's implicit
                    offsets.Add(i);
                }

                int nl = contents.IndexOf('\n', i);
                if (nl == -1)
                {
                    offsets.Add(contents.Length + 1);
                    break;
                }
                else
                {
                    i = nl + 1;
                }
            }

            return new SourceImpl(contents, description, offsets);
        }

        /// <summary>
        /// NewInfoSource creates a new Source from a SourceInfo. </summary>
        static Source NewInfoSource(SourceInfo info)
        {
            return new SourceImpl("", info.Location, info.LineOffsets, info.Positions);
        }

        /// <summary>
        /// Content returns the source content represented as a string. Examples contents are the single
        /// file contents, textbox field, or url parameter.
        /// </summary>
        string Content();

        /// <summary>
        /// Description gives a brief description of the source. Example descriptions are a file name or ui
        /// element.
        /// </summary>
        string Description();

        /// <summary>
        /// LineOffsets gives the character offsets at which lines occur. The zero-th entry should refer to
        /// the break between the first and second line, or EOF if there is only one line of source.
        /// </summary>
        IList<int> LineOffsets();

        /// <summary>
        /// LocationOffset translates a Location to an offset. Given the line and column of the Location
        /// returns the Location's character offset in the Source, and a bool indicating whether the
        /// Location was found.
        /// </summary>
        int LocationOffset(Location location);

        /// <summary>
        /// OffsetLocation translates a character offset to a Location, or false if the conversion was not
        /// feasible.
        /// </summary>
        Location OffsetLocation(int offset);

        /// <summary>
        /// NewLocation takes an input line and column and produces a Location. The default behavior is to
        /// treat the line and column as absolute, but concrete derivations may use this method to convert
        /// a relative line and column position into an absolute location.
        /// </summary>
        Location NewLocation(int line, int col);

        /// <summary>
        /// Snippet returns a line of content and whether the line was found. </summary>
        string Snippet(int line);
    }

    internal sealed class SourceImpl : Source
    {
//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly string content_Conflict;

//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly string description_Conflict;

//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly IList<int> lineOffsets_Conflict;
        private readonly IDictionary<long, int> idOffsets;

        internal SourceImpl(string content, string description, IList<int> lineOffsets) : this(content, description,
            lineOffsets, new Dictionary<long, int>())
        {
        }

        internal SourceImpl(string content, string description, IList<int> lineOffsets,
            IDictionary<long, int> idOffsets)
        {
            this.content_Conflict = content;
            this.description_Conflict = description;
            this.lineOffsets_Conflict = lineOffsets;
            this.idOffsets = idOffsets;
        }

        public string Content()
        {
            return content_Conflict;
        }

        public string Description()
        {
            return description_Conflict;
        }

        public IList<int> LineOffsets()
        {
            return lineOffsets_Conflict;
        }

        public int LocationOffset(Location location)
        {
            return findLineOffset(location.Line()) + location.Column();
        }

        public Location NewLocation(int line, int col)
        {
            return Location.NewLocation(line, col);
        }

        public Location OffsetLocation(int offset)
        {
            // findLine finds the line that contains the given character offset and
            // returns the line number and offset of the beginning of that line.
            // Note that the last line is treated as if it contains all offsets
            // beyond the end of the actual source.
            int line = 1;
            int lineOffset;
            foreach (int lo in lineOffsets_Conflict)
            {
                if (lo > offset)
                {
                    break;
                }
                else
                {
                    line++;
                }
            }

            if (line == 1)
            {
                lineOffset = 0;
            }
            else
            {
                lineOffset = lineOffsets_Conflict[line - 2];
            }

            return Location.NewLocation(line, offset - lineOffset);
        }

        public string Snippet(int line)
        {
            int charStart = findLineOffset(line);
            if (charStart < 0)
            {
                return null;
            }

            int charEnd = findLineOffset(line + 1);
            if (charEnd >= 0)
            {
                return content_Conflict.Substring(charStart, (charEnd - 1) - charStart);
            }

            return content_Conflict.Substring(charStart);
        }

        /// <summary>
        /// findLineOffset returns the offset where the (1-indexed) line begins, or false if line doesn't
        /// exist.
        /// </summary>
        private int findLineOffset(int line)
        {
            if (line == 1)
            {
                return 0;
            }

            if (line > 1 && line <= lineOffsets_Conflict.Count)
            {
                return lineOffsets_Conflict[line - 2];
            }

            return -1;
        }
    }
}