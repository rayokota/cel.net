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

using Cel.Common;
using NUnit.Framework;

namespace Cel.Parser
{
	using Expr = Google.Api.Expr.V1Alpha1.Expr;

	[TestFixture]
	public class UnparserTest
	{
	  public static string[][] UnparseIdenticalSource()
	  {
		return new string[][]
		{
			new string[] {"call_add", "a + b - c"},
			new string[] {"call_and", "a && b && c && d && e"},
			new string[] {"call_and_or", "a || b && (c || d) && e"},
			new string[] {"call_cond", "a ? b : c"},
			new string[] {"call_index", "a[1][\"b\"]"},
			new string[] {"call_index_eq", "x[\"a\"].single_int32 == 23"},
			new string[] {"call_mul", "a * (b / c) % 0"},
			new string[] {"call_mul_add", "a + b * c"},
			new string[] {"call_mul_add_nested", "(a + b) * c / (d - e)"},
			new string[] {"call_mul_nested", "a * b / c % 0"},
			new string[] {"call_not", "!true"},
			new string[] {"call_neg", "-num"},
			new string[] {"call_or", "a || b || c || d || e"},
			new string[] {"call_neg_mult", "-(1 * 2)"},
			new string[] {"call_neg_add", "-(1 + 2)"},
			new string[] {"calc_distr_paren", "(1 + 2) * 3"},
			new string[] {"calc_distr_noparen", "1 + 2 * 3"},
			new string[] {"cond_tern_simple", "(x > 5) ? (x - 5) : 0"},
			new string[] {"cond_tern_neg_expr", "-((x > 5) ? (x - 5) : 0)"},
			new string[] {"cond_tern_neg_term", "-x ? (x - 5) : 0"},
			new string[] {"func_global", "size(a ? (b ? c : d) : e)"},
			new string[] {"func_member", "a.hello(\"world\")"},
			new string[] {"func_no_arg", "zero()"},
			new string[] {"func_one_arg", "one(\"a\")"},
			new string[] {"func_two_args", "and(d, 32u)"},
			new string[] {"func_var_args", "max(a, b, 100)"},
			new string[] {"func_neq", "x != \"a\""},
			new string[] {"func_in", "a in b"},
			new string[] {"list_empty", "[]"},
			new string[] {"list_one", "[1]"},
			new string[] {"list_many", "[\"hello, world\", \"goodbye, world\", \"sure, why not?\"]"},
			new string[] {"lit_bytes", "b\"\\xc3\\x83\\xc2\\xbf\""},
			new string[] {"lit_double", "-42.101"},
			new string[] {"lit_false", "false"},
			new string[] {"lit_int", "-405069"},
			new string[] {"lit_null", "null"},
			new string[] {"lit_string", "\"hello:\\t'world'\""},
			new string[] {"lit_true", "true"},
			new string[] {"lit_uint", "42u"},
			new string[] {"ident", "my_ident"},
			new string[] {"macro_has", "has(hello.world)"},
			new string[] {"map_empty", "{}"},
			new string[] {"map_lit_key", "{\"a\": a.b.c, b\"b\": bytes(a.b.c)}"},
			new string[] {"map_expr_key", "{a: a, b: a.b, c: a.b.c, a ? b : c: false, a || b: true}"},
			new string[] {"msg_empty", "v1alpha1.Expr{}"},
			new string[] {"msg_fields", "v1alpha1.Expr{id: 1, call_expr: v1alpha1.Call_Expr{function: \"name\"}}"},
			new string[] {"select", "a.b.c"},
			new string[] {"idx_idx_sel", "a[b][c].name"},
			new string[] {"sel_expr_target", "(a + b).name"},
			new string[] {"sel_cond_target", "(a ? b : c).name"},
			new string[] {"idx_cond_target", "(a ? b : c)[0]"},
			new string[] {"cond_conj", "(a1 && a2) ? b : c"},
			new string[] {"cond_disj_conj", "a ? (b1 || b2) : (c1 && c2)"},
			new string[] {"call_cond_target", "(a ? b : c).method(d)"},
			new string[] {"cond_flat", "false && !true || false"},
			new string[] {"cond_paren", "false && (!true || false)"},
			new string[] {"cond_cond", "(false && !true || false) ? 2 : 3"},
			new string[] {"cond_binop", "(x < 5) ? x : 5"},
			new string[] {"cond_binop_binop", "(x > 5) ? (x - 5) : 0"},
			new string[] {"cond_cond_binop", "(x > 5) ? ((x > 10) ? (x - 10) : 5) : 0"}
		};
	  }

