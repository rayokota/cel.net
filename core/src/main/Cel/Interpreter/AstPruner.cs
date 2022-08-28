using System.Collections.Generic;

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
namespace Cel.Interpreter
{
	// TODO Consider having a separate walk of the AST that finds common
	//  subexpressions. This can be called before or after constant folding to find
	//  common subexpressions.

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UnknownT.isUnknown;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Util.isUnknownOrError;

	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Call = Google.Api.Expr.V1Alpha1.Expr.Call;
	using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Comprehension;
	using CreateList = Google.Api.Expr.V1Alpha1.Expr.CreateList;
	using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.CreateStruct;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.CreateStruct.Entry;
	using Select = Google.Api.Expr.V1Alpha1.Expr.Select;
	using ByteString = com.google.protobuf.ByteString;
	using NullValue = com.google.protobuf.NullValue;
	using Operator = Cel.Common.operators.Operator;
	using IteratorT = Cel.Common.Types.IteratorT;
	using Type = Cel.Common.Types.Ref.Type;
	using Val = Cel.Common.Types.Ref.Val;
	using Lister = Cel.Common.Types.Traits.Lister;
	using Mapper = Cel.Common.Types.Traits.Mapper;

	/// <summary>
	/// PruneAst prunes the given AST based on the given EvalState and generates a new AST. Given AST is
	/// copied on write and a new AST is returned.
	/// 
	/// <para>Couple of typical use cases this interface would be:
	/// 
	/// <ol>
	///   <li>
	///       <ol>
	///         <li>Evaluate expr with some unknowns,
	///         <li>If result is unknown:
	///             <ol>
	///               <li>PruneAst
	///               <li>Goto 1
	///             </ol>
	///             Functional call results which are known would be effectively cached across
	///             iterations.
	///       </ol>
	///   <li>
	///       <ol>
	///         <li>Compile the expression (maybe via a service and maybe after checking a compiled
	///             expression does not exists in local cache)
	///         <li>Prepare the environment and the interpreter. Activation might be empty.
	///         <li>Eval the expression. This might return unknown or error or a concrete value.
	///         <li>PruneAst
	///         <li>Maybe cache the expression
	///       </ol>
	/// </ol>
	/// 
	/// </para>
	/// <para>This is effectively constant folding the expression. How the environment is prepared in step 2
	/// is flexible. For example, If the caller caches the compiled and constant folded expressions, but
	/// is not willing to constant fold(and thus cache results of) some external calls, then they can
	/// prepare the overloads accordingly.
	/// </para>
	/// </summary>
	public sealed class AstPruner
	{
	  private readonly Expr expr;
	  private readonly EvalState state;
	  private long nextExprID;

	  private AstPruner(Expr expr, EvalState state, long nextExprID)
	  {
		this.expr = expr;
		this.state = state;
		this.nextExprID = nextExprID;
	  }

	  public static Expr PruneAst(Expr expr, EvalState state)
	  {
		AstPruner pruner = new AstPruner(expr, state, 1);
		Expr newExpr = pruner.Prune(expr);
		return newExpr;
	  }

	  internal static Expr CreateLiteral(long id, Constant val)
	  {
		return Expr.newBuilder().setId(id).setConstExpr(val).build();
	  }

