using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (C) 2022 The Authors of CEL-Java
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
namespace Cel.extension
{
	using EnvOption = global::Cel.EnvOption;
	using Library = global::Cel.Library;
	using ProgramOption = global::Cel.ProgramOption;
	using Decls = global::Cel.checker.Decls;
	using Err = global::Cel.common.types.Err;
	using Overload = global::Cel.interpreter.functions.Overload;

	/// <summary>
	/// StringsLib provides a <seealso cref="Cel.EnvOption"/> to configure extended functions for
	/// string manipulation. As a general note, all indices are zero-based. The implementation is ported
	/// from <a href=https://github.com/google/cel-go/blob/master/ext/strings.go>cel-go</a>.
	/// 
	/// <para>Note: Currently the overloading isn't supported.
	/// 
	/// <h3>CharAt</h3>
	/// 
	/// </para>
	/// <para>Returns the character at the given position. If the position is negative, or greater than the
	/// length of the string, the function will produce an error:
	/// 
	/// <pre>    {@code <string>.charAt(<int>) -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'hello'.charAt(4)  // return 'o'}</pre>
	/// 
	/// <pre>    {@code 'hello'.charAt(5)  // return ''}</pre>
	/// 
	/// <pre>    {@code 'hello'.charAt(-1) // error}</pre>
	/// 
	/// <h3>IndexOf</h3>
	/// 
	/// </para>
	/// <para>Returns the integer index of the first occurrence of the search string. If the search string
	/// is not found the function returns -1.
	/// 
	/// </para>
	/// <para>The function also accepts an optional position from which to begin the substring search. If
	/// the substring is the empty string, the index where the search starts is returned (zero or
	/// custom).
	/// 
	/// <pre>    {@code <string>.indexOf(<string>) -> <int>}</pre>
	/// 
	/// <pre>    {@code <string>.indexOf(<string>, <int>) -> <int>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('')         // returns 0}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('ello')     // returns 1}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('jello')    // returns -1}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('', 2)      // returns 2}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('ello', 2)  // returns 7}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.indexOf('ello', 20) // error}</pre>
	/// 
	/// <h3>Join</h3>
	/// 
	/// </para>
	/// <para>Returns a new string where the elements of string list are concatenated.
	/// 
	/// </para>
	/// <para>The function also accepts an optional separator which is placed between elements in the
	/// resulting string.
	/// 
	/// <pre>    {@code <list<string>>.join() -> <string>}</pre>
	/// 
	/// <pre>    {@code <list<string>>.join(<string>) -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code ['hello', 'mellow'].join()    // returns 'hellomellow'}</pre>
	/// 
	/// <pre>    {@code ['hello', 'mellow'].join(' ') // returns 'hello mellow'}</pre>
	/// 
	/// <pre>    {@code [].join()                     // returns ''}</pre>
	/// 
	/// <pre>    {@code [].join('/')                  // returns ''}</pre>
	/// 
	/// <h3>LastIndexOf</h3>
	/// 
	/// </para>
	/// <para>Returns the integer index at the start of the last occurrence of the search string. If the
	/// search string is not found the function returns -1.
	/// 
	/// </para>
	/// <para>The function also accepts an optional position which represents the last index to be
	/// considered as the beginning of the substring match. If the substring is the empty string, the
	/// index where the search starts is returned (string length or custom).
	/// 
	/// <pre>    {@code <string>.lastIndexOf(<string>) -> <int>}</pre>
	/// 
	/// <pre>    {@code <string>.lastIndexOf(<string>, <int>) -> <int>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'hello mellow'.lastIndexOf('')         // returns 12}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.lastIndexOf('ello')     // returns 7}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.lastIndexOf('jello')    // returns -1}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.lastIndexOf('ello', 6)  // returns 1}</pre>
	/// 
	/// <pre>    {@code 'hello mellow'.lastIndexOf('ello', -1) // error}</pre>
	/// 
	/// <h3>LowerAscii</h4>
	/// 
	/// </para>
	/// <para>Returns a new string where all ASCII characters are lower-cased.
	/// 
	/// </para>
	/// <para>This function does not perform Unicode case-mapping for characters outside the ASCII range.
	/// 
	/// <pre>    {@code <string>.lowerAscii() -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'TacoCat'.lowerAscii()     // returns 'tacocat'}</pre>
	/// 
	/// <pre>    {@code 'TacoCÆt Xii'.lowerAscii() // returns 'tacocÆt xii'}</pre>
	/// 
	/// <h3>Replace</h3>
	/// 
	/// </para>
	/// <para>Returns a new string based on the target, which replaces the occurrences of a search string
	/// with a replacement string if present. The function accepts an optional limit on the number of
	/// substring replacements to be made.
	/// 
	/// </para>
	/// <para>When the replacement limit is 0, the result is the original string. When the limit is a
	/// negative number, the function behaves the same as replace all.
	/// 
	/// <pre>    {@code <string>.replace(<string>, <string>) -> <string>}</pre>
	/// 
	/// <pre>    {@code <string>.replace(<string>, <string>, <int>) -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'hello hello'.replace('he', 'we')     // returns 'wello wello'}</pre>
	/// 
	/// <pre>    {@code 'hello hello'.replace('he', 'we', -1) // returns 'wello wello'}</pre>
	/// 
	/// <pre>    {@code 'hello hello'.replace('he', 'we', 1)  // returns 'wello hello'}</pre>
	/// 
	/// <pre>    {@code 'hello hello'.replace('he', 'we', 0)  // returns 'hello hello'}</pre>
	/// 
	/// <h3>Split</h3>
	/// 
	/// </para>
	/// <para>Returns a list of strings split from the input by the given separator. The function accepts an
	/// optional argument specifying a limit on the number of substrings produced by the split.
	/// 
	/// </para>
	/// <para>When the split limit is 0, the result is an empty list. When the limit is 1, the result is the
	/// target string to split. When the limit is a negative number, the function behaves the same as
	/// split all.
	/// 
	/// <pre>    {@code <string>.split(<string>) -> <list<string>>}</pre>
	/// 
	/// <pre>    {@code <string>.split(<string>, <int>) -> <list<string>>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'hello hello hello'.split(' ')     // returns ['hello', 'hello', 'hello']}</pre>
	/// 
	/// <pre>    {@code 'hello hello hello'.split(' ', 0)  // returns []}</pre>
	/// 
	/// <pre>    {@code 'hello hello hello'.split(' ', 1)  // returns ['hello hello hello']}</pre>
	/// 
	/// <pre>    {@code 'hello hello hello'.split(' ', 2)  // returns ['hello', 'hello hello']}</pre>
	/// 
	/// <pre>    {@code 'hello hello hello'.split(' ', -1) // returns ['hello', 'hello', 'hello']}</pre>
	/// 
	/// <h3>Substring</h3>
	/// 
	/// </para>
	/// <para>Returns the substring given a numeric range corresponding to character positions. Optionally
	/// may omit the trailing range for a substring from a given character position until the end of a
	/// string.
	/// 
	/// </para>
	/// <para>Character offsets are 0-based with an inclusive start range. It is an error to specify an end
	/// range that is lower than the start range, or for either the start or end index to be negative or
	/// exceed the string length.
	/// 
	/// <pre>    {@code <string>.substring(<int>) -> <string>}</pre>
	/// 
	/// <pre>    {@code <string>.substring(<int>,<int>)-><string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'tacocat'.substring(4)    // returns 'cat'}</pre>
	/// 
	/// <pre>    {@code 'tacocat'.substring(-1)   // error}</pre>
	/// 
	/// <pre>    {@code 'tacocat'.substring(0,4)  // returns 'taco'}</pre>
	/// 
	/// <pre>    {@code 'tacocat'.substring(2, 1) // error}</pre>
	/// 
	/// <h3>Trim</h3>
	/// 
	/// </para>
	/// <para>Returns a new string which removes the leading and trailing whitespace in the target string.
	/// The trim function uses the Unicode definition of whitespace which does not include the zero-width
	/// spaces. See: <a href="https://en.wikipedia.org/wiki/Whitespace_character#Unicode">Unicode</a>
	/// 
	/// <pre>    {@code <string>.trim() -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code ' \ttrim\n '.trim() // returns 'trim'}</pre>
	/// 
	/// <h3>UpperAscii</h3>
	/// 
	/// </para>
	/// <para>Returns a new string where all ASCII characters are upper-cased.
	/// 
	/// </para>
	/// <para>This function does not perform Unicode case-mapping for characters outside the ASCII range.
	/// 
	/// <pre>    {@code <string>.upperAscii() -> <string>}</pre>
	/// 
	/// <h4>Examples:</h4>
	/// 
	/// <pre>    {@code 'TacoCat'.upperAscii()     // returns 'TACOCAT'}</pre>
	/// 
	/// <pre>    {@code 'TacoCÆt Xii'.upperAscii() // returns 'TACOCÆT XII'}</pre>
	/// </para>
	/// </summary>
	public class StringsLib : Library
	{

