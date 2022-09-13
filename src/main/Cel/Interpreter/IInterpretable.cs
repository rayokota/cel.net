using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter.Functions;

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
///     IInterpretable can accept a given Activation and produce a value along with an accompanying
///     EvalState which can be used to inspect whether additional data might be necessary to complete the
///     evaluation.
/// </summary>
public interface IInterpretable
{
    /// <summary>
    ///     ID value corresponding to the expression node.
    /// </summary>
    long Id();

    /// <summary>
    ///     Eval an Activation to produce an output.
    /// </summary>
    IVal Eval(IActivation activation);


    // Core Interpretable implementations used during the program planning phase.

    /// <summary>
    ///     NewConstValue creates a new constant valued Interpretable.
    /// </summary>
    static IInterpretableConst NewConstValue(long id, IVal val)
    {
        return new EvalConst(id, val);
    }

    static Cost CalShortCircuitBinaryOpsCost(IInterpretable lhs, IInterpretable rhs)
    {
        var l = Cost.EstimateCost(lhs);
        var r = Cost.EstimateCost(rhs);
        return ICoster.CostOf(l.Min, l.Max + r.Max + 1);
    }

    static Cost SumOfCost(IInterpretable[] interps)
    {
        var min = 0L;
        var max = 0L;
        foreach (var interp in interps)
        {
            var t = Cost.EstimateCost(interp);
            min += t.Min;
            max += t.Max;
        }

        return ICoster.CostOf(min, max);
    }

    // Optional Intepretable implementations that specialize, subsume, or extend the core evaluation
    // plan via decorators.

    static Cost CalExhaustiveBinaryOpsCost(IInterpretable lhs, IInterpretable rhs)
    {
        var l = Cost.EstimateCost(lhs);
        var r = Cost.EstimateCost(rhs);
        return Cost.OneOne.Add(l).Add(r);
    }

}

/// <summary>
///     IInterpretableConst interface for tracking whether the Interpretable is a constant value.
/// </summary>
public interface IInterpretableConst : IInterpretable
{
    /// <summary>
    ///     Value returns the constant value of the instruction.
    /// </summary>
    IVal Value();
}

/// <summary>
///     IInterpretableAttribute interface for tracking whether the Interpretable is an attribute.
/// </summary>
public interface IInterpretableAttribute : IInterpretable, IAttribute
{
    /// <summary>
    ///     Attr returns the Attribute value.
    /// </summary>
    IAttribute Attr();

    /// <summary>
    ///     Adapter returns the type adapter to be used for adapting resolved Attribute values.
    /// </summary>
    TypeAdapter Adapter();

    /// <summary>
    ///     AddQualifier proxies the Attribute.AddQualifier method.
    ///     <para>
    ///         Note, this method may mutate the current attribute state. If the desire is to clone the
    ///         Attribute, the Attribute should first be copied before adding the qualifier. Attributes are
    ///         not copyable by default, so this is a capable that would need to be added to the
    ///         AttributeFactory or specifically to the underlying Attribute implementation.
    ///     </para>
    /// </summary>
    IAttribute AddQualifier(IQualifier qualifier);

    /// <summary>
    ///     Qualify replicates the Attribute.Qualify method to permit extension and interception of
    ///     object qualification.
    /// </summary>
    object? Qualify(IActivation vars, object obj);

    /// <summary>
    ///     Resolve returns the value of the Attribute given the current Activation.
    /// </summary>
    object? Resolve(IActivation act);
}

/// <summary>
///     IInterpretableCall interface for inspecting Interpretable instructions related to function
///     calls.
/// </summary>
public interface IInterpretableCall : IInterpretable
{
    /// <summary>
    ///     Function returns the function name as it appears in text or mangled operator name as it
    ///     appears in the operators.go file.
    /// </summary>
    string Function();

    /// <summary>
    ///     OverloadID returns the overload id associated with the function specialization. Overload ids
    ///     are stable across language boundaries and can be treated as synonymous with a unique function
    ///     signature.
    /// </summary>
    string OverloadId();

    /// <summary>
    ///     Args returns the normalized arguments to the function overload. For receiver-style functions,
    ///     the receiver target is arg 0.
    /// </summary>
    IInterpretable[] Args();
}

public sealed class EvalTestOnly : IInterpretable, ICoster
{
    private readonly StringT field;
    private readonly FieldType? fieldType;
    private readonly long id;
    private readonly IInterpretable op;

    internal EvalTestOnly(long id, IInterpretable op, StringT field, FieldType? fieldType)
    {
        this.id = id;
        this.op = op;
        this.field = field;
        this.fieldType = fieldType;
    }

