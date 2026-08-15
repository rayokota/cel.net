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

using Cel.Extension;
using NUnit.Framework;

namespace Cel;

/// <summary>
///     Tests for the strings extension - <see cref="StringsLib" />.
/// </summary>
public class StringsLibTest
{
    private static object EvalExpr(string expr)
    {
        var env = Env.NewEnv(StringsLib.Strings());
        var astIss = env.Compile(expr);
        Assert.That(astIss.HasIssues(), Is.False, () => "compile failed for " + expr + ": " + astIss.Issues);
        return env.Program(astIss.Ast!).Eval(new Dictionary<string, object>()).Val.Value();
    }

    // reverse reverses by Unicode code point, matching cel-go: surrogate pairs stay intact.
    [TestCase("'gums'.reverse() == 'smug'")]
    [TestCase("'John Smith'.reverse() == 'htimS nhoJ'")]
    [TestCase("''.reverse() == ''")]
    [TestCase("'a\\U0001F600b'.reverse() == 'b\\U0001F600a'")]
    public virtual void Reverse(string expr)
    {
        Assert.That(EvalExpr(expr), Is.EqualTo(true), expr);
    }
}
