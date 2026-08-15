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

using System.Globalization;
using NUnit.Framework;

namespace Cel.Common.Types;

public class ConversionCultureTest
{
    /// <summary>
    ///     Under a culture whose decimal separator is ',', string&lt;-&gt;number conversions must
    ///     still use '.' and reject grouped/whitespace forms, matching cel-go's locale-independent
    ///     strconv functions. Guards against a regression to culture-dependent parsing/formatting.
    /// </summary>
    [Test]
    public virtual void ConversionsAreCultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // A dotted decimal is parsed as one-and-a-half regardless of the process culture.
            Assert.That(StringT.StringOf("1.5").ConvertToType(DoubleT.DoubleType).Equal(DoubleT.DoubleOf(1.5)),
                Is.SameAs(BoolT.True));
            // A comma decimal (the de-DE separator) is not accepted.
            Assert.That(StringT.StringOf("1,5").ConvertToType(DoubleT.DoubleType), Is.InstanceOf(typeof(Err)));
            // Group separators and surrounding whitespace are rejected.
            Assert.That(StringT.StringOf("1,000").ConvertToType(IntT.IntType), Is.InstanceOf(typeof(Err)));
            Assert.That(StringT.StringOf(" 12 ").ConvertToType(IntT.IntType), Is.InstanceOf(typeof(Err)));
            // double -> string uses '.' rather than the culture's ',' separator.
            Assert.That(StringT.StringOf("-4.5").Equal(DoubleT.DoubleOf(-4.5).ConvertToType(StringT.StringType)),
                Is.SameAs(BoolT.True));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
