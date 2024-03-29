﻿/*
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

namespace Cel.Common.Operators;

public sealed class Operator
{
    public enum InnerEnum
    {
        Conditional,
        LogicalAnd,
        LogicalOr,
        LogicalNot,
        Equals,
        NotEquals,
        Less,
        LessEquals,
        Greater,
        GreaterEquals,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Negate,
        Index,
        Has,
        All,
        Exists,
        ExistsOne,
        Map,
        Filter,
        NotStrictlyFalse,
        In,
        OldNotStrictlyFalse,
        OldIn
    }

    // Symbolic operators.
    public static readonly Operator Conditional = new("Conditional", InnerEnum.Conditional, "_?_:_", 8, null);

    public static readonly Operator LogicalAnd = new("LogicalAnd", InnerEnum.LogicalAnd, "_&&_", 6, "&&");
    public static readonly Operator LogicalOr = new("LogicalOr", InnerEnum.LogicalOr, "_||_", 7, "||");
    public static readonly Operator LogicalNot = new("LogicalNot", InnerEnum.LogicalNot, "!_", 2, "!");
    public static readonly Operator Equals = new("Equals", InnerEnum.Equals, "_==_", 5, "==");
    public static readonly Operator NotEquals = new("NotEquals", InnerEnum.NotEquals, "_!=_", 5, "!=");
    public static readonly Operator Less = new("Less", InnerEnum.Less, "_<_", 5, "<");
    public static readonly Operator LessEquals = new("LessEquals", InnerEnum.LessEquals, "_<=_", 5, "<=");
    public static readonly Operator Greater = new("Greater", InnerEnum.Greater, "_>_", 5, ">");

    public static readonly Operator GreaterEquals = new("GreaterEquals", InnerEnum.GreaterEquals, "_>=_", 5, ">=");

    public static readonly Operator Add = new("Add", InnerEnum.Add, "_+_", 4, "+");
    public static readonly Operator Subtract = new("Subtract", InnerEnum.Subtract, "_-_", 4, "-");
    public static readonly Operator Multiply = new("Multiply", InnerEnum.Multiply, "_*_", 3, "*");
    public static readonly Operator Divide = new("Divide", InnerEnum.Divide, "_/_", 3, "/");
    public static readonly Operator Modulo = new("Modulo", InnerEnum.Modulo, "_%_", 3, "%");
    public static readonly Operator Negate = new("Negate", InnerEnum.Negate, "-_", 2, "-");

    public static readonly Operator Index = new("Index", InnerEnum.Index, "_[_]", 1, null);

    // Macros, must have a valid identifier.
    public static readonly Operator Has = new("Has", InnerEnum.Has, "has");
    public static readonly Operator All = new("All", InnerEnum.All, "all");
    public static readonly Operator Exists = new("Exists", InnerEnum.Exists, "exists");
    public static readonly Operator ExistsOne = new("ExistsOne", InnerEnum.ExistsOne, "exists_one");
    public static readonly Operator Map = new("Map", InnerEnum.Map, "map");

    public static readonly Operator Filter = new("Filter", InnerEnum.Filter, "filter");

    // Named operators, must not have be valid identifiers.
    public static readonly Operator NotStrictlyFalse =
        new("NotStrictlyFalse", InnerEnum.NotStrictlyFalse, "@not_strictly_false");

    public static readonly Operator In = new("In", InnerEnum.In, "@in", 5, "in");

    // Deprecated: named operators with valid identifiers.
    public static readonly Operator OldNotStrictlyFalse = new("OldNotStrictlyFalse",
        InnerEnum.OldNotStrictlyFalse, "__not_strictly_false__");

    public static readonly Operator OldIn = new("OldIn", InnerEnum.OldIn, "_in_", 5, "in");

    private static readonly List<Operator> ValueList = new();
    private static int nextOrdinal;

    private static readonly Dictionary<string, Operator> Operators;

    private static readonly Dictionary<string, Operator> OperatorsById;
    // precedence of the operator, where the higher value means higher.

    private readonly string nameValue;
    private readonly int ordinalValue;
    private readonly string? reverse;

    static Operator()
    {
        {
            var m = new Dictionary<string, Operator>();
            m.Add("+", Add);
            m.Add("/", Divide);
            m.Add("==", Equals);
            m.Add(">", Greater);
            m.Add(">=", GreaterEquals);
            m.Add("in", In);
            m.Add("<", Less);
            m.Add("<=", LessEquals);
            m.Add("%", Modulo);
            m.Add("*", Multiply);
            m.Add("!=", NotEquals);
            m.Add("-", Subtract);
            Operators = m;
        }

        ValueList.Add(Conditional);
        ValueList.Add(LogicalAnd);
        ValueList.Add(LogicalOr);
        ValueList.Add(LogicalNot);
        ValueList.Add(Equals);
        ValueList.Add(NotEquals);
        ValueList.Add(Less);
        ValueList.Add(LessEquals);
        ValueList.Add(Greater);
        ValueList.Add(GreaterEquals);
        ValueList.Add(Add);
        ValueList.Add(Subtract);
        ValueList.Add(Multiply);
        ValueList.Add(Divide);
        ValueList.Add(Modulo);
        ValueList.Add(Negate);
        ValueList.Add(Index);
        ValueList.Add(Has);
        ValueList.Add(All);
        ValueList.Add(Exists);
        ValueList.Add(ExistsOne);
        ValueList.Add(Map);
        ValueList.Add(Filter);
        ValueList.Add(NotStrictlyFalse);
        ValueList.Add(In);
        ValueList.Add(OldNotStrictlyFalse);
        ValueList.Add(OldIn);

        {
            var m = new Dictionary<string, Operator>();
            foreach (var op in Values()) m.Add(op.Id, op);

            OperatorsById = m;
        }
    }

    internal Operator(string name, InnerEnum innerEnum, string id) : this(name, innerEnum, id, 0, null)
    {
        nameValue = name;
        ordinalValue = nextOrdinal++;
        InnerEnumValue = innerEnum;
    }

    internal Operator(string name, InnerEnum innerEnum, string id, int precedence, string? reverse)
    {
        Id = id;
        Precedence = precedence;
        this.reverse = reverse;

        nameValue = name;
        ordinalValue = nextOrdinal++;
        InnerEnumValue = innerEnum;
    }

    public string Id { get; }

    public InnerEnum InnerEnumValue { get; }

    public int Precedence { get; }

    public static Operator? ById(string id)
    {
        OperatorsById.TryGetValue(id, out var op);
        return op;
    }

    // Find the internal function name for an operator, if the input text is one.
    public static Operator? Find(string text)
    {
        Operators.TryGetValue(text, out var op);
        return op;
    }

    // FindReverse returns the unmangled, text representation of the operator.
    public static string? FindReverse(string id)
    {
        var op = ById(id);
        return op != null ? op.reverse : null;
    }

    // FindReverseBinaryOperator returns the unmangled, text representation of a binary operator.
    public static string? FindReverseBinaryOperator(string id)
    {
        var op = ById(id);
        if (op == null || op == LogicalNot || op == Negate) return null;

        return op.reverse;
    }

    public static int PrecedenceById(string id)
    {
        var op = ById(id);
        return op != null ? op.Precedence : 0;
    }

    public static Operator[] Values()
    {
        return ValueList.ToArray();
    }

    public int Ordinal()
    {
        return ordinalValue;
    }

    public override string ToString()
    {
        return nameValue;
    }

    public static Operator ValueOf(string name)
    {
        foreach (var enumInstance in ValueList)
            if (enumInstance.nameValue == name)
                return enumInstance;

        throw new ArgumentException(name);
    }
}