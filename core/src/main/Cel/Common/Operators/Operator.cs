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
namespace Cel.Common.Operators
{

	public sealed class Operator
	{

	  // Symbolic operators.
	  public static readonly Operator Conditional = new Operator("Conditional", InnerEnum.Conditional, "_?_:_", 8, null);
	  public static readonly Operator LogicalAnd = new Operator("LogicalAnd", InnerEnum.LogicalAnd, "_&&_", 6, "&&");
	  public static readonly Operator LogicalOr = new Operator("LogicalOr", InnerEnum.LogicalOr, "_||_", 7, "||");
	  public static readonly Operator LogicalNot = new Operator("LogicalNot", InnerEnum.LogicalNot, "!_", 2, "!");
	  public static readonly Operator Equals = new Operator("Equals", InnerEnum.Equals, "_==_", 5, "==");
	  public static readonly Operator NotEquals = new Operator("NotEquals", InnerEnum.NotEquals, "_!=_", 5, "!=");
	  public static readonly Operator Less = new Operator("Less", InnerEnum.Less, "_<_", 5, "<");
	  public static readonly Operator LessEquals = new Operator("LessEquals", InnerEnum.LessEquals, "_<=_", 5, "<=");
	  public static readonly Operator Greater = new Operator("Greater", InnerEnum.Greater, "_>_", 5, ">");
	  public static readonly Operator GreaterEquals = new Operator("GreaterEquals", InnerEnum.GreaterEquals, "_>=_", 5, ">=");
	  public static readonly Operator Add = new Operator("Add", InnerEnum.Add, "_+_", 4, "+");
	  public static readonly Operator Subtract = new Operator("Subtract", InnerEnum.Subtract, "_-_", 4, "-");
	  public static readonly Operator Multiply = new Operator("Multiply", InnerEnum.Multiply, "_*_", 3, "*");
	  public static readonly Operator Divide = new Operator("Divide", InnerEnum.Divide, "_/_", 3, "/");
	  public static readonly Operator Modulo = new Operator("Modulo", InnerEnum.Modulo, "_%_", 3, "%");
	  public static readonly Operator Negate = new Operator("Negate", InnerEnum.Negate, "-_", 2, "-");
	  public static readonly Operator Index = new Operator("Index", InnerEnum.Index, "_[_]", 1, null);
	  // Macros, must have a valid identifier.
	  public static readonly Operator Has = new Operator("Has", InnerEnum.Has, "has");
	  public static readonly Operator All = new Operator("All", InnerEnum.All, "all");
	  public static readonly Operator Exists = new Operator("Exists", InnerEnum.Exists, "exists");
	  public static readonly Operator ExistsOne = new Operator("ExistsOne", InnerEnum.ExistsOne, "exists_one");
	  public static readonly Operator Map = new Operator("Map", InnerEnum.Map, "map");
	  public static readonly Operator Filter = new Operator("Filter", InnerEnum.Filter, "filter");
	  // Named operators, must not have be valid identifiers.
	  public static readonly Operator NotStrictlyFalse = new Operator("NotStrictlyFalse", InnerEnum.NotStrictlyFalse, "@not_strictly_false");
	  public static readonly Operator In = new Operator("In", InnerEnum.In, "@in", 5, "in");
	  // Deprecated: named operators with valid identifiers.
	  public static readonly Operator OldNotStrictlyFalse = new Operator("OldNotStrictlyFalse", InnerEnum.OldNotStrictlyFalse, "__not_strictly_false__");
	  public static readonly Operator OldIn = new Operator("OldIn", InnerEnum.OldIn, "_in_", 5, "in");

	  private static readonly List<Operator> valueList = new List<Operator>();

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

	  public readonly InnerEnum innerEnumValue;
	  private readonly string nameValue;
	  private readonly int ordinalValue;
	  private static int nextOrdinal = 0;

	  private static readonly Dictionary<string, Operator> operators;
	  private static readonly Dictionary<string, Operator> operatorsById;
	  // precedence of the operator, where the higher value means higher.

	  public readonly string id;
	  public readonly string reverse;
	  public readonly int precedence;

	  internal Operator(string name, InnerEnum innerEnum, string id) : this(name, innerEnum, id, 0, null)
	  {

		  nameValue = name;
		  ordinalValue = nextOrdinal++;
		  innerEnumValue = innerEnum;
	  }

	  internal Operator(string name, InnerEnum innerEnum, string id, int precedence, string reverse)
	  {
		this.id = id;
		this.precedence = precedence;
		this.reverse = reverse;

		  nameValue = name;
		  ordinalValue = nextOrdinal++;
		  innerEnumValue = innerEnum;
	  }

	  static Operator()
	  {
		{
		  Dictionary<string, Operator> m = new Dictionary<string, Operator>();
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
		  operators = m;
		}

		{
		  Dictionary<string, Operator> m = new Dictionary<string, Operator>();
		  foreach (Operator op in Operator.Values())
		  {
			m.Add(op.id, op);
		  }
		  operatorsById = m;
		}

		  valueList.Add(Conditional);
		  valueList.Add(LogicalAnd);
		  valueList.Add(LogicalOr);
		  valueList.Add(LogicalNot);
		  valueList.Add(Equals);
		  valueList.Add(NotEquals);
		  valueList.Add(Less);
		  valueList.Add(LessEquals);
		  valueList.Add(Greater);
		  valueList.Add(GreaterEquals);
		  valueList.Add(Add);
		  valueList.Add(Subtract);
		  valueList.Add(Multiply);
		  valueList.Add(Divide);
		  valueList.Add(Modulo);
		  valueList.Add(Negate);
		  valueList.Add(Index);
		  valueList.Add(Has);
		  valueList.Add(All);
		  valueList.Add(Exists);
		  valueList.Add(ExistsOne);
		  valueList.Add(Map);
		  valueList.Add(Filter);
		  valueList.Add(NotStrictlyFalse);
		  valueList.Add(In);
		  valueList.Add(OldNotStrictlyFalse);
		  valueList.Add(OldIn);
	  }

	  public static Operator ById(string id)
	  {
		return operatorsById[id];
	  }

	  // Find the internal function name for an operator, if the input text is one.
	  public static Operator Find(string text)
	  {
		return operators[text];
	  }

	  // FindReverse returns the unmangled, text representation of the operator.
	  public static string FindReverse(string id)
	  {
		Operator op = ById(id);
		return op != null ? op.reverse : null;
	  }

	  // FindReverseBinaryOperator returns the unmangled, text representation of a binary operator.
	  public static string findReverseBinaryOperator(string id)
	  {
		Operator op = ById(id);
		if (op == null || op == LogicalNot || op == Negate)
		{
		  return null;
		}
		return op.reverse;
	  }

	  public static int Precedence(string id)
	  {
		Operator op = ById(id);
		return op != null ? op.precedence : 0;
	  }

		public static Operator[] Values()
		{
			return valueList.ToArray();
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
			foreach (Operator enumInstance in Operator.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}