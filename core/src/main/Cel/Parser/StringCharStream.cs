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

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Cel.Parser;

public sealed class StringCharStream : ICharStream
{
    private readonly string buf;

    public StringCharStream(string buf, string src)
    {
        this.buf = buf;
        this.SourceName = src;
    }

    public void Consume()
    {
        if (Index >= buf.Length) throw new Exception("cannot consume EOF");

        Index++;
    }

    public int LA(int offset)
    {
        if (offset == 0) return 0;

        if (offset < 0) offset++;

        Index = Index + offset - 1;
        if (Index < 0 || Index >= buf.Length) return -1;

        return buf[Index];
    }

    public int Mark()
    {
        return -1;
    }

    public void Release(int marker)
    {
    }

    public int Index { get; private set; }

    public void Seek(int index)
    {
        if (index <= Index)
        {
            Index = index;
            return;
        }

        Index = Math.Min(index, buf.Length);
    }

    public int Size => buf.Length;

    public string SourceName { get; }

    public string GetText(Interval interval)
    {
        var start = interval.a;
        var stop = interval.b;
        if (stop >= buf.Length) stop = buf.Length - 1;

        if (start >= buf.Length) return "";

        return buf.Substring(start, stop + 1 - start);
    }

    public override string ToString()
    {
        return buf;
    }
}