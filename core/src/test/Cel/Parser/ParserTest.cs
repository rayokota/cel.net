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
using Cel.Common.Debug;
using NUnit.Framework;

namespace Cel.Parser
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThatThrownBy;

	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using ExprKindCase = Google.Api.Expr.V1Alpha1.Expr.ExprKindOneofCase;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;

	internal class ParserTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static String[][] testCases()
	  public static string[][] TestCases()
	  {
		return new string[][]
		{
			new string[] {"0", "\"A\"", "\"A\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"1", "true", "True^#1:*expr.Constant_BoolValue#", "", ""},
			new string[] {"2", "false", "False^#1:*expr.Constant_BoolValue#", "", ""},
			new string[] {"3", "0", "0^#1:*expr.Constant_Int64Value#", "", ""},
			new string[] {"4", "42", "42^#1:*expr.Constant_Int64Value#", "", ""},
			new string[] {"5", "0u", "0u^#1:*expr.Constant_Uint64Value#", "", ""},
			new string[] {"6", "23u", "23u^#1:*expr.Constant_Uint64Value#", "", ""},
			new string[] {"7", "24u", "24u^#1:*expr.Constant_Uint64Value#", "", ""},
			new string[] {"8", "-1", "-1^#1:*expr.Constant_Int64Value#", "", ""},
			new string[] {"9", "4--4", "_-_(\n" + "  4^#1:*expr.Constant_Int64Value#,\n" + "  -4^#3:*expr.Constant_Int64Value#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"10", "4--4.1", "_-_(\n" + "  4^#1:*expr.Constant_Int64Value#,\n" + "  -4.1^#3:*expr.Constant_DoubleValue#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"11", "b\"abc\"", "b\"abc\"^#1:*expr.Constant_BytesValue#", "", ""},
			new string[] {"12", "23.39", "23.39^#1:*expr.Constant_DoubleValue#", "", ""},
			new string[] {"13", "!a", "!_(\n" + "  a^#2:*expr.Expr_IdentExpr#\n" + ")^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"14", "null", "null^#1:*expr.Constant_NullValue#", "", ""},
			new string[] {"15", "a", "a^#1:*expr.Expr_IdentExpr#", "", ""},
			new string[] {"16", "a?b:c", "_?_:_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#,\n" + "  c^#4:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"17", "a || b", "_||_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#2:*expr.Expr_IdentExpr#\n" + ")^#3:*expr.Expr_CallExpr#", "", ""},
			new string[] {"18", "a || b || c || d || e || f ", "_||_(\n" + "  _||_(\n" + "    _||_(\n" + "      a^#1:*expr.Expr_IdentExpr#,\n" + "      b^#2:*expr.Expr_IdentExpr#\n" + "    )^#3:*expr.Expr_CallExpr#,\n" + "    c^#4:*expr.Expr_IdentExpr#\n" + "  )^#5:*expr.Expr_CallExpr#,\n" + "  _||_(\n" + "    _||_(\n" + "      d^#6:*expr.Expr_IdentExpr#,\n" + "      e^#8:*expr.Expr_IdentExpr#\n" + "    )^#9:*expr.Expr_CallExpr#,\n" + "    f^#10:*expr.Expr_IdentExpr#\n" + "  )^#11:*expr.Expr_CallExpr#\n" + ")^#7:*expr.Expr_CallExpr#", "", ""},
			new string[] {"19", "a && b", "_&&_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#2:*expr.Expr_IdentExpr#\n" + ")^#3:*expr.Expr_CallExpr#", "", ""},
			new string[] {"20", "a && b && c && d && e && f && g", "_&&_(\n" + "  _&&_(\n" + "    _&&_(\n" + "      a^#1:*expr.Expr_IdentExpr#,\n" + "      b^#2:*expr.Expr_IdentExpr#\n" + "    )^#3:*expr.Expr_CallExpr#,\n" + "    _&&_(\n" + "      c^#4:*expr.Expr_IdentExpr#,\n" + "      d^#6:*expr.Expr_IdentExpr#\n" + "    )^#7:*expr.Expr_CallExpr#\n" + "  )^#5:*expr.Expr_CallExpr#,\n" + "  _&&_(\n" + "    _&&_(\n" + "      e^#8:*expr.Expr_IdentExpr#,\n" + "      f^#10:*expr.Expr_IdentExpr#\n" + "    )^#11:*expr.Expr_CallExpr#,\n" + "    g^#12:*expr.Expr_IdentExpr#\n" + "  )^#13:*expr.Expr_CallExpr#\n" + ")^#9:*expr.Expr_CallExpr#", "", ""},
			new string[] {"21", "a && b && c && d || e && f && g && h", "_||_(\n" + "  _&&_(\n" + "    _&&_(\n" + "      a^#1:*expr.Expr_IdentExpr#,\n" + "      b^#2:*expr.Expr_IdentExpr#\n" + "    )^#3:*expr.Expr_CallExpr#,\n" + "    _&&_(\n" + "      c^#4:*expr.Expr_IdentExpr#,\n" + "      d^#6:*expr.Expr_IdentExpr#\n" + "    )^#7:*expr.Expr_CallExpr#\n" + "  )^#5:*expr.Expr_CallExpr#,\n" + "  _&&_(\n" + "    _&&_(\n" + "      e^#8:*expr.Expr_IdentExpr#,\n" + "      f^#9:*expr.Expr_IdentExpr#\n" + "    )^#10:*expr.Expr_CallExpr#,\n" + "    _&&_(\n" + "      g^#11:*expr.Expr_IdentExpr#,\n" + "      h^#13:*expr.Expr_IdentExpr#\n" + "    )^#14:*expr.Expr_CallExpr#\n" + "  )^#12:*expr.Expr_CallExpr#\n" + ")^#15:*expr.Expr_CallExpr#", "", ""},
			new string[] {"22", "a + b", "_+_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"23", "a - b", "_-_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"24", "a * b", "_*_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"25", "a / b", "_/_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"26", "a % b", "_%_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"27", "a in b", "@in(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"28", "a == b", "_==_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"29", "a != b", "_!=_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"30", "a > b", "_>_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"31", "a >= b", "_>=_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"32", "a < b", "_<_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"33", "a <= b", "_<=_(\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"34", "a.b", "a^#1:*expr.Expr_IdentExpr#.b^#2:*expr.Expr_SelectExpr#", "", ""},
			new string[] {"35", "a.b.c", "a^#1:*expr.Expr_IdentExpr#.b^#2:*expr.Expr_SelectExpr#.c^#3:*expr.Expr_SelectExpr#", "", ""},
			new string[] {"36", "a[b]", "_[_](\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"37", "foo{ }", "foo{}^#2:*expr.Expr_StructExpr#", "", ""},
			new string[] {"38", "foo{ a:b }", "foo{\n" + "  a:b^#4:*expr.Expr_IdentExpr#^#3:*expr.Expr_CreateStruct_Entry#\n" + "}^#2:*expr.Expr_StructExpr#", "", ""},
			new string[] {"39", "foo{ a:b, c:d }", "foo{\n" + "  a:b^#4:*expr.Expr_IdentExpr#^#3:*expr.Expr_CreateStruct_Entry#,\n" + "  c:d^#6:*expr.Expr_IdentExpr#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#2:*expr.Expr_StructExpr#", "", ""},
			new string[] {"40", "{}", "{}^#1:*expr.Expr_StructExpr#", "", ""},
			new string[] {"41", "{a:b, c:d}", "{\n" + "  a^#3:*expr.Expr_IdentExpr#:b^#4:*expr.Expr_IdentExpr#^#2:*expr.Expr_CreateStruct_Entry#,\n" + "  c^#6:*expr.Expr_IdentExpr#:d^#7:*expr.Expr_IdentExpr#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#1:*expr.Expr_StructExpr#", "", ""},
			new string[] {"42", "[]", "[]^#1:*expr.Expr_ListExpr#", "", ""},
			new string[] {"43", "[a]", "[\n" + "  a^#2:*expr.Expr_IdentExpr#\n" + "]^#1:*expr.Expr_ListExpr#", "", ""},
			new string[] {"44", "[a, b, c]", "[\n" + "  a^#2:*expr.Expr_IdentExpr#,\n" + "  b^#3:*expr.Expr_IdentExpr#,\n" + "  c^#4:*expr.Expr_IdentExpr#\n" + "]^#1:*expr.Expr_ListExpr#", "", ""},
			new string[] {"45", "(a)", "a^#1:*expr.Expr_IdentExpr#", "", ""},
			new string[] {"46", "((a))", "a^#1:*expr.Expr_IdentExpr#", "", ""},
			new string[] {"47", "a()", "a()^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"48", "a(b)", "a(\n" + "  b^#2:*expr.Expr_IdentExpr#\n" + ")^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"49", "a(b, c)", "a(\n" + "  b^#2:*expr.Expr_IdentExpr#,\n" + "  c^#3:*expr.Expr_IdentExpr#\n" + ")^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"50", "a.b()", "a^#1:*expr.Expr_IdentExpr#.b()^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"51", "a.b(c)", "a^#1:*expr.Expr_IdentExpr#.b(\n" + "  c^#3:*expr.Expr_IdentExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", "a^#1[1,0]#.b(\n" + "  c^#3[1,4]#\n" + ")^#2[1,3]#"},
			new string[] {"52", "*@a | b", "", "ERROR: <input>:1:1: Syntax error: extraneous input '*' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | *@a | b\n" + " | ^\n" + "ERROR: <input>:1:2: Syntax error: token recognition error at: '@'\n" + " | *@a | b\n" + " | .^\n" + "ERROR: <input>:1:5: Syntax error: token recognition error at: '| '\n" + " | *@a | b\n" + " | ....^\n" + "ERROR: <input>:1:7: Syntax error: extraneous input 'b' expecting <EOF>\n" + " | *@a | b\n" + " | ......^", ""},
			new string[] {"53", "a | b", "", "ERROR: <input>:1:3: Syntax error: token recognition error at: '| '\n" + " | a | b\n" + " | ..^\n" + "ERROR: <input>:1:5: Syntax error: extraneous input 'b' expecting <EOF>\n" + " | a | b\n" + " | ....^", ""},
			new string[] {"54", "has(m.f)", "m^#2:*expr.Expr_IdentExpr#.f~test-only~^#4:*expr.Expr_SelectExpr#", "", "m^#2[1,4]#.f~test-only~^#4[1,3]#"},
			new string[] {"55", "m.exists_one(v, f)", "__comprehension__(\n" + "  // Variable\n" + "  v,\n" + "  // Target\n" + "  m^#1:*expr.Expr_IdentExpr#,\n" + "  // Accumulator\n" + "  __result__,\n" + "  // Init\n" + "  0^#5:*expr.Constant_Int64Value#,\n" + "  // LoopCondition\n" + "  True^#7:*expr.Constant_BoolValue#,\n" + "  // LoopStep\n" + "  _?_:_(\n" + "    f^#4:*expr.Expr_IdentExpr#,\n" + "    _+_(\n" + "      __result__^#8:*expr.Expr_IdentExpr#,\n" + "      1^#6:*expr.Constant_Int64Value#\n" + "    )^#9:*expr.Expr_CallExpr#,\n" + "    __result__^#10:*expr.Expr_IdentExpr#\n" + "  )^#11:*expr.Expr_CallExpr#,\n" + "  // Result\n" + "  _==_(\n" + "    __result__^#12:*expr.Expr_IdentExpr#,\n" + "    1^#6:*expr.Constant_Int64Value#\n" + "  )^#13:*expr.Expr_CallExpr#)^#14:*expr.Expr_ComprehensionExpr#", "", ""},
			new string[] {"56", "m.map(v, f)", "__comprehension__(\n" + "  // Variable\n" + "  v,\n" + "  // Target\n" + "  m^#1:*expr.Expr_IdentExpr#,\n" + "  // Accumulator\n" + "  __result__,\n" + "  // Init\n" + "  []^#6:*expr.Expr_ListExpr#,\n" + "  // LoopCondition\n" + "  True^#7:*expr.Constant_BoolValue#,\n" + "  // LoopStep\n" + "  _+_(\n" + "    __result__^#5:*expr.Expr_IdentExpr#,\n" + "    [\n" + "      f^#4:*expr.Expr_IdentExpr#\n" + "    ]^#8:*expr.Expr_ListExpr#\n" + "  )^#9:*expr.Expr_CallExpr#,\n" + "  // Result\n" + "  __result__^#5:*expr.Expr_IdentExpr#)^#10:*expr.Expr_ComprehensionExpr#", "", ""},
			new string[] {"57", "m.map(v, p, f)", "__comprehension__(\n" + "  // Variable\n" + "  v,\n" + "  // Target\n" + "  m^#1:*expr.Expr_IdentExpr#,\n" + "  // Accumulator\n" + "  __result__,\n" + "  // Init\n" + "  []^#7:*expr.Expr_ListExpr#,\n" + "  // LoopCondition\n" + "  True^#8:*expr.Constant_BoolValue#,\n" + "  // LoopStep\n" + "  _?_:_(\n" + "    p^#4:*expr.Expr_IdentExpr#,\n" + "    _+_(\n" + "      __result__^#6:*expr.Expr_IdentExpr#,\n" + "      [\n" + "        f^#5:*expr.Expr_IdentExpr#\n" + "      ]^#9:*expr.Expr_ListExpr#\n" + "    )^#10:*expr.Expr_CallExpr#,\n" + "    __result__^#6:*expr.Expr_IdentExpr#\n" + "  )^#11:*expr.Expr_CallExpr#,\n" + "  // Result\n" + "  __result__^#6:*expr.Expr_IdentExpr#)^#12:*expr.Expr_ComprehensionExpr#", "", ""},
			new string[] {"58", "m.filter(v, p)", "__comprehension__(\n" + "  // Variable\n" + "  v,\n" + "  // Target\n" + "  m^#1:*expr.Expr_IdentExpr#,\n" + "  // Accumulator\n" + "  __result__,\n" + "  // Init\n" + "  []^#6:*expr.Expr_ListExpr#,\n" + "  // LoopCondition\n" + "  True^#7:*expr.Constant_BoolValue#,\n" + "  // LoopStep\n" + "  _?_:_(\n" + "    p^#4:*expr.Expr_IdentExpr#,\n" + "    _+_(\n" + "      __result__^#5:*expr.Expr_IdentExpr#,\n" + "      [\n" + "        v^#3:*expr.Expr_IdentExpr#\n" + "      ]^#8:*expr.Expr_ListExpr#\n" + "    )^#9:*expr.Expr_CallExpr#,\n" + "    __result__^#5:*expr.Expr_IdentExpr#\n" + "  )^#10:*expr.Expr_CallExpr#,\n" + "  // Result\n" + "  __result__^#5:*expr.Expr_IdentExpr#)^#11:*expr.Expr_ComprehensionExpr#", "", ""},
			new string[] {"59", "x * 2", "_*_(\n" + "  x^#1:*expr.Expr_IdentExpr#,\n" + "  2^#3:*expr.Constant_Int64Value#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"60", "x * 2u", "_*_(\n" + "  x^#1:*expr.Expr_IdentExpr#,\n" + "  2u^#3:*expr.Constant_Uint64Value#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"61", "x * 2.0", "_*_(\n" + "  x^#1:*expr.Expr_IdentExpr#,\n" + "  2^#3:*expr.Constant_DoubleValue#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"62", "\"\\u2764\"", "\"\u2764\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"63", "\"\u2764\"", "\"\u2764\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"64", "! false", "!_(\n" + "  False^#2:*expr.Constant_BoolValue#\n" + ")^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"65", "-a", "-_(\n" + "  a^#2:*expr.Expr_IdentExpr#\n" + ")^#1:*expr.Expr_CallExpr#", "", ""},
			new string[] {"66", "a.b(5)", "a^#1:*expr.Expr_IdentExpr#.b(\n" + "  5^#3:*expr.Constant_Int64Value#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"67", "a[3]", "_[_](\n" + "  a^#1:*expr.Expr_IdentExpr#,\n" + "  3^#3:*expr.Constant_Int64Value#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"68", "SomeMessage{foo: 5, bar: \"xyz\"}", "SomeMessage{\n" + "  foo:5^#4:*expr.Constant_Int64Value#^#3:*expr.Expr_CreateStruct_Entry#,\n" + "  bar:\"xyz\"^#6:*expr.Constant_StringValue#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#2:*expr.Expr_StructExpr#", "", ""},
			new string[] {"69", "[3, 4, 5]", "[\n" + "  3^#2:*expr.Constant_Int64Value#,\n" + "  4^#3:*expr.Constant_Int64Value#,\n" + "  5^#4:*expr.Constant_Int64Value#\n" + "]^#1:*expr.Expr_ListExpr#", "", ""},
			new string[] {"70", "[3, 4, 5,]", "[\n" + "  3^#2:*expr.Constant_Int64Value#,\n" + "  4^#3:*expr.Constant_Int64Value#,\n" + "  5^#4:*expr.Constant_Int64Value#\n" + "]^#1:*expr.Expr_ListExpr#", "", ""},
			new string[] {"71", "{foo: 5, bar: \"xyz\"}", "{\n" + "  foo^#3:*expr.Expr_IdentExpr#:5^#4:*expr.Constant_Int64Value#^#2:*expr.Expr_CreateStruct_Entry#,\n" + "  bar^#6:*expr.Expr_IdentExpr#:\"xyz\"^#7:*expr.Constant_StringValue#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#1:*expr.Expr_StructExpr#", "", ""},
			new string[] {"72", "{foo: 5, bar: \"xyz\", }", "{\n" + "  foo^#3:*expr.Expr_IdentExpr#:5^#4:*expr.Constant_Int64Value#^#2:*expr.Expr_CreateStruct_Entry#,\n" + "  bar^#6:*expr.Expr_IdentExpr#:\"xyz\"^#7:*expr.Constant_StringValue#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#1:*expr.Expr_StructExpr#", "", ""},
			new string[] {"73", "a > 5 && a < 10", "_&&_(\n" + "  _>_(\n" + "    a^#1:*expr.Expr_IdentExpr#,\n" + "    5^#3:*expr.Constant_Int64Value#\n" + "  )^#2:*expr.Expr_CallExpr#,\n" + "  _<_(\n" + "    a^#4:*expr.Expr_IdentExpr#,\n" + "    10^#6:*expr.Constant_Int64Value#\n" + "  )^#5:*expr.Expr_CallExpr#\n" + ")^#7:*expr.Expr_CallExpr#", "", ""},
			new string[] {"74", "a < 5 || a > 10", "_||_(\n" + "  _<_(\n" + "    a^#1:*expr.Expr_IdentExpr#,\n" + "    5^#3:*expr.Constant_Int64Value#\n" + "  )^#2:*expr.Expr_CallExpr#,\n" + "  _>_(\n" + "    a^#4:*expr.Expr_IdentExpr#,\n" + "    10^#6:*expr.Constant_Int64Value#\n" + "  )^#5:*expr.Expr_CallExpr#\n" + ")^#7:*expr.Expr_CallExpr#", "", ""},
			new string[] {"75", "{", "", "ERROR: <input>:1:2: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '}', '(', '.', ',', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | {\n" + " | .^", ""},
			new string[] {"76", "[] + [1,2,3,] + [4]", "_+_(\n" + "  _+_(\n" + "    []^#1:*expr.Expr_ListExpr#,\n" + "    [\n" + "      1^#4:*expr.Constant_Int64Value#,\n" + "      2^#5:*expr.Constant_Int64Value#,\n" + "      3^#6:*expr.Constant_Int64Value#\n" + "    ]^#3:*expr.Expr_ListExpr#\n" + "  )^#2:*expr.Expr_CallExpr#,\n" + "  [\n" + "    4^#9:*expr.Constant_Int64Value#\n" + "  ]^#8:*expr.Expr_ListExpr#\n" + ")^#7:*expr.Expr_CallExpr#", "", ""},
			new string[] {"77", "{1:2u, 2:3u}", "{\n" + "  1^#3:*expr.Constant_Int64Value#:2u^#4:*expr.Constant_Uint64Value#^#2:*expr.Expr_CreateStruct_Entry#,\n" + "  2^#6:*expr.Constant_Int64Value#:3u^#7:*expr.Constant_Uint64Value#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#1:*expr.Expr_StructExpr#", "", ""},
			new string[] {"78", "TestAllTypes{single_int32: 1, single_int64: 2}", "TestAllTypes{\n" + "  single_int32:1^#4:*expr.Constant_Int64Value#^#3:*expr.Expr_CreateStruct_Entry#,\n" + "  single_int64:2^#6:*expr.Constant_Int64Value#^#5:*expr.Expr_CreateStruct_Entry#\n" + "}^#2:*expr.Expr_StructExpr#", "", ""},
			new string[] {"79", "TestAllTypes(){single_int32: 1, single_int64: 2}", "", "ERROR: <input>:1:13: expected a qualified name\n" + " | TestAllTypes(){single_int32: 1, single_int64: 2}\n" + " | ............^", ""},
			new string[] {"80", "size(x) == x.size()", "_==_(\n" + "  size(\n" + "    x^#2:*expr.Expr_IdentExpr#\n" + "  )^#1:*expr.Expr_CallExpr#,\n" + "  x^#4:*expr.Expr_IdentExpr#.size()^#5:*expr.Expr_CallExpr#\n" + ")^#3:*expr.Expr_CallExpr#", "", ""},
			new string[] {"81", "1 + $", "", "ERROR: <input>:1:5: Syntax error: token recognition error at: '$'\n" + " | 1 + $\n" + " | ....^\n" + "ERROR: <input>:1:6: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | 1 + $\n" + " | .....^", ""},
			new string[] {"82", "1 + 2\n" + "3 +", "", "ERROR: <input>:2:1: Syntax error: mismatched input '3' expecting {<EOF>, '==', '!=', 'in', '<', '<=', '>=', '>', '&&', '||', '[', '{', '.', '-', '?', '+', '*', '/', '%'}\n" + " | 3 +\n" + " | ^", ""},
			new string[] {"83", "\"\\\"\"", "\"\\\"\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"84", "[1,3,4][0]", "_[_](\n" + "  [\n" + "    1^#2:*expr.Constant_Int64Value#,\n" + "    3^#3:*expr.Constant_Int64Value#,\n" + "    4^#4:*expr.Constant_Int64Value#\n" + "  ]^#1:*expr.Expr_ListExpr#,\n" + "  0^#6:*expr.Constant_Int64Value#\n" + ")^#5:*expr.Expr_CallExpr#", "", ""},
			new string[] {"85", "1.all(2, 3)", "", "ERROR: <input>:1:7: argument must be a simple name\n" + " | 1.all(2, 3)\n" + " | ......^", ""},
			new string[] {"86", "x[\"a\"].single_int32 == 23", "_==_(\n" + "  _[_](\n" + "    x^#1:*expr.Expr_IdentExpr#,\n" + "    \"a\"^#3:*expr.Constant_StringValue#\n" + "  )^#2:*expr.Expr_CallExpr#.single_int32^#4:*expr.Expr_SelectExpr#,\n" + "  23^#6:*expr.Constant_Int64Value#\n" + ")^#5:*expr.Expr_CallExpr#", "", ""},
			new string[] {"87", "x.single_nested_message != null", "_!=_(\n" + "  x^#1:*expr.Expr_IdentExpr#.single_nested_message^#2:*expr.Expr_SelectExpr#,\n" + "  null^#4:*expr.Constant_NullValue#\n" + ")^#3:*expr.Expr_CallExpr#", "", ""},
			new string[] {"88", "false && !true || false ? 2 : 3", "_?_:_(\n" + "  _||_(\n" + "    _&&_(\n" + "      False^#1:*expr.Constant_BoolValue#,\n" + "      !_(\n" + "        True^#3:*expr.Constant_BoolValue#\n" + "      )^#2:*expr.Expr_CallExpr#\n" + "    )^#4:*expr.Expr_CallExpr#,\n" + "    False^#5:*expr.Constant_BoolValue#\n" + "  )^#6:*expr.Expr_CallExpr#,\n" + "  2^#8:*expr.Constant_Int64Value#,\n" + "  3^#9:*expr.Constant_Int64Value#\n" + ")^#7:*expr.Expr_CallExpr#", "", ""},
			new string[] {"89", "b\"abc\" + B\"def\"", "_+_(\n" + "  b\"abc\"^#1:*expr.Constant_BytesValue#,\n" + "  b\"def\"^#3:*expr.Constant_BytesValue#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"90", "1 + 2 * 3 - 1 / 2 == 6 % 1", "_==_(\n" + "  _-_(\n" + "    _+_(\n" + "      1^#1:*expr.Constant_Int64Value#,\n" + "      _*_(\n" + "        2^#3:*expr.Constant_Int64Value#,\n" + "        3^#5:*expr.Constant_Int64Value#\n" + "      )^#4:*expr.Expr_CallExpr#\n" + "    )^#2:*expr.Expr_CallExpr#,\n" + "    _/_(\n" + "      1^#7:*expr.Constant_Int64Value#,\n" + "      2^#9:*expr.Constant_Int64Value#\n" + "    )^#8:*expr.Expr_CallExpr#\n" + "  )^#6:*expr.Expr_CallExpr#,\n" + "  _%_(\n" + "    6^#11:*expr.Constant_Int64Value#,\n" + "    1^#13:*expr.Constant_Int64Value#\n" + "  )^#12:*expr.Expr_CallExpr#\n" + ")^#10:*expr.Expr_CallExpr#", "", ""},
			new string[] {"91", "1 + +", "", "ERROR: <input>:1:5: Syntax error: mismatched input '+' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | 1 + +\n" + " | ....^\n" + "ERROR: <input>:1:6: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | 1 + +\n" + " | .....^", ""},
			new string[] {"92", "\"abc\" + \"def\"", "_+_(\n" + "  \"abc\"^#1:*expr.Constant_StringValue#,\n" + "  \"def\"^#3:*expr.Constant_StringValue#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			new string[] {"93", "{\"a\": 1}.\"a\"", "", "ERROR: <input>:1:10: Syntax error: mismatched input '\"a\"' expecting IDENTIFIER\n" + " | {\"a\": 1}.\"a\"\n" + " | .........^", ""},
			new string[] {"94", "\"\\xC3\\XBF\"", "\"\u00C3\u00BF\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"95", "\"\\303\\277\"", "\"\u00C3\u00BF\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"96", "\"hi\\u263A \\u263Athere\"", "\"hi\u263A \u263Athere\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"97", "\"\\U000003A8\\?\"", "\"Ψ?\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"98", "\"\\a\\b\\f\\n\\r\\t\\v'\\\"\\\\\\? Legal escapes\"", "\"\\a\\b\\f\\n\\r\\t\\v'\\\"\\\\? Legal escapes\"^#1:*expr.Constant_StringValue#", "", ""},
			new string[] {"99", "\"\\xFh\"", "", "ERROR: <input>:1:1: Syntax error: token recognition error at: '\"\\xFh'\n" + " | \"\\xFh\"\n" + " | ^\n" + "ERROR: <input>:1:6: Syntax error: token recognition error at: '\"'\n" + " | \"\\xFh\"\n" + " | .....^\n" + "ERROR: <input>:1:7: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | \"\\xFh\"\n" + " | ......^", ""},
			new string[] {"100", "\"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>\"", "", "ERROR: <input>:1:1: Syntax error: token recognition error at: '\"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>'\n" + " | \"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>\"\n" + " | ^\n" + "ERROR: <input>:1:42: Syntax error: token recognition error at: '\"'\n" + " | \"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>\"\n" + " | .........................................^\n" + "ERROR: <input>:1:43: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | \"\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\\\? Illegal escape \\>\"\n" + " | ..........................................^", ""},
			new string[] {"101", "\"😁\" in [\"😁\", \"😑\", \"😦\"]", "@in(\n" + "  \"😁\"^#1:*expr.Constant_StringValue#,\n" + "  [\n" + "    \"😁\"^#4:*expr.Constant_StringValue#,\n" + "    \"😑\"^#5:*expr.Constant_StringValue#,\n" + "    \"😦\"^#6:*expr.Constant_StringValue#\n" + "  ]^#3:*expr.Expr_ListExpr#\n" + ")^#2:*expr.Expr_CallExpr#", "", ""},
			// TODO re-enable?  (fix unicode parsing)
			//new string[] {"102", "      '😁' in ['😁', '😑', '😦']\n" + "  && in.😁", "", "ERROR: <input>:2:6: Syntax error: extraneous input 'in' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " |   && in.\uD83D\uDE01\n" + " | .....^\n" + "ERROR: <input>:2:9: Syntax error: token recognition error at: '\uD83D'\n" + " |   && in.\uD83D\uDE01\n" + " | ........^\n" + "ERROR: <input>:2:10: Syntax error: token recognition error at: '\uDE01'\n" + " |   && in.\uD83D\uDE01\n" + " | .........^\n" + "ERROR: <input>:2:11: Syntax error: missing IDENTIFIER at '<EOF>'\n" + " |   && in.\uD83D\uDE01\n" + " | ..........^", ""},
			new string[] {"103", "as", "", "ERROR: <input>:1:1: reserved identifier: as\n" + " | as\n" + " | ^", ""},
			new string[] {"104", "break", "", "ERROR: <input>:1:1: reserved identifier: break\n" + " | break\n" + " | ^", ""},
			new string[] {"105", "const", "", "ERROR: <input>:1:1: reserved identifier: const\n" + " | const\n" + " | ^", ""},
			new string[] {"106", "continue", "", "ERROR: <input>:1:1: reserved identifier: continue\n" + " | continue\n" + " | ^", ""},
			new string[] {"107", "else", "", "ERROR: <input>:1:1: reserved identifier: else\n" + " | else\n" + " | ^", ""},
			new string[] {"108", "for", "", "ERROR: <input>:1:1: reserved identifier: for\n" + " | for\n" + " | ^", ""},
			new string[] {"109", "function", "", "ERROR: <input>:1:1: reserved identifier: function\n" + " | function\n" + " | ^", ""},
			new string[] {"110", "if", "", "ERROR: <input>:1:1: reserved identifier: if\n" + " | if\n" + " | ^", ""},
			new string[] {"111", "import", "", "ERROR: <input>:1:1: reserved identifier: import\n" + " | import\n" + " | ^", ""},
			new string[] {"112", "in", "", "ERROR: <input>:1:1: Syntax error: mismatched input 'in' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | in\n" + " | ^\n" + "ERROR: <input>:1:3: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | in\n" + " | ..^", ""},
			new string[] {"113", "let", "", "ERROR: <input>:1:1: reserved identifier: let\n" + " | let\n" + " | ^", ""},
			new string[] {"114", "loop", "", "ERROR: <input>:1:1: reserved identifier: loop\n" + " | loop\n" + " | ^", ""},
			new string[] {"115", "package", "", "ERROR: <input>:1:1: reserved identifier: package\n" + " | package\n" + " | ^", ""},
			new string[] {"116", "namespace", "", "ERROR: <input>:1:1: reserved identifier: namespace\n" + " | namespace\n" + " | ^", ""},
			new string[] {"117", "return", "", "ERROR: <input>:1:1: reserved identifier: return\n" + " | return\n" + " | ^", ""},
			new string[] {"118", "var", "", "ERROR: <input>:1:1: reserved identifier: var\n" + " | var\n" + " | ^", ""},
			new string[] {"119", "void", "", "ERROR: <input>:1:1: reserved identifier: void\n" + " | void\n" + " | ^", ""},
			new string[] {"120", "while", "", "ERROR: <input>:1:1: reserved identifier: while\n" + " | while\n" + " | ^", ""},
			new string[] {"121", "[1, 2, 3].map(var, var * var)", "", "ERROR: <input>:1:14: argument is not an identifier\n" + " | [1, 2, 3].map(var, var * var)\n" + " | .............^\n" + "ERROR: <input>:1:15: reserved identifier: var\n" + " | [1, 2, 3].map(var, var * var)\n" + " | ..............^\n" + "ERROR: <input>:1:20: reserved identifier: var\n" + " | [1, 2, 3].map(var, var * var)\n" + " | ...................^\n" + "ERROR: <input>:1:26: reserved identifier: var\n" + " | [1, 2, 3].map(var, var * var)\n" + " | .........................^", ""},
			new string[] {"122", "func{{a}}", "", "ERROR: <input>:1:6: Syntax error: extraneous input '{' expecting {'}', ',', IDENTIFIER}\n" + " | func{{a}}\n" + " | .....^\n" + "ERROR: <input>:1:8: Syntax error: mismatched input '}' expecting ':'\n" + " | func{{a}}\n" + " | .......^\n" + "ERROR: <input>:1:9: Syntax error: extraneous input '}' expecting <EOF>\n" + " | func{{a}}\n" + " | ........^", ""},
			new string[] {"123", "msg{:a}", "", "ERROR: <input>:1:5: Syntax error: extraneous input ':' expecting {'}', ',', IDENTIFIER}\n" + " | msg{:a}\n" + " | ....^\n" + "ERROR: <input>:1:7: Syntax error: mismatched input '}' expecting ':'\n" + " | msg{:a}\n" + " | ......^", ""},
			new string[] {"124", "{a}", "", "ERROR: <input>:1:3: Syntax error: mismatched input '}' expecting {'==', '!=', 'in', '<', '<=', '>=', '>', '&&', '||', '[', '{', '(', '.', '-', '?', ':', '+', '*', '/', '%'}\n" + " | {a}\n" + " | ..^", ""},
			new string[] {"125", "{:a}", "", "ERROR: <input>:1:2: Syntax error: extraneous input ':' expecting {'[', '{', '}', '(', '.', ',', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | {:a}\n" + " | .^\n" + "ERROR: <input>:1:4: Syntax error: mismatched input '}' expecting {'==', '!=', 'in', '<', '<=', '>=', '>', '&&', '||', '[', '{', '(', '.', '-', '?', ':', '+', '*', '/', '%'}\n" + " | {:a}\n" + " | ...^", ""},
			new string[] {"126", "ind[a{b}]", "", "ERROR: <input>:1:8: Syntax error: mismatched input '}' expecting ':'\n" + " | ind[a{b}]\n" + " | .......^", ""},
			new string[] {"127", "--", "", "ERROR: <input>:1:3: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | --\n" + " | ..^\n" + "ERROR: <input>:1:3: Syntax error: no viable alternative at input '-'\n" + " | --\n" + " | ..^", ""},
			new string[] {"128", "?", "", "ERROR: <input>:1:1: Syntax error: mismatched input '?' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | ?\n" + " | ^\n" + "ERROR: <input>:1:2: Syntax error: mismatched input '<EOF>' expecting {'[', '{', '(', '.', '-', '!', 'true', 'false', 'null', NUM_FLOAT, NUM_INT, NUM_UINT, STRING, BYTES, IDENTIFIER}\n" + " | ?\n" + " | .^", ""},
			// TODO re-enable?
			//new string[] {"129", "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[\n" + "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[\n" + "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[\n" + "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[['too many']]]]]]]]]]]]]]]]]]]]]]]]]]]]\n" + "]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]\n" + "]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]\n" + "]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]", "", "ERROR: <input>:-1:0: expression recursion limit exceeded: 250", ""}
		};
	  }

	  /// <param name="num"> just the index of the test case </param>
	  /// <param name="i"> contains the input expression to be parsed. </param>
	  /// <param name="p"> contains the type/id adorned debug output of the expression tree. </param>
	  /// <param name="e"> contains the expected error output for a failed parse, or "" if the parse is expected
	  ///     to be successful. </param>
	  /// <param name="l"> contains the expected source adorned debug output of the expression tree. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("testCases") void parseTest(String num, String i, String p, String e, String l)
	  
	  [TestCaseSource(nameof(TestCases))]
	  public virtual void ParseTest(string num, string i, string p, string e, string l)
	  {
		Source src = Source.NewTextSource(i);
		Parser.ParseResult parseResult = Parser.ParseAllMacros(src);

		string actualErr = parseResult.Errors.ToDisplayString();
		Assert.That(actualErr, Is.EqualTo(e));
		// Hint for my future self and others: if the above "isEqualTo" fails but the strings look
		// similar,
		// look into the char[] representation... unicode can be very surprising.

		string actualWithKind = Debug.ToAdornedDebugString(parseResult.Expr, new KindAndIdAdorner());
		Assert.That(actualWithKind, Is.EqualTo(p));

		if (l.Length > 0)
		{
		  string actualWithLocation = Debug.ToAdornedDebugString(parseResult.Expr, new LocationAdorner(parseResult.SourceInfo));
		  Assert.That(actualWithLocation, Is.EqualTo(l));
		}
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void expressionSizeCodePointLimit()
	  internal virtual void ExpressionSizeCodePointLimit()
	  {
		  Options options = new Options.Builder().Macros(Macro.AllMacros).ExpressionSizeCodePointLimit(-2).Build();
		Assert.That(() => new Parser(options), Is.InstanceOf(typeof(System.ArgumentException)));

		Parser p = new Parser(new Options.Builder().Macros(Macro.AllMacros).ExpressionSizeCodePointLimit(2).Build());
		Source src = Source.NewTextSource("foo");
		Parser.ParseResult parseResult = p.Parse(src);
		Assert.That(parseResult.Errors.GetErrors, Has.Exactly(1).EqualTo(new CelError(Location.NewLocation(-1, -1), "expression code point size exceeds limit: size: 3, limit 2")));
	  }

	  internal class KindAndIdAdorner : Debug.Adorner
	  {

		public virtual string GetMetadata(object elem)
		{
		  if (elem is Expr)
		  {
			Expr e = (Expr) elem;
			if (e.ExprKindCase == ExprKindCase.ConstExpr)
			{
			  return String.Format("^#{0:D}:*expr.Constant_{1}#", e.Id, e.ConstExpr.ConstantKindCase.ToString());
			}
			else
			{
			  return String.Format("^#{0:D}:*expr.Expr_{1}#", e.Id, e.ExprKindCase.ToString());
			}
		  }
		  else if (elem is Entry)
		  {
			Expr.Types.CreateStruct.Types.Entry entry = (Expr.Types.CreateStruct.Types.Entry) elem;
			return String.Format("^#{0:D}:{1}#", entry.Id, "*expr.Expr_CreateStruct_Entry");
		  }
		  return "";
		}
	  }

	  internal class LocationAdorner : Debug.Adorner
	  {
		internal readonly SourceInfo sourceInfo;

		internal LocationAdorner(SourceInfo sourceInfo)
		{
		  this.sourceInfo = sourceInfo;
		}

		public virtual Location GetLocation(long exprID)
		{
			int pos = -1;
			sourceInfo.Positions.TryGetValue(exprID, out pos);
		  if (pos >= 0)
		  {
			int line = 1;
			foreach (int lineOffset in sourceInfo.LineOffsets)
			{
			  if (lineOffset > pos)
			  {
				break;
			  }
			  else
			  {
				line++;
			  }
			}
			long column = pos;
			if (line > 1)
			{
			  column = pos - sourceInfo.LineOffsets[line - 2];
			}
			return Location.NewLocation(line, (int) column);
		  }
		  return null;
		}

		public virtual string GetMetadata(object elem)
		{
		  long elemID;
		  if (elem is Expr)
		  {
			elemID = ((Expr) elem).Id;
		  }
		  else if (elem is Entry)
		  {
			elemID = ((Entry) elem).Id;
		  }
		  else
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new System.ArgumentException(elem.GetType().FullName);
		  }
		  Location location = GetLocation(elemID);
		  return String.Format("^#{0:D}[{1:D},{2:D}]#", elemID, location.Line(), location.Column());
		}
	  }
	}

}