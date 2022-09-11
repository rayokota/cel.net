using System.Text.RegularExpressions;
using Cel.Common;
using Cel.Common.Containers;
using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Google.Api.Expr.Test.V1.Proto2;
using Google.Api.Expr.V1Alpha1;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Checker;

public class CheckerTest
{
    internal static readonly IDictionary<string, Env> testEnvs = new Dictionary<string, Env>
    {
        {
            "default",
            new Env()
                .Functions(
                    Decls.NewFunction("fg_s", Decls.NewOverload("fg_s_0", new List<Type>(), Decls.String)),
                    Decls.NewFunction("fi_s_s",
                        Decls.NewInstanceOverload("fi_s_s_0", new List<Type> { Decls.String }, Decls.String)))
                .Idents(
                    Decls.NewVar("is", Decls.String), Decls.NewVar("ii", Decls.Int), Decls.NewVar("iu", Decls.Uint),
                    Decls.NewVar("iz", Decls.Bool), Decls.NewVar("ib", Decls.Bytes), Decls.NewVar("id", Decls.Double),
                    Decls.NewVar("ix", Decls.Null))
        }
    };

    internal static TestCase[] CheckTestCases()
    {
        return new[]
        {
            new TestCase().I("a.pancakes").Env(new Env().Idents(Decls.NewVar("a", Decls.Int))).Error(
                "ERROR: <input>:1:2: type 'int' does not support field selection\n" + " | a.pancakes\n" + " | .^"),
            new TestCase().I("\"A\"").R("\"A\"~string").Type(Decls.String),
            new TestCase().I("12").R("12~int").Type(Decls.Int),
            new TestCase().I("12u").R("12u~uint").Type(Decls.Uint),
            new TestCase().I("true").R("true~bool").Type(Decls.Bool),
            new TestCase().I("false").R("false~bool").Type(Decls.Bool),
            new TestCase().I("12.23").R("12.23~double").Type(Decls.Double),
            new TestCase().I("null").R("null~null").Type(Decls.Null),
            new TestCase().I("b\"ABC\"").R("b\"ABC\"~bytes").Type(Decls.Bytes),
            new TestCase().I("is").R("is~string^is").Type(Decls.String).Env(testEnvs["default"]),
            new TestCase().I("ii").R("ii~int^ii").Type(Decls.Int).Env(testEnvs["default"]),
            new TestCase().I("iu").R("iu~uint^iu").Type(Decls.Uint).Env(testEnvs["default"]),
            new TestCase().I("iz").R("iz~bool^iz").Type(Decls.Bool).Env(testEnvs["default"]),
            new TestCase().I("id").R("id~double^id").Type(Decls.Double).Env(testEnvs["default"]),
            new TestCase().I("ix").R("ix~null^ix").Type(Decls.Null).Env(testEnvs["default"]),
            new TestCase().I("ib").R("ib~bytes^ib").Type(Decls.Bytes).Env(testEnvs["default"]),
            new TestCase().I("id").R("id~double^id").Type(Decls.Double).Env(testEnvs["default"]),
            new TestCase().I("[]").R("[]~list(dyn)").Type(Decls.NewListType(Decls.Dyn)),
            new TestCase().I("[1]").R("[1~int]~list(int)").Type(Decls.NewListType(Decls.Int)),
            new TestCase().I("[1, \"A\"]").R("[1~int, \"A\"~string]~list(dyn)")
                .Type(Decls.NewListType(Decls.Dyn)),
            new TestCase().I("foo").R("foo~!error!").Type(Decls.Error).Error(
                "ERROR: <input>:1:1: undeclared reference to 'foo' (in container '')\n" + " | foo\n" + " | ^"),
            new TestCase().I("fg_s()").R("fg_s()~string^fg_s_0").Type(Decls.String).Env(testEnvs["default"]),
            new TestCase().I("is.fi_s_s()").R("is~string^is.fi_s_s()~string^fi_s_s_0").Type(Decls.String)
                .Env(testEnvs["default"]),
            new TestCase().I("1 + 2").R("_+_(1~int, 2~int)~int^add_int64").Type(Decls.Int)
                .Env(testEnvs["default"]),
            new TestCase().I("1 + ii").R("_+_(1~int, ii~int^ii)~int^add_int64").Type(Decls.Int)
                .Env(testEnvs["default"]),
            new TestCase().I("[1] + [2]").R("_+_([1~int]~list(int), [2~int]~list(int))~list(int)^add_list")
                .Type(Decls.NewListType(Decls.Int)).Env(testEnvs["default"]),
            new TestCase().I("[] + [1,2,3,] + [4]").Type(Decls.NewListType(Decls.Int)).R("_+_(\n" + "	_+_(\n" +
                "		[]~list(int),\n" + "		[1~int, 2~int, 3~int]~list(int))~list(int)^add_list,\n" +
                "		[4~int]~list(int))\n" + "~list(int)^add_list"),
            new TestCase().I("[1, 2u] + []")
                .R("_+_(\n" + "	[\n" + "		1~int,\n" + "		2u~uint\n" + "	]~list(dyn),\n" + "	[]~list(dyn)\n" +
                   ")~list(dyn)^add_list").Type(Decls.NewListType(Decls.Dyn)),
            new TestCase().I("{1:2u, 2:3u}").Type(Decls.NewMapType(Decls.Int, Decls.Uint))
                .R("{1~int : 2u~uint, 2~int : 3u~uint}~map(int, uint)"),
            new TestCase().I("{\"a\":1, \"b\":2}.a").Type(Decls.Int)
                .R("{\"a\"~string : 1~int, \"b\"~string : 2~int}~map(string, int).a~int"),
            new TestCase().I("{1:2u, 2u:3}").Type(Decls.NewMapType(Decls.Dyn, Decls.Dyn))
                .R("{1~int : 2u~uint, 2u~uint : 3~int}~map(dyn, dyn)"),
            new TestCase().I("TestAllTypes{single_int32: 1, single_int64: 2}")
                .Container("google.api.expr.test.v1.proto3")
                .R("google.api.expr.test.v1.proto3.TestAllTypes{\n" + "	single_int32 : 1~int,\n" +
                   "	single_int64 : 2~int\n" +
                   "}~google.api.expr.test.v1.proto3.TestAllTypes^google.api.expr.test.v1.proto3.TestAllTypes")
                .Type(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")),
            new TestCase().I("TestAllTypes{single_int32: 1u}").Container("google.api.expr.test.v1.proto3").Error(
                "ERROR: <input>:1:26: expected type of field 'single_int32' is 'int' but provided type is 'uint'\n" +
                " | TestAllTypes{single_int32: 1u}\n" + " | .........................^"),
            new TestCase().I("TestAllTypes{single_int32: 1, undefined: 2}")
                .Container("google.api.expr.test.v1.proto3")
                .Error("ERROR: <input>:1:40: undefined field 'undefined'\n" +
                       " | TestAllTypes{single_int32: 1, undefined: 2}\n" +
                       " | .......................................^"),
            new TestCase().I("size(x) == x.size()")
                .R("_==_(size(x~list(int)^x)~int^size_list, x~list(int)^x.size()~int^list_size)\n" + "  ~bool^equals")
                .Env(new Env().Idents(Decls.NewVar("x", Decls.NewListType(Decls.Int)))).Type(Decls.Bool),
            new TestCase().I("int(1u) + int(uint(\"1\"))").R("_+_(int(1u~uint)~int^uint64_to_int64,\n" +
                                                             "      int(uint(\"1\"~string)~uint^string_to_uint64)~int^uint64_to_int64)\n" +
                                                             "  ~int^add_int64").Type(Decls.Int),
            new TestCase().I("false && !true || false ? 2 : 3").R(
                "_?_:_(_||_(_&&_(false~bool, !_(true~bool)~bool^logical_not)~bool^logical_and,\n" +
                "            false~bool)\n" + "        ~bool^logical_or,\n" + "      2~int,\n" + "      3~int)\n" +
                "  ~int^conditional").Type(Decls.Int),
            new TestCase().I("b\"abc\" + b\"def\"").R("_+_(b\"abc\"~bytes, b\"def\"~bytes)~bytes^add_bytes")
                .Type(Decls.Bytes),
            new TestCase().I("1.0 + 2.0 * 3.0 - 1.0 / 2.20202 != 66.6").R(
                    "_!=_(_-_(_+_(1~double, _*_(2~double, 3~double)~double^multiply_double)\n" +
                    "           ~double^add_double,\n" +
                    "           _/_(1~double, 2.20202~double)~double^divide_double)\n" +
                    "       ~double^subtract_double,\n" + "      66.6~double)\n" + "  ~bool^not_equals")
                .Type(Decls.Bool),
            new TestCase().I("null == null && null != null").R("_&&_(\n" + "	_==_(\n" + "		null~null,\n" +
                                                               "		null~null\n" + "	)~bool^equals,\n" +
                                                               "	_!=_(\n" + "		null~null,\n" + "		null~null\n" +
                                                               "	)~bool^not_equals\n" + ")~bool^logical_and")
                .Type(Decls.Bool),
            new TestCase().I("1 == 1 && 2 != 1")
                .R("_&&_(\n" + "	_==_(\n" + "		1~int,\n" + "		1~int\n" + "	)~bool^equals,\n" + "	_!=_(\n" +
                   "		2~int,\n" + "		1~int\n" + "	)~bool^not_equals\n" + ")~bool^logical_and").Type(Decls.Bool),
            new TestCase().I("1 + 2 * 3 - 1 / 2 == 6 % 1")
                .R(
                    " _==_(_-_(_+_(1~int, _*_(2~int, 3~int)~int^multiply_int64)~int^add_int64, _/_(1~int, 2~int)~int^divide_int64)~int^subtract_int64, _%_(6~int, 1~int)~int^modulo_int64)~bool^equals")
                .Type(Decls.Bool),
            new TestCase().I("\"abc\" + \"def\"").R("_+_(\"abc\"~string, \"def\"~string)~string^add_string")
                .Type(Decls.String),
            new TestCase().I("1u + 2u * 3u - 1u / 2u == 6u % 1u").R(
                "_==_(_-_(_+_(1u~uint, _*_(2u~uint, 3u~uint)~uint^multiply_uint64)\n" +
                "\t         ~uint^add_uint64,\n" + "\t         _/_(1u~uint, 2u~uint)~uint^divide_uint64)\n" +
                "\t     ~uint^subtract_uint64,\n" + "\t    _%_(6u~uint, 1u~uint)~uint^modulo_uint64)\n" +
                "\t~bool^equals").Type(Decls.Bool),
            new TestCase().I("x.single_int32 != null")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.Proto2Message")))).Error(
                    "ERROR: <input>:1:2: [internal] unexpected failed resolution of 'google.api.expr.test.v1.proto3.Proto2Message'\n" +
                    " | x.single_int32 != null\n" + " | .^"),
            new TestCase().I("x.single_value + 1 / x.single_struct.y == 23")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_==_(\n" + "\t\t\t_+_(\n" +
                   "\t\t\t  x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_value~dyn,\n" + "\t\t\t  _/_(\n" +
                   "\t\t\t\t1~int,\n" +
                   "\t\t\t\tx~google.api.expr.test.v1.proto3.TestAllTypes^x.single_struct~map(string, dyn).y~dyn\n" +
                   "\t\t\t  )~int^divide_int64\n" + "\t\t\t)~int^add_int64,\n" + "\t\t\t23~int\n" +
                   "\t\t  )~bool^equals").Type(Decls.Bool),
            new TestCase().I("x.single_value[23] + x.single_struct['y']")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R("_+_(\n" + "_[_](\n" +
                    "  x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_value~dyn,\n" + "  23~int\n" +
                    ")~dyn^index_list|index_map,\n" + "_[_](\n" +
                    "  x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_struct~map(string, dyn),\n" +
                    "  \"y\"~string\n" + ")~dyn^index_map\n" +
                    ")~dyn^add_int64|add_uint64|add_double|add_string|add_bytes|add_list|add_timestamp_duration|add_duration_timestamp|add_duration_duration")
                .Type(Decls.Dyn),
            new TestCase().I("TestAllTypes.NestedEnum.BAR != 99").Container("google.api.expr.test.v1.proto3").R(
                "_!=_(google.api.expr.test.v1.proto3.TestAllTypes.NestedEnum.BAR\n" +
                "     ~int^google.api.expr.test.v1.proto3.TestAllTypes.NestedEnum.BAR,\n" + "    99~int)\n" +
                "~bool^not_equals").Type(Decls.Bool),
            new TestCase().I("size([] + [1])")
                .R("size(_+_([]~list(int), [1~int]~list(int))~list(int)^add_list)~int^size_list").Type(Decls.Int).Env(
                    new Env().Idents(Decls.NewVar("x",
                        Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))),
            new TestCase()
                .I("x[\"claims\"][\"groups\"][0].name == \"dummy\"\n" + "&& x.claims[\"exp\"] == y[1].time\n" +
                   "&& x.claims.structured == {'key': z}\n" + "&& z == 1.0")
                .R("_&&_(\n" + "	_&&_(\n" + "		_==_(\n" + "			_[_](\n" + "				_[_](\n" + "					_[_](\n" +
                   "						x~map(string, dyn)^x,\n" + "						\"claims\"~string\n" + "					)~dyn^index_map,\n" +
                   "					\"groups\"~string\n" + "				)~list(dyn)^index_map,\n" + "				0~int\n" +
                   "			)~dyn^index_list.name~dyn,\n" + "			\"dummy\"~string\n" + "		)~bool^equals,\n" + "		_==_(\n" +
                   "			_[_](\n" + "				x~map(string, dyn)^x.claims~dyn,\n" + "				\"exp\"~string\n" +
                   "			)~dyn^index_map,\n" + "			_[_](\n" + "				y~list(dyn)^y,\n" + "				1~int\n" +
                   "			)~dyn^index_list.time~dyn\n" + "		)~bool^equals\n" + "	)~bool^logical_and,\n" + "	_&&_(\n" +
                   "		_==_(\n" + "			x~map(string, dyn)^x.claims~dyn.structured~dyn,\n" + "		  {\n" +
                   "				\"key\"~string:z~dyn^z\n" + "			}~map(string, dyn)\n" + "		)~bool^equals,\n" + "		_==_(\n" +
                   "			z~dyn^z,\n" + "			1~double\n" + "		)~bool^equals\n" + "	)~bool^logical_and\n" +
                   ")~bool^logical_and")
                .Env(new Env().Idents(Decls.NewVar("x", Decls.NewObjectType("google.protobuf.Struct")),
                    Decls.NewVar("y", Decls.NewObjectType("google.protobuf.ListValue")),
                    Decls.NewVar("z", Decls.NewObjectType("google.protobuf.Value")))).Type(Decls.Bool),
            new TestCase().I("x + y")
                .Env(new Env().Idents(
                    Decls.NewVar("x",
                        Decls.NewListType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))),
                    Decls.NewVar("y", Decls.NewListType(Decls.Int)))).Error(
                    "ERROR: <input>:1:3: found no matching overload for '_+_' applied to '(list(google.api.expr.test.v1.proto3.TestAllTypes), list(int))'\n" +
                    " | x + y\n" + " | ..^"),
            new TestCase().I("x[1u]")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewListType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))))
                .Error(
                    "ERROR: <input>:1:2: found no matching overload for '_[_]' applied to '(list(google.api.expr.test.v1.proto3.TestAllTypes), uint)'\n" +
                    " | x[1u]\n" + " | .^"),
            new TestCase().I("(x + x)[1].single_int32 == size(x)")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewListType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))).R(
                    "_==_(_[_](_+_(x~list(google.api.expr.test.v1.proto3.TestAllTypes)^x,\n" +
                    "                x~list(google.api.expr.test.v1.proto3.TestAllTypes)^x)\n" +
                    "            ~list(google.api.expr.test.v1.proto3.TestAllTypes)^add_list,\n" +
                    "           1~int)\n" + "       ~google.api.expr.test.v1.proto3.TestAllTypes^index_list\n" +
                    "       .\n" + "       single_int32\n" + "       ~int,\n" +
                    "      size(x~list(google.api.expr.test.v1.proto3.TestAllTypes)^x)~int^size_list)\n" +
                    "  ~bool^equals").Type(Decls.Bool),
            new TestCase().I("x.repeated_int64[x.single_int32] == 23")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_==_(_[_](x~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" +
                   "           x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int32~int)\n" +
                   "       ~int^index_list,\n" + "      23~int)\n" + "  ~bool^equals").Type(Decls.Bool),
            new TestCase().I("size(x.map_int64_nested_type) == 0")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R(
                    "_==_(size(x~google.api.expr.test.v1.proto3.TestAllTypes^x.map_int64_nested_type\n" +
                    "            ~map(int, google.api.expr.test.v1.proto3.NestedTestAllTypes))\n" +
                    "       ~int^size_map,\n" + "      0~int)\n" + "  ~bool^equals").Type(Decls.Bool),
            new TestCase().I("x.all(y, y == true)").Env(new Env().Idents(Decls.NewVar("x", Decls.Bool)))
                .R("__comprehension__(\n" + "// Variable\n" + "y,\n" + "// Target\n" + "x~bool^x,\n" +
                   "// Accumulator\n" + "__result__,\n" + "// Init\n" + "true~bool,\n" + "// LoopCondition\n" +
                   "@not_strictly_false(\n" + "	__result__~bool^__result__\n" + ")~bool^not_strictly_false,\n" +
                   "// LoopStep\n" + "_&&_(\n" + "	__result__~bool^__result__,\n" + "	_==_(\n" + "	y~!error!^y,\n" +
                   "	true~bool\n" + "	)~bool^equals\n" + ")~bool^logical_and,\n" + "// Result\n" +
                   "__result__~bool^__result__)~bool")
                .Error(
                    "ERROR: <input>:1:1: expression of type 'bool' cannot be range of a comprehension (must be list, map, or dynamic)\n" +
                    " | x.all(y, y == true)\n" + " | ^"),
            new TestCase().I("x.repeated_int64.map(x, double(x))")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R("__comprehension__(\n" +
                    "// Variable\n" + "x,\n" + "// Target\n" +
                    "x~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" + "// Accumulator\n" +
                    "__result__,\n" + "// Init\n" + "[]~list(double),\n" + "// LoopCondition\n" + "true~bool,\n" +
                    "// LoopStep\n" + "_+_(\n" + "  __result__~list(double)^__result__,\n" + "  [\n" + "    double(\n" +
                    "      x~int^x\n" + "    )~double^int64_to_double\n" + "  ]~list(double)\n" +
                    ")~list(double)^add_list,\n" + "// Result\n" + "__result__~list(double)^__result__)~list(double)")
                .Type(Decls.NewListType(Decls.Double)),
            new TestCase().I("x.repeated_int64.map(x, x > 0, double(x))")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("__comprehension__(\n" + "// Variable\n" + "x,\n" + "// Target\n" +
                   "x~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" + "// Accumulator\n" +
                   "__result__,\n" + "// Init\n" + "[]~list(double),\n" + "// LoopCondition\n" + "true~bool,\n" +
                   "// LoopStep\n" + "_?_:_(\n" + "  _>_(\n" + "    x~int^x,\n" + "    0~int\n" +
                   "  )~bool^greater_int64,\n" + "  _+_(\n" + "    __result__~list(double)^__result__,\n" + "    [\n" +
                   "      double(\n" + "        x~int^x\n" + "      )~double^int64_to_double\n" +
                   "    ]~list(double)\n" + "  )~list(double)^add_list,\n" + "  __result__~list(double)^__result__\n" +
                   ")~list(double)^conditional,\n" + "// Result\n" + "__result__~list(double)^__result__)~list(double)")
                .Type(Decls.NewListType(Decls.Double)),
            new TestCase().I("x[2].single_int32 == 23")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewMapType(Decls.String,
                        Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))).Error(
                    "ERROR: <input>:1:2: found no matching overload for '_[_]' applied to '(map(string, google.api.expr.test.v1.proto3.TestAllTypes), int)'\n" +
                    " | x[2].single_int32 == 23\n" + " | .^"),
            new TestCase().I("x[\"a\"].single_int32 == 23")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewMapType(Decls.String,
                        Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))))
                .R("_==_(_[_](x~map(string, google.api.expr.test.v1.proto3.TestAllTypes)^x, \"a\"~string)\n" +
                   "~google.api.expr.test.v1.proto3.TestAllTypes^index_map\n" + ".\n" + "single_int32\n" + "~int,\n" +
                   "23~int)\n" + "~bool^equals").Type(Decls.Bool),
            new TestCase().I("x.single_nested_message.bb == 43 && has(x.single_nested_message)")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R("_&&_(\n" + "  _==_(\n" +
                    "    x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_nested_message~google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage.bb~int,\n" +
                    "    43~int\n" + "  )~bool^equals,\n" +
                    "  x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_nested_message~test-only~~bool\n" +
                    ")~bool^logical_and").Type(Decls.Bool),
            new TestCase()
                .I("x.single_nested_message.undefined == x.undefined && has(x.single_int32) && has(x.repeated_int32)")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .Error("ERROR: <input>:1:24: undefined field 'undefined'\n" +
                       " | x.single_nested_message.undefined == x.undefined && has(x.single_int32) && has(x.repeated_int32)\n" +
                       " | .......................^\n" + "ERROR: <input>:1:39: undefined field 'undefined'\n" +
                       " | x.single_nested_message.undefined == x.undefined && has(x.single_int32) && has(x.repeated_int32)\n" +
                       " | ......................................^"),
            new TestCase().I("x.single_nested_message != null")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R(
                    "_!=_(x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_nested_message\n" +
                    "~google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage,\n" + "null~null)\n" +
                    "~bool^not_equals").Type(Decls.Bool),
            new TestCase().I("x.single_int64 != null")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .Error("ERROR: <input>:1:16: found no matching overload for '_!=_' applied to '(int, null)'\n" +
                       " | x.single_int64 != null\n" + " | ...............^"),
            new TestCase().I("x.single_int64_wrapper == null")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_==_(x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int64_wrapper\n" + "~wrapper(int),\n" +
                   "null~null)\n" + "~bool^equals").Type(Decls.Bool),
            new TestCase()
                .I("x.single_bool_wrapper\n" + "&& x.single_bytes_wrapper == b'hi'\n" +
                   "&& x.single_double_wrapper != 2.0\n" + "&& x.single_float_wrapper == 1.0\n" +
                   "&& x.single_int32_wrapper != 2\n" + "&& x.single_int64_wrapper == 1\n" +
                   "&& x.single_string_wrapper == 'hi'\n" + "&& x.single_uint32_wrapper == 1u\n" +
                   "&& x.single_uint64_wrapper != 42u")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_&&_(\n" + "	_&&_(\n" + "		_&&_(\n" + "		_&&_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_bool_wrapper~wrapper(bool),\n" +
                   "			_==_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_bytes_wrapper~wrapper(bytes),\n" +
                   "			b\"hi\"~bytes\n" + "			)~bool^equals\n" + "		)~bool^logical_and,\n" + "		_!=_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_double_wrapper~wrapper(double),\n" +
                   "			2~double\n" + "		)~bool^not_equals\n" + "		)~bool^logical_and,\n" + "		_&&_(\n" + "		_==_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_float_wrapper~wrapper(double),\n" +
                   "			1~double\n" + "		)~bool^equals,\n" + "		_!=_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int32_wrapper~wrapper(int),\n" +
                   "			2~int\n" + "		)~bool^not_equals\n" + "		)~bool^logical_and\n" + "	)~bool^logical_and,\n" +
                   "	_&&_(\n" + "		_&&_(\n" + "		_==_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int64_wrapper~wrapper(int),\n" +
                   "			1~int\n" + "		)~bool^equals,\n" + "		_==_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_string_wrapper~wrapper(string),\n" +
                   "			\"hi\"~string\n" + "		)~bool^equals\n" + "		)~bool^logical_and,\n" + "		_&&_(\n" + "		_==_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_uint32_wrapper~wrapper(uint),\n" +
                   "			1u~uint\n" + "		)~bool^equals,\n" + "		_!=_(\n" +
                   "			x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_uint64_wrapper~wrapper(uint),\n" +
                   "			42u~uint\n" + "		)~bool^not_equals\n" + "		)~bool^logical_and\n" + "	)~bool^logical_and\n" +
                   ")~bool^logical_and").Type(Decls.Bool),
            new TestCase()
                .I("x.single_bool_wrapper == google.protobuf.BoolValue{value: true}\n" +
                   "&& x.single_bytes_wrapper == google.protobuf.BytesValue{value: b'hi'}\n" +
                   "&& x.single_double_wrapper != google.protobuf.DoubleValue{value: 2.0}\n" +
                   "&& x.single_float_wrapper == google.protobuf.FloatValue{value: 1.0}\n" +
                   "&& x.single_int32_wrapper != google.protobuf.Int32Value{value: -2}\n" +
                   "&& x.single_int64_wrapper == google.protobuf.Int64Value{value: 1}\n" +
                   "&& x.single_string_wrapper == google.protobuf.StringValue{value: 'hi'}\n" +
                   "&& x.single_string_wrapper == google.protobuf.Value{string_value: 'hi'}\n" +
                   "&& x.single_uint32_wrapper == google.protobuf.UInt32Value{value: 1u}\n" +
                   "&& x.single_uint64_wrapper != google.protobuf.UInt64Value{value: 42u}")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).Type(Decls.Bool),
            new TestCase()
                .I("x.single_bool_wrapper == google.protobuf.BoolValue{value: true}\n" +
                   "&& x.single_bytes_wrapper == google.protobuf.BytesValue{value: b'hi'}\n" +
                   "&& x.single_double_wrapper != google.protobuf.DoubleValue{value: 2.0}\n" +
                   "&& x.single_float_wrapper == google.protobuf.FloatValue{value: 1.0}\n" +
                   "&& x.single_int32_wrapper != google.protobuf.Int32Value{value: -2}\n" +
                   "&& x.single_int64_wrapper == google.protobuf.Int64Value{value: 1}\n" +
                   "&& x.single_string_wrapper == google.protobuf.StringValue{value: 'hi'}\n" +
                   "&& x.single_string_wrapper == google.protobuf.Value{string_value: 'hi'}")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).Type(Decls.Bool),
            new TestCase().I("x.repeated_int64.exists(y, y > 10) && y < 5")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).Error(
                    "ERROR: <input>:1:39: undeclared reference to 'y' (in container '')\n" +
                    " | x.repeated_int64.exists(y, y > 10) && y < 5\n" + " | ......................................^"),
            new TestCase()
                .I(
                    "x.repeated_int64.all(e, e > 0) && x.repeated_int64.exists(e, e < 0) && x.repeated_int64.exists_one(e, e == 0)")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_&&_(\n" + "\t\t\t_&&_(\n" + "\t\t\t  __comprehension__(\n" + "\t\t\t\t// Variable\n" +
                   "\t\t\t\te,\n" + "\t\t\t\t// Target\n" +
                   "\t\t\t\tx~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" +
                   "\t\t\t\t// Accumulator\n" + "\t\t\t\t__result__,\n" + "\t\t\t\t// Init\n" + "\t\t\t\ttrue~bool,\n" +
                   "\t\t\t\t// LoopCondition\n" + "\t\t\t\t@not_strictly_false(\n" +
                   "\t\t\t\t  __result__~bool^__result__\n" + "\t\t\t\t)~bool^not_strictly_false,\n" +
                   "\t\t\t\t// LoopStep\n" + "\t\t\t\t_&&_(\n" + "\t\t\t\t  __result__~bool^__result__,\n" +
                   "\t\t\t\t  _>_(\n" + "\t\t\t\t\te~int^e,\n" + "\t\t\t\t\t0~int\n" +
                   "\t\t\t\t  )~bool^greater_int64\n" + "\t\t\t\t)~bool^logical_and,\n" + "\t\t\t\t// Result\n" +
                   "\t\t\t\t__result__~bool^__result__)~bool,\n" + "\t\t\t  __comprehension__(\n" +
                   "\t\t\t\t// Variable\n" + "\t\t\t\te,\n" + "\t\t\t\t// Target\n" +
                   "\t\t\t\tx~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" +
                   "\t\t\t\t// Accumulator\n" + "\t\t\t\t__result__,\n" + "\t\t\t\t// Init\n" +
                   "\t\t\t\tfalse~bool,\n" + "\t\t\t\t// LoopCondition\n" + "\t\t\t\t@not_strictly_false(\n" +
                   "\t\t\t\t  !_(\n" + "\t\t\t\t\t__result__~bool^__result__\n" + "\t\t\t\t  )~bool^logical_not\n" +
                   "\t\t\t\t)~bool^not_strictly_false,\n" + "\t\t\t\t// LoopStep\n" + "\t\t\t\t_||_(\n" +
                   "\t\t\t\t  __result__~bool^__result__,\n" + "\t\t\t\t  _<_(\n" + "\t\t\t\t\te~int^e,\n" +
                   "\t\t\t\t\t0~int\n" + "\t\t\t\t  )~bool^less_int64\n" + "\t\t\t\t)~bool^logical_or,\n" +
                   "\t\t\t\t// Result\n" + "\t\t\t\t__result__~bool^__result__)~bool\n" +
                   "\t\t\t)~bool^logical_and,\n" + "\t\t\t__comprehension__(\n" + "\t\t\t  // Variable\n" +
                   "\t\t\t  e,\n" + "\t\t\t  // Target\n" +
                   "\t\t\t  x~google.api.expr.test.v1.proto3.TestAllTypes^x.repeated_int64~list(int),\n" +
                   "\t\t\t  // Accumulator\n" + "\t\t\t  __result__,\n" + "\t\t\t  // Init\n" + "\t\t\t  0~int,\n" +
                   "\t\t\t  // LoopCondition\n" + "\t\t\t  true~bool,\n" + "\t\t\t  // LoopStep\n" +
                   "\t\t\t  _?_:_(\n" + "\t\t\t\t_==_(\n" + "\t\t\t\t  e~int^e,\n" + "\t\t\t\t  0~int\n" +
                   "\t\t\t\t)~bool^equals,\n" + "\t\t\t\t_+_(\n" + "\t\t\t\t  __result__~int^__result__,\n" +
                   "\t\t\t\t  1~int\n" + "\t\t\t\t)~int^add_int64,\n" + "\t\t\t\t__result__~int^__result__\n" +
                   "\t\t\t  )~int^conditional,\n" + "\t\t\t  // Result\n" + "\t\t\t  _==_(\n" +
                   "\t\t\t\t__result__~int^__result__,\n" + "\t\t\t\t1~int\n" + "\t\t\t  )~bool^equals)~bool\n" +
                   "\t\t  )~bool^logical_and").Type(Decls.Bool),
            new TestCase().I("x.all(e, 0)")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).Error(
                    "ERROR: <input>:1:1: expression of type 'google.api.expr.test.v1.proto3.TestAllTypes' cannot be range of a comprehension (must be list, map, or dynamic)\n" +
                    " | x.all(e, 0)\n" + " | ^\n" +
                    "ERROR: <input>:1:6: found no matching overload for '_&&_' applied to '(bool, int)'\n" +
                    " | x.all(e, 0)\n" + " | .....^"),
            new TestCase().I("lists.filter(x, x > 1.5)").R("__comprehension__(\n" + "\t\t\t// Variable\n" +
                                                           "\t\t\tx,\n" + "\t\t\t// Target\n" +
                                                           "\t\t\tlists~dyn^lists,\n" + "\t\t\t// Accumulator\n" +
                                                           "\t\t\t__result__,\n" + "\t\t\t// Init\n" +
                                                           "\t\t\t[]~list(dyn),\n" + "\t\t\t// LoopCondition\n" +
                                                           "\t\t\ttrue~bool,\n" + "\t\t\t// LoopStep\n" +
                                                           "\t\t\t_?_:_(\n" + "\t\t\t  _>_(\n" +
                                                           "\t\t\t\tx~dyn^x,\n" + "\t\t\t\t1.5~double\n" +
                                                           "\t\t\t  )~bool^greater_double,\n" + "\t\t\t  _+_(\n" +
                                                           "\t\t\t\t__result__~list(dyn)^__result__,\n" +
                                                           "\t\t\t\t[\n" + "\t\t\t\t  x~dyn^x\n" +
                                                           "\t\t\t\t]~list(dyn)\n" +
                                                           "\t\t\t  )~list(dyn)^add_list,\n" +
                                                           "\t\t\t  __result__~list(dyn)^__result__\n" +
                                                           "\t\t\t)~list(dyn)^conditional,\n" +
                                                           "\t\t\t// Result\n" +
                                                           "\t\t\t__result__~list(dyn)^__result__)~list(dyn)")
                .Type(Decls.NewListType(Decls.Dyn)).Env(new Env().Idents(Decls.NewVar("lists", Decls.Dyn))),
            new TestCase().I("google.api.expr.test.v1.proto3.TestAllTypes")
                .R("google.api.expr.test.v1.proto3.TestAllTypes\n" +
                   "\t~type(google.api.expr.test.v1.proto3.TestAllTypes)\n" +
                   "\t^google.api.expr.test.v1.proto3.TestAllTypes")
                .Type(Decls.NewTypeType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))),
            new TestCase().I("proto3.TestAllTypes").Container("google.api.expr.test.v1")
                .R("google.api.expr.test.v1.proto3.TestAllTypes\n" +
                   "\t~type(google.api.expr.test.v1.proto3.TestAllTypes)\n" +
                   "\t^google.api.expr.test.v1.proto3.TestAllTypes")
                .Type(Decls.NewTypeType(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))),
            new TestCase().I("1 + x")
                .Error("ERROR: <input>:1:5: undeclared reference to 'x' (in container '')\n" + " | 1 + x\n" +
                       " | ....^"),
            new TestCase()
                .I("x == google.protobuf.Any{\n" +
                   "\t\t\t\ttype_url:'types.googleapis.com/google.api.expr.test.v1.proto3.TestAllTypes'\n" +
                   "\t\t\t} && x.single_nested_message.bb == 43\n" +
                   "\t\t\t|| x == google.api.expr.test.v1.proto3.TestAllTypes{}\n" + "\t\t\t|| y < x\n" +
                   "\t\t\t|| x >= x")
                .Env(new Env().Idents(Decls.NewVar("x", Decls.Any),
                    Decls.NewVar("y", Decls.NewWrapperType(Decls.Int))))
                .R("_||_(\n" + "\t_||_(\n" + "\t\t_&&_(\n" + "\t\t\t_==_(\n" + "\t\t\t\tx~any^x,\n" +
                   "\t\t\t\tgoogle.protobuf.Any{\n" +
                   "\t\t\t\t\ttype_url:\"types.googleapis.com/google.api.expr.test.v1.proto3.TestAllTypes\"~string\n" +
                   "\t\t\t\t}~any^google.protobuf.Any\n" + "\t\t\t)~bool^equals,\n" + "\t\t\t_==_(\n" +
                   "\t\t\t\tx~any^x.single_nested_message~dyn.bb~dyn,\n" + "\t\t\t\t43~int\n" +
                   "\t\t\t)~bool^equals\n" + "\t\t)~bool^logical_and,\n" + "\t\t_==_(\n" + "\t\t\tx~any^x,\n" +
                   "\t\t\tgoogle.api.expr.test.v1.proto3.TestAllTypes{}~google.api.expr.test.v1.proto3.TestAllTypes^google.api.expr.test.v1.proto3.TestAllTypes\n" +
                   "\t\t)~bool^equals\n" + "\t)~bool^logical_or,\n" + "\t_||_(\n" + "\t\t_<_(\n" +
                   "\t\t\ty~wrapper(int)^y,\n" + "\t\t\tx~any^x\n" + "\t\t)~bool^less_int64,\n" + "\t\t_>=_(\n" +
                   "\t\t\tx~any^x,\n" + "\t\t\tx~any^x\n" +
                   "\t\t)~bool^greater_equals_bool|greater_equals_int64|greater_equals_uint64|greater_equals_double|greater_equals_string|greater_equals_bytes|greater_equals_timestamp|greater_equals_duration\n" +
                   "\t)~bool^logical_or\n" + ")~bool^logical_or").Type(Decls.Bool),
            new TestCase().I("x").Container("container")
                .Env(new Env().Idents(Decls.NewVar("container.x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("container.x~google.api.expr.test.v1.proto3.TestAllTypes^container.x")
                .Type(Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")),
            new TestCase().I("list == .type([1]) && map == .type({1:2u})").R(
                "_&&_(_==_(list~type(list(dyn))^list,\n" +
                "           type([1~int]~list(int))~type(list(int))^type)\n" + "       ~bool^equals,\n" +
                "      _==_(map~type(map(dyn, dyn))^map,\n" +
                "            type({1~int : 2u~uint}~map(int, uint))~type(map(int, uint))^type)\n" +
                "        ~bool^equals)\n" + "  ~bool^logical_and").Type(Decls.Bool),
            /*
            new TestCase().I("list == .type([1]) && map == .type({1:2u})").R(
                "_&&_(\n"
                + "  _==_(\n"
                + "    list~type(list({ \"typeParam\": \"A\" }))^list,\n"
                + "    type(\n"
                + "      [\n"
                + "        1~int\n"
                + "      ]~list(int)\n"
                + "    )~type({ \"typeParam\": \"A\" })^type\n"
                + "  )~bool^equals,\n"
                + "  _==_(\n"
                + "    map~type(map({ \"typeParam\": \"A\" }, { \"typeParam\": \"B\" }))^map,\n"
                + "    type(\n"
                + "      {\n"
                + "        1~int:2u~uint\n"
                + "      }~map(int, uint)\n"
                + "    )~type({ \"typeParam\": \"A\" })^type\n"
                + "  )~bool^equals\n"
                + ")~bool^logical_and").Type(Decls.Bool),
                */
            new TestCase().I("myfun(1, true, 3u) + 1.myfun(false, 3u).myfun(true, 42u)")
                .Env(new Env().Functions(Decls.NewFunction("myfun",
                    Decls.NewInstanceOverload("myfun_instance", new List<Type> { Decls.Int, Decls.Bool, Decls.Uint },
                        Decls.Int),
                    Decls.NewOverload("myfun_static", new List<Type> { Decls.Int, Decls.Bool, Decls.Uint },
                        Decls.Int)))).R("_+_(\n" + "    \t\t  myfun(\n" + "    \t\t    1~int,\n" +
                                        "    \t\t    true~bool,\n" + "    \t\t    3u~uint\n" +
                                        "    \t\t  )~int^myfun_static,\n" + "    \t\t  1~int.myfun(\n" +
                                        "    \t\t    false~bool,\n" + "    \t\t    3u~uint\n" +
                                        "    \t\t  )~int^myfun_instance.myfun(\n" + "    \t\t    true~bool,\n" +
                                        "    \t\t    42u~uint\n" + "    \t\t  )~int^myfun_instance\n" +
                                        "    \t\t)~int^add_int64").Type(Decls.Int),
            new TestCase().I("size(x) > 4").Env(new Env()
                .Idents(Decls.NewVar("x", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))
                .Functions(Decls.NewFunction("size",
                    Decls.NewOverload("size_message",
                        new List<Type> { Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes") },
                        Decls.Int)))).Type(Decls.Bool),
            new TestCase().I("x.single_int64_wrapper + 1 != 23")
                .Env(new Env().Idents(Decls.NewVar("x",
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))).R(
                    "_!=_(_+_(x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int64_wrapper\n" +
                    "~wrapper(int),\n" + "1~int)\n" + "~int^add_int64,\n" + "23~int)\n" + "~bool^not_equals")
                .Type(Decls.Bool),
            new TestCase().I("x.single_int64_wrapper + y != 23")
                .Env(new Env().Idents(
                    Decls.NewVar("x", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")),
                    Decls.NewVar("y", Decls.NewObjectType("google.protobuf.Int32Value"))))
                .R("_!=_(\n" + "\t_+_(\n" +
                   "\t  x~google.api.expr.test.v1.proto3.TestAllTypes^x.single_int64_wrapper~wrapper(int),\n" +
                   "\t  y~wrapper(int)^y\n" + "\t)~int^add_int64,\n" + "\t23~int\n" + "  )~bool^not_equals")
                .Type(Decls.Bool),
            new TestCase().I("1 in [1, 2, 3]").R("@in(\n" + "    \t\t  1~int,\n" + "    \t\t  [\n" +
                                                 "    \t\t    1~int,\n" + "    \t\t    2~int,\n" +
                                                 "    \t\t    3~int\n" + "    \t\t  ]~list(int)\n" +
                                                 "    \t\t)~bool^in_list").Type(Decls.Bool),
            new TestCase().I("1 in dyn([1, 2, 3])").R("@in(\n" + "\t\t\t1~int,\n" + "\t\t\tdyn(\n" +
                                                      "\t\t\t  [\n" + "\t\t\t\t1~int,\n" + "\t\t\t\t2~int,\n" +
                                                      "\t\t\t\t3~int\n" + "\t\t\t  ]~list(int)\n" +
                                                      "\t\t\t)~dyn^to_dyn\n" + "\t\t  )~bool^in_list|in_map")
                .Type(Decls.Bool),
            new TestCase().I("type(null) == null_type").R("_==_(\n" + "    \t\t  type(\n" +
                                                          "    \t\t    null~null\n" +
                                                          "    \t\t  )~type(null)^type,\n" +
                                                          "    \t\t  null_type~type(null)^null_type\n" +
                                                          "    \t\t)~bool^equals").Type(Decls.Bool),
            /*
            new TestCase().I("type(null) == null_type").R("_==_(\n" + "  type(\n"
                                                                    + "    null~null\n"
                                                                    + "  )~type({ \"typeParam\": \"A\" })^type,\n"
                                                                    + "  null_type~type(null)^null_type\n"
                                                                    + ")~bool^equals").Type(Decls.Bool),
                                                                    
            new TestCase().I("type(type) == type")
                .R("_==_(\n" + "\t\t  type(\n" + "\t\t    type~type(type())^type\n" +
                   "\t\t  )~type(type(type()))^type,\n" + "\t\t  type~type(type())^type\n" + "\t\t)~bool^equals")
                .Type(Decls.Bool),
                /*
            new TestCase().I("type(type) == type").R(
                "_==_(\n"
                + "  type(\n"
                + "    type~type(type)^type\n"
                + "  )~type({ \"typeParam\": \"A\" })^type,\n"
                + "  type~type(type)^type\n"
                + ")~bool^equals").Type(Decls.Bool),
                */

            new TestCase().I("name in [1, 2u, 'string']")
                .Env(new Env().Idents(Decls.NewVar("name", Decls.String)).Functions(Decls.NewFunction(Operator.In.id,
                    Decls.NewOverload(Overloads.InList,
                        new List<Type> { Decls.String, Decls.NewListType(Decls.String) }, Decls.Bool))))
                .HomogeneousAggregateLiterals().DisableStdEnv()
                .R("@in(\n" + "\t\t\tname~string^name,\n" + "\t\t\t[\n" + "\t\t\t\t1~int,\n" + "\t\t\t\t2u~uint,\n" +
                   "\t\t\t\t\"string\"~string\n" + "\t\t\t]~list(string)\n" + "\t\t)~bool^in_list").Error(
                    "ERROR: <input>:1:13: expected type 'int' but found 'uint'\n" + " | name in [1, 2u, 'string']\n" +
                    " | ............^"),
            new TestCase().I("name in [1, 2, 3]")
                .Env(new Env().Idents(Decls.NewVar("name", Decls.String)).Functions(Decls.NewFunction(Operator.In.id,
                    Decls.NewOverload(Overloads.InList,
                        new List<Type> { Decls.String, Decls.NewListType(Decls.String) }, Decls.Bool))))
                .HomogeneousAggregateLiterals().DisableStdEnv()
                .R("@in(\n" + "\t\t\tname~string^name,\n" + "\t\t\t[\n" + "\t\t\t\t1~int,\n" + "\t\t\t\t2~int,\n" +
                   "\t\t\t\t3~int\n" + "\t\t\t]~list(int)\n" + "\t\t)~!error!")
                .Error("ERROR: <input>:1:6: found no matching overload for '@in' applied to '(string, list(int))'\n" +
                       " | name in [1, 2, 3]\n" + " | .....^"),
            new TestCase().I("name in [\"1\", \"2\", \"3\"]")
                .Env(new Env().Idents(Decls.NewVar("name", Decls.String)).Functions(Decls.NewFunction(Operator.In.id,
                    Decls.NewOverload(Overloads.InList,
                        new List<Type> { Decls.String, Decls.NewListType(Decls.String) }, Decls.Bool))))
                .HomogeneousAggregateLiterals().DisableStdEnv().R("@in(\n" + "\t\t\tname~string^name,\n" + "\t\t\t[\n" +
                                                                  "\t\t\t\t\"1\"~string,\n" +
                                                                  "\t\t\t\t\"2\"~string,\n" + "\t\t\t\t\"3\"~string\n" +
                                                                  "\t\t\t]~list(string)\n" + "\t\t)~bool^in_list")
                .Type(Decls.Bool),
            new TestCase().I("([[[1]], [[2]], [[3]]][0][0] + [2, 3, {'four': {'five': 'six'}}])[3]").R("_[_](\n" +
                "\t\t\t_+_(\n" + "\t\t\t\t_[_](\n" + "\t\t\t\t\t_[_](\n" + "\t\t\t\t\t\t[\n" + "\t\t\t\t\t\t\t[\n" +
                "\t\t\t\t\t\t\t\t[\n" + "\t\t\t\t\t\t\t\t\t1~int\n" + "\t\t\t\t\t\t\t\t]~list(int)\n" +
                "\t\t\t\t\t\t\t]~list(list(int)),\n" + "\t\t\t\t\t\t\t[\n" + "\t\t\t\t\t\t\t\t[\n" +
                "\t\t\t\t\t\t\t\t\t2~int\n" + "\t\t\t\t\t\t\t\t]~list(int)\n" +
                "\t\t\t\t\t\t\t]~list(list(int)),\n" + "\t\t\t\t\t\t\t[\n" + "\t\t\t\t\t\t\t\t[\n" +
                "\t\t\t\t\t\t\t\t\t3~int\n" + "\t\t\t\t\t\t\t\t]~list(int)\n" +
                "\t\t\t\t\t\t\t]~list(list(int))\n" + "\t\t\t\t\t\t]~list(list(list(int))),\n" +
                "\t\t\t\t\t\t0~int\n" + "\t\t\t\t\t)~list(list(int))^index_list,\n" + "\t\t\t\t\t0~int\n" +
                "\t\t\t\t)~list(int)^index_list,\n" + "\t\t\t\t[\n" + "\t\t\t\t\t2~int,\n" + "\t\t\t\t\t3~int,\n" +
                "\t\t\t\t\t{\n" + "\t\t\t\t\t\t\"four\"~string:{\n" +
                "\t\t\t\t\t\t\t\"five\"~string:\"six\"~string\n" + "\t\t\t\t\t\t}~map(string, string)\n" +
                "\t\t\t\t\t}~map(string, map(string, string))\n" + "\t\t\t\t]~list(dyn)\n" +
                "\t\t\t)~list(dyn)^add_list,\n" + "\t\t\t3~int\n" + "\t\t)~dyn^index_list").Type(Decls.Dyn),
            new TestCase().I("[1] + [dyn('string')]").R("_+_(\n" + "\t\t\t[\n" + "\t\t\t\t1~int\n" +
                                                        "\t\t\t]~list(int),\n" + "\t\t\t[\n" + "\t\t\t\tdyn(\n" +
                                                        "\t\t\t\t\t\"string\"~string\n" +
                                                        "\t\t\t\t)~dyn^to_dyn\n" + "\t\t\t]~list(dyn)\n" +
                                                        "\t\t)~list(dyn)^add_list")
                .Type(Decls.NewListType(Decls.Dyn)),
            new TestCase().I("[dyn('string')] + [1]").R("_+_(\n" + "\t\t\t[\n" + "\t\t\t\tdyn(\n" +
                                                        "\t\t\t\t\t\"string\"~string\n" +
                                                        "\t\t\t\t)~dyn^to_dyn\n" + "\t\t\t]~list(dyn),\n" +
                                                        "\t\t\t[\n" + "\t\t\t\t1~int\n" + "\t\t\t]~list(int)\n" +
                                                        "\t\t)~list(dyn)^add_list")
                .Type(Decls.NewListType(Decls.Dyn)),
            /* NOTE: original
            new TestCase().I("[].map(x, [].map(y, x in y && y in x))").Error(
                "ERROR: <input>:1:33: found no matching overload for '@in' applied to '(type_param: \"_var2\", type_param: \"_var0\")'\n" +
                " | [].map(x, [].map(y, x in y && y in x))\n" + " | ................................^"),
            */
            new TestCase().I("[].map(x, [].map(y, x in y && y in x))").Error(
                "ERROR: <input>:1:33: found no matching overload for '@in' applied to '({ \"typeParam\": \"_var2\" }, { \"typeParam\": \"_var0\" })'\n"
                + " | [].map(x, [].map(y, x in y && y in x))\n"
                + " | ................................^"),
            new TestCase().I("args.user[\"myextension\"].customAttributes.filter(x, x.name == \"hobbies\")").R(
                    "__comprehension__(\n" + "\t\t\t// Variable\n" + "\t\t\tx,\n" + "\t\t\t// Target\n" +
                    "\t\t\t_[_](\n" + "\t\t\targs~map(string, dyn)^args.user~dyn,\n" +
                    "\t\t\t\"myextension\"~string\n" + "\t\t\t)~dyn^index_map.customAttributes~dyn,\n" +
                    "\t\t\t// Accumulator\n" + "\t\t\t__result__,\n" + "\t\t\t// Init\n" + "\t\t\t[]~list(dyn),\n" +
                    "\t\t\t// LoopCondition\n" + "\t\t\ttrue~bool,\n" + "\t\t\t// LoopStep\n" + "\t\t\t_?_:_(\n" +
                    "\t\t\t_==_(\n" + "\t\t\t\tx~dyn^x.name~dyn,\n" + "\t\t\t\t\"hobbies\"~string\n" +
                    "\t\t\t)~bool^equals,\n" + "\t\t\t_+_(\n" + "\t\t\t\t__result__~list(dyn)^__result__,\n" +
                    "\t\t\t\t[\n" + "\t\t\t\tx~dyn^x\n" + "\t\t\t\t]~list(dyn)\n" + "\t\t\t)~list(dyn)^add_list,\n" +
                    "\t\t\t__result__~list(dyn)^__result__\n" + "\t\t\t)~list(dyn)^conditional,\n" +
                    "\t\t\t// Result\n" + "\t\t\t__result__~list(dyn)^__result__)~list(dyn)")
                .Env(new Env().Idents(Decls.NewVar("args", Decls.NewMapType(Decls.String, Decls.Dyn))))
                .Type(Decls.NewListType(Decls.Dyn)),
            new TestCase().I("a.b + 1 == a[0]").R("_==_(\n" + "\t\t\t_+_(\n" + "\t\t\t  a~dyn^a.b~dyn,\n" +
                                                  "\t\t\t  1~int\n" + "\t\t\t)~int^add_int64,\n" +
                                                  "\t\t\t_[_](\n" + "\t\t\t  a~dyn^a,\n" + "\t\t\t  0~int\n" +
                                                  "\t\t\t)~dyn^index_list|index_map\n" + "\t\t  )~bool^equals")
                .Env(new Env().Idents(Decls.NewVar("a", Decls.NewTypeParamType("T")))).Type(Decls.Bool),
            new TestCase()
                .I("!has(pb2.single_int64)\n" + "\t\t&& !has(pb2.repeated_int32)\n" +
                   "\t\t&& !has(pb2.map_string_string)\n" + "\t\t&& !has(pb3.single_int64)\n" +
                   "\t\t&& !has(pb3.repeated_int32)\n" + "\t\t&& !has(pb3.map_string_string)")
                .Env(new Env().Idents(
                    Decls.NewVar("pb2", Decls.NewObjectType("google.api.expr.test.v1.proto2.TestAllTypes")),
                    Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))))
                .R("_&&_(\n" + "\t_&&_(\n" + "\t  _&&_(\n" + "\t\t!_(\n" +
                   "\t\t  pb2~google.api.expr.test.v1.proto2.TestAllTypes^pb2.single_int64~test-only~~bool\n" +
                   "\t\t)~bool^logical_not,\n" + "\t\t!_(\n" +
                   "\t\t  pb2~google.api.expr.test.v1.proto2.TestAllTypes^pb2.repeated_int32~test-only~~bool\n" +
                   "\t\t)~bool^logical_not\n" + "\t  )~bool^logical_and,\n" + "\t  !_(\n" +
                   "\t\tpb2~google.api.expr.test.v1.proto2.TestAllTypes^pb2.map_string_string~test-only~~bool\n" +
                   "\t  )~bool^logical_not\n" + "\t)~bool^logical_and,\n" + "\t_&&_(\n" + "\t  _&&_(\n" + "\t\t!_(\n" +
                   "\t\t  pb3~google.api.expr.test.v1.proto3.TestAllTypes^pb3.single_int64~test-only~~bool\n" +
                   "\t\t)~bool^logical_not,\n" + "\t\t!_(\n" +
                   "\t\t  pb3~google.api.expr.test.v1.proto3.TestAllTypes^pb3.repeated_int32~test-only~~bool\n" +
                   "\t\t)~bool^logical_not\n" + "\t  )~bool^logical_and,\n" + "\t  !_(\n" +
                   "\t\tpb3~google.api.expr.test.v1.proto3.TestAllTypes^pb3.map_string_string~test-only~~bool\n" +
                   "\t  )~bool^logical_not\n" + "\t)~bool^logical_and\n" + "  )~bool^logical_and").Type(Decls.Bool),
            new TestCase().I("TestAllTypes{}.repeated_nested_message").Container("google.api.expr.test.v1.proto2")
                .R("google.api.expr.test.v1.proto2.TestAllTypes{}~google.api.expr.test.v1.proto2.TestAllTypes^\n" +
                   "\t\tgoogle.api.expr.test.v1.proto2.TestAllTypes.repeated_nested_message\n" +
                   "\t\t~list(google.api.expr.test.v1.proto2.TestAllTypes.NestedMessage)").Type(
                    Decls.NewListType(
                        Decls.NewObjectType("google.api.expr.test.v1.proto2.TestAllTypes.NestedMessage"))),
            new TestCase().I("TestAllTypes{}.repeated_nested_message").Container("google.api.expr.test.v1.proto3")
                .R("google.api.expr.test.v1.proto3.TestAllTypes{}~google.api.expr.test.v1.proto3.TestAllTypes^\n" +
                   "\t\tgoogle.api.expr.test.v1.proto3.TestAllTypes.repeated_nested_message\n" +
                   "\t\t~list(google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage)")
                .Type(Decls.NewListType(
                    Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage"))),
            new TestCase().I("base64.encode('hello')")
                .Env(new Env().Functions(Decls.NewFunction("base64.encode",
                    Decls.NewOverload("base64_encode_string", new List<Type> { Decls.String }, Decls.String))))
                .R("base64.encode(\n" + "\t\t\t\"hello\"~string\n" + "\t\t)~string^base64_encode_string")
                .Type(Decls.String),
            new TestCase().I("encode('hello')").Container("base64")
                .Env(new Env().Functions(Decls.NewFunction("base64.encode",
                    Decls.NewOverload("base64_encode_string", new List<Type> { Decls.String }, Decls.String))))
                .R("base64.encode(\n" + "\t\t\t\"hello\"~string\n" + "\t\t)~string^base64_encode_string")
                .Type(Decls.String)
        };
    }

    [TestCaseSource(nameof(CheckTestCases))]
    public virtual void Check(TestCase tc)
    {
        if (tc.disabled != null) return;

        var src = ISource.NewTextSource(tc.i);
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.Errors.GetErrors, Is.Empty);

        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(
            new TestAllTypes(),
            new Google.Api.Expr.Test.V1.Proto3.TestAllTypes());
        var cont = Container.NewContainer(Container.Name(tc.container));
        var env = CheckerEnv.NewStandardCheckerEnv(cont!, reg);
        if (tc.disableStdEnv) env = CheckerEnv.NewCheckerEnv(cont!, reg);

        if (tc.homogeneousAggregateLiterals) env.EnableDynamicAggregateLiterals(false);

        if (tc.env != null)
        {
            if (tc.env.idents != null)
                foreach (var ident in tc.env.idents)
                    env.Add(ident);

            if (tc.env.functions != null)
                foreach (var fn in tc.env.functions)
                    env.Add(fn);
        }

        var checkResult = Checker.Check(parsed, src, env);
        if (checkResult.HasErrors())
        {
            var errorString = checkResult.Errors.ToDisplayString();
            var b = errorString.Equals(tc.error);
            if (tc.error != null)
                Assert.That(errorString, Is.EqualTo(tc.error));
            else
                Assert.Fail("Unexpected type-check errors: {0}", errorString);
        }
        else if (tc.error != null)
        {
            Assert.That(tc.error, Is.Null);
        }

        var actual = checkResult.CheckedExpr.TypeMap[parsed.Expr!.Id];
        if (tc.error == null)
            if (actual == null || !actual.Equals(tc.type))
                Assert.Fail("Type Error: '{0}' vs expected '{1}'", actual, tc.type);

        if (tc.r != null)
        {
            var actualStr = Printer.Print(checkResult.CheckedExpr.Expr, checkResult.CheckedExpr);
            var actualCmp = Regex.Replace(actualStr, "[ \n\t]", "");
            var rCmp = Regex.Replace(tc.r, "[ \n\t]", "");
            var b = actualCmp.Equals(rCmp);
            Assert.That(actualCmp, Is.EqualTo(rCmp));
        }
    }

    public class TestCase
    {
        /// <summary>
        ///     Container is the container name to use for test.
        /// </summary>
        internal string container = "";

        internal string? disabled;

        /// <summary>
        ///     DisableStdEnv indicates whether the standard functions should be disabled.
        /// </summary>
        internal bool disableStdEnv;

        /// <summary>
        ///     Env is the environment to use for testing.
        /// </summary>
        internal Env? env;

        /// <summary>
        ///     Error is the expected error for negative test cases.
        /// </summary>
        internal string? error;

        /// <summary>
        ///     HomogeneousAggregateLiterals indicates whether list and map literals must have homogeneous
        ///     element types, false by default.
        /// </summary>
        internal bool homogeneousAggregateLiterals;

        // I contains the input expression to be parsed. */
        internal string i;

        // R contains the result output. */
        internal string? r;

        /// <summary>
        ///     Type is the expected type of the expression
        /// </summary>
        internal Type? type;

        public override string ToString()
        {
            return i;
        }

        internal virtual TestCase Disabled(string reason)
        {
            disabled = reason;
            return this;
        }

        internal virtual TestCase I(string i)
        {
            this.i = i;
            return this;
        }

        internal virtual TestCase R(string r)
        {
            this.r = r;
            return this;
        }

        internal virtual TestCase Type(Type type)
        {
            this.type = type;
            return this;
        }

        internal virtual TestCase Container(string container)
        {
            this.container = container;
            return this;
        }

        internal virtual TestCase Env(Env env)
        {
            this.env = env;
            return this;
        }

        internal virtual TestCase Error(string error)
        {
            this.error = error;
            return this;
        }

        internal virtual TestCase DisableStdEnv()
        {
            disableStdEnv = true;
            return this;
        }

        internal virtual TestCase HomogeneousAggregateLiterals()
        {
            homogeneousAggregateLiterals = true;
            return this;
        }
    }

    internal class Env
    {
        internal Decl[]? functions;
        internal Decl[]? idents;

        internal virtual Env Idents(params Decl[] idents)
        {
            this.idents = idents;
            return this;
        }

        internal virtual Env Functions(params Decl[] functions)
        {
            this.functions = functions;
            return this;
        }
    }
}