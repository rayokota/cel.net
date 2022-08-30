using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter.Functions;
using FieldTester = Cel.Common.Types.Traits.FieldTester;

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
///     Interpretable can accept a given Activation and produce a value along with an accompanying
///     EvalState which can be used to inspect whether additional data might be necessary to complete the
///     evaluation.
/// </summary>
public interface Interpretable
{
    /// <summary>
    ///     ID value corresponding to the expression node.
    /// </summary>
    long Id();

    /// <summary>
    ///     Eval an Activation to produce an output.
    /// </summary>
    Val Eval(Activation activation);

    /// <summary>
    /// InterpretableConst interface for tracking whether the Interpretable is a constant value. </summary>

    /// <summary>
    /// InterpretableAttribute interface for tracking whether the Interpretable is an attribute. </summary>

    /// <summary>
    /// InterpretableCall interface for inspecting Interpretable instructions related to function
    /// calls.
    /// </summary>

    // Core Interpretable implementations used during the program planning phase.

    /// <summary>
    ///     NewConstValue creates a new constant valued Interpretable.
    /// </summary>
    static Interpretable_InterpretableConst NewConstValue(long id, Val val)
    {
        return new Interpretable_EvalConst(id, val);
    }

    static Coster_Cost CalShortCircuitBinaryOpsCost(Interpretable lhs, Interpretable rhs)
    {
        var l = Coster_Cost.EstimateCost(lhs);
        var r = Coster_Cost.EstimateCost(rhs);
        return Coster.CostOf(l.min, l.max + r.max + 1);
    }

    static Coster_Cost SumOfCost(Interpretable[] interps)
    {
        var min = 0L;
        var max = 0L;
        foreach (var interp in interps)
        {
            var t = Coster_Cost.EstimateCost(interp);
            min += t.min;
            max += t.max;
        }

        return Coster.CostOf(min, max);
    }

    // Optional Intepretable implementations that specialize, subsume, or extend the core evaluation
    // plan via decorators.

    /// <summary>
    ///     evalSetMembership is an Interpretable implementation which tests whether an input value exists
    ///     within the set of map keys used to model a set.
    /// </summary>
    /// <summary>
    ///     evalWatch is an Interpretable implementation that wraps the execution of a given expression so
    ///     that it may observe the computed value and send it to an observer.
    /// </summary>
    /// <summary>
    ///     evalWatchAttr describes a watcher of an instAttr Interpretable.
    ///     <para>
    ///         Since the watcher may be selected against at a later stage in program planning, the watcher
    ///         must implement the instAttr interface by proxy.
    ///     </para>
    /// </summary>
    /// <summary>
    ///     evalWatchConstQual observes the qualification of an object using a constant boolean, int,
    ///     string, or uint.
    /// </summary>
    /// <summary>
    ///     evalWatchQual observes the qualification of an object by a value computed at runtime.
    /// </summary>
    /// <summary>
    ///     evalWatchConst describes a watcher of an instConst Interpretable.
    /// </summary>
    /// <summary>
    ///     evalExhaustiveOr is just like evalOr, but does not short-circuit argument evaluation.
    /// </summary>
    /// <summary>
    ///     evalExhaustiveAnd is just like evalAnd, but does not short-circuit argument evaluation.
    /// </summary>
    static Coster_Cost CalExhaustiveBinaryOpsCost(Interpretable lhs, Interpretable rhs)
    {
        var l = Coster_Cost.EstimateCost(lhs);
        var r = Coster_Cost.EstimateCost(rhs);
        return Coster_Cost.OneOne.Add(l).Add(r);
    }

    /// <summary>
    /// evalExhaustiveConditional is like evalConditional, but does not short-circuit argument
    /// evaluation.
    /// </summary>

    /// <summary>
    /// evalExhaustiveFold is like evalFold, but does not short-circuit argument evaluation. </summary>

    /// <summary>
    /// evalAttr evaluates an Attribute value. </summary>
}

