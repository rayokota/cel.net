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
using Cel.Common.Operators;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Cel;

/// <summary>
///     TestExpr packages an Expr with SourceInfo, for testing.
/// </summary>
public class TestExpr
{
    /// <summary>
    ///     Empty generates a program with no instructions.
    /// </summary>
    public static readonly TestExpr Empty = new(new Expr(), new SourceInfo());


    private static readonly SourceInfo Info1 = new();
    private static readonly SourceInfo Info2 = new();

    /// <summary>
    ///     Exists generates "[1, 1u, 1.0].exists(x, type(x) == uint)".
    /// </summary>
    public static readonly TestExpr Exists = new(
        ExprComprehension(1, "x",
            ExprList(8, ExprLiteral(2, 0L), ExprLiteral(3, 1L), ExprLiteral(4, 2L), ExprLiteral(5, 3L),
                ExprLiteral(6, 4L), ExprLiteral(7, (ulong)5)), "__result__", ExprLiteral(9, false),
            ExprCall(12,
                Operator.NotStrictlyFalse.id,
                ExprCall(10, Operator.LogicalNot.id, ExprIdent(11, "__result__"))),
            ExprCall(13, Operator.LogicalOr.id, ExprIdent(14, "__result__"),
                ExprCall(15, Operator.Equals.id, ExprCall(16, "type", ExprIdent(17, "x")),
                    ExprIdent(18, "uint"))),
            ExprIdent(19, "__result__")),
        Info1);

    /// <summary>
    ///     ExistsWithInput generates "elems.exists(x, type(x) == uint)".
    /// </summary>
    public static readonly TestExpr ExistsWithInput = new(
        ExprComprehension(1, "x", ExprIdent(2, "elems"), "__result__", ExprLiteral(3, false),
            ExprCall(4, Operator.LogicalNot.id, ExprIdent(5, "__result__")),
            ExprCall(6, Operator.Equals.id, ExprCall(7, "type", ExprIdent(8, "x")), ExprIdent(9, "uint")),
            ExprIdent(10, "__result__")),
        Info2);

    /// <summary>
    ///     DynMap generates a map literal:
    ///     <code><pre>
    ///         {"hello": "world".size(),
    ///         "dur": duration.Duration{10},
    ///         "ts": timestamp.Timestamp{1000},
    ///         "null": null,
    ///         "bytes": b"bytes-string"}
    ///     </pre></code>
    /// </summary>
    public static readonly TestExpr DynMap = new(
        ExprMap(17, ExprEntry(2, ExprLiteral(1, "hello"), ExprMemberCall(3, "size", ExprLiteral(4, "world"))),
            ExprEntry(12, ExprLiteral(11, "null"), ExprLiteral(13, null)),
            ExprEntry(15, ExprLiteral(14, "bytes"),
                ExprLiteral(16, Encoding.UTF8.GetBytes("bytes-string")))),
        new SourceInfo());

    /// <summary>
    ///     LogicalAnd generates "a && {c: true}.c".
    /// </summary>
    public static readonly TestExpr LogicalAnd =
        new(
            ExprCall(2, Operator.LogicalAnd.id, ExprIdent(1, "a"),
                ExprSelect(8, ExprMap(5, ExprEntry(4, ExprLiteral(6, "c"), ExprLiteral(7, true))), "c")),
            new SourceInfo());

    /// <summary>
    ///     LogicalOr generates "{c: false}.c || a".
    /// </summary>
    public static readonly TestExpr LogicalOr =
        new(
            ExprCall(2, Operator.LogicalOr.id,
                ExprSelect(8, ExprMap(5, ExprEntry(4, ExprLiteral(6, "c"), ExprLiteral(7, false))), "c"),
                ExprIdent(1, "a")),
            new SourceInfo());

    /// <summary>
    ///     LogicalOrEquals generates "a || b == 'b'".
    /// </summary>
    public static readonly TestExpr LogicalOrEquals =
        new(
            ExprCall(5, Operator.LogicalOr.id, ExprIdent(1, "a"),
                ExprCall(4, Operator.Equals.id, ExprIdent(2, "b"), ExprLiteral(3, "b"))),
            new SourceInfo());

