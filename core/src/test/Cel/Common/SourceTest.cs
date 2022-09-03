using System.Collections.Generic;
using NUnit.Framework;

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
namespace Cel.Common
{
    internal class SourceTest
    {
        internal const string unexpectedSnippet = "got snippet '%s', want '%s'";
        internal const string snippetFound = "snippet found at line %d, wanted none";

        /// <summary>
        /// the error description method. </summary>
        [Test]
        public virtual void Description()
        {
            string contents = "example content\nsecond line";

            Source source = Source.NewStringSource(contents, "description-test");

            Assert.That(source.Content(), Is.EqualTo(contents));
            Assert.That(source.Description(), Is.EqualTo("description-test"));

            // Assert that the snippets on lines 1 & 2 are what was expected.
            Assert.That(source.Snippet(2), Is.EqualTo("second line"));
            Assert.That(source.Snippet(1), Is.EqualTo("example content"));
        }

        /// <summary>
        /// make sure that the offsets accurately reflect the location of a character in source. </summary>
        [Test]
        public virtual void EmptyContents()
        {
            Source source = Source.NewStringSource("", "empty-test");

            Assert.That(source.Snippet(1), Is.EqualTo(""));

            string str2 = source.Snippet(2);
            Assert.That(str2, Is.Null);
        }

        /// <summary>
        /// snippets from a single line source. </summary>
        [Test]
        public virtual void SnippetSingleline()
        {
            Source source = Source.NewStringSource("hello, world", "one-line-test");

            Assert.That(source.Snippet(1), Is.EqualTo("hello, world"));

            string str2 = source.Snippet(2);
            Assert.That(str2, Is.Null);
        }

        /// <summary>
        /// snippets of text from a multiline source. </summary>
        [Test]
        public virtual void SnippetMultiline()
        {
            IList<string> testLines = new List<string> { "", "", "hello", "world", "", "my", "bub", "", "" };

            Source source = Source.NewStringSource(string.Join("\n", testLines), "mulit-line-test");

            Assert.That(source.Snippet(testLines.Count + 1), Is.Null);
            Assert.That(source.Snippet(0), Is.Null);

            for (int i = 1; i <= testLines.Count; i++)
            {
                string testLine = testLines[i - 1];

                string str = source.Snippet(i);
                Assert.That(str, Is.EqualTo(testLine));
            }
        }

        /// <summary>
        /// make sure that the offsets accurately reflect the location of a character in source. </summary>
        [Test]
        public virtual void LocationOffset()
        {
            string contents = "c.d &&\n\t b.c.arg(10) &&\n\t test(10)";
            Source source = Source.NewStringSource(contents, "offset-test");
            Assert.That(source.LineOffsets(), Is.EquivalentTo(new List<int>{ 7, 24, 35}));

            // Ensure that selecting a set of characters across multiple lines works as
            // expected.
            int charStart = source.LocationOffset(Location.NewLocation(1, 2));
            int charEnd = source.LocationOffset(Location.NewLocation(3, 2));
            Assert.That(contents.Substring(charStart, charEnd - charStart), Is.EqualTo("d &&\n\t b.c.arg(10) &&\n\t "));
            Assert.That(source.LocationOffset(Location.NewLocation(4, 0)), Is.EqualTo(-1));
        }

        /// <summary>
        /// Ensure there is no panic when passing nil, NewInfoSource should use proto v2 style accessors.
        /// </summary>
        [Test]
        public virtual void NoPanicOnNil()
        {
            // Not implemented - there's no 'nil' in Java
            //  _ = NewInfoSource(nil)
        }
    }
}