public interface Interpretable_InterpretableConst : Interpretable
{
    /// <summary>
    ///     Value returns the constant value of the instruction.
    /// </summary>
    Val Value();
}

public interface Interpretable_InterpretableAttribute : Interpretable, AttributeFactory_Qualifier,
    AttributeFactory_Attribute
{
    /// <summary>
    ///     Attr returns the Attribute value.
    /// </summary>
    AttributeFactory_Attribute Attr();

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
    AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier qualifier);

    /// <summary>
    ///     Qualify replicates the Attribute.Qualify method to permit extension and interception of
    ///     object qualification.
    /// </summary>
    object Qualify(Activation vars, object obj);

    /// <summary>
    ///     Resolve returns the value of the Attribute given the current Activation.
    /// </summary>
    object Resolve(Activation act);
}

public interface Interpretable_InterpretableCall : Interpretable
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
    string OverloadID();

    /// <summary>
    ///     Args returns the normalized arguments to the function overload. For receiver-style functions,
    ///     the receiver target is arg 0.
    /// </summary>
    Interpretable[] Args();
}

public sealed class Interpretable_EvalTestOnly : Interpretable, Coster
{
    internal readonly StringT field;
    internal readonly FieldType fieldType;
    internal readonly long id;
    internal readonly Interpretable op;

    internal Interpretable_EvalTestOnly(long id, Interpretable op, StringT field, FieldType fieldType)
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
    public Coster_Cost Cost()
    {
        var c = Coster_Cost.EstimateCost(op);
        return c.Add(Coster_Cost.OneOne);
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
    public Val Eval(Activation ctx)
    {
        // Handle field selection on a proto in the most efficient way possible.
        if (fieldType != null)
            if (op is Interpretable_InterpretableAttribute)
            {
                var opAttr = (Interpretable_InterpretableAttribute)op;
                var opVal = opAttr.Resolve(ctx);
                if (opVal is Val)
                {
                    var refVal = (Val)opVal;
                    opVal = refVal.Value();
                }

                if (fieldType.isSet(opVal)) return BoolT.True;

                return BoolT.False;
            }

        var obj = op.Eval(ctx);
        if (obj is FieldTester) return ((FieldTester)obj).IsSet(field);

        if (obj is Container) return ((Container)obj).Contains(field);

        return Err.ValOrErr(obj, "invalid type for field selection.");
    }

    public override string ToString()
    {
        return "EvalTestOnly{" + "id=" + id + ", field=" + field + '}';
    }
}

public abstract class Interpretable_AbstractEval : Interpretable
{
    protected internal readonly long id;

    internal Interpretable_AbstractEval(long id)
    {
        this.id = id;
    }

    public abstract Val Eval(Activation activation);

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

public abstract class Interpretable_AbstractEvalLhsRhs : Interpretable_AbstractEval, Coster
{
    protected internal readonly Interpretable lhs;
    protected internal readonly Interpretable rhs;

    internal Interpretable_AbstractEvalLhsRhs(long id, Interpretable lhs, Interpretable rhs) : base(id)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }

    public abstract Coster_Cost Cost();
    public abstract override Val Eval(Activation activation);

