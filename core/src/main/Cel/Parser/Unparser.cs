using System.Collections.Generic;
using System.Text;

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
namespace Cel.Parser
{
	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
	using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Types.Comprehension;
	using CreateList = Google.Api.Expr.V1Alpha1.Expr.Types.CreateList;
	using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using ExprKindCase = Google.Api.Expr.V1Alpha1.Expr.ExprKindOneofCase;
	using Ident = Google.Api.Expr.V1Alpha1.Expr.Types.Ident;
	using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
	using Debug = global::Cel.Common.Debug.Debug;
	using Operator = global::Cel.Common.Operators.Operator;

	/// <summary>
	/// Unparse takes an input expression and source position information and generates a human-readable
	/// expression.
	/// 
	/// <para>Note, unparsing an AST will often generate the same expression as was originally parsed, but
	/// some formatting may be lost in translation, notably:
	/// 
	/// </para>
	/// <para>- All quoted literals are doubled quoted. - Byte literals are represented as octal escapes
	/// (same as Google SQL). - Floating point values are converted to the small number of digits needed
	/// to represent the value. - Spacing around punctuation marks may be lost. - Parentheses will only
	/// be applied when they affect operator precedence.
	/// </para>
	/// </summary>
	public sealed class Unparser
	{

	  private readonly SourceInfo info;
	  private readonly StringBuilder str;

	  /// <summary>
	  /// unparser visits an expression to reconstruct a human-readable string from an AST. </summary>
	  public static string Unparse(Expr expr, SourceInfo info)
	  {
		Unparser unparser = new Unparser(info);
		unparser.Visit(expr);
		return unparser.str.ToString();
	  }

	  private Unparser(SourceInfo info)
	  {
		this.info = info;
		str = new StringBuilder();
	  }

	  internal void Visit(Expr expr)
	  {
		switch (expr.ExprKindCase)
		{
		  case ExprKindCase.CallExpr:
			VisitCall(expr.CallExpr);
			break;
		  case ExprKindCase.ComprehensionExpr:
			VisitComprehension(expr.ComprehensionExpr);
			break;
		  case ExprKindCase.ConstExpr:
			VisitConst(expr.ConstExpr);
			break;
		  case ExprKindCase.IdentExpr:
			VisitIdent(expr.IdentExpr);
			break;
		  case ExprKindCase.ListExpr:
			VisitList(expr.ListExpr);
			break;
		  case ExprKindCase.SelectExpr:
			VisitSelect(expr.SelectExpr);
			break;
		  case ExprKindCase.StructExpr:
			VisitStruct(expr.StructExpr);
			break;
		  default:
			throw new System.NotSupportedException(string.Format("Unsupported expr: {0}", expr.GetType().Name));
		}
	  }

	  internal void VisitCall(Call expr)
	  {
		Operator op = Operator.ById(expr.Function);
		if (op != null)
		{
		  switch (op.innerEnumValue)
		  {
			  // ternary operator
			case Operator.InnerEnum.Conditional:
			  VisitCallConditional(expr);
			  return;
			  // index operator
			case Operator.InnerEnum.Index:
			  VisitCallIndex(expr);
			  return;
			  // unary operators
			case Operator.InnerEnum.LogicalNot:
			case Operator.InnerEnum.Negate:
			  VisitCallUnary(expr);
			  return;
			  // binary operators
			case Operator.InnerEnum.Add:
			case Operator.InnerEnum.Divide:
			case Operator.InnerEnum.Equals:
			case Operator.InnerEnum.Greater:
			case Operator.InnerEnum.GreaterEquals:
			case Operator.InnerEnum.In:
			case Operator.InnerEnum.Less:
			case Operator.InnerEnum.LessEquals:
			case Operator.InnerEnum.LogicalAnd:
			case Operator.InnerEnum.LogicalOr:
			case Operator.InnerEnum.Modulo:
			case Operator.InnerEnum.Multiply:
			case Operator.InnerEnum.NotEquals:
			case Operator.InnerEnum.OldIn:
			case Operator.InnerEnum.Subtract:
			  VisitCallBinary(expr);
			  return;
		  }
		}
		// standard function calls.
		VisitCallFunc(expr);
	  }