    /// <summary>
    ///     Cost provides the heuristic cost of a `has(field)` macro. The cost has at least 1 for
    ///     determining if the field exists, apart from the cost of accessing the field.
    /// </summary>
    public Cost Cost()
    {
        var c = Interpreter.Cost.EstimateCost(op);
        return c.Add(Interpreter.Cost.OneOne);
    }

    /// <summary>
    ///     ID implements the Interpretable interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public IVal Eval(IActivation ctx)
    {
        // Handle field selection on a proto in the most efficient way possible.
        if (fieldType != null)
            if (op is IInterpretableAttribute)
            {
                var opAttr = (IInterpretableAttribute)op;
                var opVal = opAttr.Resolve(ctx);
                if (opVal is IVal)
                {
                    var refVal = (IVal)opVal;
                    opVal = refVal.Value();
                }

                if (fieldType.IsSet(opVal)) return BoolT.True;

                return BoolT.False;
            }

        var obj = op.Eval(ctx);
        if (obj is IFieldTester) return ((IFieldTester)obj).IsSet(field);

        if (obj is IContainer) return ((IContainer)obj).Contains(field);

        return Err.ValOrErr(obj, "invalid type for field selection.");
    }

    public override string ToString()
    {
        return "EvalTestOnly{" + "id=" + id + ", field=" + field + '}';
    }
}

public abstract class AbstractEval : IInterpretable
{
    internal readonly long id;

    internal AbstractEval(long id)
    {
        this.id = id;
    }

    public abstract IVal Eval(IActivation activation);

    /// <summary>
    ///     ID implements the Interpretable interface method.
    /// </summary>
    public virtual long Id()
    {
        return id;
    }

    public override string ToString()
    {
        return "id=" + id;
    }
}

public abstract class AbstractEvalLhsRhs : AbstractEval, ICoster
{
    internal readonly IInterpretable lhs;
    internal readonly IInterpretable rhs;

