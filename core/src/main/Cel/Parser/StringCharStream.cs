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
namespace Cel.Parser
{
	using Antlr4.Runtime;

	public sealed class StringCharStream : ICharStream
	{

	  private readonly string buf;
	  private readonly string src;
	  private int pos;

	  public StringCharStream(string buf, string src)
	  {
		this.buf = buf;
		this.src = src;
	  }

	  public void Consume()
	  {
		if (pos >= buf.Length)
		{
		  throw new Exception("cannot consume EOF");
		}
		pos++;
	  }

	  public int LA(int offset)
	  {
		if (offset == 0)
		{
		  return 0;
		}
		if (offset < 0)
		{
		  offset++;
		}
		pos = pos + offset - 1;
		if (pos < 0 || pos >= buf.Length)
		{
			return -1;
		}
		return buf[pos];
	  }

	  public int Mark()
	  {
		return -1;
	  }

	  public void Release(int marker)
	  {
	  }

	  public int Index
	  {
		  get { return pos;  }
	  }

	  public void Seek(int index)
	  {
		if (index <= pos)
		{
		  pos = index;
		  return;
		}
		pos = Math.Min(index, buf.Length);
	  }

	  public int Size
	  {
		  get { return buf.Length; }
	  }

	  public string SourceName
	  {
		  get
		  {
			return src;
		  }
	  }

	  public string GetText(Antlr4.Runtime.Misc.Interval interval)
	  {
		int start = interval.a;
		int stop = interval.b;
		if (stop >= buf.Length)
		{
		  stop = buf.Length - 1;
		}
		if (start >= buf.Length)
		{
		  return "";
		}
		return buf.Substring(start, (stop + 1) - start);
	  }

	  public override string ToString()
	  {
		return buf;
	  }
	}

}