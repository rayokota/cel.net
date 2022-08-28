using System.Collections.Generic;
using Cel.Common.Types;

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
//	import static Cel.Common.Types.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.throwErrorAsIllegalStateException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.Activation.emptyActivation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.Interpretable.newConstValue;

	using IteratorT = Cel.Common.Types.IteratorT;
	using Overloads = Cel.Common.Types.Overloads;
	using Util = Cel.Common.Types.Util;
	using Type = Cel.Common.Types.Ref.Type;
	using Val = Cel.Common.Types.Ref.Val;
	using Lister = Cel.Common.Types.Traits.Lister;

	/// <summary>
	/// InterpretableDecorator is a functional interface for decorating or replacing Interpretable
	/// expression nodes at construction time.
	/// </summary>
	public delegate Interpretable InterpretableDecorator(Interpretable i);
	
	public interface IInterpretableDecorator
	{
	  Interpretable Decorate(Interpretable i);

	  /// <summary>
	  /// evalObserver is a functional interface that accepts an expression id and an observed value. </summary>

	  /// <summary>
	  /// decObserveEval records evaluation state into an EvalState object. </summary>
	  static InterpretableDecorator DecObserveEval(InterpretableDecorator_EvalObserver observer)
	  {
		return i =>
		{
		  if ((i is Interpretable_EvalWatch) || (i is Interpretable_EvalWatchAttr) || (i is Interpretable_EvalWatchConst))
		  {
			// these instruction are already watching, return straight-away.
			return i;
		  }
		  if (i is Interpretable_InterpretableAttribute)
		  {
			return new Interpretable_EvalWatchAttr((Interpretable_InterpretableAttribute) i, observer);
		  }
		  if (i is Interpretable_InterpretableConst)
		  {
			return new Interpretable_EvalWatchConst((Interpretable_InterpretableConst) i, observer);
		  }
		  return new Interpretable_EvalWatch(i, observer);
		};
	  }

	  /// <summary>
	  /// decDisableShortcircuits ensures that all branches of an expression will be evaluated, no
	  /// short-circuiting.
	  /// </summary>
	  static InterpretableDecorator DecDisableShortcircuits()
	  {
		return i =>
		{
		  if (i is Interpretable_EvalOr)
		  {
			Interpretable_EvalOr expr = (Interpretable_EvalOr) i;
			return new Interpretable_EvalExhaustiveOr(expr.id, expr.lhs, expr.rhs);
		  }
		  if (i is Interpretable_EvalAnd)
		  {
			Interpretable_EvalAnd expr = (Interpretable_EvalAnd) i;
			return new Interpretable_EvalExhaustiveAnd(expr.id, expr.lhs, expr.rhs);
		  }
		  if (i is Interpretable_EvalFold)
		  {
			Interpretable_EvalFold expr = (Interpretable_EvalFold) i;
			return new Interpretable_EvalExhaustiveFold(expr.id, expr.accu, expr.accuVar, expr.iterRange, expr.iterVar, expr.cond, expr.step, expr.result);
		  }
		  if (i is Interpretable_InterpretableAttribute)
		  {
			Interpretable_InterpretableAttribute expr = (Interpretable_InterpretableAttribute) i;
			if (expr.Attr() is AttributeFactory_ConditionalAttribute)
			{
			  return new Interpretable_EvalExhaustiveConditional(i.Id(), expr.Adapter(), (AttributeFactory_ConditionalAttribute) expr.Attr());
			}
		  }
		  return i;
		};
	  }

	  /// <summary>
	  /// decOptimize optimizes the program plan by looking for common evaluation patterns and
	  /// conditionally precomputating the result.
	  /// 
	  /// <ul>
	  ///   <li>build list and map values with constant elements.
	  ///   <li>convert 'in' operations to set membership tests if possible.
	  /// </ul>
	  /// </summary>
	  static InterpretableDecorator DecOptimize()
	  {
		return i =>
		{
		  if (i is Interpretable_EvalList)
		  {
			return MaybeBuildListLiteral(i, (Interpretable_EvalList) i);
		  }
		  if (i is Interpretable_EvalMap)
		  {
			return MaybeBuildMapLiteral(i, (Interpretable_EvalMap) i);
		  }
		  if (i is Interpretable_InterpretableCall)
		  {
			Interpretable_InterpretableCall inst = (Interpretable_InterpretableCall) i;
			if (inst.OverloadID().Equals(Overloads.InList))
			{
			  return MaybeOptimizeSetMembership(i, inst);
			}
			if (Overloads.IsTypeConversionFunction(inst.Function()))
			{
			  return MaybeOptimizeConstUnary(i, inst);
			}
		  }
		  return i;
		};
	  }

	  static Interpretable MaybeOptimizeConstUnary(Interpretable i, Interpretable_InterpretableCall call)
	  {
		Interpretable[] args = call.Args();
		if (args.Length != 1)
		{
		  return i;
		}
		if (!(args[0] is Interpretable_InterpretableConst))
		{
		  return i;
		}
		Val val = call.Eval(Activation.EmptyActivation());
		Err.ThrowErrorAsIllegalStateException(val);
		return Interpretable.NewConstValue(call.Id(), val);
	  }

	  static Interpretable MaybeBuildListLiteral(Interpretable i, Interpretable_EvalList l)
	  {
		foreach (Interpretable elem in l.elems)
		{
		  if (!(elem is Interpretable_InterpretableConst))
		  {
			return i;
		  }
		}
		return Interpretable.NewConstValue(l.Id(), l.Eval(Activation.EmptyActivation()));
	  }

	  static Interpretable MaybeBuildMapLiteral(Interpretable i, Interpretable_EvalMap mp)
	  {
		for (int idx = 0; idx < mp.keys.Length; idx++)
		{
		  if (!(mp.keys[idx] is Interpretable_InterpretableConst))
		  {
			return i;
		  }
		  if (!(mp.vals[idx] is Interpretable_InterpretableConst))
		  {
			return i;
		  }
		}
		return Interpretable.NewConstValue(mp.Id(), mp.Eval(Activation.EmptyActivation()));
	  }

	  /// <summary>
	  /// maybeOptimizeSetMembership may convert an 'in' operation against a list to map key membership
	  /// test if the following conditions are true:
	  /// 
	  /// <ul>
	  ///   <li>the list is a constant with homogeneous element types.
	  ///   <li>the elements are all of primitive type.
	  /// </ul>
	  /// </summary>
	  static Interpretable MaybeOptimizeSetMembership(Interpretable i, Interpretable_InterpretableCall inlist)
	  {
		Interpretable[] args = inlist.Args();
		Interpretable lhs = args[0];
		Interpretable rhs = args[1];
		if (!(rhs is Interpretable_InterpretableConst))
		{
		  return i;
		}
		Interpretable_InterpretableConst l = (Interpretable_InterpretableConst) rhs;
		// When the incoming binary call is flagged with as the InList overload, the value will
		// always be convertible to a `traits.Lister` type.
		Lister list = (Lister) l.Value();
		if (list.Size() == IntT.IntZero)
		{
		  return Interpretable.NewConstValue(inlist.Id(), BoolT.False);
		}
		IteratorT it = list.Iterator();
		Type typ = null;
		ISet<Val> valueSet = new HashSet<Val>();
		while (it.HasNext() == BoolT.True)
		{
		  Val elem = it.Next();
		  if (!Util.IsPrimitiveType(elem))
		  {
			// Note, non-primitive type are not yet supported.
			return i;
		  }
		  if (typ == null)
		  {
			typ = elem.Type();
		  }
		  else if (!typ.TypeName().Equals(elem.Type().TypeName()))
		  {
			return i;
		  }
		  valueSet.Add(elem);
		}
		return new Interpretable_EvalSetMembership(inlist, lhs, typ.TypeName(), valueSet);
	  }
	}

	public delegate void InterpretableDecorator_EvalObserver(long id, Val v);
}