	  internal Expr MaybeCreateLiteral(long id, Val v)
	  {
		Type t = v.Type();
		switch (t.TypeEnum().innerEnumValue)
		{
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bool:
			return CreateLiteral(id, Constant.newBuilder().setBoolValue((bool?) v.Value()).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Int:
			return CreateLiteral(id, Constant.newBuilder().setInt64Value(((Number) v.Value()).longValue()).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Uint:
			return CreateLiteral(id, Constant.newBuilder().setUint64Value(((Number) v.Value()).longValue()).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.String:
			return CreateLiteral(id, Constant.newBuilder().setStringValue(v.Value().ToString()).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.double:
			return CreateLiteral(id, Constant.newBuilder().setDoubleValue(((Number) v.Value()).doubleValue()).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bytes:
			return CreateLiteral(id, Constant.newBuilder().setBytesValue(ByteString.copyFrom((sbyte[]) v.Value())).build());
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Null:
			return CreateLiteral(id, Constant.newBuilder().setNullValue(NullValue.NULL_VALUE).build());
		}

		// Attempt to build a list literal.
		if (v is Lister)
		{
		  Lister list = (Lister) v;
		  int sz = (int) list.Size().IntValue();
		  IList<Expr> elemExprs = new List<Expr>(sz);
		  for (int i = 0; i < sz; i++)
		  {
			Val elem = list.Get(intOf(i));
			if (isUnknownOrError(elem))
			{
			  return null;
			}
			Expr elemExpr = MaybeCreateLiteral(NextID(), elem);
			if (elemExpr == null)
			{
			  return null;
			}
			elemExprs.Add(elemExpr);
		  }
		  return Expr.newBuilder().setId(id).setListExpr(Expr.CreateList.newBuilder().addAllElements(elemExprs).build()).build();
		}

		// Create a map literal if possible.
		if (v is Mapper)
		{
		  Mapper mp = (Mapper) v;
		  IteratorT it = mp.Iterator();
		  IList<Expr.CreateStruct.Entry> entries = new List<Expr.CreateStruct.Entry>((int) mp.Size().IntValue());
		  while (it.HasNext() == True)
		  {
			Val key = it.Next();
			Val val = mp.Get(key);
			if (isUnknownOrError(key) || isUnknownOrError(val))
			{
			  return null;
			}
			Expr keyExpr = MaybeCreateLiteral(NextID(), key);
			if (keyExpr == null)
			{
			  return null;
			}
			Expr valExpr = MaybeCreateLiteral(NextID(), val);
			if (valExpr == null)
			{
			  return null;
			}
			Expr.CreateStruct.Entry entry = Expr.CreateStruct.Entry.newBuilder().setId(NextID()).setMapKey(keyExpr).setValue(valExpr).build();
			entries.Add(entry);
		  }
		  return Expr.newBuilder().setId(id).setStructExpr(Expr.CreateStruct.newBuilder().addAllEntries(entries)).build();
		}

		// TODO(issues/377) To construct message literals, the type provider will need to support
		//  the enumeration the fields for a given message.
		return null;
	  }

	  internal Expr MaybePruneAndOr(Expr node)
	  {
		if (!ExistsWithUnknownValue(node.getId()))
		{
		  return null;
		}

		Expr.Call call = node.getCallExpr();
		// We know result is unknown, so we have at least one unknown arg
		// and if one side is a known value, we know we can ignore it.
		if (ExistsWithKnownValue(call.getArgs(0).getId()))
		{
		  return call.getArgs(1);
		}
		if (ExistsWithKnownValue(call.getArgs(1).getId()))
		{
		  return call.getArgs(0);
		}
		return null;
	  }

	  internal Expr MaybePruneConditional(Expr node)
	  {
		if (!ExistsWithUnknownValue(node.getId()))
		{
		  return null;
		}

		Expr.Call call = node.getCallExpr();
		Val condVal = Value(call.getArgs(0).getId());
		if (condVal == null || isUnknownOrError(condVal))
		{
		  return null;
		}

		if (condVal == True)
		{
		  return call.getArgs(1);
		}
		return call.getArgs(2);
	  }

	  internal Expr MaybePruneFunction(Expr node)
	  {
		Expr.Call call = node.getCallExpr();
		if (call.getFunction().Equals(Operator.LogicalOr.id) || call.getFunction().Equals(Operator.LogicalAnd.id))
		{
		  return MaybePruneAndOr(node);
		}
		if (call.getFunction().Equals(Operator.Conditional.id))
		{
		  return MaybePruneConditional(node);
		}

		return null;
	  }

	  internal Expr Prune(Expr node)
	  {
		if (node == null)
		{
		  return null;
		}
		Val val = Value(node.getId());
		if (val != null && !isUnknownOrError(val))
		{
		  Expr newNode = MaybeCreateLiteral(node.getId(), val);
		  if (newNode != null)
		  {
			return newNode;
		  }
		}

		// We have either an unknown/error value, or something we dont want to
		// transform, or expression was not evaluated. If possible, drill down
		// more.

		switch (node.getExprKindCase())
		{
		  case SELECT_EXPR:
			Expr.Select select = node.getSelectExpr();
			Expr operand = Prune(select.getOperand());
			if (operand != null && operand != select.getOperand())
			{
			  return Expr.newBuilder().setId(node.getId()).setSelectExpr(Expr.Select.newBuilder().setOperand(operand).setField(select.getField()).setTestOnly(select.getTestOnly())).build();
			}
			break;
		  case CALL_EXPR:
			Expr.Call call = node.getCallExpr();
			Expr newExpr = MaybePruneFunction(node);
			if (newExpr != null)
			{
			  newExpr = Prune(newExpr);
			  return newExpr;
			}
			bool prunedCall = false;
			IList<Expr> args = call.getArgsList();
			IList<Expr> newArgs = new List<Expr>(args.Count);
			for (int i = 0; i < args.Count; i++)
			{
			  Expr arg = args[i];
			  newArgs.Add(arg);
			  Expr newArg = Prune(arg);
			  if (newArg != null && newArg != arg)
			  {
				prunedCall = true;
				newArgs[i] = newArg;
			  }
			}
			Expr.Call newCall = Expr.Call.newBuilder().setFunction(call.getFunction()).setTarget(call.getTarget()).addAllArgs(newArgs).build();
			Expr newTarget = Prune(call.getTarget());
			if (newTarget != null && newTarget != call.getTarget())
			{
			  prunedCall = true;
			  newCall = Expr.Call.newBuilder().setFunction(call.getFunction()).setTarget(newTarget).addAllArgs(newArgs).build();
			}
			if (prunedCall)
			{
			  return Expr.newBuilder().setId(node.getId()).setCallExpr(newCall).build();
			}
			break;
		  case LIST_EXPR:
			Expr.CreateList list = node.getListExpr();
			IList<Expr> elems = list.getElementsList();
			IList<Expr> newElems = new List<Expr>(elems.Count);
			bool prunedList = false;
			for (int i = 0; i < elems.Count; i++)
			{
			  Expr elem = elems[i];
			  newElems.Add(elem);
			  Expr newElem = Prune(elem);
			  if (newElem != null && newElem != elem)
			  {
				newElems[i] = newElem;
				prunedList = true;
			  }
			}
			if (prunedList)
			{
			  return Expr.newBuilder().setId(node.getId()).setListExpr(Expr.CreateList.newBuilder().addAllElements(newElems)).build();
			}
			break;
		  case STRUCT_EXPR:
			bool prunedStruct = false;
			Expr.CreateStruct @struct = node.getStructExpr();
			IList<Expr.CreateStruct.Entry> entries = @struct.getEntriesList();
			string messageType = @struct.getMessageName();
			IList<Expr.CreateStruct.Entry> newEntries = new List<Expr.CreateStruct.Entry>(entries.Count);
			for (int i = 0; i < entries.Count; i++)
			{
			  Expr.CreateStruct.Entry entry = entries[i];
			  newEntries.Add(entry);
			  Expr mapKey = entry.getMapKey();
			  Expr newKey = mapKey != Expr.CreateStruct.Entry.getDefaultInstance().getMapKey() ? Prune(mapKey) : null;
			  Expr newValue = Prune(entry.getValue());
			  if ((newKey == null || newKey == mapKey) && (newValue == null || newValue == entry.getValue()))
			  {
				continue;
			  }
			  prunedStruct = true;
			  Expr.CreateStruct.Entry newEntry;
			  if (messageType.Length > 0)
			  {
				newEntry = Expr.CreateStruct.Entry.newBuilder().setFieldKey(entry.getFieldKey()).setValue(newValue).build();
			  }
			  else
			  {
				newEntry = Expr.CreateStruct.Entry.newBuilder().setMapKey(newKey).setValue(newValue).build();
			  }
			  newEntries[i] = newEntry;
			}
			if (prunedStruct)
			{
			  return Expr.newBuilder().setId(node.getId()).setStructExpr(Expr.CreateStruct.newBuilder().setMessageName(messageType).addAllEntries(entries)).build();
			}
			break;
		  case COMPREHENSION_EXPR:
			Expr.Comprehension compre = node.getComprehensionExpr();
			// Only the range of the comprehension is pruned since the state tracking only records
			// the last iteration of the comprehension and not each step in the evaluation which
			// means that the any residuals computed in between might be inaccurate.
			Expr newRange = Prune(compre.getIterRange());
			if (newRange != null && newRange != compre.getIterRange())
			{
			  return Expr.newBuilder().setId(node.getId()).setComprehensionExpr(Expr.Comprehension.newBuilder().setIterVar(compre.getIterVar()).setIterRange(newRange).setAccuVar(compre.getAccuVar()).setAccuInit(compre.getAccuInit()).setLoopCondition(compre.getLoopCondition()).setLoopStep(compre.getLoopStep()).setResult(compre.getResult())).build();
			}
		}

		// Note: original Go implementation returns "node, false". We could wrap 'node' in some
		// 'PruneResult' wrapper, but that would just exchange allocation cost at one point w/
		// allocation cost at another point. So go with the simple approach - at least for now.
		return node;
	  }

	  internal Val Value(long id)
	  {
		return state.Value(id);
	  }

	  internal bool ExistsWithUnknownValue(long id)
	  {
		Val val = Value(id);
		return isUnknown(val);
	  }

	  internal bool ExistsWithKnownValue(long id)
	  {
		Val val = Value(id);
		return val != null && !isUnknown(val);
	  }

	  internal long NextID()
	  {
		while (true)
		{
		  if (state.Value(nextExprID) != null)
		  {
			nextExprID++;
		  }
		  else
		  {
			return nextExprID++;
		  }
		}
	  }
	}

}