	  internal void VisitCallBinary(Call expr)
	  {
		string fun = expr.Function;
		IList<Expr> args = expr.Args;
		Expr lhs = args[0];
		// add parens if the current operator is lower precedence than the lhs expr operator.
		bool lhsParen = IsComplexOperatorWithRespectTo(fun, lhs);
		Expr rhs = args[1];
		// add parens if the current operator is lower precedence than the rhs expr operator,
		// or the same precedence and the operator is left recursive.
		bool rhsParen = IsComplexOperatorWithRespectTo(fun, rhs);
		if (!rhsParen && IsLeftRecursive(fun))
		{
		  rhsParen = IsSamePrecedence(Operator.Precedence(fun), rhs);
		}
		VisitMaybeNested(lhs, lhsParen);
		string unmangled = Operator.FindReverseBinaryOperator(fun);
		if (string.ReferenceEquals(unmangled, null))
		{
		  throw new System.InvalidOperationException(string.Format("cannot unmangle operator: {0}", fun));
		}
		str.Append(" ");
		str.Append(unmangled);
		str.Append(" ");
		VisitMaybeNested(rhs, rhsParen);
	  }

	  internal void VisitCallConditional(Call expr)
	  {
		IList<Expr> args = expr.Args;
		// add parens if operand is a conditional itself.
		bool nested = IsSamePrecedence(Operator.Conditional.precedence, args[0]) || IsComplexOperator(args[0]);
		VisitMaybeNested(args[0], nested);
		str.Append(" ? ");
		// add parens if operand is a conditional itself.
		nested = IsSamePrecedence(Operator.Conditional.precedence, args[1]) || IsComplexOperator(args[1]);
		VisitMaybeNested(args[1], nested);
		str.Append(" : ");
		// add parens if operand is a conditional itself.
		nested = IsSamePrecedence(Operator.Conditional.precedence, args[2]) || IsComplexOperator(args[2]);

		VisitMaybeNested(args[2], nested);
	  }

	  internal void VisitCallFunc(Call expr)
	  {
		string fun = expr.Function;
		IList<Expr> args = expr.Args;
		if (expr.Target != null)
		{
		  bool nested = IsBinaryOrTernaryOperator(expr.Target);
		  VisitMaybeNested(expr.Target, nested);
		  str.Append(".");
		}
		str.Append(fun);
		str.Append("(");
		for (int i = 0; i < args.Count; i++)
		{
		  if (i > 0)
		  {
			str.Append(", ");
		  }
		  Visit(args[i]);
		}
		str.Append(")");
	  }

	  internal void VisitCallIndex(Call expr)
	  {
		IList<Expr> args = expr.Args;
		bool nested = IsBinaryOrTernaryOperator(args[0]);
		VisitMaybeNested(args[0], nested);
		str.Append("[");
		Visit(args[1]);
		str.Append("]");
	  }

	  internal void VisitCallUnary(Call expr)
	  {
		string fun = expr.Function;
		IList<Expr> args = expr.Args;
		string unmangled = Operator.FindReverse(fun);
		if (string.ReferenceEquals(unmangled, null))
		{
		  throw new System.InvalidOperationException(string.Format("cannot unmangle operator: {0}", fun));
		}
		str.Append(unmangled);
		bool nested = IsComplexOperator(args[0]);
		VisitMaybeNested(args[0], nested);
	  }

	  internal void VisitComprehension(Comprehension expr)
	  {
		// TODO: introduce a macro expansion map between the top-level comprehension id and the
		// function call that the macro replaces.
		throw new System.InvalidOperationException(string.Format("unimplemented : {0}", expr.GetType().Name));
	  }

	  internal void VisitConst(Constant v)
	  {
		str.Append(Debug.FormatLiteral(v));
	  }

	  internal void VisitIdent(Ident expr)
	  {
		str.Append(expr.Name);
	  }

	  internal void VisitList(CreateList expr)
	  {
		IList<Expr> elems = expr.Elements;
		str.Append("[");
		for (int i = 0; i < elems.Count; i++)
		{
		  if (i > 0)
		  {
			str.Append(", ");
		  }
		  Expr elem = elems[i];
		  Visit(elem);
		}
		str.Append("]");
	  }

	  internal void VisitSelect(Select expr)
	  {
		// handle the case when the select expression was generated by the has() macro.
		if (expr.TestOnly)
		{
		  str.Append("has(");
		}
		bool nested = !expr.TestOnly && IsBinaryOrTernaryOperator(expr.Operand);
		VisitMaybeNested(expr.Operand, nested);
		str.Append(".");
		str.Append(expr.Field);
		if (expr.TestOnly)
		{
		  str.Append(")");
		}
	  }

