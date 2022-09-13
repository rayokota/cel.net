using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;

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
///     IInterpretableDecorator is a functional interface for decorating or replacing Interpretable
///     expression nodes at construction time.
/// </summary>
public delegate IInterpretable? InterpretableDecorator(IInterpretable? i);

public interface IInterpretableDecorator
{
    IInterpretable Decorate(IInterpretable i);

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

            if (i is IInterpretableAttribute)
                return new EvalWatchAttr((IInterpretableAttribute)i, observer);

            if (i is IInterpretableConst)
                return new EvalWatchConst((IInterpretableConst)i, observer);

            return new EvalWatch(i!, observer);
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

            if (i is IInterpretableAttribute)
            {
                var expr = (IInterpretableAttribute)i;
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
    ///         </li>
    ///         <li>
    ///             convert 'in' operations to set membership tests if possible.
    ///         </li>
    ///     </ul>
    /// </summary>
    static InterpretableDecorator DecOptimize()
    {
        return i =>
        {
            if (i is EvalList) return MaybeBuildListLiteral(i, (EvalList)i);

            if (i is EvalMap) return MaybeBuildMapLiteral(i, (EvalMap)i);

            if (i is IInterpretableCall)
            {
                var inst = (IInterpretableCall)i;
                if (inst.OverloadId().Equals(Overloads.InList)) return MaybeOptimizeSetMembership(i, inst);

                if (Overloads.IsTypeConversionFunction(inst.Function())) return MaybeOptimizeConstUnary(i, inst);
            }

            return i;
        };
    }

    static IInterpretable MaybeOptimizeConstUnary(IInterpretable i, IInterpretableCall call)
    {
        var args = call.Args();
        if (args.Length != 1) return i;

        if (!(args[0] is IInterpretableConst)) return i;

        var val = call.Eval(IActivation.EmptyActivation());
        Err.ThrowErrorAsIllegalStateException(val);
        return IInterpretable.NewConstValue(call.Id(), val);
    }

    static IInterpretable MaybeBuildListLiteral(IInterpretable i, EvalList l)
    {
        foreach (var elem in l.elems)
            if (!(elem is IInterpretableConst))
                return i;

        return IInterpretable.NewConstValue(l.Id(), l.Eval(IActivation.EmptyActivation()));
    }

    static IInterpretable MaybeBuildMapLiteral(IInterpretable i, EvalMap mp)
    {
        for (var idx = 0; idx < mp.keys.Length; idx++)
        {
            if (!(mp.keys[idx] is IInterpretableConst)) return i;

            if (!(mp.vals[idx] is IInterpretableConst)) return i;
        }

        return IInterpretable.NewConstValue(mp.Id(), mp.Eval(IActivation.EmptyActivation()));
    }

    /// <summary>
    ///     maybeOptimizeSetMembership may convert an 'in' operation against a list to map key membership
    ///     test if the following conditions are true:
    ///     <ul>
    ///         <li>
    ///             the list is a constant with homogeneous element types.
    ///         </li>
    ///         <li>
    ///             the elements are all of primitive type.
    ///         </li>
    ///     </ul>
    /// </summary>
    static IInterpretable MaybeOptimizeSetMembership(IInterpretable i, IInterpretableCall inlist)
    {
        var args = inlist.Args();
        var lhs = args[0];
        var rhs = args[1];
        if (!(rhs is IInterpretableConst)) return i;

        var l = (IInterpretableConst)rhs;
        // When the incoming binary call is flagged with as the InList overload, the value will
        // always be convertible to a `traits.Lister` type.
        var list = (ILister)l.Value();
        if (list.Size() == IntT.IntZero) return IInterpretable.NewConstValue(inlist.Id(), BoolT.False);

        var it = list.Iterator();
        IType? typ = null;
        ISet<IVal> valueSet = new HashSet<IVal>();
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

public delegate void EvalObserver(long id, IVal v);