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

using Cel.Common;
using NUnit.Framework;

namespace Cel.Parser;

[TestFixture]
public class UnparserTest
{
    public static string[][] UnparseIdenticalSource()
    {
        return new[]
        {
            new[] { "call_add", "a + b - c" },
            new[] { "call_and", "a && b && c && d && e" },
            new[] { "call_and_or", "a || b && (c || d) && e" },
            new[] { "call_cond", "a ? b : c" },
            new[] { "call_index", "a[1][\"b\"]" },
            new[] { "call_index_eq", "x[\"a\"].single_int32 == 23" },
            new[] { "call_mul", "a * (b / c) % 0" },
            new[] { "call_mul_add", "a + b * c" },
            new[] { "call_mul_add_nested", "(a + b) * c / (d - e)" },
            new[] { "call_mul_nested", "a * b / c % 0" },
            new[] { "call_not", "!true" },
            new[] { "call_neg", "-num" },
            new[] { "call_or", "a || b || c || d || e" },
            new[] { "call_neg_mult", "-(1 * 2)" },
            new[] { "call_neg_add", "-(1 + 2)" },
            new[] { "calc_distr_paren", "(1 + 2) * 3" },
            new[] { "calc_distr_noparen", "1 + 2 * 3" },
            new[] { "cond_tern_simple", "(x > 5) ? (x - 5) : 0" },
            new[] { "cond_tern_neg_expr", "-((x > 5) ? (x - 5) : 0)" },
            new[] { "cond_tern_neg_term", "-x ? (x - 5) : 0" },
            new[] { "func_global", "size(a ? (b ? c : d) : e)" },
            new[] { "func_member", "a.hello(\"world\")" },
            new[] { "func_no_arg", "zero()" },
            new[] { "func_one_arg", "one(\"a\")" },
            new[] { "func_two_args", "and(d, 32u)" },
            new[] { "func_var_args", "max(a, b, 100)" },
            new[] { "func_neq", "x != \"a\"" },
            new[] { "func_in", "a in b" },
            new[] { "list_empty", "[]" },
            new[] { "list_one", "[1]" },
            new[] { "list_many", "[\"hello, world\", \"goodbye, world\", \"sure, why not?\"]" },
            new[] { "lit_bytes", "b\"\\xc3\\x83\\xc2\\xbf\"" },
            new[] { "lit_double", "-42.101" },
            new[] { "lit_false", "false" },
            new[] { "lit_int", "-405069" },
            new[] { "lit_null", "null" },
            new[] { "lit_string", "\"hello:\\t'world'\"" },
            new[] { "lit_true", "true" },
            new[] { "lit_uint", "42u" },
            new[] { "ident", "my_ident" },
            new[] { "macro_has", "has(hello.world)" },
            new[] { "map_empty", "{}" },
            new[] { "map_lit_key", "{\"a\": a.b.c, b\"b\": bytes(a.b.c)}" },
            new[] { "map_expr_key", "{a: a, b: a.b, c: a.b.c, a ? b : c: false, a || b: true}" },
            new[] { "msg_empty", "v1alpha1.Expr{}" },
            new[] { "msg_fields", "v1alpha1.Expr{id: 1, call_expr: v1alpha1.Call_Expr{function: \"name\"}}" },
            new[] { "select", "a.b.c" },
            new[] { "idx_idx_sel", "a[b][c].name" },
            new[] { "sel_expr_target", "(a + b).name" },
            new[] { "sel_cond_target", "(a ? b : c).name" },
            new[] { "idx_cond_target", "(a ? b : c)[0]" },
            new[] { "cond_conj", "(a1 && a2) ? b : c" },
            new[] { "cond_disj_conj", "a ? (b1 || b2) : (c1 && c2)" },
            new[] { "call_cond_target", "(a ? b : c).method(d)" },
            new[] { "cond_flat", "false && !true || false" },
            new[] { "cond_paren", "false && (!true || false)" },
            new[] { "cond_cond", "(false && !true || false) ? 2 : 3" },
            new[] { "cond_binop", "(x < 5) ? x : 5" },
            new[] { "cond_binop_binop", "(x > 5) ? (x - 5) : 0" },
            new[] { "cond_cond_binop", "(x > 5) ? ((x > 10) ? (x - 10) : 5) : 0" }
        };
    }

    [TestCaseSource(nameof(UnparseIdenticalSource))]
    public virtual void UnparseIdentical(string name, string @in)
    {
        var parser = new Parser(new Options.Builder().Build());

        var p = parser.Parse(ISource.NewTextSource(@in));
        if (p.HasErrors()) Assert.Fail(p.Errors.ToDisplayString());

        var @out = Unparser.Unparse(p.Expr, p.SourceInfo);
        Assert.That(@out, Is.EqualTo(@in));

        var p2 = parser.Parse(ISource.NewTextSource(@out));
        if (p2.HasErrors()) Assert.Fail(p2.Errors.ToDisplayString());

        var before = p.Expr;
        var after = p2.Expr;
        Assert.That(before, Is.EqualTo(after));
    }

    public static object[] UnparseEquivalentSource()
    {
        return new[]
        {
            new object[]
            {
                "call_add", new[] { "a+b-c", "a + b - c" }
            },
            new object[]
            {
                "call_cond", new[] { "a ? b          : c", "a ? b : c" }
            },
            new object[]
            {
                "call_index", new[] { "a[  1  ][\"b\"]", "a[1][\"b\"]" }
            },
            new object[]
            {
                "call_or_and", new[] { "(false && !true) || false", "false && !true || false" }
            },
            new object[]
            {
                "call_not_not", new[] { "!!true", "true" }
            },
            new object[]
            {
                "lit_quote_bytes", new[] { "b'aaa\"\\'bbb'", "b\"aaa\\\"'bbb\"" }
            },
            new object[]
            {
                "lit_quote_bytes2", new[] { "b\"\\141\\141\\141\\042\\047\\142\\142\\142\"", "b\"aaa\\\"'bbb\"" }
            },
            new object[]
            {
                "select", new[] { "a . b . c", "a.b.c" }
            },
            new object[]
            {
                "lit_unprintable",
                new[]
                {
                    "b'\\000\\001\\002\\003\\004\\005\\006\\007\\010\\032\\033\\034\\035\\036\\037\\040abcdef012345'",
                    "b\"\\x00\\x01\\x02\\x03\\x04\\x05\\x06\\a\\b\\x1a\\x1b\\x1c\\x1d\\x1e\\x1f abcdef012345\""
                }
            }
        };
    }

    [TestCaseSource(nameof(UnparseEquivalentSource))]
    public virtual void UnparseEquivalent(string name, string[] @in)
    {
        var parser = new Parser(new Options.Builder().Build());

        var p = parser.Parse(ISource.NewTextSource(@in[0]));
        if (p.HasErrors()) Assert.Fail(p.Errors.ToDisplayString());
        var @out = Unparser.Unparse(p.Expr, p.SourceInfo);
        Assert.That(@out, Is.EqualTo(@in[1]));

        var p2 = parser.Parse(ISource.NewTextSource(@out));
        if (p2.HasErrors()) Assert.Fail(p2.Errors.ToDisplayString());
        var before = p.Expr;
        var after = p2.Expr;
        Assert.That(before, Is.EqualTo(after));
    }
}