	  internal void VisitStruct(CreateStruct expr)
	  {
		// If the message name is non-empty, then this should be treated as message construction.
		if (expr.MessageName.Length != 0)
		{
		  VisitStructMsg(expr);
		}
		else
		{
		  // Otherwise, build a map.
		  VisitStructMap(expr);
		}
	  }

	  internal void VisitStructMsg(CreateStruct expr)
	  {
		IList<Entry> entries = expr.Entries;
		str.Append(expr.MessageName);
		str.Append("{");
		for (int i = 0; i < entries.Count; i++)
		{
		  if (i > 0)
		  {
			str.Append(", ");
		  }
		  Entry entry = entries[i];
		  string f = entry.FieldKey;
		  str.Append(f);
		  str.Append(": ");
		  Expr v = entry.Value;
		  Visit(v);
		}
		str.Append("}");
	  }

	  internal void VisitStructMap(CreateStruct expr)
	  {
		IList<Entry> entries = expr.Entries;
		str.Append("{");
		for (int i = 0; i < entries.Count; i++)
		{
		  if (i > 0)
		  {
			str.Append(", ");
		  }
		  Entry entry = entries[i];
		  Expr k = entry.MapKey;
		  Visit(k);
		  str.Append(": ");
		  Expr v = entry.Value;
		  Visit(v);
		}
		str.Append("}");
	  }

	  internal void VisitMaybeNested(Expr expr, bool nested)
	  {
		if (nested)
		{
		  str.Append("(");
		}
		Visit(expr);
		if (nested)
		{
		  str.Append(")");
		}
	  }

	  /// <summary>
	  /// isLeftRecursive indicates whether the parser resolves the call in a left-recursive manner as
	  /// this can have an effect of how parentheses affect the order of operations in the AST.
	  /// </summary>
	  internal bool IsLeftRecursive(string op)
	  {
		Operator o = Operator.ById(op);
		return o != Operator.LogicalAnd && o != Operator.LogicalOr;
	  }

	  /// <summary>
	  ///* isSamePrecedence indicates whether the precedence of the input operator is the same as the
	  /// precedence of the (possible) operation represented in the input Expr.
	  /// 
	  /// If the expr is not a Call, the result is false. 
	  /// </summary>
	  internal bool IsSamePrecedence(int opPrecedence, Expr expr)
	  {
		if (expr.ExprKindCase != ExprKindCase.CallExpr)
		{
		  return false;
		}
		string other = expr.CallExpr.Function;
		return opPrecedence == Operator.Precedence(other);
	  }

	  /// <summary>
	  /// isLowerPrecedence indicates whether the precedence of the input operator is lower precedence
	  /// than the (possible) operation represented in the input Expr.
	  /// 
	  /// <para>If the expr is not a Call, the result is false.
	  /// </para>
	  /// </summary>
	  internal bool IsLowerPrecedence(string op, Expr expr)
	  {
		if (expr.ExprKindCase != ExprKindCase.CallExpr)
		{
		  return false;
		}
		string other = expr.CallExpr.Function;
		return Operator.Precedence(op) < Operator.Precedence(other);
	  }

	  /// <summary>
	  /// Indicates whether the expr is a complex operator, i.e., a call expression with 2 or more
	  /// arguments.
	  /// </summary>
	  internal bool IsComplexOperator(Expr expr)
	  {
		return expr.ExprKindCase == ExprKindCase.CallExpr && expr.CallExpr.Args.Count >= 2;
	  }

	  /// <summary>
	  /// Indicates whether it is a complex operation compared to another. expr is *not* considered
	  /// complex if it is not a call expression or has less than two arguments, or if it has a higher
	  /// precedence than op.
	  /// </summary>
	  internal bool IsComplexOperatorWithRespectTo(string op, Expr expr)
	  {
		if (!IsComplexOperator(expr))
		{
		  return false;
		}
		return IsLowerPrecedence(op, expr);
	  }

	  /// <summary>
	  /// Indicate whether this is a binary or ternary operator. </summary>
	  internal bool IsBinaryOrTernaryOperator(Expr expr)
	  {
		if (!IsComplexOperator(expr))
		{
		  return false;
		}
		bool isBinaryOp = Operator.FindReverseBinaryOperator(expr.CallExpr.Function) != null;
		return isBinaryOp || IsSamePrecedence(Operator.Conditional.precedence, expr);
	  }
	}

}