	  private const string CHAR_AT = "charAt";
	  private const string INDEX_OF = "indexOf";
	  private const string JOIN = "join";
	  private const string LAST_INDEX_OF = "lastIndexOf";
	  private const string LOWER_ASCII = "lowerAscii";
	  private const string REPLACE = "replace";
	  private const string SPLIT = "split";
	  private const string SUBSTR = "substring";
	  private const string TRIM_SPACE = "trim";
	  private const string UPPER_ASCII = "upperAscii";

	  // whitespace characters definition from
	  // https://en.wikipedia.org/wiki/Whitespace_character#Unicode
	  private static readonly ISet<char> UNICODE_WHITE_SPACES = new HashSet<char>(java.util.Arrays.asList((char) 0x0009, (char) 0x000A, (char) 0x000B, (char) 0x000C, (char) 0x000D, (char) 0x0020, (char) 0x0085, (char) 0x00A0, (char) 0x1680, (char) 0x2000, (char) 0x2001, (char) 0x2002, (char) 0x2003, (char) 0x2004, (char) 0x2005, (char) 0x2006, (char) 0x2007, (char) 0x2008, (char) 0x2009, (char) 0x200A, (char) 0x2028, (char) 0x2029, (char) 0x202F, (char) 0x205F, (char) 0x3000));