    /// <summary>
    ///     LogicalAndMissingType generates "a && TestProto{c: true}.c" where the type 'TestProto' is
    ///     undefined.
    /// </summary>
    public static readonly TestExpr LogicalAndMissingType =
        new(
            ExprCall(2, Operator.LogicalAnd.id, ExprIdent(1, "a"),
                ExprSelect(7, ExprType(5, "TestProto", ExprField(4, "c", ExprLiteral(6, true))), "c")),
            new SourceInfo());

    /// <summary>
    ///     Conditional generates "a ? b < 1.0 : c== ["hello"]". 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public static readonly TestExpr Conditional = new(
        ExprCall(9, Operator.Conditional.id, ExprIdent(1, "a"),
            ExprCall(3, Operator.Less.id, ExprIdent(2, "b"), ExprLiteral(4, 1.0)),
            ExprCall(6, Operator.Equals.id, ExprIdent(5, "c"), ExprList(8, ExprLiteral(7, "hello")))),
        new SourceInfo());

    /// <summary>
    ///     Select generates "a.b.c".
    /// </summary>
    public static readonly TestExpr Select = new(ExprSelect(3, ExprSelect(2, ExprIdent(1, "a"), "b"), "c"),
        new SourceInfo());

    /// <summary>
    ///     Equality generates "a == 42".
    /// </summary>
    public static readonly TestExpr Equality =
        new(ExprCall(2, Operator.Equals.id, ExprIdent(1, "a"), ExprLiteral(3, 42L)),
            new SourceInfo());

    /// <summary>
    ///     TypeEquality generates "type(a) == uint".
    /// </summary>
    public static readonly TestExpr TypeEquality =
        new(ExprCall(4, Operator.Equals.id, ExprCall(1, "type", ExprIdent(2, "a")), ExprIdent(3, "uint")),
            new SourceInfo());

    internal readonly Expr expr;
    internal readonly SourceInfo sourceInfo;

    static TestExpr()
    {
        var map = TestUtil.MapOf(0L, 12, 1L, 0, 2L, 1, 3L, 4, 4L, 8, 5L, 0, 6L, 18, 7L, 18, 8L,
            18, 9L, 18, 10L, 18, 11L, 20, 12L, 20, 13L, 28, 14L, 28, 15L, 28, 16L, 28, 17L, 28, 18L, 28, 19L,
            28);
        foreach (var entry in map) Info1.Positions[(long)entry.Key] = (int)entry.Value;
        var map2 = TestUtil.MapOf(0L, 12, 1L, 0, 2L, 1, 3L, 4, 4L, 8, 5L, 0, 6L, 18, 7L, 18, 8L,
            18, 9L, 18, 10L, 18);
        foreach (var entry in map) Info2.Positions[(long)entry.Key] = (int)entry.Value;
    }

    public TestExpr(Expr expr, SourceInfo sourceInfo)
    {
        this.expr = expr;
        this.sourceInfo = sourceInfo;
    }

    /// <summary>
    ///     Info returns a copy of the SourceInfo with the given location.
    /// </summary>
    public virtual SourceInfo Info(string location)
    {
        var sourceInfo = new SourceInfo();
        sourceInfo.Location = location;
        return sourceInfo;
    }

    /// <summary>
    ///     ExprIdent creates an ident (variable) Expr.
    /// </summary>
    public static Expr ExprIdent(long id, string name)
    {
        var ident = new Expr.Types.Ident();
        ident.Name = name;
        var expr = new Expr();
        expr.Id = id;
        expr.IdentExpr = ident;
        return expr;
    }

    /// <summary>
    ///     ExprSelect creates a select Expr.
    /// </summary>
    public static Expr ExprSelect(long id, Expr operand, string field)
    {
        var sel = new Expr.Types.Select();
        sel.Operand = operand;
        sel.Field = field;
        sel.TestOnly = false;
        var expr = new Expr();
        expr.SelectExpr = sel;
        return expr;
    }

    /// <summary>
    ///     ExprLiteral creates a literal (constant) Expr.
    /// </summary>
    public static Expr ExprLiteral(long id, object? value)
    {
        var literal = new Constant();
        if (value is bool)
            literal.BoolValue = (bool)value;
        else if (value is double)
            literal.DoubleValue = (double)value;
        else if (value is float)
            literal.DoubleValue = (float)value;
        else if (value is ulong)
            literal.Uint64Value = (ulong)value;
        else if (value is string)
            literal.StringValue = value.ToString();
        else if (value is byte[])
            literal.BytesValue = ByteString.CopyFrom((byte[])value);
        else if (value == null)
            literal.NullValue = NullValue.NullValue;
        else
            throw new ArgumentException("literal type not implemented");

        var expr = new Expr();
        expr.Id = id;
        expr.ConstExpr = literal;
        return expr;
    }

    /// <summary>
    ///     ExprCall creates a call Expr.
    /// </summary>
    public static Expr ExprCall(long id, string function, params Expr[] args)
    {
        var call = new Expr.Types.Call();
        call.Function = function;
        call.Args.Add(args);
        var expr = new Expr();
        expr.Id = id;
        expr.CallExpr = call;
        return expr;
    }

    /// <summary>
    ///     ExprMemberCall creates a receiver-style call Expr.
    /// </summary>
    public static Expr ExprMemberCall(long id, string function, Expr target, params Expr[] args)
    {
        var call = new Expr.Types.Call();
        call.Target = target;
        call.Function = function;
        call.Args.Add(args);
        var expr = new Expr();
        expr.Id = id;
        expr.CallExpr = call;
        return expr;
    }

    /// <summary>
    ///     ExprList creates a create list Expr.
    /// </summary>
    public static Expr ExprList(long id, params Expr[] elements)
    {
        var createList = new Expr.Types.CreateList();
        createList.Elements.Add(elements);
        var expr = new Expr();
        expr.Id = id;
        expr.ListExpr = createList;
        return expr;
    }

    /// <summary>
    ///     ExprMap creates a create struct Expr for a map.
    /// </summary>
    public static Expr ExprMap(long id, params Expr.Types.CreateStruct.Types.Entry[] entries)
    {
        var createStruct = new Expr.Types.CreateStruct();
        createStruct.Entries.Add(entries);
        var expr = new Expr();
        expr.Id = id;
        expr.StructExpr = createStruct;
        return expr;
    }

    /// <summary>
    ///     ExprType creates creates a create struct Expr for a message.
    /// </summary>
    public static Expr ExprType(long id, string messageName, params Expr.Types.CreateStruct.Types.Entry[] entries)
    {
        var createStruct = new Expr.Types.CreateStruct();
        createStruct.MessageName = messageName;
        createStruct.Entries.Add(entries);
        var expr = new Expr();
        expr.Id = id;
        expr.StructExpr = createStruct;
        return expr;
    }

    /// <summary>
    ///     ExprEntry creates a map entry for a create struct Expr.
    /// </summary>
    public static Expr.Types.CreateStruct.Types.Entry ExprEntry(long id, Expr key, Expr value)
    {
        var entry = new Expr.Types.CreateStruct.Types.Entry();
        entry.Id = id;
        entry.MapKey = key;
        entry.Value = value;
        return entry;
    }

    /// <summary>
    ///     ExprField creates a field entry for a create struct Expr.
    /// </summary>
    public static Expr.Types.CreateStruct.Types.Entry ExprField(long id, string field, Expr value)
    {
        var entry = new Expr.Types.CreateStruct.Types.Entry();
        entry.Id = id;
        entry.FieldKey = field;
        entry.Value = value;
        return entry;
    }

    /// <summary>
    ///     ExprComprehension returns a comprehension Expr.
    /// </summary>
    public static Expr ExprComprehension(long id, string iterVar, Expr iterRange, string accuVar, Expr accuInit,
        Expr loopCondition, Expr loopStep, Expr resultExpr)
    {
        var comp = new Expr.Types.Comprehension();
        comp.IterVar = iterVar;
        comp.IterRange = iterRange;
        comp.AccuVar = accuVar;
        comp.AccuInit = accuInit;
        comp.LoopCondition = loopCondition;
        comp.LoopStep = loopStep;
        comp.Result = resultExpr;
        var expr = new Expr();
        expr.Id = id;
        expr.ComprehensionExpr = comp;
        return expr;
    }
}