    internal AbstractEvalLhsRhs(long id, IInterpretable lhs, IInterpretable rhs) : base(id)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }

    public abstract Cost Cost();
    public abstract override IVal Eval(IActivation activation);

    public override string ToString()
    {
        return "AbstractEvalLhsRhs{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class EvalConst : AbstractEval, IInterpretableConst, ICoster
{
    private readonly IVal val;

    internal EvalConst(long id, IVal val) : base(id)
    {
        this.val = val;
    }

    /// <summary>
    ///     Cost returns zero for a constant valued Interpretable.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.None;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation activation)
    {
        return val;
    }

    /// <summary>
    ///     Value implements the InterpretableConst interface method.
    /// </summary>
    public IVal Value()
    {
        return val;
    }

    public override string ToString()
    {
        return "EvalConst{" + "id=" + id + ", val=" + val + '}';
    }
}

public sealed class EvalOr : AbstractEvalLhsRhs
{
    // TODO combine with EvalExhaustiveOr
    internal EvalOr(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        // short-circuit lhs.
        var lVal = lhs.Eval(ctx);
        if (lVal == BoolT.True) return BoolT.True;

        // short-circuit on rhs.
        var rVal = rhs.Eval(ctx);
        if (rVal == BoolT.True) return BoolT.True;

        // return if both sides are bool false.
        if (lVal == BoolT.False && rVal == BoolT.False) return BoolT.False;

        // TODO: return both values as a set if both are unknown or error.
        // prefer left unknown to right unknown.
        if (UnknownT.IsUnknown(lVal)) return lVal;

        if (UnknownT.IsUnknown(rVal)) return rVal;

        // If the left-hand side is non-boolean return it as the error.
        if (Err.IsError(lVal)) return lVal;

        return Err.NoSuchOverload(lVal, Operator.LogicalOr.Id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method. The minimum possible cost incurs when the
    ///     left-hand side expr is sufficient in determining the evaluation result.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalShortCircuitBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalOr{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class EvalAnd : AbstractEvalLhsRhs
{
    // TODO combine with EvalExhaustiveAnd
    internal EvalAnd(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        // short-circuit lhs.
        var lVal = lhs.Eval(ctx);
        if (lVal == BoolT.False) return BoolT.False;

        // short-circuit on rhs.
        var rVal = rhs.Eval(ctx);
        if (rVal == BoolT.False) return BoolT.False;

        // return if both sides are bool true.
        if (lVal == BoolT.True && rVal == BoolT.True) return BoolT.True;

        // TODO: return both values as a set if both are unknown or error.
        // prefer left unknown to right unknown.
        if (UnknownT.IsUnknown(lVal)) return lVal;

        if (UnknownT.IsUnknown(rVal)) return rVal;

        // If the left-hand side is non-boolean return it as the error.
        if (Err.IsError(lVal)) return lVal;

        return Err.NoSuchOverload(lVal, Operator.LogicalAnd.Id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method. The minimum possible cost incurs when the
    ///     left-hand side expr is sufficient in determining the evaluation result.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalShortCircuitBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalAnd{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class EvalEq : AbstractEvalLhsRhs, IInterpretableCall
{
    internal EvalEq(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        return lVal.Equal(rVal);
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return Operator.Equals.Id;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return Overloads.Equals;
    }

    /// <summary>
    ///     Args implements the InterpretableCall interface method.
    /// </summary>
    public IInterpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalEq{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class EvalNe : AbstractEvalLhsRhs, IInterpretableCall
{
    internal EvalNe(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        var eqVal = lVal.Equal(rVal);
        switch (eqVal.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Err:
                return eqVal;
            case TypeEnum.InnerEnum.Bool:
                return ((INegater)eqVal).Negate();
        }

        return Err.NoSuchOverload(lVal, Operator.NotEquals.Id, rVal);
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return Operator.NotEquals.Id;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return Overloads.NotEquals;
    }

    /// <summary>
    ///     Args implements the InterpretableCall interface method.
    /// </summary>
    public IInterpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalNe{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class EvalZeroArity : AbstractEval, IInterpretableCall,
    ICoster
{
    private readonly string function;
    private readonly FunctionOp impl;
    private readonly string overload;

    internal EvalZeroArity(long id, string function, string overload, FunctionOp impl) : base(id)
    {
        this.function = function;
        this.overload = overload;
        this.impl = impl;
    }

    /// <summary>
    ///     Cost returns 1 representing the heuristic cost of the function.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.OneOne;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation activation)
    {
        return impl();
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return function;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public IInterpretable[] Args()
    {
        return new IInterpretable[0];
    }

    public override string ToString()
    {
        return "EvalZeroArity{" + "id=" + id + ", function='" + function + '\'' + ", overload='" + overload + '\'' +
               ", impl=" + impl + '}';
    }
}

public sealed class EvalUnary : AbstractEval, IInterpretableCall, ICoster
{
    private readonly IInterpretable arg;
    private readonly string function;
    private readonly UnaryOp? impl;
    private readonly string overload;
    private readonly Trait trait;

    internal EvalUnary(long id, string function, string overload, IInterpretable arg, Trait trait,
        UnaryOp? impl) : base(id)
    {
        this.function = function;
        this.overload = overload;
        this.arg = arg;
        this.trait = trait;
        this.impl = impl;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        var c = Interpreter.Cost.EstimateCost(arg);
        return Interpreter.Cost.OneOne.Add(c); // add cost for function
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var argVal = arg.Eval(ctx);
        // Early return if the argument to the function is unknown or error.
        if (Util.IsUnknownOrError(argVal)) return argVal;

        // If the implementation is bound and the argument value has the right traits required to
        // invoke it, then call the implementation.
        if (impl != null && (trait == Trait.None || argVal.Type().HasTrait(trait))) return impl.Invoke(argVal);

        // Otherwise, if the argument is a ReceiverType attempt to invoke the receiver method on the
        // operand (arg0).
        if (argVal.Type().HasTrait(Trait.ReceiverType)) return ((IReceiver)argVal).Receive(function, overload);

        return Err.NoSuchOverload(argVal, function, overload, new IVal[] { });
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return function;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public IInterpretable[] Args()
    {
        return new[] { arg };
    }

    public override string ToString()
    {
        return "EvalUnary{" + "id=" + id + ", function='" + function + '\'' + ", overload='" + overload + '\'' +
               ", arg=" + arg + ", trait=" + trait + ", impl=" + impl + '}';
    }
}

public sealed class EvalBinary : AbstractEvalLhsRhs, IInterpretableCall
{
    private readonly string function;
    private readonly BinaryOp? impl;
    private readonly string overload;
    private readonly Trait trait;

    internal EvalBinary(long id, string function, string overload, IInterpretable lhs,
        IInterpretable rhs, Trait trait, BinaryOp? impl) : base(id, lhs, rhs)
    {
        this.function = function;
        this.overload = overload;
        this.trait = trait;
        this.impl = impl;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        // Early return if any argument to the function is unknown or error.
        if (Util.IsUnknownOrError(lVal)) return lVal;

        if (Util.IsUnknownOrError(rVal)) return rVal;

        // If the implementation is bound and the argument value has the right traits required to
        // invoke it, then call the implementation.
        if (impl != null && (trait == Trait.None || lVal.Type().HasTrait(trait))) return impl.Invoke(lVal, rVal);

        // Otherwise, if the argument is a ReceiverType attempt to invoke the receiver method on the
        // operand (arg0).
        if (lVal.Type().HasTrait(Trait.ReceiverType)) return ((IReceiver)lVal).Receive(function, overload, rVal);

        return Err.NoSuchOverload(lVal, function, overload, new[] { rVal });
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return function;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public IInterpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalBinary{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + ", function='" + function + '\'' +
               ", overload='" + overload + '\'' + ", trait=" + trait + ", impl=" + impl + '}';
    }
}

public sealed class EvalVarArgs : AbstractEval, ICoster, IInterpretableCall
{
    private readonly IInterpretable[] args;
    private readonly string function;
    private readonly FunctionOp? impl;
    private readonly string overload;
    private readonly Trait trait;

    public EvalVarArgs(long id, string function, string overload, IInterpretable[] args, Trait trait,
        FunctionOp? impl) : base(id)
    {
        this.function = function;
        this.overload = overload;
        this.args = args;
        this.trait = trait;
        this.impl = impl;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        var c = IInterpretable.SumOfCost(args);
        return c.Add(Interpreter.Cost.OneOne); // add cost for function
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var argVals = new IVal[args.Length];
        // Early return if any argument to the function is unknown or error.
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            argVals[i] = arg.Eval(ctx);
            if (Util.IsUnknownOrError(argVals[i])) return argVals[i];
        }

        // If the implementation is bound and the argument value has the right traits required to
        // invoke it, then call the implementation.
        var arg0 = argVals[0];
        if (impl != null && (trait == Trait.None || arg0.Type().HasTrait(trait))) return impl.Invoke(argVals);

        // Otherwise, if the argument is a ReceiverType attempt to invoke the receiver method on the
        // operand (arg0).
        if (arg0.Type().HasTrait(Trait.ReceiverType))
        {
            var newArgVals = new IVal[argVals.Length - 1];
            Array.Copy(argVals, 1, newArgVals, 0, argVals.Length - 1);
            return ((IReceiver)arg0).Receive(function, overload, newArgVals);
        }

        return Err.NoSuchOverload(arg0, function, overload, argVals);
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return function;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadId()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public IInterpretable[] Args()
    {
        return args;
    }

    public override string ToString()
    {
        return "EvalVarArgs{" + "id=" + id + ", function='" + function + '\'' + ", overload='" + overload + '\'' +
               ", args="
               + "[" + string.Join(", ", args.Select(o => o.ToString())) + "]" + ", trait=" + trait + ", impl=" +
               impl + '}';
    }
}

public sealed class EvalList : AbstractEval, ICoster
{
    private readonly TypeAdapter adapter;
    internal readonly IInterpretable[] elems;

    internal EvalList(long id, IInterpretable[] elems, TypeAdapter adapter) : base(id)
    {
        this.elems = elems;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return IInterpretable.SumOfCost(elems);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var elemVals = new IVal[elems.Length];
        // If any argument is unknown or error early terminate.
        for (var i = 0; i < elems.Length; i++)
        {
            var elem = elems[i];
            var elemVal = elem.Eval(ctx);
            if (Util.IsUnknownOrError(elemVal)) return elemVal;

            elemVals[i] = elemVal;
        }

        return adapter(elemVals);
    }

    public override string ToString()
    {
        return "EvalList{" + "id=" + id + ", elems=" + "["
               + string.Join(", ", elems.Select(o => o.ToString())) + "]" + '}';
    }
}

public sealed class EvalMap : AbstractEval, ICoster
{
    private readonly TypeAdapter adapter;
    internal readonly IInterpretable[] keys;
    internal readonly IInterpretable[] vals;

    internal EvalMap(long id, IInterpretable[] keys, IInterpretable[] vals, TypeAdapter adapter) :
        base(id)
    {
        this.keys = keys;
        this.vals = vals;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        var k = IInterpretable.SumOfCost(keys);
        var v = IInterpretable.SumOfCost(vals);
        return k.Add(v);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        IDictionary<IVal, IVal> entries = new Dictionary<IVal, IVal>();
        // If any argument is unknown or error early terminate.
        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            var keyVal = key.Eval(ctx);
            if (Util.IsUnknownOrError(keyVal)) return keyVal;

            var valVal = vals[i].Eval(ctx);
            if (Util.IsUnknownOrError(valVal)) return valVal;

            entries[keyVal] = valVal;
        }

        return adapter(entries);
    }

    public override string ToString()
    {
        return "EvalMap{" + "id=" + id + ", keys=" + "["
               + string.Join(", ", keys.Select(o => o.ToString())) + "]" + ", vals=" + "["
               + string.Join(", ", vals.Select(o => o.ToString())) + "]" + '}';
    }
}

public sealed class EvalObj : AbstractEval, ICoster
{
    private readonly string[] fields;
    private readonly ITypeProvider provider;
    private readonly string typeName;
    private readonly IInterpretable[] vals;

    internal EvalObj(long id, string typeName, string[] fields, IInterpretable[] vals,
        ITypeProvider provider) : base(id)
    {
        this.typeName = typeName;
        this.fields = fields;
        this.vals = vals;
        this.provider = provider;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return IInterpretable.SumOfCost(vals);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        IDictionary<string, IVal> fieldVals = new Dictionary<string, IVal>();
        // If any argument is unknown or error early terminate.
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var val = vals[i].Eval(ctx);
            if (Util.IsUnknownOrError(val)) return val;

            fieldVals[field] = val;
        }

        return provider.NewValue(typeName, fieldVals);
    }

    public override string ToString()
    {
        return "EvalObj{" + "id=" + id + ", typeName='" + typeName + '\'' + ", fields=" + "[" +
               string.Join(", ", fields) + "]" + ", vals=" + "["
               + string.Join(", ", vals.Select(o => o.ToString())) + "]" + ", provider=" + provider + '}';
    }
}

public sealed class EvalFold : AbstractEval, ICoster
{
    internal readonly IInterpretable accu;

    // TODO combine with EvalExhaustiveFold
    internal readonly string accuVar;
    internal readonly IInterpretable cond;
    internal readonly IInterpretable iterRange;
    internal readonly string iterVar;
    internal readonly IInterpretable result;
    internal readonly IInterpretable step;

    internal EvalFold(long id, string accuVar, IInterpretable accu, string iterVar,
        IInterpretable iterRange, IInterpretable cond, IInterpretable step, IInterpretable result) : base(id)
    {
        this.accuVar = accuVar;
        this.iterVar = iterVar;
        this.iterRange = iterRange;
        this.accu = accu;
        this.cond = cond;
        this.step = step;
        this.result = result;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        // Compute the cost for evaluating iterRange.
        var i = Interpreter.Cost.EstimateCost(iterRange);

        // Compute the size of iterRange. If the size depends on the input, return the maximum
        // possible
        // cost range.
        var foldRange = iterRange.Eval(IActivation.EmptyActivation());
        if (!foldRange.Type().HasTrait(Trait.IterableType)) return Interpreter.Cost.Unknown;

        var rangeCnt = 0L;
        var it = ((IIterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            it.Next();
            rangeCnt++;
        }

        var a = Interpreter.Cost.EstimateCost(accu);
        var c = Interpreter.Cost.EstimateCost(cond);
        var s = Interpreter.Cost.EstimateCost(step);
        var r = Interpreter.Cost.EstimateCost(result);

        // The cond and step costs are multiplied by size(iterRange). The minimum possible cost incurs
        // when the evaluation result can be determined by the first iteration.
        return i.Add(a).Add(r).Add(ICoster.CostOf(c.Min, c.Max * rangeCnt))
            .Add(ICoster.CostOf(s.Min, s.Max * rangeCnt));
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var foldRange = iterRange.Eval(ctx);
        if (!foldRange.Type().HasTrait(Trait.IterableType))
            return Err.ValOrErr(foldRange, "got '{0}', expected iterable type", foldRange.GetType().FullName!);

        // Configure the fold activation with the accumulator initial value.
        var accuCtx = new VarActivation();
        accuCtx.parent = ctx;
        accuCtx.name = accuVar;
        accuCtx.val = accu.Eval(ctx);
        var iterCtx = new VarActivation();
        iterCtx.parent = accuCtx;
        iterCtx.name = iterVar;
        var it = ((IIterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            // Modify the iter var in the fold activation.
            iterCtx.val = it.Next();

            // Evaluate the condition, terminate the loop if false.
            var c = cond.Eval(iterCtx);
            if (c == BoolT.False) break;

            // Evalute the evaluation step into accu var.
            accuCtx.val = step.Eval(iterCtx);
        }

        // Compute the result.
        return result.Eval(accuCtx);
    }

    public override string ToString()
    {
        return "EvalFold{" + "id=" + id + ", accuVar='" + accuVar + '\'' + ", iterVar='" + iterVar + '\'' +
               ", iterRange=" + iterRange + ", accu=" + accu + ", cond=" + cond + ", step=" + step + ", result=" +
               result + '}';
    }
}

/// <summary>
///     evalSetMembership is an Interpretable implementation which tests whether an input value exists
///     within the set of map keys used to model a set.
/// </summary>
public sealed class EvalSetMembership : AbstractEval, ICoster
{
    private readonly IInterpretable arg;
    private readonly string argTypeName;
    private readonly IInterpretable inst;
    private readonly ISet<IVal> valueSet;

    internal EvalSetMembership(IInterpretable inst, IInterpretable arg, string argTypeName,
        ISet<IVal> valueSet) : base(inst.Id())
    {
        this.inst = inst;
        this.arg = arg;
        this.argTypeName = argTypeName;
        this.valueSet = valueSet;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(arg);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var val = arg.Eval(ctx);
        if (!val.Type().TypeName().Equals(argTypeName)) return Err.NoSuchOverload(null, Operator.In.Id, val);

        return valueSet.Contains(val) ? BoolT.True : BoolT.False;
    }

    public override string ToString()
    {
        return "EvalSetMembership{" + "id=" + id + ", inst=" + inst + ", arg=" + arg + ", argTypeName='" +
               argTypeName + '\'' + ", valueSet=" + valueSet + '}';
    }
}

/// <summary>
///     evalWatch is an Interpretable implementation that wraps the execution of a given expression so
///     that it may observe the computed value and send it to an observer.
/// </summary>
public sealed class EvalWatch : IInterpretable, ICoster
{
    private readonly IInterpretable i;
    private readonly EvalObserver observer;

    public EvalWatch(IInterpretable i, EvalObserver observer)
    {
        this.i = i;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(i);
    }

    public long Id()
    {
        return i.Id();
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public IVal Eval(IActivation ctx)
    {
        var val = i.Eval(ctx);
        observer(Id(), val);
        return val;
    }

    public override string ToString()
    {
        return "EvalWatch{" + i + '}';
    }
}

/// <summary>
///     evalWatchAttr describes a watcher of an instAttr Interpretable.
///     <para>
///         Since the watcher may be selected against at a later stage in program planning, the watcher
///         must implement the instAttr interface by proxy.
///     </para>
/// </summary>
public sealed class EvalWatchAttr : ICoster, IInterpretableAttribute
{
    private readonly IInterpretableAttribute attr;
    private readonly EvalObserver observer;

    public EvalWatchAttr(IInterpretableAttribute attr,
        EvalObserver observer)
    {
        this.attr = attr;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(attr);
    }

    public long Id()
    {
        return ((IInterpretable)attr).Id();
    }

    /// <summary>
    ///     AddQualifier creates a wrapper over the incoming qualifier which observes the qualification
    ///     result.
    /// </summary>
    public IAttribute AddQualifier(IQualifier q)
    {
        if (q is IConstantQualifierEquator)
        {
            var cq = (IConstantQualifierEquator)q;
            q = new EvalWatchConstQualEquat(cq, observer, attr.Adapter());
        }
        else if (q is IConstantQualifier)
        {
            var cq = (IConstantQualifier)q;
            q = new EvalWatchConstQual(cq, observer, attr.Adapter());
        }
        else
        {
            q = new EvalWatchQual(q, observer, attr.Adapter());
        }

        attr.AddQualifier(q);
        return this;
    }

    public IAttribute Attr()
    {
        return attr.Attr();
    }

    public TypeAdapter Adapter()
    {
        return attr.Adapter();
    }

    public object? Qualify(IActivation vars, object obj)
    {
        return attr.Qualify(vars, obj);
    }

    public object? Resolve(IActivation act)
    {
        return attr.Resolve(act);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public IVal Eval(IActivation ctx)
    {
        var val = attr.Eval(ctx);
        observer(Id(), val);
        return val;
    }

    public override string ToString()
    {
        return "EvalWatchAttr{" + attr + '}';
    }
}

public abstract class AbstractEvalWatch<T> : AbstractEval, ICoster,
    IQualifier where T : IQualifier
{
    private readonly TypeAdapter adapter;
    internal readonly T @delegate;
    private readonly EvalObserver observer;

    internal AbstractEvalWatch(T @delegate, EvalObserver observer,
        TypeAdapter adapter) : base(@delegate.Id())
    {
        this.@delegate = @delegate;
        this.observer = observer;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public virtual Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(@delegate);
    }

    /// <summary>
    ///     Qualify observes the qualification of a object via a value computed at runtime.
    /// </summary>
    public virtual object? Qualify(IActivation vars, object obj)
    {
        var @out = @delegate.Qualify(vars, obj);
        IVal val;
        if (@out != null)
            val = adapter(@out);
        else
            val = Err.NewErr(string.Format("qualify failed, vars={0}, obj={1}", vars, obj));

        observer(Id(), val);
        return @out;
    }

    public abstract override IVal Eval(IActivation activation);
}

public sealed class EvalWatchConstQualEquat :
    AbstractEvalWatch<IConstantQualifierEquator>,
    IConstantQualifierEquator
{
    internal EvalWatchConstQualEquat(IConstantQualifierEquator @delegate,
        EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public IVal Value()
    {
        return @delegate.Value();
    }

    /// <summary>
    ///     QualifierValueEquals tests whether the incoming value is equal to the qualificying constant.
    /// </summary>
    public bool QualifierValueEquals(object? value)
    {
        return @delegate.QualifierValueEquals(value);
    }

    public override IVal Eval(IActivation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchConstQualEquat{" + @delegate + '}';
    }
}

/// <summary>
///     evalWatchConstQual observes the qualification of an object using a constant boolean, int,
///     string, or uint.
/// </summary>
public sealed class EvalWatchConstQual :
    AbstractEvalWatch<IConstantQualifier>, IConstantQualifier
{
    internal EvalWatchConstQual(IConstantQualifier @delegate,
        EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public IVal Value()
    {
        return @delegate.Value();
    }

    public override IVal Eval(IActivation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchConstQual{" + @delegate + '}';
    }
}

/// <summary>
///     evalWatchQual observes the qualification of an object by a value computed at runtime.
/// </summary>
public sealed class EvalWatchQual : AbstractEvalWatch<IQualifier>
{
    public EvalWatchQual(IQualifier @delegate,
        EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public override IVal Eval(IActivation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchQual{" + @delegate + '}';
    }
}

/// <summary>
///     evalWatchConst describes a watcher of an instConst Interpretable.
/// </summary>
public sealed class EvalWatchConst : IInterpretableConst, ICoster
{
    private readonly IInterpretableConst c;
    private readonly EvalObserver observer;

    internal EvalWatchConst(IInterpretableConst c,
        EvalObserver observer)
    {
        this.c = c;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(c);
    }

    public long Id()
    {
        return c.Id();
    }

    public IVal Eval(IActivation activation)
    {
        var val = Value();
        observer(Id(), val);
        return val;
    }

    public IVal Value()
    {
        return c.Value();
    }

    public override string ToString()
    {
        return "EvalWatchConst{" + c + '}';
    }
}

/// <summary>
///     evalExhaustiveOr is just like evalOr, but does not short-circuit argument evaluation.
/// </summary>
public sealed class EvalExhaustiveOr : AbstractEvalLhsRhs
{
    // TODO combine with EvalOr
    internal EvalExhaustiveOr(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        if (lVal == BoolT.True || rVal == BoolT.True) return BoolT.True;

        if (lVal == BoolT.False && rVal == BoolT.False) return BoolT.False;

        if (UnknownT.IsUnknown(lVal)) return lVal;

        if (UnknownT.IsUnknown(rVal)) return rVal;

        // TODO: Combine the errors into a set in the future.
        // If the left-hand side is non-boolean return it as the error.
        if (Err.IsError(lVal)) return lVal;

        return Err.NoSuchOverload(lVal, Operator.LogicalOr.Id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalExhaustiveOr{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

/// <summary>
///     evalExhaustiveAnd is just like evalAnd, but does not short-circuit argument evaluation.
/// </summary>
public sealed class EvalExhaustiveAnd : AbstractEvalLhsRhs
{
    // TODO combine with EvalAnd
    internal EvalExhaustiveAnd(long id, IInterpretable lhs, IInterpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        if (lVal == BoolT.False || rVal == BoolT.False) return BoolT.False;

        if (lVal == BoolT.True && rVal == BoolT.True) return BoolT.True;

        if (UnknownT.IsUnknown(lVal)) return lVal;

        if (UnknownT.IsUnknown(rVal)) return rVal;

        if (Err.IsError(lVal)) return lVal;

        return Err.NoSuchOverload(lVal, Operator.LogicalAnd.Id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Cost Cost()
    {
        return IInterpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalExhaustiveAnd{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

/// <summary>
/// evalExhaustiveConditional is like evalConditional, but does not short-circuit argument
/// evaluation.
/// </summary>
public sealed class EvalExhaustiveConditional : AbstractEval, ICoster
{
    // TODO combine with EvalConditional
    private readonly TypeAdapter adapter;
    private readonly ConditionalAttribute attr;

    internal EvalExhaustiveConditional(long id, TypeAdapter adapter,
        ConditionalAttribute attr) : base(id)
    {
        this.adapter = adapter;
        this.attr = attr;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return attr.Cost();
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var cVal = attr.expr.Eval(ctx);
        var tVal = attr.truthy.Resolve(ctx);
        var fVal = attr.falsy.Resolve(ctx);
        if (cVal == BoolT.True)
            return adapter(tVal);
        if (cVal == BoolT.False)
            return adapter(fVal);
        return Err.NoSuchOverload(null, Operator.Conditional.Id, cVal);
    }

    public override string ToString()
    {
        return "EvalExhaustiveConditional{" + "id=" + id + ", attr=" + attr + '}';
    }
}

/// <summary>
///     evalExhaustiveFold is like evalFold, but does not short-circuit argument evaluation.
/// </summary>
public sealed class EvalExhaustiveFold : AbstractEval, ICoster
{
    private readonly IInterpretable accu;

    // TODO combine with EvalFold
    private readonly string accuVar;
    private readonly IInterpretable cond;
    private readonly IInterpretable iterRange;
    private readonly string iterVar;
    private readonly IInterpretable result;
    private readonly IInterpretable step;

    internal EvalExhaustiveFold(long id, IInterpretable accu, string accuVar, IInterpretable iterRange,
        string iterVar, IInterpretable cond, IInterpretable step, IInterpretable result) : base(id)
    {
        this.accuVar = accuVar;
        this.iterVar = iterVar;
        this.iterRange = iterRange;
        this.accu = accu;
        this.cond = cond;
        this.step = step;
        this.result = result;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        // Compute the cost for evaluating iterRange.
        var i = Interpreter.Cost.EstimateCost(iterRange);

        // Compute the size of iterRange. If the size depends on the input, return the maximum
        // possible
        // cost range.
        var foldRange = iterRange.Eval(IActivation.EmptyActivation());
        if (!foldRange.Type().HasTrait(Trait.IterableType)) return Interpreter.Cost.Unknown;

        var rangeCnt = 0L;
        var it = ((IIterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            it.Next();
            rangeCnt++;
        }

        var a = Interpreter.Cost.EstimateCost(accu);
        var c = Interpreter.Cost.EstimateCost(cond);
        var s = Interpreter.Cost.EstimateCost(step);
        var r = Interpreter.Cost.EstimateCost(result);

        // The cond and step costs are multiplied by size(iterRange).
        return i.Add(a).Add(c.Multiply(rangeCnt)).Add(s.Multiply(rangeCnt)).Add(r);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        var foldRange = iterRange.Eval(ctx);
        if (!foldRange.Type().HasTrait(Trait.IterableType))
            return Err.ValOrErr(foldRange, "got '{0}', expected iterable type", foldRange.GetType().FullName);

        // Configure the fold activation with the accumulator initial value.
        var accuCtx = new VarActivation();
        accuCtx.parent = ctx;
        accuCtx.name = accuVar;
        accuCtx.val = accu.Eval(ctx);
        var iterCtx = new VarActivation();
        iterCtx.parent = accuCtx;
        iterCtx.name = iterVar;
        var it = ((IIterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            // Modify the iter var in the fold activation.
            iterCtx.val = it.Next();

            // Evaluate the condition, but don't terminate the loop as this is exhaustive eval!
            cond.Eval(iterCtx);

            // Evalute the evaluation step into accu var.
            accuCtx.val = step.Eval(iterCtx);
        }

        // Compute the result.
        return result.Eval(accuCtx);
    }

    public override string ToString()
    {
        return "EvalExhaustiveFold{" + "id=" + id + ", accuVar='" + accuVar + '\'' + ", iterVar='" + iterVar +
               '\'' + ", iterRange=" + iterRange + ", accu=" + accu + ", cond=" + cond + ", step=" + step +
               ", result=" + result + '}';
    }
}

/// <summary>
///     evalAttr evaluates an Attribute value.
/// </summary>
public sealed class EvalAttr : AbstractEval, IInterpretableAttribute, ICoster
{
    private readonly TypeAdapter adapter;
    private IAttribute attr;

    internal EvalAttr(TypeAdapter adapter, IAttribute attr) : base(attr.Id())
    {
        this.adapter = adapter;
        this.attr = attr;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        return Interpreter.Cost.EstimateCost(attr);
    }

    /// <summary>
    ///     AddQualifier implements the instAttr interface method.
    /// </summary>
    public IAttribute AddQualifier(IQualifier qualifier)
    {
        attr = attr.AddQualifier(qualifier);
        return attr;
    }

    /// <summary>
    ///     Attr implements the instAttr interface method.
    /// </summary>
    public IAttribute Attr()
    {
        return attr;
    }

    /// <summary>
    ///     Adapter implements the instAttr interface method.
    /// </summary>
    public TypeAdapter Adapter()
    {
        return adapter;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override IVal Eval(IActivation ctx)
    {
        try
        {
            var v = attr.Resolve(ctx);
            if (v != null) return adapter(v);

            return Err.NewErr(string.Format("eval failed, ctx: {0}", ctx));
        }
        catch (Exception e)
        {
            return Err.NewErr(e, e.ToString());
        }
    }

    /// <summary>
    ///     Qualify proxies to the Attribute's Qualify method.
    /// </summary>
    public object? Qualify(IActivation ctx, object obj)
    {
        return attr.Qualify(ctx, obj);
    }

    /// <summary>
    ///     Resolve proxies to the Attribute's Resolve method.
    /// </summary>
    public object? Resolve(IActivation ctx)
    {
        return attr.Resolve(ctx);
    }

    public override string ToString()
    {
        return "EvalAttr{" + "id=" + id + ", attr=" + attr + '}';
    }
}