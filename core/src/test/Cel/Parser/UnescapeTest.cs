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

using System.Text;
using Cel.Parser;
using NUnit.Framework;

namespace Cel.Parser
{
    internal class UnescapeTest
    {
[Test]
        public virtual void UnescapeSingleQuote()
        {
            string text = Utf8(Unescape.DoUnescape("'hello'", false));
            Assert.That(text, Is.EqualTo("hello"));
        }

[Test]
        public virtual void UnescapeDoubleQuote()
        {
            string text = Utf8(Unescape.DoUnescape("\"\"", false));
            Assert.That(text, Is.EqualTo(""));
        }

[Test]
        public virtual void UnescapeEscapedQuote()
        {
            // The argument to unescape is dquote-backslash-dquote-dquote where both
            // the backslash and inner double-quote are escaped.
            string text = Utf8(Unescape.DoUnescape("\"\\\\\\\"\"", false));
            Assert.That(text, Is.EqualTo("\\\""));
        }

[Test]
        public virtual void UnescapeEscapedEscape()
        {
            string text = Utf8(Unescape.DoUnescape("\"\\\\\"", false));
            Assert.That(text, Is.EqualTo("\\"));
        }

[Test]
        public virtual void UnescapeTripleSingleQuote()
        {
            string text = Utf8(Unescape.DoUnescape("'''x''x'''", false));
            Assert.That(text, Is.EqualTo("x''x"));
        }

[Test]
        public virtual void UnescapeTripleDoubleQuote()
        {
            string text = Utf8(Unescape.DoUnescape("\"\"\"x\"\"x\"\"\"", false));
            Assert.That(text, Is.EqualTo("x\"\"x"));
        }

[Test]
        public virtual void UnescapeMultiOctalSequence()
        {
            // Octal 303 -> Code point 195 (Ã)
            // Octal 277 -> Code point 191 (¿)
            string text = Utf8(Unescape.DoUnescape("\"\x00C3\x00BF\"", false));
            Assert.That(text, Is.EqualTo("Ã¿"));
        }

[Test]
        public virtual void UnescapeOctalSequence()
        {
            // Octal 377 -> Code point 255 (ÿ)
            string text = Utf8(Unescape.DoUnescape("\"\x00FF\"", false));
            Assert.That(text, Is.EqualTo("ÿ"));
        }

[Test]
        public virtual void UnescapeUnicodeSequence()
        {
            string text = Utf8(Unescape.DoUnescape("\"\u263A\u263A\"", false));
            Assert.That(text, Is.EqualTo("☺☺"));
        }

[Test]
        public virtual void UnescapeLegalEscapes()
        {
            string text = Utf8(Unescape.DoUnescape("\"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Legal escapes\"", false));
            Assert.That(text, Is.EqualTo("\x0007\b\f\n\r\t\x000B'\"\\? Legal escapes"));
        }

[Test]
        public virtual void UnescapeIllegalEscapes()
        {
            // The first escape sequences are legal, but the '\>' is not.
            Assert.That(() => Unescape.DoUnescape("\"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>\"", false),
                Throws.Exception.TypeOf(typeof(System.ArgumentException)));
        }

[Test]
        public virtual void UnescapeBytesAscii()
        {
            string bs = Utf8(Unescape.DoUnescape("\"abc\"", true));
            Assert.That(bs, Is.EqualTo("abc"));
        }

[Test]
        public virtual void UnescapeBytesUnicode()
        {
            byte[] bs = Bytes(Unescape.DoUnescape("\"ÿ\"", true));
            Assert.That(bs, Is.EqualTo(new byte[] { unchecked((byte)0xc3), unchecked((byte)0xbf) }));
        }

[Test]
        public virtual void UnescapeBytesOctal()
        {
            byte[] bs = Bytes(Unescape.DoUnescape("\"\\303\\277\"", true));
            Assert.That(bs, Is.EqualTo(new byte[] { unchecked((byte)0xc3), unchecked((byte)0xbf) }));
        }

[Test]
        public virtual void UnescapeBytesOctalMax()
        {
            byte[] bs = Bytes(Unescape.DoUnescape("\"\\377\"", true));
            Assert.That(bs, Is.EqualTo(new byte[] { unchecked((byte)0xff) }));
        }

[Test]
        public virtual void UnescapeBytesQuoting()
        {
            string bs = Utf8(Unescape.DoUnescape("'''\"Kim\\t\"'''", true));
            Assert.That(bs,
                Is.EqualTo(new string(new char[]
                    { (char)0x22, (char)0x4b, (char)0x69, (char)0x6d, (char)0x09, (char)0x22 })));
        }

[Test]
        public virtual void UnescapeBytesHex()
        {
            byte[] bs = Bytes(Unescape.DoUnescape("\"\\xc3\\xbf\"", true));
            Assert.That(bs, Is.EqualTo(new byte[] { unchecked((byte)0xc3), unchecked((byte)0xbf) }));
        }

[Test]
        public virtual void UnescapeBytesHexMax()
        {
            byte[] bs = Bytes(Unescape.DoUnescape("\"\\xff\"", true));
            Assert.That(bs, Is.EqualTo(new byte[] { unchecked((byte)0xff) }));
        }

[Test]
        public virtual void UnescapeBytesUnicodeEscape()
        {
            Assert.That(() => Unescape.DoUnescape("\"\\u00ff\"", true),
                Throws.Exception.TypeOf(typeof(System.ArgumentException)));
        }

        // TODO
        /*
[Test]
        public virtual void InvalidUtf8()
        {
            Assert.That(() => Unescape.ToUtf8(new MemoryStream(new byte[] { 0, unchecked((byte)255) })),
                Throws.Exception.TypeOf(typeof(Exception)));
        }
        */

        private static byte[] Bytes(MemoryStream unescape)
        {
            byte[] bytes = new byte[unescape.Length];
            unescape.Read(bytes);
            return bytes;
        }

        private static string Utf8(MemoryStream unescape)
        {
            byte[] bytes = Bytes(unescape);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}