    [TestCaseSource(nameof(UnparseIdenticalSource))]
	  public virtual void UnparseIdentical(string name, string @in)
	  {
		Parser parser = new Parser(new Options.Builder().Build());

		Parser.ParseResult p = parser.Parse(Source.NewTextSource(@in));
		if (p.HasErrors())
		{
		  Assert.Fail(p.Errors.ToDisplayString());
		}

		string @out = Unparser.Unparse(p.Expr, p.SourceInfo);
		Assert.That(@out, Is.EqualTo(@in));

		Parser.ParseResult p2 = parser.Parse(Source.NewTextSource(@out));
		if (p2.HasErrors())
		{
		  Assert.Fail(p2.Errors.ToDisplayString());
		}

		Expr before = p.Expr;
		Expr after = p2.Expr;
		Assert.That(before, Is.EqualTo(after));
	  }

	  public static object[] UnparseEquivalentSource()
	  {
		return new object[][]
		{
			new object[]
			{
				"call_add", new string[] {"a+b-c", "a + b - c"}
			},
			new object[]
			{
				"call_cond", new string[] {"a ? b          : c", "a ? b : c"}
			},
			new object[]
			{
				"call_index", new string[] {"a[  1  ][\"b\"]", "a[1][\"b\"]"}
			},
			new object[]
			{
				"call_or_and", new string[] {"(false && !true) || false", "false && !true || false"}
			},
			new object[]
			{
				"call_not_not", new string[] {"!!true", "true"}
			},
			new object[]
			{
				"lit_quote_bytes", new string[] {"b'aaa\"\\'bbb'", "b\"aaa\\\"'bbb\""}
			},
			new object[]
			{
				"lit_quote_bytes2", new string[] {"b\"\\141\\141\\141\\042\\047\\142\\142\\142\"", "b\"aaa\\\"'bbb\""}
			},
			new object[]
			{
				"select", new string[] {"a . b . c", "a.b.c"}
			},
			new object[]
			{
				"lit_unprintable", new string[] {"b'\\000\\001\\002\\003\\004\\005\\006\\007\\010\\032\\033\\034\\035\\036\\037\\040abcdef012345'", "b\"\\x00\\x01\\x02\\x03\\x04\\x05\\x06\\a\\b\\x1a\\x1b\\x1c\\x1d\\x1e\\x1f abcdef012345\""}
			}
		};
	  }

    [TestCaseSource(nameof(UnparseEquivalentSource))]
	  public virtual void UnparseEquivalent(string name, string[] @in)
	  {
		Parser parser = new Parser(new Options.Builder().Build());

		Parser.ParseResult p = parser.Parse(Source.NewTextSource(@in[0]));
		if (p.HasErrors())
		{
		  Assert.Fail(p.Errors.ToDisplayString());
		}
		string @out = Unparser.Unparse(p.Expr, p.SourceInfo);
		Assert.That(@out, Is.EqualTo(@in[1]));

		Parser.ParseResult p2 = parser.Parse(Source.NewTextSource(@out));
		if (p2.HasErrors())
		{
		  Assert.Fail(p2.Errors.ToDisplayString());
		}
		Expr before = p.Expr;
		Expr after = p2.Expr;
		Assert.That(before, Is.EqualTo(after));
	  }
	}

}