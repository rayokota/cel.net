using System.Text;
using Cel.Checker;
using Cel.Common;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter.Functions;
using Google.Api.Expr.Test.V1.Proto2;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using Container = Cel.Common.Containers.Container;
using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
using Type = Google.Api.Expr.V1Alpha1.Type;
using Value = Google.Api.Expr.V1Alpha1.Value;

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
namespace Cel.Interpreter;

using TestAllTypesPb2 = TestAllTypes;
using TestAllTypesPb3 = Google.Api.Expr.Test.V1.Proto3.TestAllTypes;
using NestedTestAllTypesPb2 = NestedTestAllTypes;
using NestedTestAllTypesPb3 = Google.Api.Expr.Test.V1.Proto3.NestedTestAllTypes;
using Message = IMessage;

internal class InterpreterTest
{
    private static IVal Base64Encode(IVal val)
    {
        if (!(val is StringT)) return Err.NoSuchOverload(val, "base64Encode", "", new IVal[] { });


        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(val.Value().ToString()));
        return StringT.StringOf(b64);
    }

    internal static TestCase[] TestCases()
    {
        var t1 = new TestAllTypesPb2();
        t1.SingleInt32 = 150;

        var any1 = new Any();
        any1.TypeUrl = "type.googleapis.com/google.api.expr.test.v1.proto2.TestAllTypes";
        any1.Value = ByteString.CopyFrom(8, 150, 1);
        var v1 = new Value();
        v1.ObjectValue = any1;

        var t2 = new TestAllTypesPb3();
        t2.StandaloneEnum = TestAllTypesPb3.Types.NestedEnum.Baz;

        IDictionary<long, NestedTestAllTypesPb3> dict1 = new Dictionary<long, NestedTestAllTypesPb3>();
        dict1.Add(1, new NestedTestAllTypesPb3());
        var mf1 = new MapField<long, NestedTestAllTypesPb3>();
        mf1.Add(dict1);
        var t3 = new TestAllTypesPb3();
        t3.RepeatedBool.Add(false);
        t3.MapInt64NestedType.Add(mf1);

        var c1 = new Constant();
        c1.StringValue = "oneof_test";
        var expr1 = new Expr();
        expr1.Id = 1;
        expr1.ConstExpr = c1;

        var d1 = new Duration();
        d1.Seconds = 123;
        d1.Nanos = 123456789;

        var t4 = new TestAllTypesPb3();
        t4.RepeatedNestedEnum.Add(TestAllTypesPb3.Types.NestedEnum.Foo);
        t4.RepeatedNestedEnum.Add(TestAllTypesPb3.Types.NestedEnum.Baz);
        t4.RepeatedNestedEnum.Add(TestAllTypesPb3.Types.NestedEnum.Bar);
        t4.RepeatedInt32.Add(0);
        t4.RepeatedInt32.Add(2);

        var ts1 = new Timestamp();
        ts1.Seconds = 514862620;


        IDictionary<long, NestedTestAllTypesPb2> dict2 = new Dictionary<long, NestedTestAllTypesPb2>();
        dict2.Add(1, new NestedTestAllTypesPb2());
        var mf2 = new MapField<long, NestedTestAllTypesPb2>();
        mf2.Add(dict2);
        var t5 = new TestAllTypesPb2();
        t5.RepeatedBool.Add(false);
        t5.MapInt64NestedType.Add(mf2);

        var n1 = new TestAllTypesPb3.Types.NestedMessage();
        n1.Bb = 1234;
        var t6 = new TestAllTypesPb3();
        t6.SingleNestedMessage = n1;


        var t7 = new TestAllTypesPb3();
        t7.SingleInt32 = 1;
        var nt1 = new NestedTestAllTypesPb3();
        nt1.Payload = t7;
        var nt2 = new NestedTestAllTypesPb3();
        nt2.Child = nt1;
        IDictionary<long, NestedTestAllTypesPb3> dict3 = new Dictionary<long, NestedTestAllTypesPb3>();
        dict3.Add(0, nt2);
        var mf3 = new MapField<long, NestedTestAllTypesPb3>();
        mf3.Add(dict3);
        var t8 = new TestAllTypesPb3();
        t8.MapInt64NestedType.Add(mf3);

        var v2 = new Google.Protobuf.WellKnownTypes.Value();
        v2.StringValue = "world";
        var l1 = new ListValue();
        l1.Values.Add(v2);
        var v3 = new Google.Protobuf.WellKnownTypes.Value();
        v3.ListValue = l1;
        IDictionary<string, Google.Protobuf.WellKnownTypes.Value> dict4 =
            new Dictionary<string, Google.Protobuf.WellKnownTypes.Value>();
        dict4.Add("list", v3);
        var mf4 = new MapField<string, Google.Protobuf.WellKnownTypes.Value>();
        mf4.Add(dict4);
        var s1 = new Struct();
        s1.Fields.Add(mf4);
        var v4 = new Google.Protobuf.WellKnownTypes.Value();
        v4.StructValue = s1;

        var t9 = new TestAllTypesPb3();
        t9.RepeatedNestedEnum.Add(TestAllTypesPb3.Types.NestedEnum.Bar);

        var t10 = new TestAllTypesPb3();
        t10.SingleInt64Wrapper = 0;
        t10.SingleStringWrapper = "hello";

        var t11 = new TestAllTypesPb3();
        t11.SingleUint64 = 10;

        var t12 = new TestAllTypesPb3();
        t12.SingleInt64 = 10;

        return new[]
        {
            new TestCase(InterpreterTestCase.literal_any)
                .Expr(
                    "google.protobuf.Any{type_url: 'type.googleapis.com/google.api.expr.test.v1.proto2.TestAllTypes', value: b'\\x08\\x96\\x01'}")
                .Types(new TestAllTypesPb2(), new Any())
                .Out(t1),
            new TestCase(InterpreterTestCase.literal_var).Expr("x")
                .Env(Decls.NewVar("x", Decls.NewObjectType("google.protobuf.Any")))
                .Types(new Any(), new Value(),
                    new TestAllTypesPb2())
                .In("x", v1)
                .Out(t1),
            // TODO
            /*
            (new TestCase(InterpreterTestCase.select_pb3_unset)).Expr("TestAllTypes{}.single_struct")
            .Container("google.api.expr.test.v1.proto3")
            .Types(new Google.Api.Expr.Test.V1.Proto3.TestAllTypes())
            .Out(new Struct()),
            */
            new TestCase(InterpreterTestCase.elem_in_mixed_type_list_error)
                .Expr("'elem' in [1u, 'str', 2, b'bytes']").Err("no such overload: string.@in(uint,bytes,...)"),
            new TestCase(InterpreterTestCase.elem_in_mixed_type_list).Expr("'elem' in [1, 'elem', 2]")
                .Out(Common.Types.Types.BoolOf(true)),
            new TestCase(InterpreterTestCase.select_literal_uint).Expr("google.protobuf.UInt32Value{value: 123u}")
                .Out((ulong)123),
            new TestCase(InterpreterTestCase.select_on_int64).Expr("a.pancakes")
                .Types(Decls.NewVar("a", Decls.Int)).In("a", IntT.IntOf(15)).Err("no such overload: int.ref-resolve(*)")
                .Unchecked(),
            new TestCase(InterpreterTestCase.select_pb3_empty_list).Container("google.api.expr.test.v1.proto3")
                .Expr("TestAllTypes{list_value: []}.list_value")
                .Types(new TestAllTypesPb3())
                .Out(new ListValue()),
            // TODO fix enum?
            new TestCase(InterpreterTestCase.select_pb3_enum_big).Container("google.api.expr.test.v1.proto3")
                .Expr("x.standalone_enum")
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("x", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))).In("x",
                    t2).Out(IntT.IntOf(2)),
            new TestCase(InterpreterTestCase.eq_list_elem_mixed_types_error).Expr("[1] == [1.0]").Unchecked()
                .Err("no such overload: int._==_(double)"),
            // TODO
            /*
            (new TestCase(InterpreterTestCase.parse_nest_message_literal))
            .Container("google.api.expr.test.v1.proto3")
            .Expr(
                "NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: " +
                "NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: " +
                "NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: " +
                "NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: " +
                "NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: NestedTestAllTypes{child: " +
                "NestedTestAllTypes{payload: TestAllTypes{single_int64: 137}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}.payload.single_int64")
            .Types(new Google.Api.Expr.Test.V1.Proto3.NestedTestAllTypes())
            .Out(IntT.IntOf(0)),
            */
            new TestCase(InterpreterTestCase.parse_repeat_index)
                .Expr(
                    "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[['foo']]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0][0]")
                .Out(StringT.StringOf("foo")),
            new TestCase(InterpreterTestCase.cond_bad_type).Expr("'cows' ? false : 17").Err("no such overload")
                .Unchecked(),
            new TestCase(InterpreterTestCase.and_false_1st).Expr("false && true").Cost(ICoster.CostOf(0, 1))
                .ExhaustiveCost(ICoster.CostOf(1, 1)).Out(BoolT.False),
            new TestCase(InterpreterTestCase.and_false_2nd).Expr("true && false").Cost(ICoster.CostOf(0, 1))
                .ExhaustiveCost(ICoster.CostOf(1, 1)).Out(BoolT.False),
            new TestCase(InterpreterTestCase.and_error_1st_false).Expr("1/0 != 0 && false")
                .Cost(ICoster.CostOf(2, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Out(BoolT.False),
            new TestCase(InterpreterTestCase.and_error_2nd_false).Expr("false && 1/0 != 0")
                .Cost(ICoster.CostOf(0, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Out(BoolT.False),
            new TestCase(InterpreterTestCase.and_error_1st_error).Expr("1/0 != 0 && true")
                .Cost(ICoster.CostOf(2, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Err("divide by zero"),
            new TestCase(InterpreterTestCase.and_error_2nd_error).Expr("true && 1/0 != 0")
                .Cost(ICoster.CostOf(0, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Err("divide by zero"),
            new TestCase(InterpreterTestCase.call_no_args).Expr("zero()").Cost(ICoster.CostOf(1, 1)).Unchecked()
                .Funcs(Overload.Function("zero", args => IntT.IntZero)).Out(IntT.IntZero),
            new TestCase(InterpreterTestCase.call_one_arg).Expr("neg(1)").Cost(ICoster.CostOf(1, 1)).Unchecked()
                .Funcs(Overload.Unary("neg", Trait.NegatorType, arg => ((INegater)arg).Negate())).Out(IntT.IntNegOne),
            new TestCase(InterpreterTestCase.call_two_arg).Expr("b'abc'.concat(b'def')").Cost(ICoster.CostOf(1, 1))
                .Unchecked().Funcs(Overload.Binary("concat", Trait.AdderType, (lhs, rhs) => ((IAdder)lhs).Add(rhs)))
                .Out(new[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' }),
            new TestCase(InterpreterTestCase.call_varargs).Expr("addall(a, b, c, d) == 10")
                .Cost(ICoster.CostOf(6, 6)).Unchecked().Funcs(Overload.Function("addall", Trait.AdderType, args =>
                {
                    var val = 0;
                    foreach (var arg in args) val += (int)arg.IntValue();

                    return IntT.IntOf(val);
                })).In("a", 1, "b", 2, "c", 3, "d", 4),
            new TestCase(InterpreterTestCase.call_ns_func).Expr("base64.encode('hello')")
                .Cost(ICoster.CostOf(1, 1))
                .Env(Decls.NewFunction("base64.encode",
                    new List<Decl.Types.FunctionDecl.Types.Overload>
                    {
                        Decls.NewOverload("base64_encode_string", new List<Type> { Decls.String },
                            Decls.String)
                    })).Funcs(Overload.Unary("base64.encode", Base64Encode),
                    Overload.Unary("base64_encode_string", Base64Encode)).Out("aGVsbG8="),
            new TestCase(InterpreterTestCase.call_ns_func_unchecked).Expr("base64.encode('hello')")
                .Cost(ICoster.CostOf(1, 1)).Unchecked()
                .Funcs(Overload.Unary("base64.encode", Base64Encode)).Out("aGVsbG8="),
            new TestCase(InterpreterTestCase.call_ns_func_in_pkg).Container("base64").Expr("encode('hello')")
                .Cost(ICoster.CostOf(1, 1))
                .Env(Decls.NewFunction("base64.encode",
                    new List<Decl.Types.FunctionDecl.Types.Overload>
                    {
                        Decls.NewOverload("base64_encode_string", new List<Type> { Decls.String },
                            Decls.String)
                    })).Funcs(Overload.Unary("base64.encode", Base64Encode),
                    Overload.Unary("base64_encode_string", Base64Encode)).Out("aGVsbG8="),
            new TestCase(InterpreterTestCase.call_ns_func_unchecked_in_pkg).Expr("encode('hello')")
                .Cost(ICoster.CostOf(1, 1)).Container("base64").Unchecked()
                .Funcs(Overload.Unary("base64.encode", Base64Encode)).Out("aGVsbG8="),
            new TestCase(InterpreterTestCase.complex)
                .Expr("!(headers.ip in [\"10.0.1.4\", \"10.0.1.5\"]) && \n" +
                      "((headers.path.startsWith(\"v1\") && headers.token in [\"v1\", \"v2\", \"admin\"]) || \n" +
                      "(headers.path.startsWith(\"v2\") && headers.token in [\"v2\", \"admin\"]) || \n" +
                      "(headers.path.startsWith(\"/admin\") && headers.token == \"admin\" && headers.ip in [\"10.0.1.2\", \"10.0.1.2\", \"10.0.1.2\"]))")
                .Cost(ICoster.CostOf(3, 24)).ExhaustiveCost(ICoster.CostOf(24, 24)).OptimizedCost(ICoster.CostOf(2, 20))
                .Env(Decls.NewVar("headers", Decls.NewMapType(Decls.String, Decls.String))).In("headers",
                    TestUtil.BindingsOf("ip", "10.0.1.2", "path", "/admin/edit", "token", "admin")),
            new TestCase(InterpreterTestCase.complex_qual_vars)
                .Expr("!(headers.ip in [\"10.0.1.4\", \"10.0.1.5\"]) && \n" +
                      "((headers.path.startsWith(\"v1\") && headers.token in [\"v1\", \"v2\", \"admin\"]) || \n" +
                      "(headers.path.startsWith(\"v2\") && headers.token in [\"v2\", \"admin\"]) || \n" +
                      "(headers.path.startsWith(\"/admin\") && headers.token == \"admin\" && headers.ip in [\"10.0.1.2\", \"10.0.1.2\", \"10.0.1.2\"]))")
                .Cost(ICoster.CostOf(3, 24)).ExhaustiveCost(ICoster.CostOf(24, 24)).OptimizedCost(ICoster.CostOf(2, 20))
                .Env(Decls.NewVar("headers.ip", Decls.String), Decls.NewVar("headers.path", Decls.String),
                    Decls.NewVar("headers.token", Decls.String))
                .In("headers.ip", "10.0.1.2", "headers.path", "/admin/edit", "headers.token", "admin"),
            new TestCase(InterpreterTestCase.cond).Expr("a ? b < 1.2 : c == ['hello']").Cost(ICoster.CostOf(3, 3))
                .Env(Decls.NewVar("a", Decls.Bool), Decls.NewVar("b", Decls.Double),
                    Decls.NewVar("c", Decls.NewListType(Decls.String)))
                .In("a", true, "b", 2.0, "c", new[] { "hello" }).Out(BoolT.False),
            new TestCase(InterpreterTestCase.in_list).Expr("6 in [2, 12, 6]").Cost(ICoster.CostOf(1, 1))
                .OptimizedCost(ICoster.CostOf(0, 0)),
            new TestCase(InterpreterTestCase.in_map).Expr("'other-key' in {'key': null, 'other-key': 42}")
                .Cost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.index)
                .Expr("m['key'][1] == 42u && m['null'] == null && m[string(0)] == 10").Cost(ICoster.CostOf(2, 9))
                .ExhaustiveCost(ICoster.CostOf(9, 9)).OptimizedCost(ICoster.CostOf(2, 8))
                .Env(Decls.NewVar("m", Decls.NewMapType(Decls.String, Decls.Dyn)))
                .In("m",
                    TestUtil.BindingsOf("key", new object[] { (ulong)21, (ulong)42 }, "null", null, "0",
                        10)),
            new TestCase(InterpreterTestCase.index_relative)
                .Expr("([[[1]], [[2]], [[3]]][0][0] + [2, 3, {'four': {'five': 'six'}}])[3].four.five == 'six'")
                .Cost(ICoster.CostOf(2, 2)),
            new TestCase(InterpreterTestCase.literal_bool_false).Expr("false").Cost(ICoster.CostOf(0, 0))
                .Out(BoolT.False),
            new TestCase(InterpreterTestCase.literal_bool_true).Expr("true").Cost(ICoster.CostOf(0, 0)),
            new TestCase(InterpreterTestCase.literal_empty).Expr("google.protobuf.Any{}")
                .Err("conversion error: got Any with empty type-url"),
            new TestCase(InterpreterTestCase.literal_null).Expr("null").Cost(ICoster.CostOf(0, 0))
                .Out(NullT.NullValue),
            new TestCase(InterpreterTestCase.literal_list).Expr("[1, 2, 3]").Cost(ICoster.CostOf(0, 0))
                .Out(new long[] { 1, 2, 3 }),
            new TestCase(InterpreterTestCase.literal_map).Expr("{'hi': 21, 'world': 42u}")
                .Cost(ICoster.CostOf(0, 0)).Out(TestUtil.BindingsOf("hi", 21, "world", (ulong)42)),
            new TestCase(InterpreterTestCase.literal_equiv_string_bytes)
                .Expr("string(bytes(\"\\303\\277\")) == '''\\303\\277'''").Cost(ICoster.CostOf(3, 3))
                .OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.literal_not_equiv_string_bytes)
                .Expr("string(b\"\\303\\277\") != '''\\303\\277'''").Cost(ICoster.CostOf(2, 2))
                .OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.literal_equiv_bytes_string)
                .Expr("string(b\"\\303\\277\") == '\u00FF'").Cost(ICoster.CostOf(2, 2))
                .OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.literal_bytes_string).Expr("string(b'aaa\"bbb')")
                .Cost(ICoster.CostOf(1, 1)).OptimizedCost(ICoster.CostOf(0, 0)).Out("aaa\"bbb"),
            new TestCase(InterpreterTestCase.literal_bytes_string2).Expr("string(b\"\"\"Kim\\t\"\"\")")
                .Cost(ICoster.CostOf(1, 1)).OptimizedCost(ICoster.CostOf(0, 0)).Out("Kim\t"),
            new TestCase(InterpreterTestCase.literal_pb_struct)
                .Expr("google.protobuf.Struct{fields: {'uno': 1.0, 'dos': 2.0}}")
                .Out(TestUtil.BindingsOf("uno", 1.0d, "dos", 2.0d)),
            new TestCase(InterpreterTestCase.literal_pb3_msg).Container("google.api.expr")
                .Types(new Expr())
                .Expr("v1alpha1.Expr{ \n" + "	id: 1, \n" + "	const_expr: v1alpha1.Constant{ \n" +
                      "		string_value: \"oneof_test\" \n" + "	}\n" + "}").Cost(ICoster.CostOf(0, 0)).Out(expr1),
            new TestCase(InterpreterTestCase.literal_pb_enum).Container("google.api.expr.test.v1.proto3")
                .Types(new TestAllTypesPb3())
                .Expr("TestAllTypes{\n" + "repeated_nested_enum: [\n" + "	0,\n" + "	TestAllTypes.NestedEnum.BAZ,\n" +
                      "	TestAllTypes.NestedEnum.BAR],\n" + "repeated_int32: [\n" + "	TestAllTypes.NestedEnum.FOO,\n" +
                      "	TestAllTypes.NestedEnum.BAZ]}").Cost(ICoster.CostOf(0, 0))
                .Out(t4),
            new TestCase(InterpreterTestCase.timestamp_eq_timestamp).Expr("timestamp(0) == timestamp(0)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.timestamp_ne_timestamp).Expr("timestamp(1) != timestamp(2)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.timestamp_lt_timestamp).Expr("timestamp(0) < timestamp(1)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.timestamp_le_timestamp).Expr("timestamp(2) <= timestamp(2)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.timestamp_gt_timestamp).Expr("timestamp(1) > timestamp(0)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.timestamp_ge_timestamp).Expr("timestamp(2) >= timestamp(2)")
                .Cost(ICoster.CostOf(3, 3)).OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.string_to_timestamp).Expr("timestamp('1986-04-26T01:23:40Z')")
                .Cost(ICoster.CostOf(1, 1)).OptimizedCost(ICoster.CostOf(0, 0))
                .Out(ts1),
            new TestCase(InterpreterTestCase.macro_all_non_strict)
                .Expr("![0, 2, 4].all(x, 4/x != 2 && 4/(4-x) != 2)").Cost(ICoster.CostOf(5, 38))
                .ExhaustiveCost(ICoster.CostOf(38, 38)),
            new TestCase(InterpreterTestCase.macro_all_non_strict_var)
                .Expr("code == \"111\" && [\"a\", \"b\"].all(x, x in tags) \n" +
                      "|| code == \"222\" && [\"a\", \"b\"].all(x, x in tags)")
                .Env(Decls.NewVar("code", Decls.String), Decls.NewVar("tags", Decls.NewListType(Decls.String)))
                .In("code", "222", "tags", new[] { "a", "b" }),
            new TestCase(InterpreterTestCase.macro_exists_lit).Expr(
                "[1, 2, 3, 4, 5u, 1.0].exists(e, type(e) == uint)"),
            new TestCase(InterpreterTestCase.macro_exists_nonstrict).Expr(
                "[0, 2, 4].exists(x, 4/x == 2 && 4/(4-x) == 2)"),
            new TestCase(InterpreterTestCase.macro_exists_var).Expr("elems.exists(e, type(e) == uint)")
                .Cost(ICoster.CostOf(0, 9223372036854775807L)).ExhaustiveCost(ICoster.CostOf(0, 9223372036854775807L))
                .Env(Decls.NewVar("elems", Decls.NewListType(Decls.Dyn)))
                .In("elems", new object[] { 0, 1, 2, 3, 4, (ulong)5, 6 }),
            new TestCase(InterpreterTestCase.macro_exists_one).Expr("[1, 2, 3].exists_one(x, (x % 2) == 0)"),
            new TestCase(InterpreterTestCase.macro_filter).Expr("[1, 2, 3].filter(x, x > 2) == [3]"),
            new TestCase(InterpreterTestCase.macro_has_map_key).Expr("has({'a':1}.a) && !has({}.a)")
                .Cost(ICoster.CostOf(1, 4)).ExhaustiveCost(ICoster.CostOf(4, 4)),
            new TestCase(InterpreterTestCase.macro_has_pb2_field).Container("google.api.expr.test.v1.proto2")
                .Types(new TestAllTypesPb2())
                .Env(Decls.NewVar("pb2", Decls.NewObjectType("google.api.expr.test.v1.proto2.TestAllTypes")))
                .In("pb2", t5)
                .Expr("has(TestAllTypes{standalone_enum: TestAllTypes.NestedEnum.BAR}.standalone_enum) \n" +
                      "&& has(TestAllTypes{standalone_enum: TestAllTypes.NestedEnum.FOO}.standalone_enum) \n" +
                      "&& !has(TestAllTypes{single_nested_enum: TestAllTypes.NestedEnum.FOO}.single_nested_message) \n" +
                      "&& !has(TestAllTypes{}.standalone_enum) \n" +
                      "&& has(TestAllTypes{single_nested_enum: TestAllTypes.NestedEnum.FOO}.single_nested_enum) \n" +
                      "&& !has(pb2.single_int64) \n" + "&& has(pb2.repeated_bool) \n" +
                      "&& !has(pb2.repeated_int32) \n" + "&& has(pb2.map_int64_nested_type) \n" +
                      "&& !has(pb2.map_string_string)").Cost(ICoster.CostOf(1, 29))
                .ExhaustiveCost(ICoster.CostOf(29, 29)),
            // TODO fix
            new TestCase(InterpreterTestCase.macro_has_pb3_field)
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))
                .Container("google.api.expr.test.v1.proto3")
                .In("pb3", t3)
                .Expr("has(TestAllTypes{standalone_enum: TestAllTypes.NestedEnum.BAR}.standalone_enum) \n" +
                      //"&& !has(TestAllTypes{standalone_enum: TestAllTypes.NestedEnum.FOO}.standalone_enum) \n" +
                      "&& !has(TestAllTypes{single_nested_enum: TestAllTypes.NestedEnum.FOO}.single_nested_message) \n" +
                      "&& has(TestAllTypes{single_nested_enum: TestAllTypes.NestedEnum.FOO}.single_nested_enum) \n" +
                      "&& !has(TestAllTypes{}.single_nested_message) \n" +
                      "&& has(TestAllTypes{single_nested_message: TestAllTypes.NestedMessage{}}.single_nested_message) \n" +
                      //"&& !has(TestAllTypes{}.standalone_enum) \n" + 
                      //"&& !has(pb3.single_int64) \n" +
                      "&& has(pb3.repeated_bool) \n" + "&& !has(pb3.repeated_int32) \n" +
                      "&& has(pb3.map_int64_nested_type) \n" + "&& !has(pb3.map_string_string)"),
            //.Cost(Coster.CostOf(1, 35)).ExhaustiveCost(Coster.CostOf(35, 35)),
            new TestCase(InterpreterTestCase.macro_map).Expr("[1, 2, 3].map(x, x * 2) == [2, 4, 6]")
                .Cost(ICoster.CostOf(6, 14)).ExhaustiveCost(ICoster.CostOf(14, 14)),
            new TestCase(InterpreterTestCase.matches)
                .Expr("input.matches('k.*') \n" + "&& !'foo'.matches('k.*') \n" + "&& !'bar'.matches('k.*') \n" +
                      "&& 'kilimanjaro'.matches('.*ro')").Cost(ICoster.CostOf(2, 10))
                .ExhaustiveCost(ICoster.CostOf(10, 10)).Env(Decls.NewVar("input", Decls.String))
                .In("input", "kathmandu"),
            new TestCase(InterpreterTestCase.nested_proto_field).Expr("pb3.single_nested_message.bb")
                .Cost(ICoster.CostOf(1, 1))
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))
                .In("pb3", t6).Out(IntT.IntOf(1234)),
            new TestCase(InterpreterTestCase.nested_proto_field_with_index)
                .Expr("pb3.map_int64_nested_type[0].child.payload.single_int32 == 1").Cost(ICoster.CostOf(2, 2))
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))).In(
                    "pb3", t8),
            new TestCase(InterpreterTestCase.or_true_1st).Expr("ai == 20 || ar[\"foo\"] == \"bar\"")
                .Cost(ICoster.CostOf(2, 5)).ExhaustiveCost(ICoster.CostOf(5, 5))
                .Env(Decls.NewVar("ai", Decls.Int),
                    Decls.NewVar("ar", Decls.NewMapType(Decls.String, Decls.String)))
                .In("ai", 20, "ar", TestUtil.BindingsOf("foo", "bar")),
            new TestCase(InterpreterTestCase.or_true_2nd).Expr("ai == 20 || ar[\"foo\"] == \"bar\"")
                .Cost(ICoster.CostOf(2, 5)).ExhaustiveCost(ICoster.CostOf(5, 5))
                .Env(Decls.NewVar("ai", Decls.Int),
                    Decls.NewVar("ar", Decls.NewMapType(Decls.String, Decls.String)))
                .In("ai", 2, "ar", TestUtil.BindingsOf("foo", "bar")),
            new TestCase(InterpreterTestCase.or_false).Expr("ai == 20 || ar[\"foo\"] == \"bar\"")
                .Cost(ICoster.CostOf(2, 5)).ExhaustiveCost(ICoster.CostOf(5, 5))
                .Env(Decls.NewVar("ai", Decls.Int),
                    Decls.NewVar("ar", Decls.NewMapType(Decls.String, Decls.String)))
                .In("ai", 2, "ar", TestUtil.BindingsOf("foo", "baz")).Out(BoolT.False),
            new TestCase(InterpreterTestCase.or_error_1st_error).Expr("1/0 != 0 || false")
                .Cost(ICoster.CostOf(2, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Err("divide by zero"),
            new TestCase(InterpreterTestCase.or_error_2nd_error).Expr("false || 1/0 != 0")
                .Cost(ICoster.CostOf(0, 3)).ExhaustiveCost(ICoster.CostOf(3, 3)).Err("divide by zero"),
            new TestCase(InterpreterTestCase.or_error_1st_true).Expr("1/0 != 0 || true")
                .Cost(ICoster.CostOf(2, 3))
                .ExhaustiveCost(ICoster.CostOf(3, 3)).Out(BoolT.True),
            new TestCase(InterpreterTestCase.or_error_2nd_true).Expr("true || 1/0 != 0")
                .Cost(ICoster.CostOf(0, 3))
                .ExhaustiveCost(ICoster.CostOf(3, 3)).Out(BoolT.True),
            new TestCase(InterpreterTestCase.pkg_qualified_id).Expr("b.c.d != 10").Cost(ICoster.CostOf(2, 2))
                .Container("a.b").Env(Decls.NewVar("a.b.c.d", Decls.Int)).In("a.b.c.d", 9),
            new TestCase(InterpreterTestCase.pkg_qualified_id_unchecked).Expr("c.d != 10")
                .Cost(ICoster.CostOf(2, 2)).Unchecked().Container("a.b").In("a.c.d", 9),
            new TestCase(InterpreterTestCase.pkg_qualified_index_unchecked).Expr("b.c['d'] == 10")
                .Cost(ICoster.CostOf(2, 2)).Unchecked().Container("a.b").In("a.b.c", TestUtil.BindingsOf("d", 10)),
            new TestCase(InterpreterTestCase.select_key)
                .Expr("m.strMap['val'] == 'string'\n" + "&& m.floatMap['val'] == 1.5\n" +
                      "&& m.doubleMap['val'] == -2.0\n" + "&& m.intMap['val'] == -3\n" +
                      "&& m.int32Map['val'] == 4\n" +
                      "&& m.int64Map['val'] == -5\n" + "&& m.uintMap['val'] == 6u\n" +
                      "&& m.uint32Map['val'] == 7u\n" +
                      "&& m.uint64Map['val'] == 8u\n" + "&& m.boolMap['val'] == true\n" +
                      "&& m.boolMap['val'] != false").Cost(ICoster.CostOf(2, 32))
                .ExhaustiveCost(ICoster.CostOf(32, 32))
                .Env(Decls.NewVar("m", Decls.NewMapType(Decls.String, Decls.Dyn)))
                .In("m",
                    TestUtil.BindingsOf("strMap", TestUtil.MapOf("val", "string"), "floatMap",
                        TestUtil.MapOf("val", 1.5f),
                        "doubleMap", TestUtil.MapOf("val", -2.0d), "intMap", TestUtil.MapOf("val", -3), "int32Map",
                        TestUtil.MapOf("val", 4), "int64Map", TestUtil.MapOf("val", -5L), "uintMap",
                        TestUtil.MapOf("val", (ulong)6), "uint32Map",
                        TestUtil.MapOf("val", (ulong)7),
                        "uint64Map", TestUtil.MapOf("val", (ulong)8L), "boolMap",
                        TestUtil.MapOf("val", true))),
            new TestCase(InterpreterTestCase.select_bool_key)
                .Expr("m.boolStr[true] == 'string'\n" + "&& m.boolFloat32[true] == 1.5\n" +
                      "&& m.boolFloat64[false] == -2.1\n" + "&& m.boolInt[false] == -3\n" +
                      "&& m.boolInt32[false] == 0\n" + "&& m.boolInt64[true] == 4\n" +
                      "&& m.boolUint[true] == 5u\n" +
                      "&& m.boolUint32[true] == 6u\n" + "&& m.boolUint64[false] == 7u\n" + "&& m.boolBool[true]\n" +
                      "&& m.boolIface[false] == true").Cost(ICoster.CostOf(2, 31))
                .ExhaustiveCost(ICoster.CostOf(31, 31))
                .Env(Decls.NewVar("m", Decls.NewMapType(Decls.String, Decls.Dyn))).In("m",
                    TestUtil.BindingsOf("boolStr", TestUtil.MapOf(true, "string"), "boolFloat32",
                        TestUtil.MapOf(true, 1.5f),
                        "boolFloat64", TestUtil.MapOf(false, -2.1d), "boolInt", TestUtil.MapOf(false, -3),
                        "boolInt32",
                        TestUtil.MapOf(false, 0), "boolInt64", TestUtil.MapOf(true, 4L), "boolUint",
                        TestUtil.MapOf(true, (ulong)5), "boolUint32",
                        TestUtil.MapOf(true, (ulong)6),
                        "boolUint64", TestUtil.MapOf(false, (ulong)7L), "boolBool",
                        TestUtil.MapOf(true, true),
                        "boolIface", TestUtil.MapOf(false, true))),
            new TestCase(InterpreterTestCase.select_uint_key)
                .Expr("m.uintIface[1u] == 'string'\n" + "&& m.uint32Iface[2u] == 1.5\n" +
                      "&& m.uint64Iface[3u] == -2.1\n" + "&& m.uint64String[4u] == 'three'")
                .Cost(ICoster.CostOf(2, 11))
                .ExhaustiveCost(ICoster.CostOf(11, 11))
                .Env(Decls.NewVar("m", Decls.NewMapType(Decls.String, Decls.Dyn)))
                .In("m",
                    TestUtil.BindingsOf("uintIface", TestUtil.MapOf((ulong)1, "string"), "uint32Iface",
                        TestUtil.MapOf((ulong)2, 1.5), "uint64Iface",
                        TestUtil.MapOf((ulong)3, -2.1),
                        "uint64String", TestUtil.MapOf((ulong)4, "three"))),
            new TestCase(InterpreterTestCase.select_index)
                .Expr("m.strList[0] == 'string'\n" + "&& m.floatList[0] == 1.5\n" + "&& m.doubleList[0] == -2.0\n" +
                      "&& m.intList[0] == -3\n" + "&& m.int32List[0] == 4\n" + "&& m.int64List[0] == -5\n" +
                      "&& m.uintList[0] == 6u\n" + "&& m.uint32List[0] == 7u\n" + "&& m.uint64List[0] == 8u\n" +
                      "&& m.boolList[0] == true\n" + "&& m.boolList[1] != true\n" + "&& m.ifaceList[0] == {}")
                .Cost(ICoster.CostOf(2, 35)).ExhaustiveCost(ICoster.CostOf(35, 35))
                .Env(Decls.NewVar("m", Decls.NewMapType(Decls.String, Decls.Dyn))).In("m",
                    TestUtil.BindingsOf("strList", new[] { "string" }, "floatList", new float?[] { 1.5f },
                        "doubleList", new double?[] { -2.0d }, "intList", new[] { -3 }, "int32List",
                        new[] { 4 }, "int64List", new[] { -5L }, "uintList",
                        new object[] { (ulong)6 },
                        "uint32List", new object[] { (ulong)7 }, "uint64List",
                        new object[] { (ulong)8L }, "boolList", new[] { true, false }, "ifaceList",
                        new object[] { new Dictionary<object, object>() })),
            new TestCase(InterpreterTestCase.select_field)
                .Expr("a.b.c\n" + "&& pb3.repeated_nested_enum[0] == TestAllTypes.NestedEnum.BAR\n" +
                      "&& json.list[0] == 'world'").Cost(ICoster.CostOf(1, 7)).ExhaustiveCost(ICoster.CostOf(7, 7))
                .Container("google.api.expr.test.v1.proto3")
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("a.b", Decls.NewMapType(Decls.String, Decls.Bool)),
                    Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")),
                    Decls.NewVar("json", Decls.NewMapType(Decls.String, Decls.Dyn)))
                .In("a.b", TestUtil.BindingsOf("c", true), "pb3", t9, "json", v4),
            new TestCase(InterpreterTestCase.select_pb2_primitive_fields)
                .Expr("!has(a.single_int32)\n" + "&& a.single_int32 == -32\n" + "&& a.single_int64 == -64\n" +
                      "&& a.single_uint32 == 32u\n" + "&& a.single_uint64 == 64u\n" + "&& a.single_float == 3.0\n" +
                      "&& a.single_double == 6.4\n" + "&& a.single_bool\n" + "&& \"empty\" == a.single_string")
                .Cost(ICoster.CostOf(3, 26)).ExhaustiveCost(ICoster.CostOf(26, 26))
                .Types(new TestAllTypesPb2()).In("a", new TestAllTypesPb2())
                .Env(Decls.NewVar("a", Decls.NewObjectType("google.api.expr.test.v1.proto2.TestAllTypes"))),
            new TestCase(InterpreterTestCase.select_pb3_wrapper_fields)
                .Expr("!has(a.single_int32_wrapper) && a.single_int32_wrapper == null\n" +
                      "&& has(a.single_int64_wrapper) && a.single_int64_wrapper == 0\n" +
                      "&& has(a.single_string_wrapper) && a.single_string_wrapper == \"hello\"\n" +
                      "&& a.single_int64_wrapper == Int32Value{value: 0}").Cost(ICoster.CostOf(3, 21))
                .ExhaustiveCost(ICoster.CostOf(21, 21))
                .Types(new TestAllTypesPb3())
                .Abbrevs("google.protobuf.Int32Value")
                .Env(Decls.NewVar("a", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))
                .In("a", t10),
            new TestCase(InterpreterTestCase.select_pb3_compare).Expr("a.single_uint64 > 3u")
                .Cost(ICoster.CostOf(2, 2)).Container("google.api.expr.test.v1.proto3")
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("a", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))).In("a",
                    t11).Out(BoolT.True),
            new TestCase(InterpreterTestCase.select_pb3_compare_signed).Expr("a.single_int64 > 3")
                .Cost(ICoster.CostOf(2, 2)).Container("google.api.expr.test.v1.proto3")
                .Types(new TestAllTypesPb3())
                .Env(Decls.NewVar("a", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes")))
                .In("a", t12).Out(BoolT.True),
            // TODO custAttrFactory
            /*
            (new TestCase(InterpreterTestCase.select_custom_pb3_compare)).Expr("a.bb > 100")
            .Cost(Coster.CostOf(2, 2)).Container("google.api.expr.test.v1.proto3")
            .Types(new Google.Api.Expr.Test.V1.Proto3.TestAllTypes.Types.NestedMessage())
            .Env(Decls.NewVar("a",
                Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes.NestedMessage")))
            .Attrs(new CustAttrFactory(AttributeFactory.NewAttributeFactory(
                TestContainer("google.api.expr.test.v1.proto3"), DefaultTypeAdapter.Instance,
                ProtoTypeRegistry.NewEmptyRegistry()))).In("a",
                com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.NestedMessage.newBuilder()
                    .setBb(101).build()).Out(BoolT.True),
                    */
            new TestCase(InterpreterTestCase.select_relative).Expr("json('{\"hi\":\"world\"}').hi == 'world'")
                .Cost(ICoster.CostOf(2, 2))
                .Env(Decls.NewFunction("json",
                    new List<Decl.Types.FunctionDecl.Types.Overload>
                        { Decls.NewOverload("string_to_json", new List<Type> { Decls.String }, Decls.Dyn) }))
                .Funcs(
                    Overload.Unary("json", val =>
                    {
                        if (val.Type() != StringT.StringType)
                            return Err.NoSuchOverload(StringT.StringType, "json", val);

                        var str = (StringT)val;
                        IDictionary<object, object> m = new Dictionary<object, object>();
                        throw new NotSupportedException("IMPLEMENT ME");
                    })).Disabled("would need some JSON library to implement this test..."),
            new TestCase(InterpreterTestCase.select_subsumed_field).Expr("a.b.c").Cost(ICoster.CostOf(1, 1))
                .Env(Decls.NewVar("a.b.c", Decls.Int),
                    Decls.NewVar("a.b", Decls.NewMapType(Decls.String, Decls.String)))
                .In("a.b.c", 10, "a.b", TestUtil.BindingsOf("c", "ten")).Out(IntT.IntOf(10)),
            new TestCase(InterpreterTestCase.select_empty_repeated_nested)
                .Expr("TestAllTypes{}.repeated_nested_message.size() == 0").Cost(ICoster.CostOf(2, 2))
                .Types(new TestAllTypesPb3())
                .Container("google.api.expr.test.v1.proto3").Out(BoolT.True),
            new TestCase(InterpreterTestCase.duration_get_milliseconds).Expr("x.getMilliseconds()")
                .Env(Decls.NewVar("x", Decls.Duration))
                .In("x", d1)
                .Cost(ICoster.CostOf(2, 2)).ExhaustiveCost(ICoster.CostOf(2, 2)).Out(123123),
            new TestCase(InterpreterTestCase.timestamp_get_hours_tz)
                .Expr("timestamp('2009-02-13T23:31:30Z').getHours('2:00')").Out(IntT.IntOf(1))
                .Cost(ICoster.CostOf(2, 2))
                .OptimizedCost(ICoster.CostOf(1, 1)),
            new TestCase(InterpreterTestCase.index_out_of_range).Expr("[1, 2, 3][3]")
                .Err("invalid_argument: index '3' out of range in list of size '3'"),
            new TestCase(InterpreterTestCase.parse_nest_list_index)
                .Expr(
                    "a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[a[0]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]")
                .Env(Decls.NewVar("a", Decls.NewListType(Decls.Int))).In("a", new long[] { 0 }).Out(IntT.IntOf(0))
        };
    }

    [TestCaseSource(nameof(TestCases))]
    public virtual void Interpreter(TestCase tc)
    {
        if (tc.disabled != null) return;

        var prg = program(tc);
        IVal want = BoolT.True;
        if (tc.@out != null) want = (IVal)tc.@out;

        var got = prg.interpretable.Eval(prg.activation);
        if (UnknownT.IsUnknown(want))
        {
            Assert.That(got, Is.EqualTo(want));
        }
        else if (tc.err != null)
        {
            Assert.That(got, Is.InstanceOf(typeof(Err)));
        }
        else if (Err.IsError(want))
        {
            Assert.That(got, Is.EqualTo(want));
            Assert.That(got.Equal(want), Is.SameAs(BoolT.True));
        }
        else
        {
            if (Err.IsError(got) && ((Err)got).HasCause()) throw ((Err)got).ToRuntimeException();

            Assert.That(got, Is.EqualTo(want));
            Assert.That(got.Equal(want), Is.SameAs(BoolT.True));
        }

        if (tc.cost != null)
        {
            var cost = Cost.EstimateCost(prg.interpretable);
            Assert.That(cost, Is.EqualTo(tc.cost));
        }

        var state = IEvalState.NewEvalState();
        IDictionary<string, InterpretableDecorator> opts = new Dictionary<string, InterpretableDecorator>();
        opts["Interpreter.Optimize"] = global::Cel.Interpreter.IInterpreter.Optimize();
        opts["exhaustive"] = global::Cel.Interpreter.IInterpreter.ExhaustiveEval(state);
        opts["track"] = global::Cel.Interpreter.IInterpreter.TrackState(state);
        foreach (var en in opts)
        {
            var mode = en.Key;
            var opt = en.Value;

            prg = program(tc, opt);
            got = prg.interpretable.Eval(prg.activation);
            if (UnknownT.IsUnknown(want))
            {
                Assert.That(got, Is.EqualTo(want));
            }
            else if (tc.err != null)
            {
                Assert.That(got, Is.InstanceOf(typeof(Err)));
            }
            else
            {
                Assert.That(got, Is.EqualTo(want));
                Assert.That(got.Equal(want), Is.SameAs(BoolT.True));
            }

            if ("exhaustive".Equals(mode) && tc.cost != null)
            {
                var wantedCost = tc.cost;
                if (tc.exhaustiveCost != null) wantedCost = tc.exhaustiveCost;

                var cost = Cost.EstimateCost(prg.interpretable);
                Assert.That(cost, Is.EqualTo(wantedCost));
            }

            if ("Interpreter.Optimize".Equals(mode) && tc.cost != null)
            {
                var wantedCost = tc.cost;
                if (tc.optimizedCost != null) wantedCost = tc.optimizedCost;

                var cost = Cost.EstimateCost(prg.interpretable);
                Assert.That(cost, Is.EqualTo(wantedCost));
            }

            state.Reset();
        }
    }

    [Test]
    public virtual void ProtoAttributeOpt()
    {
        var t0 = new TestAllTypesPb3();
        t0.SingleInt32 = 1;
        var n = new NestedTestAllTypesPb3();
        n.Payload = t0;
        var n1 = new NestedTestAllTypesPb3();
        n1.Child = n;
        IDictionary<long, NestedTestAllTypesPb3> dict1 = new Dictionary<long, NestedTestAllTypesPb3>();
        dict1.Add(0, n1);
        var mf1 = new MapField<long, NestedTestAllTypesPb3>();
        mf1.Add(dict1);
        var t = new TestAllTypesPb3();
        t.MapInt64NestedType.Add(mf1);
        var inst =
            program(
                new TestCase(InterpreterTestCase.nested_proto_field_with_index)
                    .Expr("pb3.map_int64_nested_type[0].child.payload.single_int32")
                    .Types(new TestAllTypesPb3())
                    .Env(Decls.NewVar("pb3", Decls.NewObjectType("google.api.expr.test.v1.proto3.TestAllTypes"))).In(
                        "pb3", t),
                global::Cel.Interpreter.IInterpreter.Optimize());
        Assert.That(inst.interpretable, Is.InstanceOf(typeof(IInterpretableAttribute)));
        var attr = (IInterpretableAttribute)inst.interpretable;
        Assert.That(attr.Attr(), Is.InstanceOf(typeof(INamespacedAttribute)));
        var absAttr = (INamespacedAttribute)attr.Attr();
        var quals = absAttr.Qualifiers();
        Assert.That(quals.Count, Is.EqualTo(5));
        Assert.That(IsFieldQual(quals[0], "map_int64_nested_type"), Is.True);
        Assert.That(IsConstQual(quals[1], IntT.IntZero), Is.True);
        Assert.That(IsFieldQual(quals[2], "child"), Is.True);
        Assert.That(IsFieldQual(quals[3], "payload"), Is.True);
        Assert.That(IsFieldQual(quals[4], "single_int32"), Is.True);
    }

    [Test]
    public virtual void LogicalAndMissingType()
    {
        var src = ISource.NewTextSource("a && TestProto{c: true}.c");

        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);

        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var cont = Container.DefaultContainer;
        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var intr =
            global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        Assert.That(() => intr.NewUncheckedInterpretable(parsed.Expr!),
            Throws.Exception.InstanceOf(typeof(InvalidOperationException)));
    }

    [Test]
    public virtual void ExhaustiveConditionalExpr()
    {
        var src = ISource.NewTextSource("a ? b < 1.0 : c == ['hello']");
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);

        var state = IEvalState.NewEvalState();
        var cont = Container.DefaultContainer;
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new ParsedExpr());
        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var intr =
            global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        var interpretable =
            intr.NewUncheckedInterpretable(parsed.Expr!, global::Cel.Interpreter.IInterpreter.ExhaustiveEval(state));
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("a", BoolT.True, "b", DoubleT.DoubleOf(0.999),
            "c", ListT.NewStringArrayList(new[] { "hello" })));
        var result = interpretable.Eval(vars);
        // Operator "_==_" is at Expr 7, should be evaluated in exhaustive mode
        // even though "a" is true
        var ev = state.Value(7);
        // "==" should be evaluated in exhaustive mode though unnecessary
        Assert.That(ev, Is.SameAs(BoolT.True));
        Assert.That(result, Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void ExhaustiveLogicalOrEquals()
    {
        // a || b == "b"
        // Operator "==" is at Expr 4, should be evaluated though "a" is true
        var src = ISource.NewTextSource("a || b == \"b\"");
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);

        var state = IEvalState.NewEvalState();
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
        var cont = TestContainer("test");
        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var interp =
            global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        var i = interp.NewUncheckedInterpretable(parsed.Expr!,
            global::Cel.Interpreter.IInterpreter.ExhaustiveEval(state));
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("a", true, "b", "b"));
        var result = i.Eval(vars);
        var rhv = state.Value(3);
        // "==" should be evaluated in exhaustive mode though unnecessary
        Assert.That(rhv, Is.SameAs(BoolT.True));
        Assert.That(result, Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void SetProto2PrimitiveFields()
    {
        // Test the use of proto2 primitives within object construction.
        var src = ISource.NewTextSource("input == TestAllTypes{\n" + "  single_int32: 1,\n" +
                                       "  single_int64: 2,\n" + "  single_uint32: 3u,\n" +
                                       "  single_uint64: 4u,\n" + "  single_float: -3.3,\n" +
                                       "  single_double: -2.2,\n" + "  single_string: \"hello world\",\n" +
                                       "  single_bool: true\n" + "}");
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);

        var cont = TestContainer("google.api.expr.test.v1.proto2");
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry(new TestAllTypesPb2());
        var env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
        env.Add(new List<Decl>
        {
            Decls.NewVar("input",
                Decls.NewObjectType("google.api.expr.test.v1.proto2.TestAllTypes"))
        });
        var checkResult = Checker.Checker.Check(parsed, src, env);
        if (parsed.HasErrors()) throw new ArgumentException(parsed.Errors.ToDisplayString());

        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var i =
            global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        var eval = i.NewInterpretable(checkResult.CheckedExpr);
        var one = 1;
        var two = 2L;
        var three = 3;
        var four = 4L;
        var five = -3.3f;
        var six = -2.2d;
        var str = "hello world";
        var truth = true;
        var input = new TestAllTypesPb2();
        input.SingleInt32 = one;
        input.SingleInt64 = two;
        input.SingleUint32 = (uint)three;
        input.SingleUint64 = (ulong)four;
        input.SingleFloat = five;
        input.SingleDouble = six;
        input.SingleString = str;
        input.SingleBool = truth;
        var vars = IActivation.NewActivation(TestUtil.BindingsOf("input", reg.ToTypeAdapter()(input)));
        var result = eval.Eval(vars);
        Assert.That(result.Value(), Is.InstanceOf(typeof(bool)));
        var got = ((bool?)result.Value()).Value;
        Assert.That(got, Is.True);
    }

    [Test]
    public virtual void MissingIdentInSelect()
    {
        var src = ISource.NewTextSource("a.b.c");
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);

        var cont = TestContainer("test");
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
        env.Add(Decls.NewVar("a.b", Decls.Dyn));
        var checkResult = Checker.Checker.Check(parsed, src, env);
        if (parsed.HasErrors()) throw new ArgumentException(parsed.Errors.ToDisplayString());

        var attrs = AttributePattern.NewPartialAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var interp = global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg,
            reg.ToTypeAdapter(), attrs);
        var i = interp.NewInterpretable(checkResult.CheckedExpr);
        IActivation vars = IActivation.NewPartialActivation(TestUtil.BindingsOf("a.b", TestUtil.MapOf("d", "hello")),
            AttributePattern.NewAttributePattern("a.b").QualString("c"));
        var result = i.Eval(vars);
        Assert.That(result, Is.InstanceOf(typeof(UnknownT)));

        result = i.Eval(IActivation.EmptyActivation());
        Assert.That(result, Is.InstanceOf(typeof(Err)));
    }

    internal static ConvTestCase[] TypeConversionOptTests()
    {
        var ts1 = new Timestamp();
        ts1.Seconds = TimestampT.maxUnixTime;

        var d1 = new Duration();
        d1.Seconds = 12;

        var ts2 = new Timestamp();
        ts2.Seconds = 123;

        return new[]
        {
            // TODO
            //(new ConvTestCase("string(b'\\000\\xff')")).Err("invalid UTF-8"),
            new ConvTestCase("b'\\000\\xff'").Out(BytesT.BytesOf(new byte[] { 0, 0xff })),
            new ConvTestCase("double(18446744073709551615u)").Out(DoubleT.DoubleOf(1.8446744073709551615e19)),
            new ConvTestCase("uint(1e19)").Out(UintT.UintOf(10000000000000000000)),
            new ConvTestCase("int(-123.456)").Out(IntT.IntOf(-123)),
            new ConvTestCase("int(1.9)").Out(IntT.IntOf(1)), new ConvTestCase("int(-7.9)").Out(IntT.IntOf(-7)),
            new ConvTestCase("int(11.5)").Out(IntT.IntOf(11)),
            new ConvTestCase("int(-3.5)").Out(IntT.IntOf(-3)),
            new ConvTestCase("string(timestamp('2009-02-13T23:31:30Z'))").Out(
                StringT.StringOf("2009-02-13T23:31:30Z")),
            new ConvTestCase("string(timestamp('2009-02-13T23:31:30.999999999Z'))").Out(
                StringT.StringOf("2009-02-13T23:31:30.999999999Z")),
            new ConvTestCase("string(duration('1000000s'))").Out(StringT.StringOf("1000000s")),
            // TODO
            //(new ConvTestCase("timestamp('0000-01-01T00:00:00Z')")).Err("range"),
            new ConvTestCase("timestamp('9999-12-31T23:59:59Z')").Out(TimestampT.TimestampOf(ts1)),
            new ConvTestCase("timestamp('10000-01-01T00:00:00Z')").Err("error"),
            new ConvTestCase("bool('tru')").Fail(), new ConvTestCase("bool(\"true\")").Out(BoolT.True),
            new ConvTestCase("bytes(\"hello\")").Out(BytesT.BytesOf(Encoding.UTF8.GetBytes("hello"))),
            new ConvTestCase("double(\"_123\")").Fail(),
            new ConvTestCase("double(\"123.0\")").Out(DoubleT.DoubleOf(123.0)),
            new ConvTestCase("duration('12hh3')").Fail(),
            new ConvTestCase("duration('12s')").Out(DurationT.DurationOf(d1)),
            new ConvTestCase("duration('-320000000000s')").Err("range"),
            new ConvTestCase("duration('320000000000s')").Err("range"),
            new ConvTestCase("dyn(1u)").Out(UintT.UintOf(1)), new ConvTestCase("int('11l')").Fail(),
            new ConvTestCase("int('11')").Out(IntT.IntOf(11)),
            new ConvTestCase("string('11')").Out(StringT.StringOf("11")),
            new ConvTestCase("timestamp('123')").Fail(),
            new ConvTestCase("timestamp(123)").Out(TimestampT.TimestampOf(ts2)),
            new ConvTestCase("type(null)").Out(NullT.NullType),
            new ConvTestCase("type(timestamp(int('123')))").Out(TimestampT.TimestampType),
            new ConvTestCase("uint(-1)").Fail(), new ConvTestCase("uint(1)").Out(UintT.UintOf(1))
        };
    }

    [TestCaseSource(nameof(TypeConversionOptTests))]
    public virtual void TypeConversionOpt(ConvTestCase tc)
    {
        var src = ISource.NewTextSource(tc.@in);
        var parsed = Parser.Parser.ParseAllMacros(src);
        Assert.That(parsed.HasErrors(), Is.False);
        var cont = Container.DefaultContainer;
        ITypeRegistry reg = ProtoTypeRegistry.NewRegistry();
        var env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
        var checkResult = Checker.Checker.Check(parsed, src, env);
        if (parsed.HasErrors()) throw new ArgumentException(parsed.Errors.ToDisplayString());

        var attrs = IAttributeFactory.NewAttributeFactory(cont, reg.ToTypeAdapter(), reg);
        var interp =
            global::Cel.Interpreter.IInterpreter.NewStandardInterpreter(cont, reg, reg.ToTypeAdapter(), attrs);
        // Show that program planning will now produce an error.

        if (!tc.fail)
        {
            TypeConversionOptCheck(tc, checkResult, interp);
        }
        else
        {
            Exception? err = null;
            try
            {
                TypeConversionOptCheck(tc, checkResult, interp);
            }
            catch (Exception e)
            {
                err = e;
            }

            Assert.That(err, Is.Not.Null);
            // TODO 'err' below comes from "try-catch" of the preceding 'newInterpretable'
            //  Show how the error returned during program planning is the same as the runtime
            //  error which would be produced normally.
            var i2 = interp.NewInterpretable(checkResult.CheckedExpr);
            var errVal = i2.Eval(IActivation.EmptyActivation());
            var errValStr = errVal.ToString();
            Assert.That(errValStr, Is.EqualTo(err.Message));

            if (tc.err != null) Assert.That(errValStr, Does.Contain(tc.err));
        }
    }

    private void TypeConversionOptCheck(ConvTestCase tc, Checker.Checker.CheckResult checkResult,
        IInterpreter interp)
    {
        var i =
            interp.NewInterpretable(checkResult.CheckedExpr, global::Cel.Interpreter.IInterpreter.Optimize());
        if (tc.@out != null)
        {
            Assert.That(i, Is.InstanceOf(typeof(IInterpretableConst)));
            var ic = (IInterpretableConst)i;
            Assert.That(ic.Value(), Is.EqualTo(tc.@out));
        }
    }

    internal static Container TestContainer(string name)
    {
        return Container.NewContainer(Container.Name(name))!;
    }

    internal static Program program(TestCase tst, params InterpretableDecorator[] opts)
    {
        // Configure the package.
        var cont = Container.DefaultContainer;
        if (tst.container != null) cont = TestContainer(tst.container);

        if (tst.abbrevs != null)
            cont = Container.NewContainer(Container.Name(cont.Name()), Container.Abbrevs(tst.abbrevs));

        ITypeRegistry reg;
        reg = ProtoTypeRegistry.NewRegistry();
        if (tst.types != null) reg = ProtoTypeRegistry.NewRegistry(tst.types);

        var attrs = IAttributeFactory.NewAttributeFactory(cont!, reg.ToTypeAdapter(), reg);
        if (tst.attrs != null) attrs = tst.attrs;

        // Configure the environment.
        var env = CheckerEnv.NewStandardCheckerEnv(cont, reg);
        if (tst.env != null) env.Add(tst.env);

        // Configure the program input.
        var vars = IActivation.EmptyActivation();
        if (tst.@in != null) vars = IActivation.NewActivation(tst.@in);

        // Adapt the test output, if needed.
        if (tst.@out != null) tst.@out = reg.ToTypeAdapter()(tst.@out);

        var disp = IDispatcher.NewDispatcher();
        disp.Add(Overload.StandardOverloads());
        if (tst.funcs != null) disp.Add(tst.funcs);

        var interp =
            global::Cel.Interpreter.IInterpreter.NewInterpreter(disp, cont, reg, reg.ToTypeAdapter(), attrs);

        // Parse the expression.
        var s = ISource.NewTextSource(tst.expr);
        var parsed = Parser.Parser.ParseAllMacros(s);
        Assert.That(parsed.HasErrors(), Is.False);
        IInterpretable prg;
        if (tst.@unchecked)
        {
            // Build the program plan.
            prg = interp.NewUncheckedInterpretable(parsed.Expr!, opts)!;
            return new Program(prg, vars);
        }

        // Check the expression.
        var checkResult = Checker.Checker.Check(parsed, s, env);

        Assert.That(checkResult.HasErrors(), Is.False);

        // Build the program plan.
        prg = interp.NewInterpretable(checkResult.CheckedExpr, opts)!;
        return new Program(prg, vars);
    }

    internal static bool IsConstQual(IQualifier q, IVal val)
    {
        if (!(q is IConstantQualifier)) return false;

        return ((IConstantQualifier)q).Value().Equal(val) == BoolT.True;
    }

    internal static bool IsFieldQual(IQualifier q, string fieldName)
    {
        if (!(q is FieldQualifier)) return false;

        return ((FieldQualifier)q).Name().Equals(fieldName);
    }

    internal class TestCase
    {
        internal readonly InterpreterTestCase name;
        internal string[]? abbrevs;
        internal IAttributeFactory? attrs;
        internal string? container;
        internal Cost? cost;
        internal string? disabled;
        internal Decl[]? env;
        internal string? err;
        internal Cost? exhaustiveCost;
        internal string? expr;
        internal Overload[]? funcs;

        internal IDictionary<string, object>? @in;
        internal Cost? optimizedCost;
        internal object? @out;
        internal Message[]? types;
        internal bool @unchecked;

        internal TestCase(InterpreterTestCase name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name.ToString();
        }

        internal virtual TestCase Disabled(string reason)
        {
            disabled = reason;
            return this;
        }

        internal virtual TestCase Expr(string expr)
        {
            this.expr = expr;
            return this;
        }

        internal virtual TestCase Container(string container)
        {
            this.container = container;
            return this;
        }

        internal virtual TestCase Cost(Cost cost)
        {
            this.cost = cost;
            return this;
        }

        internal virtual TestCase ExhaustiveCost(Cost exhaustiveCost)
        {
            this.exhaustiveCost = exhaustiveCost;
            return this;
        }

        internal virtual TestCase OptimizedCost(Cost optimizedCost)
        {
            this.optimizedCost = optimizedCost;
            return this;
        }

        internal virtual TestCase Abbrevs(params string[] abbrevs)
        {
            this.abbrevs = abbrevs;
            return this;
        }

        internal virtual TestCase Env(params Decl[] env)
        {
            this.env = env;
            return this;
        }

        internal virtual TestCase Types(params Message[] types)
        {
            this.types = types;
            return this;
        }

        internal virtual TestCase Funcs(params Overload[] funcs)
        {
            this.funcs = funcs;
            return this;
        }

        internal virtual TestCase Attrs(IAttributeFactory attrs)
        {
            this.attrs = attrs;
            return this;
        }

        internal virtual TestCase Unchecked()
        {
            @unchecked = true;
            return this;
        }

        internal virtual TestCase In(params object[] kvPairs)
        {
            if (kvPairs.Length == 0)
            {
                @in = TestUtil.BindingsOf();
            }
            else
            {
                var subarray = new object[kvPairs.Length - 2];
                Array.Copy(kvPairs, 2, subarray, 0, subarray.Length);
                @in = TestUtil.BindingsOf(kvPairs[0].ToString(), kvPairs[1], subarray);
            }

            return this;
        }

        internal virtual TestCase Out(object @out)
        {
            this.@out = @out;
            return this;
        }

        internal virtual TestCase Err(string err)
        {
            this.err = err;
            return this;
        }
    }

    internal class ConvTestCase
    {
        internal readonly string @in;
        internal string? err;
        internal bool fail;
        internal IVal? @out;

        internal ConvTestCase(string @in)
        {
            this.@in = @in;
        }

        internal virtual ConvTestCase Out(IVal @out)
        {
            this.@out = @out;
            return this;
        }

        internal virtual ConvTestCase Err(string err)
        {
            fail = true;
            this.err = err;
            return this;
        }

        internal virtual ConvTestCase Fail()
        {
            fail = true;
            return this;
        }

        public override string ToString()
        {
            return "ConvTestCase{" + "in='" + @in + '\'' + '}';
        }
    }

    internal class Program
    {
        internal readonly IActivation activation;
        internal readonly IInterpretable interpretable;

        internal Program(IInterpretable interpretable, IActivation activation)
        {
            this.interpretable = interpretable;
            this.activation = activation;
        }
    }
}