using System;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BytesT.bytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DurationT.durationOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.timestampOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.uintOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.Interpretable.newConstValue;

	using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
	using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Types.Comprehension;
	using CreateList = Google.Api.Expr.V1Alpha1.Expr.Types.CreateList;
	using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using Ident = Google.Api.Expr.V1Alpha1.Expr.Types.Ident;
	using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
	using Reference = Google.Api.Expr.V1Alpha1.Reference;
	using Type = Google.Api.Expr.V1Alpha1.Type;
	using Container = Cel.Common.Containers.Container;
	using Operator = Cel.Common.Operators.Operator;
	using NullT = Cel.Common.Types.NullT;
	using FieldType = Cel.Common.Types.Ref.FieldType;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeProvider = Cel.Common.Types.Ref.TypeProvider;
	using Val = Cel.Common.Types.Ref.Val;
	using Trait = Cel.Common.Types.Traits.Trait;
	using BinaryOp = Cel.Interpreter.Functions.BinaryOp;
	using FunctionOp = Cel.Interpreter.Functions.FunctionOp;
	using Overload = Cel.Interpreter.Functions.Overload;
	using UnaryOp = Cel.Interpreter.Functions.UnaryOp;

	/// <summary>
	/// interpretablePlanner creates an Interpretable evaluation plan from a proto Expr value. </summary>
	public interface InterpretablePlanner
	{
	  /// <summary>
	  /// Plan generates an Interpretable value (or error) from the input proto Expr. </summary>
	  Interpretable Plan(Expr expr);

	  /// <summary>
	  /// newPlanner creates an interpretablePlanner which references a Dispatcher, TypeProvider,
	  /// TypeAdapter, Container, and CheckedExpr value. These pieces of data are used to resolve
	  /// functions, types, and namespaced identifiers at plan time rather than at runtime since it only
	  /// needs to be done once and may be semi-expensive to compute.
	  /// </summary>
	  static InterpretablePlanner NewPlanner(Dispatcher disp, TypeProvider provider, TypeAdapter adapter, AttributeFactory attrFactory, Container cont, CheckedExpr @checked, params InterpretableDecorator[] decorators)
	  {
		return new InterpretablePlanner_Planner(disp, provider, adapter, attrFactory, cont, @checked.getReferenceMapMap(), @checked.getTypeMapMap(), decorators);
	  }

	  /// <summary>
	  /// newUncheckedPlanner creates an interpretablePlanner which references a Dispatcher,
	  /// TypeProvider, TypeAdapter, and Container to resolve functions and types at plan time.
	  /// Namespaces present in Select expressions are resolved lazily at evaluation time.
	  /// </summary>
	  static InterpretablePlanner NewUncheckedPlanner(Dispatcher disp, TypeProvider provider, TypeAdapter adapter, AttributeFactory attrFactory, Container cont, params InterpretableDecorator[] decorators)
	  {
		return new InterpretablePlanner_Planner(disp, provider, adapter, attrFactory, cont, new Dictionary<>(), new Dictionary<>(), decorators);
	  }

	  /// <summary>
	  /// planner is an implementatio of the interpretablePlanner interface. </summary>
	}

	  public sealed class InterpretablePlanner_Planner : InterpretablePlanner
	  {
	internal readonly Dispatcher disp;
	internal readonly TypeProvider provider;
	internal readonly TypeAdapter adapter;
	internal readonly AttributeFactory attrFactory;
	internal readonly Container container;
	internal readonly IDictionary<long, Reference> refMap;
	internal readonly IDictionary<long, Type> typeMap;
	internal readonly InterpretableDecorator[] decorators;

	internal InterpretablePlanner_Planner(Dispatcher disp, TypeProvider provider, TypeAdapter adapter, AttributeFactory attrFactory, Container container, IDictionary<long, Reference> refMap, IDictionary<long, Type> typeMap, InterpretableDecorator[] decorators)
	{
	  this.disp = disp;
	  this.provider = provider;
	  this.adapter = adapter;
	  this.attrFactory = attrFactory;
	  this.container = container;
	  this.refMap = refMap;
	  this.typeMap = typeMap;
	  this.decorators = decorators;
	}

	/// <summary>
	/// Plan implements the interpretablePlanner interface. This implementation of the Plan method
	/// also applies decorators to each Interpretable generated as part of the overall plan.
	/// Decorators are useful for layering functionality into the evaluation that is not natively
	/// understood by CEL, such as state-tracking, expression re-write, and possibly efficient
	/// thread-safe memoization of repeated expressions.
	/// </summary>
	public Interpretable Plan(Expr expr)
	{
	  switch (expr.getExprKindCase())
	  {
		case CALL_EXPR:
		  return Decorate(PlanCall(expr));
		case IDENT_EXPR:
		  return Decorate(PlanIdent(expr));
		case SELECT_EXPR:
		  return Decorate(PlanSelect(expr));
		case LIST_EXPR:
		  return Decorate(PlanCreateList(expr));
		case STRUCT_EXPR:
		  return Decorate(PlanCreateStruct(expr));
		case COMPREHENSION_EXPR:
		  return Decorate(PlanComprehension(expr));
		case CONST_EXPR:
		  return Decorate(PlanConst(expr));
	  }
	  throw new System.ArgumentException(string.Format("unsupported expr of kind {0}: '{1}'", expr.getExprKindCase(), expr));
	}

	/// <summary>
	/// decorate applies the InterpretableDecorator functions to the given Interpretable. Both the
	/// Interpretable and error generated by a Plan step are accepted as arguments for convenience.
	/// </summary>
	internal Interpretable Decorate(Interpretable i)
	{
	  foreach (InterpretableDecorator dec in decorators)
	  {
		i = dec.Decorate(i);
		if (i == null)
		{
		  return null;
		}
	  }
	  return i;
	}

	/// <summary>
	/// planIdent creates an Interpretable that resolves an identifier from an Activation. </summary>
	internal Interpretable PlanIdent(Expr expr)
	{
	  // Establish whether the identifier is in the reference map.
	  Reference identRef = refMap[expr.getId()];
	  if (identRef != null)
	  {
		return PlanCheckedIdent(expr.getId(), identRef);
	  }
	  // Create the possible attribute list for the unresolved reference.
	  Expr.Ident ident = expr.getIdentExpr();
	  return new Interpretable_EvalAttr(adapter, attrFactory.MaybeAttribute(expr.getId(), ident.getName()));
	}

	internal Interpretable PlanCheckedIdent(long id, Reference identRef)
	{
	  // Plan a constant reference if this is the case for this simple identifier.
	  if (identRef.getValue() != Reference.getDefaultInstance().getValue())
	  {
		return Plan(Expr.newBuilder().setId(id).setConstExpr(identRef.getValue()).build());
	  }

	  // Check to see whether the type map indicates this is a type name. All types should be
	  // registered with the provider.
	  Type cType = typeMap[id];
	  if (cType != null && cType.getType() != Type.getDefaultInstance())
	  {
		Val cVal = provider.FindIdent(identRef.getName());
		if (cVal == null)
		{
		  throw new System.InvalidOperationException(string.Format("reference to undefined type: {0}", identRef.getName()));
		}
		return newConstValue(id, cVal);
	  }

	  // Otherwise, return the attribute for the resolved identifier name.
	  return new Interpretable_EvalAttr(adapter, attrFactory.AbsoluteAttribute(id, identRef.getName()));
	}

	/// <summary>
	/// planSelect creates an Interpretable with either:
	/// 
	/// <ol>
	///   <li>selects a field from a map or proto.
	///   <li>creates a field presence test for a select within a has() macro.
	///   <li>resolves the select expression to a namespaced identifier.
	/// </ol>
	/// </summary>
	internal Interpretable PlanSelect(Expr expr)
	{
	  // If the Select id appears in the reference map from the CheckedExpr proto then it is either
	  // a namespaced identifier or enum value.
	  Reference identRef = refMap[expr.getId()];
	  if (identRef != null)
	  {
		return PlanCheckedIdent(expr.getId(), identRef);
	  }

	  Expr.Select sel = expr.getSelectExpr();
	  // Plan the operand evaluation.
	  Interpretable op = Plan(sel.getOperand());

	  // Determine the field type if this is a proto message type.
	  FieldType fieldType = null;
	  Type opType = typeMap[sel.getOperand().getId()];
	  if (opType != null && !opType.getMessageType().isEmpty())
	  {
		FieldType ft = provider.FindFieldType(opType.getMessageType(), sel.getField());
		if (ft != null && ft.isSet != null && ft.getFrom != null)
		{
		  fieldType = ft;
		}
	  }

	  // If the Select was marked TestOnly, this is a presence test.
	  //
	  // Note: presence tests are defined for structured (e.g. proto) and dynamic values (map, json)
	  // as follows:
	  //  - True if the object field has a non-default value, e.g. obj.str != ""
	  //  - True if the dynamic value has the field defined, e.g. key in map
	  //
	  // However, presence tests are not defined for qualified identifier names with primitive
	  // types.
	  // If a string named 'a.b.c' is declared in the environment and referenced within
	  // `has(a.b.c)`,
	  // it is not clear whether has should error or follow the convention defined for structured
	  // values.
	  if (sel.getTestOnly())
	  {
		// Return the test only eval expression.
		return new Interpretable_EvalTestOnly(expr.getId(), op, stringOf(sel.getField()), fieldType);
	  }
	  // Build a qualifier.
	  AttributeFactory_Qualifier qual = attrFactory.NewQualifier(opType, expr.getId(), sel.getField());
	  if (qual == null)
	  {
		return null;
	  }
	  // Lastly, create a field selection Interpretable.
	  if (op is InterpretableAttribute)
	  {
		Interpretable_InterpretableAttribute attr = (Interpretable_InterpretableAttribute) op;
		attr.AddQualifier(qual);
		return attr;
	  }

	  Interpretable_InterpretableAttribute relAttr = RelativeAttr(op.Id(), op);
	  if (relAttr == null)
	  {
		return null;
	  }
	  relAttr.AddQualifier(qual);
	  return relAttr;
	}

	/// <summary>
	/// planCall creates a callable Interpretable while specializing for common functions and
	/// invocation patterns. Specifically, conditional operators &&, ||, ?:, and (in)equality
	/// functions result in optimized Interpretable values.
	/// </summary>
	internal Interpretable PlanCall(Expr expr)
	{
	  Expr.Call call = expr.getCallExpr();
	  ResolvedFunction resolvedFunc = ResolveFunction(expr);
	  // target, fnName, oName := p.resolveFunction(expr)
	  int argCount = call.getArgsCount();
	  int offset = 0;
	  if (resolvedFunc.target != null)
	  {
		argCount++;
		offset++;
	  }

	  Interpretable[] args = new Interpretable[argCount];
	  if (resolvedFunc.target != null)
	  {
		Interpretable arg = Plan(resolvedFunc.target);
		if (arg == null)
		{
		  return null;
		}
		args[0] = arg;
	  }
	  for (int i = 0; i < call.getArgsCount(); i++)
	  {
		Expr argExpr = call.getArgs(i);
		Interpretable arg = Plan(argExpr);
		args[i + offset] = arg;
	  }

	  // Generate specialized Interpretable operators by function name if possible.
	  if (resolvedFunc.fnName.Equals(Operator.LogicalAnd.id))
	  {
		  return PlanCallLogicalAnd(expr, args);
	  }
	  if (resolvedFunc.fnName.Equals(Operator.LogicalOr.id))
	  {
		  return PlanCallLogicalOr(expr, args);
	  }
	  if (resolvedFunc.fnName.Equals(Operator.Conditional.id))
	  {
		return PlanCallConditional(expr, args);
	  }
	  if (resolvedFunc.fnName.Equals(Operator.Equals.id))
	  {
		  return PlanCallEqual(expr, args);
	  }
	  if (resolvedFunc.fnName.Equals(Operator.NotEquals.id))
	  {
		  return PlanCallNotEqual(expr, args);
	  }
	  if (resolvedFunc.fnName.Equals(Operator.Index.id))
	  {
		  return PlanCallIndex(expr, args);
	  }

	  // Otherwise, generate Interpretable calls specialized by argument count.
	  // Try to find the specific function by overload id.
	  Overload fnDef = null;
	  if (!string.ReferenceEquals(resolvedFunc.overloadId, null) && resolvedFunc.overloadId.Length == 0)
	  {
		fnDef = disp.FindOverload(resolvedFunc.overloadId);
	  }
	  // If the overload id couldn't resolve the function, try the simple function name.
	  if (fnDef == null)
	  {
		fnDef = disp.FindOverload(resolvedFunc.fnName);
	  }
	  switch (argCount)
	  {
		case 0:
		  return PlanCallZero(expr, resolvedFunc.fnName, resolvedFunc.overloadId, fnDef);
		case 1:
		  return PlanCallUnary(expr, resolvedFunc.fnName, resolvedFunc.overloadId, fnDef, args);
		case 2:
		  return PlanCallBinary(expr, resolvedFunc.fnName, resolvedFunc.overloadId, fnDef, args);
		default:
		  return PlanCallVarArgs(expr, resolvedFunc.fnName, resolvedFunc.overloadId, fnDef, args);
	  }
	}

	/// <summary>
	/// planCallZero generates a zero-arity callable Interpretable. </summary>
	internal Interpretable PlanCallZero(Expr expr, string function, string overload, Overload impl)
	{
	  if (impl == null || impl.function == null)
	  {
		throw new System.ArgumentException(string.Format("no such overload: {0}()", function));
	  }
	  return new Interpretable_EvalZeroArity(expr.getId(), function, overload, impl.function);
	}

	/// <summary>
	/// planCallUnary generates a unary callable Interpretable. </summary>
	internal Interpretable PlanCallUnary(Expr expr, string function, string overload, Overload impl, Interpretable[] args)
	{
	  UnaryOp fn = null;
	  Trait trait = null;
	  if (impl != null)
	  {
		if (impl.unary == null)
		{
		  throw new System.InvalidOperationException(string.Format("no such overload: {0}(arg)", function));
		}
		fn = impl.unary;
		trait = impl.operandTrait;
	  }
	  return new Interpretable_EvalUnary(expr.getId(), function, overload, args[0], trait, fn);
	}

	/// <summary>
	/// planCallBinary generates a binary callable Interpretable. </summary>
	internal Interpretable PlanCallBinary(Expr expr, string function, string overload, Overload impl, params Interpretable[] args)
	{
	  BinaryOp fn = null;
	  Trait trait = null;
	  if (impl != null)
	  {
		if (impl.binary == null)
		{
		  throw new System.InvalidOperationException(string.Format("no such overload: {0}(lhs, rhs)", function));
		}
		fn = impl.binary;
		trait = impl.operandTrait;
	  }
	  return new Interpretable_EvalBinary(expr.getId(), function, overload, args[0], args[1], trait, fn);
	}

	/// <summary>
	/// planCallVarArgs generates a variable argument callable Interpretable. </summary>
	internal Interpretable PlanCallVarArgs(Expr expr, string function, string overload, Overload impl, params Interpretable[] args)
	{
	  FunctionOp fn = null;
	  Trait trait = null;
	  if (impl != null)
	  {
		if (impl.function == null)
		{
		  throw new System.InvalidOperationException(string.Format("no such overload: {0}(...)", function));
		}
		fn = impl.function;
		trait = impl.operandTrait;
	  }
	  return new Interpretable_EvalVarArgs(expr.getId(), function, overload, args, trait, fn);
	}

	/// <summary>
	/// planCallEqual generates an equals (==) Interpretable. </summary>
	internal Interpretable PlanCallEqual(Expr expr, params Interpretable[] args)
	{
	  return new Interpretable_EvalEq(expr.getId(), args[0], args[1]);
	}

	/// <summary>
	/// planCallNotEqual generates a not equals (!=) Interpretable. </summary>
	internal Interpretable PlanCallNotEqual(Expr expr, params Interpretable[] args)
	{
	  return new Interpretable_EvalNe(expr.getId(), args[0], args[1]);
	}

	/// <summary>
	/// planCallLogicalAnd generates a logical and (&&) Interpretable. </summary>
	internal Interpretable PlanCallLogicalAnd(Expr expr, params Interpretable[] args)
	{
	  return new Interpretable_EvalAnd(expr.getId(), args[0], args[1]);
	}

	/// <summary>
	/// planCallLogicalOr generates a logical or (||) Interpretable. </summary>
	internal Interpretable PlanCallLogicalOr(Expr expr, params Interpretable[] args)
	{
	  return new Interpretable_EvalOr(expr.getId(), args[0], args[1]);
	}

	/// <summary>
	/// planCallConditional generates a conditional / ternary (c ? t : f) Interpretable. </summary>
	internal Interpretable PlanCallConditional(Expr expr, params Interpretable[] args)
	{
	  Interpretable cond = args[0];

	  Interpretable t = args[1];
	  AttributeFactory_Attribute tAttr;
	  if (t is InterpretableAttribute)
	  {
		Interpretable_InterpretableAttribute truthyAttr = (Interpretable_InterpretableAttribute) t;
		tAttr = truthyAttr.Attr();
	  }
	  else
	  {
		tAttr = attrFactory.RelativeAttribute(t.Id(), t);
	  }

	  Interpretable f = args[2];
	  AttributeFactory_Attribute fAttr;
	  if (f is InterpretableAttribute)
	  {
		Interpretable_InterpretableAttribute falsyAttr = (Interpretable_InterpretableAttribute) f;
		fAttr = falsyAttr.Attr();
	  }
	  else
	  {
		fAttr = attrFactory.RelativeAttribute(f.Id(), f);
	  }

	  return new Interpretable_EvalAttr(adapter, attrFactory.ConditionalAttribute(expr.getId(), cond, tAttr, fAttr));
	}

	/// <summary>
	/// planCallIndex either extends an attribute with the argument to the index operation, or
	/// creates a relative attribute based on the return of a function call or operation.
	/// </summary>
	internal Interpretable PlanCallIndex(Expr expr, params Interpretable[] args)
	{
	  Interpretable op = args[0];
	  Interpretable ind = args[1];
	  Interpretable_InterpretableAttribute opAttr = RelativeAttr(op.Id(), op);
	  if (opAttr == null)
	  {
		return null;
	  }
	  Type opType = typeMap[expr.getCallExpr().getTarget().getId()];
	  if (ind is InterpretableConst)
	  {
		Interpretable_InterpretableConst indConst = (Interpretable_InterpretableConst) ind;
		AttributeFactory_Qualifier qual = attrFactory.NewQualifier(opType, expr.getId(), indConst.Value());
		if (qual == null)
		{
		  return null;
		}
		opAttr.AddQualifier(qual);
		return opAttr;
	  }
	  if (ind is InterpretableAttribute)
	  {
		Interpretable_InterpretableAttribute indAttr = (Interpretable_InterpretableAttribute) ind;
		AttributeFactory_Qualifier qual = attrFactory.NewQualifier(opType, expr.getId(), indAttr);
		if (qual == null)
		{
		  return null;
		}
		opAttr.AddQualifier(qual);
		return opAttr;
	  }
	  Interpretable_InterpretableAttribute indQual = RelativeAttr(expr.getId(), ind);
	  if (indQual == null)
	  {
		return null;
	  }
	  opAttr.AddQualifier(indQual);
	  return opAttr;
	}

	/// <summary>
	/// planCreateList generates a list construction Interpretable. </summary>
	internal Interpretable PlanCreateList(Expr expr)
	{
	  Expr.CreateList list = expr.getListExpr();
	  Interpretable[] elems = new Interpretable[list.getElementsCount()];
	  for (int i = 0; i < list.getElementsCount(); i++)
	  {
		Expr elem = list.getElements(i);
		Interpretable elemVal = Plan(elem);
		if (elemVal == null)
		{
		  return null;
		}
		elems[i] = elemVal;
	  }
	  return new Interpretable_EvalList(expr.getId(), elems, adapter);
	}

	/// <summary>
	/// planCreateStruct generates a map or object construction Interpretable. </summary>
	internal Interpretable PlanCreateStruct(Expr expr)
	{
	  Expr.CreateStruct str = expr.getStructExpr();
	  if (!str.getMessageName().isEmpty())
	  {
		return PlanCreateObj(expr);
	  }
	  IList<Expr.CreateStruct.Entry> entries = str.getEntriesList();
	  Interpretable[] keys = new Interpretable[entries.Count];
	  Interpretable[] vals = new Interpretable[entries.Count];
	  for (int i = 0; i < entries.Count; i++)
	  {
		Expr.CreateStruct.Entry entry = entries[i];
		Interpretable keyVal = Plan(entry.getMapKey());
		if (keyVal == null)
		{
		  return null;
		}
		keys[i] = keyVal;

		Interpretable valVal = Plan(entry.getValue());
		if (valVal == null)
		{
		  return null;
		}
		vals[i] = valVal;
	  }
	  return new Interpretable_EvalMap(expr.getId(), keys, vals, adapter);
	}

	/// <summary>
	/// planCreateObj generates an object construction Interpretable. </summary>
	internal Interpretable PlanCreateObj(Expr expr)
	{
	  Expr.CreateStruct obj = expr.getStructExpr();
	  string typeName = ResolveTypeName(obj.getMessageName());
	  if (string.ReferenceEquals(typeName, null))
	  {
		throw new System.InvalidOperationException(string.Format("unknown type: {0}", obj.getMessageName()));
	  }
	  IList<Expr.CreateStruct.Entry> entries = obj.getEntriesList();
	  string[] fields = new string[entries.Count];
	  Interpretable[] vals = new Interpretable[entries.Count];
	  for (int i = 0; i < entries.Count; i++)
	  {
		Expr.CreateStruct.Entry entry = entries[i];
		fields[i] = entry.getFieldKey();
		Interpretable val = Plan(entry.getValue());
		if (val == null)
		{
		  return null;
		}
		vals[i] = val;
	  }
	  return new Interpretable_EvalObj(expr.getId(), typeName, fields, vals, provider);
	}

	/// <summary>
	/// planComprehension generates an Interpretable fold operation. </summary>
	internal Interpretable PlanComprehension(Expr expr)
	{
	  Expr.Comprehension fold = expr.getComprehensionExpr();
	  Interpretable accu = Plan(fold.getAccuInit());
	  if (accu == null)
	  {
		return null;
	  }
	  Interpretable iterRange = Plan(fold.getIterRange());
	  if (iterRange == null)
	  {
		return null;
	  }
	  Interpretable cond = Plan(fold.getLoopCondition());
	  if (cond == null)
	  {
		return null;
	  }
	  Interpretable step = Plan(fold.getLoopStep());
	  if (step == null)
	  {
		return null;
	  }
	  Interpretable result = Plan(fold.getResult());
	  if (result == null)
	  {
		return null;
	  }
	  return new Interpretable_EvalFold(expr.getId(), fold.getAccuVar(), accu, fold.getIterVar(), iterRange, cond, step, result);
	}

	/// <summary>
	/// planConst generates a constant valued Interpretable. </summary>
	internal Interpretable PlanConst(Expr expr)
	{
	  Val val = ConstValue(expr.getConstExpr());
	  if (val == null)
	  {
		return null;
	  }
	  return newConstValue(expr.getId(), val);
	}

	/// <summary>
	/// constValue converts a proto Constant value to a ref.Val. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") Cel.Common.Types.ref.Val constValue(Google.Api.Expr.V1Alpha1.Constant c)
	internal Val ConstValue(Constant c)
	{
	  switch (c.getConstantKindCase())
	  {
		case BOOL_VALUE:
		  return boolOf(c.getBoolValue());
		case BYTES_VALUE:
		  return bytesOf(c.getBytesValue());
		case DOUBLE_VALUE:
		  return doubleOf(c.getDoubleValue());
		case DURATION_VALUE:
		  return durationOf(c.getDurationValue());
		case INT64_VALUE:
		  return intOf(c.getInt64Value());
		case NULL_VALUE:
		  return NullT.NullValue;
		case STRING_VALUE:
		  return stringOf(c.getStringValue());
		case TIMESTAMP_VALUE:
		  return timestampOf(c.getTimestampValue());
		case UINT64_VALUE:
		  return uintOf(c.getUint64Value());
	  }
	  throw new System.ArgumentException(string.Format("unknown constant type: '{0}' of kind '{1}'", c, c.getConstantKindCase()));
	}

	/// <summary>
	/// resolveTypeName takes a qualified string constructed at parse time, applies the proto
	/// namespace resolution rules to it in a scan over possible matching types in the TypeProvider.
	/// </summary>
	internal string ResolveTypeName(string typeName)
	{
	  foreach (string qualifiedTypeName in container.ResolveCandidateNames(typeName))
	  {
		if (provider.FindType(qualifiedTypeName) != null)
		{
		  return qualifiedTypeName;
		}
	  }
	  return null;
	}

	internal sealed class ResolvedFunction
	{
	  internal readonly Expr target;
	  internal readonly string fnName;
	  internal readonly string overloadId;

	  internal ResolvedFunction(Expr target, string fnName, string overloadId)
	  {
		this.target = target;
		this.fnName = fnName;
		this.overloadId = overloadId;
	  }
	}

	/// <summary>
	/// resolveFunction determines the call target, function name, and overload name from a given
	/// Expr value.
	/// 
	/// <para>The resolveFunction resolves ambiguities where a function may either be a receiver-style
	/// invocation or a qualified global function name.
	/// 
	/// <ul>
	///   <li>The target expression may only consist of ident and select expressions.
	///   <li>The function is declared in the environment using its fully-qualified name.
	///   <li>The fully-qualified function name matches the string serialized target value.
	/// </ul>
	/// </para>
	/// </summary>
	internal ResolvedFunction ResolveFunction(Expr expr)
	{
	  // Note: similar logic exists within the `checker/checker.go`. If making changes here
	  // please consider the impact on checker.go and consolidate implementations or mirror code
	  // as appropriate.
	  Expr.Call call = expr.getCallExpr();
	  Expr target = call.hasTarget() ? call.getTarget() : null;
	  string fnName = call.getFunction();

	  // Checked expressions always have a reference map entry, and _should_ have the fully
	  // qualified
	  // function name as the fnName value.
	  Reference oRef = refMap[expr.getId()];
	  if (oRef != null)
	  {
		if (oRef.getOverloadIdCount() == 1)
		{
		  return new ResolvedFunction(target, fnName, oRef.getOverloadId(0));
		}
		// Note, this namespaced function name will not appear as a fully qualified name in ASTs
		// built and stored before cel-go v0.5.0; however, this functionality did not work at all
		// before the v0.5.0 release.
		return new ResolvedFunction(target, fnName, "");
	  }

	  // Parse-only expressions need to handle the same logic as is normally performed at check
	  // time,
	  // but with potentially much less information. The only reliable source of information about
	  // which functions are configured is the dispatcher.
	  if (target == null)
	  {
		// If the user has a parse-only expression, then it should have been configured as such in
		// the interpreter dispatcher as it may have been omitted from the checker environment.
		foreach (string qualifiedName in container.ResolveCandidateNames(fnName))
		{
		  if (disp.FindOverload(qualifiedName) != null)
		  {
			return new ResolvedFunction(target, qualifiedName, "");
		  }
		}
		// It's possible that the overload was not found, but this situation is accounted for in
		// the planCall phase; however, the leading dot used for denoting fully-qualified
		// namespaced identifiers must be stripped, as all declarations already use fully-qualified
		// names. This stripping behavior is handled automatically by the ResolveCandidateNames
		// call.
		return new ResolvedFunction(target, StripLeadingDot(fnName), "");
	  }

	  // Handle the situation where the function target actually indicates a qualified function
	  // name.
	  string qualifiedPrefix = ToQualifiedName(target);
	  if (!string.ReferenceEquals(qualifiedPrefix, null))
	  {
		string maybeQualifiedName = qualifiedPrefix + "." + fnName;
		foreach (string qualifiedName in container.ResolveCandidateNames(maybeQualifiedName))
		{
		  if (disp.FindOverload(qualifiedName) != null)
		  {
			// Clear the target to ensure the proper arity is used for finding the
			// implementation.
			return new ResolvedFunction(null, qualifiedName, "");
		  }
		}
	  }
	  // In the default case, the function is exactly as it was advertised: a receiver call on with
	  // an expression-based target with the given simple function name.
	  return new ResolvedFunction(target, fnName, "");
	}

	internal Interpretable_InterpretableAttribute RelativeAttr(long id, Interpretable eval)
	{
	  Interpretable_InterpretableAttribute eAttr;
	  if (eval is InterpretableAttribute)
	  {
		eAttr = (Interpretable_InterpretableAttribute) eval;
	  }
	  else
	  {
		eAttr = new Interpretable_EvalAttr(adapter, attrFactory.RelativeAttribute(id, eval));
	  }
	  Interpretable decAttr = Decorate(eAttr);
	  if (decAttr == null)
	  {
		return null;
	  }
	  if (!(decAttr is InterpretableAttribute))
	  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new System.InvalidOperationException(string.Format("invalid attribute decoration: {0}({1})", decAttr, decAttr.GetType().FullName));
	  }
	  eAttr = (Interpretable_InterpretableAttribute) decAttr;
	  return eAttr;
	}

	/// <summary>
	/// toQualifiedName converts an expression AST into a qualified name if possible, with a boolean
	/// 'found' value that indicates if the conversion is successful.
	/// </summary>
	internal string ToQualifiedName(Expr operand)
	{
	  // If the checker identified the expression as an attribute by the type-checker, then it can't
	  // possibly be part of qualified name in a namespace.
	  if (refMap.ContainsKey(operand.getId()))
	  {
		return "";
	  }
	  // Since functions cannot be both namespaced and receiver functions, if the operand is not an
	  // qualified variable name, return the (possibly) qualified name given the expressions.
	  return Container.ToQualifiedName(operand);
	}

	internal string StripLeadingDot(string name)
	{
	  return name.StartsWith(".", StringComparison.Ordinal) ? name.Substring(1) : name;
	}
	  }

}