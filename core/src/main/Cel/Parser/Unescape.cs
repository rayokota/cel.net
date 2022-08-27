﻿using System;
using System.Text;

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
    public sealed class Unescape
    {
        /// <summary>
        /// Unescape takes a quoted string, unquotes, and unescapes it.
        /// 
        /// <para>This function performs escaping compatible with GoogleSQL.
        /// </para>
        /// </summary>
        public static MemoryStream DoUnescape(string value, bool isBytes)
        {
            // All strings normalize newlines to the \n representation.
            value = value.Replace("\r\n", "\n").Replace("\r", "\n");
            int n = value.Length;

            // Nothing to unescape / decode.
            if (n < 2)
            {
                return WrapBlindly(value); // fmt.Errorf("unable to unescape string")
            }

            // Raw string preceded by the 'r|R' prefix.
            bool isRawLiteral = false;
            if (value[0] == 'r' || value[0] == 'R')
            {
                value = value.Substring(1);
                n = value.Length;
                isRawLiteral = true;
            }

            // Quoted string of some form, must have same first and last char.
            if (value[0] != value[n - 1] || (value[0] != '"' && value[0] != '\''))
            {
                return WrapBlindly(value); // fmt.Errorf("unable to unescape string")
            }

            // Normalize the multi-line CEL string representation to a standard
            // Go quoted string.
            // TODO remove the substring()s here (and update i + n accordingly)
            if (n >= 6)
            {
                if (value.StartsWith("'''", StringComparison.Ordinal))
                {
                    if (!value.EndsWith("'''", StringComparison.Ordinal))
                    {
                        return WrapBlindly(value); // fmt.Errorf("unable to unescape string")
                    }

                    value = "\"" + value.Substring(3, (n - 3) - 3) + "\"";
                    n = value.Length;
                }
                else if (value.StartsWith("\"\"\"", StringComparison.Ordinal))
                {
                    if (!value.EndsWith("\"\"\"", StringComparison.Ordinal))
                    {
                        return WrapBlindly(value); // fmt.Errorf("unable to unescape string")
                    }

                    value = "\"" + value.Substring(3, (n - 3) - 3) + "\"";
                    n = value.Length;
                }
            }

            value = value.Substring(1, (n - 1) - 1);
            n = n - 2;
            // If there is nothing to escape, then return.
            if (isRawLiteral || value.IndexOf('\\') == -1)
            {
                return WrapBlindly(value);
            }

            // Otherwise the string contains escape characters.
            MemoryStream buf = new MemoryStream(value.Length * 3 / 2);
            for (int i = 0; i < n; i++)
            {
                char c = value[i];
                if (c == '\\')
                {
                    // \ escape sequence
                    i++;
                    if (i == n)
                    {
                        throw new System.ArgumentException("unable to unescape string, found '\\' as last character");
                    }

                    c = value[i];

                    switch (c)
                    {
                        case 'a':
                            buf.WriteByte(Convert.ToByte(7));
                            break; // BEL
                        case 'b':
                            buf.WriteByte(Convert.ToByte('\b'));
                            break;
                        case 'f':
                            buf.WriteByte(Convert.ToByte('\f'));
                            break;
                        case 'n':
                            buf.WriteByte(Convert.ToByte('\n'));
                            break;
                        case 'r':
                            buf.WriteByte(Convert.ToByte('\r'));
                            break;
                        case 't':
                            buf.WriteByte(Convert.ToByte('\t'));
                            break;
                        case 'v':
                            buf.WriteByte(Convert.ToByte(11));
                            break; // VT
                        case '\\':
                            buf.WriteByte(Convert.ToByte('\\'));
                            break;
                        case '\'':
                            buf.WriteByte(Convert.ToByte('\''));
                            break;
                        case '"':
                            buf.WriteByte(Convert.ToByte('\"'));
                            break;
                        case '`':
                            buf.WriteByte(Convert.ToByte('`'));
                            break;
                        case '?':
                            buf.WriteByte(Convert.ToByte('?'));
                            break;

                        // 4. Unicode escape sequences, reproduced from `strconv/quote.go`
                        case 'x':
                        case 'X':
                        case 'u':
                        case 'U':
                            int nHex = 0;
                            switch (c)
                            {
                                case 'x':
                                case 'X':
                                    nHex = 2;
                                    break;
                                case 'u':
                                    nHex = 4;
                                    if (isBytes)
                                    {
                                        throw UnableToUnescapeString();
                                    }

                                    break;
                                case 'U':
                                    nHex = 8;
                                    if (isBytes)
                                    {
                                        throw UnableToUnescapeString();
                                    }

                                    break;
                            }

                            if (n - nHex < i)
                            {
                                throw UnableToUnescapeString();
                            }

                            int v = 0;
                            for (int j = 0; j < nHex; j++)
                            {
                                i++;
                                c = value[i];
                                int nib = Unhex(c);
                                if (nib == -1)
                                {
                                    throw UnableToUnescapeString();
                                }

                                v = (v << 4) | nib;
                            }

                            if (!isBytes)
                            {
                                EncodeCodePoint(buf, v, Encoding.UTF8);
                            }
                            else
                            {
                                buf.WriteByte(Convert.ToByte(v));
                            }

                            break;

                        // 5. Octal escape sequences, must be three digits \[0-3][0-7][0-7]
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                            if (n - 3 < i)
                            {
                                throw UnableToUnescapeOctalSequence();
                            }

                            v = (c - '0');
                            for (int j = 0; j < 2; j++)
                            {
                                i++;
                                c = value[i];
                                if (c < '0' || c > '7')
                                {
                                    throw UnableToUnescapeOctalSequence();
                                }

                                v = (v << 3) | (c - '0');
                            }

                            if (!isBytes)
                            {
                                EncodeCodePoint(buf, v, Encoding.UTF8);
                            }
                            else
                            {
                                buf.WriteByte(Convert.ToByte(v));
                            }

                            break;

                        // Unknown escape sequence.
                        default:
                            throw UnableToUnescapeString();
                    }
                }
                else
                {
                    // not an escape sequence
                    if (!isBytes)
                    {
                        EncodeCodePoint(buf, c, Encoding.UTF8);
                    }
                    else
                    {
                        buf.WriteByte(Convert.ToByte(c));
                    }
                }
            }

            return buf;
        }

        private static MemoryStream WrapBlindly(string value)
        {
            Encoding encoding = Encoding.UTF8;
            return new MemoryStream(encoding.GetBytes(value));
        }

        private static System.ArgumentException UnableToUnescapeOctalSequence()
        {
            return new System.ArgumentException("unable to unescape octal sequence in string");
        }

        private static System.ArgumentException UnableToUnescapeString()
        {
            return new System.ArgumentException("unable to unescape string");
        }

        private static void EncodeCodePoint(MemoryStream buf, int v, Encoding enc)
        {
            String s = Char.ConvertFromUtf32(v);
            byte[] bytes = enc.GetBytes(s);
            buf.Write(bytes, 0, bytes.Length);
        }

        internal static int Unhex(char b)
        {
            if (b >= '0' && b <= '9')
            {
                return b - '0';
            }
            else if (b >= 'a' && b <= 'f')
            {
                return b - 'a' + 10;
            }
            else if (b >= 'A' && b <= 'F')
            {
                return b - 'A' + 10;
            }

            return -1;
        }

        public static string ToUtf8(MemoryStream buf)
        {
            StreamReader reader = new StreamReader(buf, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}