    public override string ToString()
    {
        return "AbstractEvalLhsRhs{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalConst : Interpretable_AbstractEval, Interpretable_InterpretableConst, Coster
{
    internal readonly Val val;

    internal Interpretable_EvalConst(long id, Val val) : base(id)
    {
        this.val = val;
    }

    /// <summary>
    ///     Cost returns zero for a constant valued Interpretable.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.None;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation activation)
    {
        return val;
    }

    /// <summary>
    ///     Value implements the InterpretableConst interface method.
    /// </summary>
    public Val Value()
    {
        return val;
    }

    public override string ToString()
    {
        return "EvalConst{" + "id=" + id + ", val=" + val + '}';
    }
}

public sealed class Interpretable_EvalOr : Interpretable_AbstractEvalLhsRhs
{
    // TODO combine with EvalExhaustiveOr
    internal Interpretable_EvalOr(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
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

        return Err.NoSuchOverload(lVal, Operator.LogicalOr.id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method. The minimum possible cost incurs when the
    ///     left-hand side expr is sufficient in determining the evaluation result.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalShortCircuitBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalOr{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalAnd : Interpretable_AbstractEvalLhsRhs
{
    // TODO combine with EvalExhaustiveAnd
    internal Interpretable_EvalAnd(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
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

        return Err.NoSuchOverload(lVal, Operator.LogicalAnd.id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method. The minimum possible cost incurs when the
    ///     left-hand side expr is sufficient in determining the evaluation result.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalShortCircuitBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalAnd{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalEq : Interpretable_AbstractEvalLhsRhs, Interpretable_InterpretableCall
{
    internal Interpretable_EvalEq(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
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
        return Operator.Equals.id;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadID()
    {
        return Overloads.Equals;
    }

    /// <summary>
    ///     Args implements the InterpretableCall interface method.
    /// </summary>
    public Interpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalEq{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalNe : Interpretable_AbstractEvalLhsRhs, Interpretable_InterpretableCall
{
    internal Interpretable_EvalNe(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        var eqVal = lVal.Equal(rVal);
        switch (eqVal.Type().TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Err:
                return eqVal;
            case TypeEnum.InnerEnum.Bool:
                return ((Negater)eqVal).Negate();
        }

        return Err.NoSuchOverload(lVal, Operator.NotEquals.id, rVal);
    }

    /// <summary>
    ///     Function implements the InterpretableCall interface method.
    /// </summary>
    public string Function()
    {
        return Operator.NotEquals.id;
    }

    /// <summary>
    ///     OverloadID implements the InterpretableCall interface method.
    /// </summary>
    public string OverloadID()
    {
        return Overloads.NotEquals;
    }

    /// <summary>
    ///     Args implements the InterpretableCall interface method.
    /// </summary>
    public Interpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalNe{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalZeroArity : Interpretable_AbstractEval, Interpretable_InterpretableCall,
    Coster
{
    internal readonly string function;
    internal readonly FunctionOp impl;
    internal readonly string overload;

    internal Interpretable_EvalZeroArity(long id, string function, string overload, FunctionOp impl) : base(id)
    {
        this.function = function;
        this.overload = overload;
        this.impl = impl;
    }

    /// <summary>
    ///     Cost returns 1 representing the heuristic cost of the function.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.OneOne;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation activation)
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
    public string OverloadID()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public Interpretable[] Args()
    {
        return new Interpretable[0];
    }

    public override string ToString()
    {
        return "EvalZeroArity{" + "id=" + id + ", function='" + function + '\'' + ", overload='" + overload + '\'' +
               ", impl=" + impl + '}';
    }
}

public sealed class Interpretable_EvalUnary : Interpretable_AbstractEval, Interpretable_InterpretableCall, Coster
{
    internal readonly Interpretable arg;
    internal readonly string function;
    internal readonly UnaryOp impl;
    internal readonly string overload;
    internal readonly Trait trait;

    internal Interpretable_EvalUnary(long id, string function, string overload, Interpretable arg, Trait trait,
        UnaryOp impl) : base(id)
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
    public Coster_Cost Cost()
    {
        var c = Coster_Cost.EstimateCost(arg);
        return Coster_Cost.OneOne.Add(c); // add cost for function
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var argVal = arg.Eval(ctx);
        // Early return if the argument to the function is unknown or error.
        if (Util.IsUnknownOrError(argVal)) return argVal;

        // If the implementation is bound and the argument value has the right traits required to
        // invoke it, then call the implementation.
        if (impl != null && (trait == Trait.None || argVal.Type().HasTrait(trait))) return impl.Invoke(argVal);

        // Otherwise, if the argument is a ReceiverType attempt to invoke the receiver method on the
        // operand (arg0).
        if (argVal.Type().HasTrait(Trait.ReceiverType)) return ((Receiver)argVal).Receive(function, overload);

        return Err.NoSuchOverload(argVal, function, overload, new Val[] { });
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
    public string OverloadID()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public Interpretable[] Args()
    {
        return new[] { arg };
    }

    public override string ToString()
    {
        return "EvalUnary{" + "id=" + id + ", function='" + function + '\'' + ", overload='" + overload + '\'' +
               ", arg=" + arg + ", trait=" + trait + ", impl=" + impl + '}';
    }
}

public sealed class Interpretable_EvalBinary : Interpretable_AbstractEvalLhsRhs, Interpretable_InterpretableCall
{
    internal readonly string function;
    internal readonly BinaryOp impl;
    internal readonly string overload;
    internal readonly Trait trait;

    internal Interpretable_EvalBinary(long id, string function, string overload, Interpretable lhs,
        Interpretable rhs, Trait trait, BinaryOp impl) : base(id, lhs, rhs)
    {
        this.function = function;
        this.overload = overload;
        this.trait = trait;
        this.impl = impl;
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
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
        if (lVal.Type().HasTrait(Trait.ReceiverType)) return ((Receiver)lVal).Receive(function, overload, rVal);

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
    public string OverloadID()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public Interpretable[] Args()
    {
        return new[] { lhs, rhs };
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalBinary{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + ", function='" + function + '\'' +
               ", overload='" + overload + '\'' + ", trait=" + trait + ", impl=" + impl + '}';
    }
}

public sealed class Interpretable_EvalVarArgs : Interpretable_AbstractEval, Coster, Interpretable_InterpretableCall
{
    internal readonly Interpretable[] args;
    internal readonly string function;
    internal readonly FunctionOp impl;
    internal readonly string overload;
    internal readonly Trait trait;

    public Interpretable_EvalVarArgs(long id, string function, string overload, Interpretable[] args, Trait trait,
        FunctionOp impl) : base(id)
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
    public Coster_Cost Cost()
    {
        var c = Interpretable.SumOfCost(args);
        return c.Add(Coster_Cost.OneOne); // add cost for function
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var argVals = new Val[args.Length];
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
            var newArgVals = new Val[argVals.Length - 1];
            Array.Copy(argVals, 1, newArgVals, 0, argVals.Length - 1);
            return ((Receiver)arg0).Receive(function, overload, newArgVals);
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
    public string OverloadID()
    {
        return overload;
    }

    /// <summary>
    ///     Args returns the argument to the unary function.
    /// </summary>
    public Interpretable[] Args()
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

public sealed class Interpretable_EvalList : Interpretable_AbstractEval, Coster
{
    internal readonly TypeAdapter adapter;
    internal readonly Interpretable[] elems;

    internal Interpretable_EvalList(long id, Interpretable[] elems, TypeAdapter adapter) : base(id)
    {
        this.elems = elems;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Interpretable.SumOfCost(elems);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var elemVals = new Val[elems.Length];
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

public sealed class Interpretable_EvalMap : Interpretable_AbstractEval, Coster
{
    internal readonly TypeAdapter adapter;
    internal readonly Interpretable[] keys;
    internal readonly Interpretable[] vals;

    internal Interpretable_EvalMap(long id, Interpretable[] keys, Interpretable[] vals, TypeAdapter adapter) :
        base(id)
    {
        this.keys = keys;
        this.vals = vals;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        var k = Interpretable.SumOfCost(keys);
        var v = Interpretable.SumOfCost(vals);
        return k.Add(v);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        IDictionary<Val, Val> entries = new Dictionary<Val, Val>();
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

public sealed class Interpretable_EvalObj : Interpretable_AbstractEval, Coster
{
    internal readonly string[] fields;
    internal readonly TypeProvider provider;
    internal readonly string typeName;
    internal readonly Interpretable[] vals;

    internal Interpretable_EvalObj(long id, string typeName, string[] fields, Interpretable[] vals,
        TypeProvider provider) : base(id)
    {
        this.typeName = typeName;
        this.fields = fields;
        this.vals = vals;
        this.provider = provider;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Interpretable.SumOfCost(vals);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        IDictionary<string, Val> fieldVals = new Dictionary<string, Val>();
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

public sealed class Interpretable_EvalFold : Interpretable_AbstractEval, Coster
{
    internal readonly Interpretable accu;

    // TODO combine with EvalExhaustiveFold
    internal readonly string accuVar;
    internal readonly Interpretable cond;
    internal readonly Interpretable iterRange;
    internal readonly string iterVar;
    internal readonly Interpretable result;
    internal readonly Interpretable step;

    internal Interpretable_EvalFold(long id, string accuVar, Interpretable accu, string iterVar,
        Interpretable iterRange, Interpretable cond, Interpretable step, Interpretable result) : base(id)
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
    public Coster_Cost Cost()
    {
        // Compute the cost for evaluating iterRange.
        var i = Coster_Cost.EstimateCost(iterRange);

        // Compute the size of iterRange. If the size depends on the input, return the maximum
        // possible
        // cost range.
        var foldRange = iterRange.Eval(Activation.EmptyActivation());
        if (!foldRange.Type().HasTrait(Trait.IterableType)) return Coster_Cost.Unknown;

        var rangeCnt = 0L;
        var it = ((IterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            it.Next();
            rangeCnt++;
        }

        var a = Coster_Cost.EstimateCost(accu);
        var c = Coster_Cost.EstimateCost(cond);
        var s = Coster_Cost.EstimateCost(step);
        var r = Coster_Cost.EstimateCost(result);

        // The cond and step costs are multiplied by size(iterRange). The minimum possible cost incurs
        // when the evaluation result can be determined by the first iteration.
        return i.Add(a).Add(r).Add(Coster.CostOf(c.min, c.max * rangeCnt))
            .Add(Coster.CostOf(s.min, s.max * rangeCnt));
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var foldRange = iterRange.Eval(ctx);
        if (!foldRange.Type().HasTrait(Trait.IterableType))
            return Err.ValOrErr(foldRange, "got '{0}', expected iterable type", foldRange.GetType().FullName);

        // Configure the fold activation with the accumulator initial value.
        var accuCtx = new Activation_VarActivation();
        accuCtx.parent = ctx;
        accuCtx.name = accuVar;
        accuCtx.val = accu.Eval(ctx);
        var iterCtx = new Activation_VarActivation();
        iterCtx.parent = accuCtx;
        iterCtx.name = iterVar;
        var it = ((IterableT)foldRange).Iterator();
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

public sealed class Interpretable_EvalSetMembership : Interpretable_AbstractEval, Coster
{
    internal readonly Interpretable arg;
    internal readonly string argTypeName;
    internal readonly Interpretable inst;
    internal readonly ISet<Val> valueSet;

    internal Interpretable_EvalSetMembership(Interpretable inst, Interpretable arg, string argTypeName,
        ISet<Val> valueSet) : base(inst.Id())
    {
        this.inst = inst;
        this.arg = arg;
        this.argTypeName = argTypeName;
        this.valueSet = valueSet;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(arg);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var val = arg.Eval(ctx);
        if (!val.Type().TypeName().Equals(argTypeName)) return Err.NoSuchOverload(null, Operator.In.id, val);

        return valueSet.Contains(val) ? BoolT.True : BoolT.False;
    }

    public override string ToString()
    {
        return "EvalSetMembership{" + "id=" + id + ", inst=" + inst + ", arg=" + arg + ", argTypeName='" +
               argTypeName + '\'' + ", valueSet=" + valueSet + '}';
    }
}

public sealed class Interpretable_EvalWatch : Interpretable, Coster
{
    internal readonly Interpretable i;
    internal readonly InterpretableDecorator_EvalObserver observer;

    public Interpretable_EvalWatch(Interpretable i, InterpretableDecorator_EvalObserver observer)
    {
        this.i = i;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(i);
    }

    public long Id()
    {
        return i.Id();
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public Val Eval(Activation ctx)
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

public sealed class Interpretable_EvalWatchAttr : Coster, Interpretable_InterpretableAttribute,
    AttributeFactory_Attribute
{
    internal readonly Interpretable_InterpretableAttribute attr;
    internal readonly InterpretableDecorator_EvalObserver observer;

    public Interpretable_EvalWatchAttr(Interpretable_InterpretableAttribute attr,
        InterpretableDecorator_EvalObserver observer)
    {
        this.attr = attr;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(attr);
    }

    public long Id()
    {
        return ((Interpretable)attr).Id();
    }

    /// <summary>
    ///     AddQualifier creates a wrapper over the incoming qualifier which observes the qualification
    ///     result.
    /// </summary>
    public AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier q)
    {
        if (q is AttributeFactory_ConstantQualifierEquator)
        {
            var cq = (AttributeFactory_ConstantQualifierEquator)q;
            q = new Interpretable_EvalWatchConstQualEquat(cq, observer, attr.Adapter());
        }
        else if (q is AttributeFactory_ConstantQualifier)
        {
            var cq = (AttributeFactory_ConstantQualifier)q;
            q = new Interpretable_EvalWatchConstQual(cq, observer, attr.Adapter());
        }
        else
        {
            q = new Interpretable_EvalWatchQual(q, observer, attr.Adapter());
        }

        attr.AddQualifier(q);
        return this;
    }

    public AttributeFactory_Attribute Attr()
    {
        return attr.Attr();
    }

    public TypeAdapter Adapter()
    {
        return attr.Adapter();
    }

    public object Qualify(Activation vars, object obj)
    {
        return attr.Qualify(vars, obj);
    }

    public object Resolve(Activation act)
    {
        return attr.Resolve(act);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public Val Eval(Activation ctx)
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

public abstract class Interpretable_AbstractEvalWatch<T> : Interpretable_AbstractEval, Coster,
    AttributeFactory_Qualifier where T : AttributeFactory_Qualifier
{
    protected internal readonly TypeAdapter adapter;
    protected internal readonly T @delegate;
    protected internal readonly InterpretableDecorator_EvalObserver observer;

    internal Interpretable_AbstractEvalWatch(T @delegate, InterpretableDecorator_EvalObserver observer,
        TypeAdapter adapter) : base(@delegate.Id())
    {
        this.@delegate = @delegate;
        this.observer = observer;
        this.adapter = adapter;
    }

    /// <summary>
    ///     Qualify observes the qualification of a object via a value computed at runtime.
    /// </summary>
    public virtual object Qualify(Activation vars, object obj)
    {
        var @out = @delegate.Qualify(vars, obj);
        Val val;
        if (@out != null)
            val = adapter(@out);
        else
            val = Err.NewErr(string.Format("qualify failed, vars={0}, obj={1}", vars, obj));

        observer(Id(), val);
        return @out;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public virtual Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(@delegate);
    }

    public abstract override Val Eval(Activation activation);
}

public sealed class Interpretable_EvalWatchConstQualEquat :
    Interpretable_AbstractEvalWatch<AttributeFactory_ConstantQualifierEquator>,
    AttributeFactory_ConstantQualifierEquator
{
    internal Interpretable_EvalWatchConstQualEquat(AttributeFactory_ConstantQualifierEquator @delegate,
        InterpretableDecorator_EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public Val Value()
    {
        return @delegate.Value();
    }

    /// <summary>
    ///     QualifierValueEquals tests whether the incoming value is equal to the qualificying constant.
    /// </summary>
    public bool QualifierValueEquals(object value)
    {
        return @delegate.QualifierValueEquals(value);
    }

    public override Val Eval(Activation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchConstQualEquat{" + @delegate + '}';
    }
}

public sealed class Interpretable_EvalWatchConstQual :
    Interpretable_AbstractEvalWatch<AttributeFactory_ConstantQualifier>, AttributeFactory_ConstantQualifier, Coster
{
    internal Interpretable_EvalWatchConstQual(AttributeFactory_ConstantQualifier @delegate,
        InterpretableDecorator_EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public Val Value()
    {
        return @delegate.Value();
    }

    public override Val Eval(Activation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchConstQual{" + @delegate + '}';
    }
}

public sealed class Interpretable_EvalWatchQual : Interpretable_AbstractEvalWatch<AttributeFactory_Qualifier>
{
    public Interpretable_EvalWatchQual(AttributeFactory_Qualifier @delegate,
        InterpretableDecorator_EvalObserver observer, TypeAdapter adapter) : base(@delegate, observer, adapter)
    {
    }

    public override Val Eval(Activation activation)
    {
        throw new NotSupportedException("WTF?");
    }

    public override string ToString()
    {
        return "EvalWatchQual{" + @delegate + '}';
    }
}

public sealed class Interpretable_EvalWatchConst : Interpretable_InterpretableConst, Coster
{
    internal readonly Interpretable_InterpretableConst c;
    internal readonly InterpretableDecorator_EvalObserver observer;

    internal Interpretable_EvalWatchConst(Interpretable_InterpretableConst c,
        InterpretableDecorator_EvalObserver observer)
    {
        this.c = c;
        this.observer = observer;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(c);
    }

    public long Id()
    {
        return c.Id();
    }

    public Val Eval(Activation activation)
    {
        var val = Value();
        observer(Id(), val);
        return val;
    }

    public Val Value()
    {
        return c.Value();
    }

    public override string ToString()
    {
        return "EvalWatchConst{" + c + '}';
    }
}

public sealed class Interpretable_EvalExhaustiveOr : Interpretable_AbstractEvalLhsRhs
{
    // TODO combine with EvalOr
    internal Interpretable_EvalExhaustiveOr(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
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

        return Err.NoSuchOverload(lVal, Operator.LogicalOr.id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalExhaustiveOr{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalExhaustiveAnd : Interpretable_AbstractEvalLhsRhs
{
    // TODO combine with EvalAnd
    internal Interpretable_EvalExhaustiveAnd(long id, Interpretable lhs, Interpretable rhs) : base(id, lhs, rhs)
    {
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var lVal = lhs.Eval(ctx);
        var rVal = rhs.Eval(ctx);
        if (lVal == BoolT.False || rVal == BoolT.False) return BoolT.False;

        if (lVal == BoolT.True && rVal == BoolT.True) return BoolT.True;

        if (UnknownT.IsUnknown(lVal)) return lVal;

        if (UnknownT.IsUnknown(rVal)) return rVal;

        if (Err.IsError(lVal)) return lVal;

        return Err.NoSuchOverload(lVal, Operator.LogicalAnd.id, rVal);
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public override Coster_Cost Cost()
    {
        return Interpretable.CalExhaustiveBinaryOpsCost(lhs, rhs);
    }

    public override string ToString()
    {
        return "EvalExhaustiveAnd{" + "id=" + id + ", lhs=" + lhs + ", rhs=" + rhs + '}';
    }
}

public sealed class Interpretable_EvalExhaustiveConditional : Interpretable_AbstractEval, Coster
{
    // TODO combine with EvalConditional
    internal readonly TypeAdapter adapter;
    internal readonly AttributeFactory_ConditionalAttribute attr;

    internal Interpretable_EvalExhaustiveConditional(long id, TypeAdapter adapter,
        AttributeFactory_ConditionalAttribute attr) : base(id)
    {
        this.adapter = adapter;
        this.attr = attr;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return attr.Cost();
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var cVal = attr.expr.Eval(ctx);
        var tVal = attr.truthy.Resolve(ctx);
        var fVal = attr.falsy.Resolve(ctx);
        if (cVal == BoolT.True)
            return adapter(tVal);
        if (cVal == BoolT.False)
            return adapter(fVal);
        return Err.NoSuchOverload(null, Operator.Conditional.id, cVal);
    }

    public override string ToString()
    {
        return "EvalExhaustiveConditional{" + "id=" + id + ", attr=" + attr + '}';
    }
}

public sealed class Interpretable_EvalExhaustiveFold : Interpretable_AbstractEval, Coster
{
    internal readonly Interpretable accu;

    // TODO combine with EvalFold
    internal readonly string accuVar;
    internal readonly Interpretable cond;
    internal readonly Interpretable iterRange;
    internal readonly string iterVar;
    internal readonly Interpretable result;
    internal readonly Interpretable step;

    internal Interpretable_EvalExhaustiveFold(long id, Interpretable accu, string accuVar, Interpretable iterRange,
        string iterVar, Interpretable cond, Interpretable step, Interpretable result) : base(id)
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
    public Coster_Cost Cost()
    {
        // Compute the cost for evaluating iterRange.
        var i = Coster_Cost.EstimateCost(iterRange);

        // Compute the size of iterRange. If the size depends on the input, return the maximum
        // possible
        // cost range.
        var foldRange = iterRange.Eval(Activation.EmptyActivation());
        if (!foldRange.Type().HasTrait(Trait.IterableType)) return Coster_Cost.Unknown;

        var rangeCnt = 0L;
        var it = ((IterableT)foldRange).Iterator();
        while (it.HasNext() == BoolT.True)
        {
            it.Next();
            rangeCnt++;
        }

        var a = Coster_Cost.EstimateCost(accu);
        var c = Coster_Cost.EstimateCost(cond);
        var s = Coster_Cost.EstimateCost(step);
        var r = Coster_Cost.EstimateCost(result);

        // The cond and step costs are multiplied by size(iterRange).
        return i.Add(a).Add(c.Multiply(rangeCnt)).Add(s.Multiply(rangeCnt)).Add(r);
    }

    /// <summary>
    ///     Eval implements the Interpretable interface method.
    /// </summary>
    public override Val Eval(Activation ctx)
    {
        var foldRange = iterRange.Eval(ctx);
        if (!foldRange.Type().HasTrait(Trait.IterableType))
            return Err.ValOrErr(foldRange, "got '{0}', expected iterable type", foldRange.GetType().FullName);

        // Configure the fold activation with the accumulator initial value.
        var accuCtx = new Activation_VarActivation();
        accuCtx.parent = ctx;
        accuCtx.name = accuVar;
        accuCtx.val = accu.Eval(ctx);
        var iterCtx = new Activation_VarActivation();
        iterCtx.parent = accuCtx;
        iterCtx.name = iterVar;
        var it = ((IterableT)foldRange).Iterator();
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

public sealed class Interpretable_EvalAttr : Interpretable_AbstractEval, Interpretable_InterpretableAttribute,
    Coster, AttributeFactory_Qualifier, AttributeFactory_Attribute
{
    internal readonly TypeAdapter adapter;
    internal AttributeFactory_Attribute attr;

    internal Interpretable_EvalAttr(TypeAdapter adapter, AttributeFactory_Attribute attr) : base(attr.Id())
    {
        this.adapter = adapter;
        this.attr = attr;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Coster_Cost Cost()
    {
        return Coster_Cost.EstimateCost(attr);
    }

    /// <summary>
    ///     AddQualifier implements the instAttr interface method.
    /// </summary>
    public AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier qualifier)
    {
        attr = attr.AddQualifier(qualifier);
        return attr;
    }

    /// <summary>
    ///     Attr implements the instAttr interface method.
    /// </summary>
    public AttributeFactory_Attribute Attr()
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
    public override Val Eval(Activation ctx)
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
    public object Qualify(Activation ctx, object obj)
    {
        return attr.Qualify(ctx, obj);
    }

    /// <summary>
    ///     Resolve proxies to the Attribute's Resolve method.
    /// </summary>
    public object Resolve(Activation ctx)
    {
        return attr.Resolve(ctx);
    }

    public override string ToString()
    {
        return "EvalAttr{" + "id=" + id + ", attr=" + attr + '}';
    }
}