using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Type = Cel.Common.Types.Ref.Type;

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

/// <summary>
///     InterpretableDecorator is a functional interface for decorating or replacing Interpretable
///     expression nodes at construction time.
/// </summary>
public delegate Interpretable InterpretableDecorator(Interpretable i);

public interface IInterpretableDecorator
{
    Interpretable Decorate(Interpretable i);

    /// <summary>
    ///     evalObserver is a functional interface that accepts an expression id and an observed value.
    /// </summary>
    /// <summary>
    ///     decObserveEval records evaluation state into an EvalState object.
    /// </summary>
    static InterpretableDecorator DecObserveEval(EvalObserver observer)
    {
        return i =>
        {
            if (i is EvalWatch || i is EvalWatchAttr ||
                i is EvalWatchConst)
                // these instruction are already watching, return straight-away.
                return i;

            if (i is InterpretableAttribute)
                return new EvalWatchAttr((InterpretableAttribute)i, observer);

            if (i is InterpretableConst)
                return new EvalWatchConst((InterpretableConst)i, observer);

            return new EvalWatch(i, observer);
        };
    }

    /// <summary>
    ///     decDisableShortcircuits ensures that all branches of an expression will be evaluated, no
    ///     short-circuiting.
    /// </summary>
    static InterpretableDecorator DecDisableShortcircuits()
    {
        return i =>
        {
            if (i is EvalOr)
            {
                var expr = (EvalOr)i;
                return new EvalExhaustiveOr(expr.id, expr.lhs, expr.rhs);
            }

            if (i is EvalAnd)
            {
                var expr = (EvalAnd)i;
                return new EvalExhaustiveAnd(expr.id, expr.lhs, expr.rhs);
            }

            if (i is EvalFold)
            {
                var expr = (EvalFold)i;
                return new EvalExhaustiveFold(expr.id, expr.accu, expr.accuVar, expr.iterRange,
                    expr.iterVar, expr.cond, expr.step, expr.result);
            }

            if (i is InterpretableAttribute)
            {
                var expr = (InterpretableAttribute)i;
                if (expr.Attr() is ConditionalAttribute)
                    return new EvalExhaustiveConditional(i.Id(), expr.Adapter(),
                        (ConditionalAttribute)expr.Attr());
            }

            return i;
        };
    }

    /// <summary>
    ///     decOptimize optimizes the program plan by looking for common evaluation patterns and
    ///     conditionally precomputating the result.
    ///     <ul>
    ///         <li>
    ///             build list and map values with constant elements.
    ///             <li>convert 'in' operations to set membership tests if possible.
    ///     </ul>
    /// </summary>
    static InterpretableDecorator DecOptimize()
    {
        return i =>
        {
            if (i is EvalList) return MaybeBuildListLiteral(i, (EvalList)i);

            if (i is EvalMap) return MaybeBuildMapLiteral(i, (EvalMap)i);

            if (i is InterpretableCall)
            {
                var inst = (InterpretableCall)i;
                if (inst.OverloadID().Equals(Overloads.InList)) return MaybeOptimizeSetMembership(i, inst);

                if (Overloads.IsTypeConversionFunction(inst.Function())) return MaybeOptimizeConstUnary(i, inst);
            }

            return i;
        };
    }

    static Interpretable MaybeOptimizeConstUnary(Interpretable i, InterpretableCall call)
    {
        var args = call.Args();
        if (args.Length != 1) return i;

        if (!(args[0] is InterpretableConst)) return i;

        var val = call.Eval(Activation.EmptyActivation());
        Err.ThrowErrorAsIllegalStateException(val);
        return Interpretable.NewConstValue(call.Id(), val);
    }

    static Interpretable MaybeBuildListLiteral(Interpretable i, EvalList l)
    {
        foreach (var elem in l.elems)
            if (!(elem is InterpretableConst))
                return i;

        return Interpretable.NewConstValue(l.Id(), l.Eval(Activation.EmptyActivation()));
    }

    static Interpretable MaybeBuildMapLiteral(Interpretable i, EvalMap mp)
    {
        for (var idx = 0; idx < mp.keys.Length; idx++)
        {
            if (!(mp.keys[idx] is InterpretableConst)) return i;

            if (!(mp.vals[idx] is InterpretableConst)) return i;
        }

        return Interpretable.NewConstValue(mp.Id(), mp.Eval(Activation.EmptyActivation()));
    }

    /// <summary>
    ///     maybeOptimizeSetMembership may convert an 'in' operation against a list to map key membership
    ///     test if the following conditions are true:
    ///     <ul>
    ///         <li>
    ///             the list is a constant with homogeneous element types.
    ///             <li>the elements are all of primitive type.
    ///     </ul>
    /// </summary>
    static Interpretable MaybeOptimizeSetMembership(Interpretable i, InterpretableCall inlist)
    {
        var args = inlist.Args();
        var lhs = args[0];
        var rhs = args[1];
        if (!(rhs is InterpretableConst)) return i;

        var l = (InterpretableConst)rhs;
        // When the incoming binary call is flagged with as the InList overload, the value will
        // always be convertible to a `traits.Lister` type.
        var list = (Lister)l.Value();
        if (list.Size() == IntT.IntZero) return Interpretable.NewConstValue(inlist.Id(), BoolT.False);

        var it = list.Iterator();
        Type typ = null;
        ISet<Val> valueSet = new HashSet<Val>();
        while (it.HasNext() == BoolT.True)
        {
            var elem = it.Next();
            if (!Util.IsPrimitiveType(elem))
                // Note, non-primitive type are not yet supported.
                return i;

            if (typ == null)
                typ = elem.Type();
            else if (!typ.TypeName().Equals(elem.Type().TypeName())) return i;

            valueSet.Add(elem);
        }

        return new EvalSetMembership(inlist, lhs, typ.TypeName(), valueSet);
    }
}

public delegate void EvalObserver(long id, Val v);