	  public static EnvOption Strings()
	  {
		return Library.Lib(new StringsLib());
	  }

	  public virtual IList<EnvOption> CompileOptions
	  {
		  get
		  {
			IList<EnvOption> list = new List<EnvOption>();
			EnvOption option = EnvOption.declarations(Decls.NewFunction(CHAR_AT, Decls.NewInstanceOverload("string_char_at_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.Int}, Decls.String)), Decls.NewFunction(INDEX_OF, Decls.NewInstanceOverload("string_index_of_string", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String}, Decls.Int), Decls.NewInstanceOverload("string_index_of_string_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String, Decls.Int}, Decls.Int)), Decls.NewFunction(JOIN, Decls.NewInstanceOverload("list_join", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.NewListType(Decls.String)}, Decls.String), Decls.NewInstanceOverload("list_join_string", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.NewListType(Decls.String), Decls.String}, Decls.String)), Decls.NewFunction(LAST_INDEX_OF, Decls.NewInstanceOverload("string_last_index_of_string", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String}, Decls.Int), Decls.NewInstanceOverload("string_last_index_of_string_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String, Decls.Int}, Decls.Int)), Decls.NewFunction(LOWER_ASCII, Decls.NewInstanceOverload("string_lower_ascii", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String}, Decls.String)), Decls.NewFunction(REPLACE, Decls.NewInstanceOverload("string_replace_string_string", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String, Decls.String}, Decls.String), Decls.NewInstanceOverload("string_replace_string_string_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String, Decls.String, Decls.Int}, Decls.String)), Decls.NewFunction(SPLIT, Decls.NewInstanceOverload("string_split_string", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String}, Decls.Dyn), Decls.NewInstanceOverload("string_split_string_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.String, Decls.Int}, Decls.Dyn)), Decls.NewFunction(SUBSTR, Decls.NewInstanceOverload("string_substring_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.Int}, Decls.String), Decls.NewInstanceOverload("string_substring_int_int", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String, Decls.Int, Decls.Int}, Decls.String)), Decls.NewFunction(TRIM_SPACE, Decls.NewInstanceOverload("string_trim", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String}, Decls.String)), Decls.NewFunction(UPPER_ASCII, Decls.NewInstanceOverload("string_upper_ascii", new List<Google.Api.Expr.V1Alpha1.Type> {Decls.String}, Decls.String)));
			list.Add(option);
			return list;
		  }
	  }

	  public virtual IList<ProgramOption> ProgramOptions
	  {
		  get
		  {
			IList<ProgramOption> list = new List<ProgramOption>();
			ProgramOption functions = ProgramOption.functions(Overload.Binary(CHAR_AT, Guards.CallInStrIntOutStr(StringsLib.charAt)), Overload.overload(INDEX_OF, null, null, Guards.CallInStrStrOutInt(StringsLib.indexOf), Guards.CallInStrStrIntOutInt(StringsLib.indexOfOffset)), Overload.overload(JOIN, null, Guards.CallInStrArrayOutStr(StringsLib.join), Guards.CallInStrArrayStrOutStr(StringsLib.joinSepartor), null), Overload.overload(LAST_INDEX_OF, null, null, Guards.CallInStrStrOutInt(StringsLib.lastIndexOf), Guards.CallInStrStrIntOutInt(StringsLib.lastIndexOfOffset)), Overload.Unary(LOWER_ASCII, Guards.CallInStrOutStr(StringsLib.lowerASCII)), Overload.overload(REPLACE, null, null, null, values =>
			{
			if (values.length == 3)
			{
				return Guards.CallInStrStrStrOutStr(StringsLib.replace).Invoke(values);
			}
			if (values.length == 4)
			{
				return Guards.CallInStrStrStrIntOutStr(StringsLib.replaceN).Invoke(values);
			}
			return Err.MaybeNoSuchOverloadErr(null);
			}), Overload.overload(SPLIT, null, null, Guards.CallInStrStrOutStrArr(StringsLib.split), Guards.CallInStrStrIntOutStrArr(StringsLib.splitN)), Overload.overload(SUBSTR, null, null, Guards.CallInStrIntOutStr(StringsLib.substr), Guards.CallInStrIntIntOutStr(StringsLib.substrRange)), Overload.unary(TRIM_SPACE, Guards.CallInStrOutStr(StringsLib.trimSpace)), Overload.unary(UPPER_ASCII, Guards.CallInStrOutStr(StringsLib.upperASCII)));
			list.Add(functions);
			return list;
		  }
	  }

	  internal static string CharAt(string str, int index)
	  {
		if (str.Length == index)
		{
		  return "";
		}
		return str[index].ToString();
	  }

	  internal static int IndexOf(string str, string substr)
	  {
		return str.IndexOf(substr, StringComparison.Ordinal);
	  }

	  internal static int IndexOfOffset(string str, string substr, int offset)
	  {
		if (offset < 0 || offset > str.Length)
		{
		  throw new System.IndexOutOfRangeException("String index out of range: " + offset);
		}
		return str.IndexOf(substr, offset, StringComparison.Ordinal);
	  }

	  internal static string Join(string[] strs)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		strs.ForEach(stringBuilder.append);
		return stringBuilder.ToString();
	  }

	  internal static string JoinSepartor(string[] strs, string seperator)
	  {
		return String.join(seperator, strs);
	  }

	  internal static int LastIndexOf(string str, string substr)
	  {
		return str.LastIndexOf(substr, StringComparison.Ordinal);
	  }

	  internal static int LastIndexOfOffset(string str, string substr, int offset)
	  {
		if (offset < 0 || offset > str.Length)
		{
		  throw new System.IndexOutOfRangeException("String index out of range: " + offset);
		}
		return str.LastIndexOf(substr, offset, StringComparison.Ordinal);
	  }

	  internal static string LowerASCII(string str)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in str.ToCharArray())
		{
		  if (c >= 'A' && c <= 'Z')
		  {
			stringBuilder.Append(char.ToLower(c));
		  }
		  else
		  {
			stringBuilder.Append(c);
		  }
		}
		return stringBuilder.ToString();
	  }

	  internal static string Replace(string str, string old, string replacement)
	  {
		return str.Replace(old, replacement);
	  }

	  /// <summary>
	  /// replace first n non-overlapping instance of {old} replaced by {replacement}. It works as <a
	  /// ref="https://pkg.go.dev/strings#Replace">strings.Replace in Go</a> to have consistent behavior
	  /// as cel in Go
	  /// 
	  /// <para>if {@code n == 0}, there is no change to the string
	  /// 
	  /// </para>
	  /// <para>if {@code n < 0}, there is no limit on the number of replacement
	  /// 
	  /// </para>
	  /// <para>if {old} is empty, it matches at the beginning of the string and after each UTF-8 sequence,
	  /// yielding up to k+1 replacements for a k-rune string
	  /// </para>
	  /// </summary>
	  internal static string ReplaceN(string str, string old, string replacement, int n)
	  {
		if (n == 0 || old.Equals(replacement))
		{
		  return str;
		}
		if (n < 0)
		{
		  return str.Replace(old, replacement);
		}
		StringBuilder stringBuilder = new StringBuilder();
		int index = 0;
		int count = 0;

		for (; count < n && index < str.Length; count++)
		{
		  if (old.Length == 0)
		  {
			stringBuilder.Append(replacement).Append(str, index, index + 1);
			index++;
		  }
		  else
		  {
			int found = str.IndexOf(old, index, StringComparison.Ordinal);
			if (found == -1)
			{
			  // not found, append to the end
			  stringBuilder.Append(str, index, str.Length);
			  return stringBuilder.ToString();
			}
			if (found > index)
			{
			  stringBuilder.Append(str, index, found);
			}
			stringBuilder.Append(replacement);
			index = found + old.Length;
		  }
		}
		if (index < str.Length)
		{
		  stringBuilder.Append(str, index, str.Length);
		}
		return stringBuilder.ToString();
	  }

	  internal static string[] Split(string str, string separator)
	  {
		return str.Split(Pattern.quote(separator), true);
	  }

	  /// <summary>
	  /// SplitN slices s into substrings separated by sep and returns an array of the substrings between
	  /// those separators. The count determines the number of substrings to return:
	  /// 
	  /// <para>If {@code n > 0}, at most n substrings; the last substring will be the unsplit remainder.
	  /// 
	  /// </para>
	  /// <para>If {@code n == 0}, the result is empty array
	  /// 
	  /// </para>
	  /// <para>If {@code n < 0}, all substrings
	  /// 
	  /// </para>
	  /// <para>If sep is empty, splits after each UTF-8 sequence.
	  /// 
	  /// </para>
	  /// <para>If both s and sep are empty, Split returns an empty array.
	  /// </para>
	  /// </summary>
	  internal static string[] SplitN(string s, string sep, int n)
	  {
		if (n < 0)
		{
		  return Split(s, sep);
		}
		if (n == 0)
		{
		  return new string[0];
		}
		if (n == 1)
		{
		  return new string[] {s};
		}
		if (sep.Length == 0)
		{
		  return Explode(s, n);
		}

		int index = 0;
		int count = 0;
		IList<string> list = new List<string>();
		for (; index < s.Length && count < n - 1; count++)
		{
		  int found = s.IndexOf(sep, index, StringComparison.Ordinal);
		  if (found < 0)
		  {
			break;
		  }
		  list.Add(s.Substring(index, found - index));
		  index = found + sep.Length;
		}
		if (index <= s.Length)
		{
		  list.Add(s.Substring(index));
		}

		return ((List<string>)list).ToArray();
	  }

	  /// <summary>
	  /// explode splits s into an array of UTF-8 strings, one string per Unicode character up to a
	  /// maximum of n (n < 0 means no limit).
	  /// 
	  /// <para>ported from <a href="https://github.com/golang/go/blob/master/src/strings/strings.go">Go:
	  /// strings.explode()</a>
	  /// </para>
	  /// </summary>
	  private static string[] Explode(string s, int n)
	  {
		if (n < 0 || n > s.Length)
		{
		  n = s.Length;
		}

		string[] arr = new string[n];
		for (int i = 0; i < n - 1; i++)
		{
		  arr[i] = s.Substring(i, 1);
		}
		if (n > 0)
		{
		  arr[n - 1] = s.Substring(n - 1);
		}
		return arr;
	  }

	  internal static string Substr(string str, int start)
	  {
		return str.Substring(start);
	  }

	  internal static string SubstrRange(string str, int start, int end)
	  {
		if (start < 0 || start > str.Length)
		{
		  throw new System.IndexOutOfRangeException("String index out of range: " + start);
		}
		if (end < 0 || end > str.Length)
		{
		  throw new System.IndexOutOfRangeException("String index out of range: " + end);
		}
		if (start > end)
		{
		  throw new System.IndexOutOfRangeException(string.Format("invalid substring range. start: {0:D}, end: {1:D}", start, end));
		}
		return str.Substring(start, end - start);
	  }

	  internal static string TrimSpace(string str)
	  {
		char[] chars = str.ToCharArray();
		int start = 0;
		int end = str.Length - 1;
		while (start < str.Length)
		{
		  if (!IsWhiteSpace(chars[start]))
		  {
			break;
		  }
		  start++;
		}
		while (end > start)
		{
		  if (!IsWhiteSpace(chars[end]))
		  {
			break;
		  }
		  end--;
		}

		return str.Substring(start, (end + 1) - start);
	  }

	  /// <summary>
	  /// test if given character is whitespace as defined by <a
	  /// href="https://en.wikipedia.org/wiki/Whitespace_character#Unicode">Unicode</a>
	  /// 
	  /// <para>Java functions like <seealso cref="java.lang.Character.isWhitespace(char)"/> or {@link
	  /// java.lang.Character#isWhitespace(int)} use different whitespace definition hence they can't be
	  /// used here.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="ch"> the character to be tested </param>
	  /// <returns> true if the character is a Unicode whitespace character; false otherwise. </returns>
	  private static bool IsWhiteSpace(char ch)
	  {
		// cel-go 'trim' extension function uses strings.TrimSpace()
		return UNICODE_WHITE_SPACES.Contains(ch);
	  }

	  internal static string UpperASCII(string str)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in str.ToCharArray())
		{
		  if (c >= 'a' && c <= 'z')
		  {
			stringBuilder.Append(char.ToUpper(c));
		  }
		  else
		  {
			stringBuilder.Append(c);
		  }
		}
		return stringBuilder.ToString();
	  }
	}

}