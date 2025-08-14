﻿using Cel.Common.Containers;
using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter.Functions;
using Google.Api.Expr.V1Alpha1;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
///     IInterpretablePlanner creates an Interpretable evaluation plan from a proto Expr value.
/// </summary>
public interface IInterpretablePlanner
{
    /// <summary>
    ///     Plan generates an Interpretable value (or error) from the input proto Expr.
    /// </summary>
    IInterpretable? Plan(Expr expr);

    /// <summary>
    ///     newPlanner creates an interpretablePlanner which references a Dispatcher, TypeProvider,
    ///     TypeAdapter, Container, and CheckedExpr value. These pieces of data are used to resolve
    ///     functions, types, and namespaced identifiers at plan time rather than at runtime since it only
    ///     needs to be done once and may be semi-expensive to compute.
    /// </summary>
    

    /// <summary>
    ///     newUncheckedPlanner creates an interpretablePlanner which references a Dispatcher,
    ///     TypeProvider, TypeAdapter, and Container to resolve functions and types at plan time.
    ///     Namespaces present in Select expressions are resolved lazily at evaluation time.
    /// </summary>
}

public static class InterpretablePlannerFactory
{
    public static IInterpretablePlanner NewPlanner(IDispatcher disp, ITypeProvider provider, TypeAdapter adapter,
        IAttributeFactory attrFactory, Container cont, CheckedExpr @checked,
        params InterpretableDecorator[] decorators)
    {
        return new Planner(disp, provider, adapter, attrFactory, cont, @checked.ReferenceMap,
            @checked.TypeMap, decorators);
    }

    public static IInterpretablePlanner NewUncheckedPlanner(IDispatcher disp, ITypeProvider provider, TypeAdapter adapter,
        IAttributeFactory attrFactory, Container cont, params InterpretableDecorator[] decorators)
    {
        return new Planner(disp, provider, adapter, attrFactory, cont,
            new Dictionary<long, Reference>(), new Dictionary<long, Type>(), decorators);
    }
}

/// <summary>
///     planner is an implementatio of the interpretablePlanner interface.
/// </summary>
public sealed class Planner : IInterpretablePlanner
{
    private readonly TypeAdapter adapter;
    private readonly IAttributeFactory attrFactory;
    private readonly Container container;
    private readonly InterpretableDecorator[] decorators;
    private readonly IDispatcher disp;
    private readonly ITypeProvider provider;
    private readonly IDictionary<long, Reference> refMap;
    private readonly IDictionary<long, Type> typeMap;

    internal Planner(IDispatcher disp, ITypeProvider provider, TypeAdapter adapter,
        IAttributeFactory attrFactory, Container container, IDictionary<long, Reference> refMap,
        IDictionary<long, Type> typeMap, InterpretableDecorator[] decorators)
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
    ///     Plan implements the interpretablePlanner interface. This implementation of the Plan method
    ///     also applies decorators to each Interpretable generated as part of the overall plan.
    ///     Decorators are useful for layering functionality into the evaluation that is not natively
    ///     understood by CEL, such as state-tracking, expression re-write, and possibly efficient
    ///     thread-safe memoization of repeated expressions.
    /// </summary>
    public IInterpretable? Plan(Expr expr)
    {
        switch (expr.ExprKindCase)
        {
            case Expr.ExprKindOneofCase.CallExpr:
                return Decorate(PlanCall(expr));
            case Expr.ExprKindOneofCase.IdentExpr:
                return Decorate(PlanIdent(expr));
            case Expr.ExprKindOneofCase.SelectExpr:
                return Decorate(PlanSelect(expr));
            case Expr.ExprKindOneofCase.ListExpr:
                return Decorate(PlanCreateList(expr));
            case Expr.ExprKindOneofCase.StructExpr:
                return Decorate(PlanCreateStruct(expr));
            case Expr.ExprKindOneofCase.ComprehensionExpr:
                return Decorate(PlanComprehension(expr));
            case Expr.ExprKindOneofCase.ConstExpr:
                return Decorate(PlanConst(expr));
        }

        throw new ArgumentException(string.Format("unsupported expr of kind {0}: '{1}'", expr.ExprKindCase,
            expr));
    }

    /// <summary>
    ///     decorate applies the InterpretableDecorator functions to the given Interpretable. Both the
    ///     Interpretable and error generated by a Plan step are accepted as arguments for convenience.
    /// </summary>
    internal IInterpretable? Decorate(IInterpretable? i)
    {
        foreach (var dec in decorators)
        {
            i = dec(i);
            if (i == null) return null;
        }

        return i;
    }

    /// <summary>
    ///     planIdent creates an Interpretable that resolves an identifier from an Activation.
    /// </summary>
    internal IInterpretable? PlanIdent(Expr expr)
    {
        // Establish whether the identifier is in the reference map.
        refMap.TryGetValue(expr.Id, out var identRef);
        if (identRef != null) return PlanCheckedIdent(expr.Id, identRef);

        // Create the possible attribute list for the unresolved reference.
        var ident = expr.IdentExpr;
        return new EvalAttr(adapter, attrFactory.MaybeAttribute(expr.Id, ident.Name));
    }

    internal IInterpretable? PlanCheckedIdent(long id, Reference identRef)
    {
        // Plan a constant reference if this is the case for this simple identifier.
        if (identRef.Value != null && !Equals(identRef.Value, new Reference().Value))
        {
            var expr = new Expr();
            expr.Id = id;
            expr.ConstExpr = identRef.Value;
            return Plan(expr);
        }

        // Check to see whether the type map indicates this is a type name. All types should be
        // registered with the provider.
        typeMap.TryGetValue(id, out var cType);
        if (cType != null && cType.Type_ != null && !Equals(cType.Type_, new Type()))
        {
            var cVal = provider.FindIdent(identRef.Name);
            if (cVal == null)
                throw new InvalidOperationException(string.Format("reference to undefined type: {0}",
                    identRef.Name));

            return InterpretableUtils.NewConstValue(id, cVal);
        }

        // Otherwise, return the attribute for the resolved identifier name.
        return new EvalAttr(adapter, attrFactory.AbsoluteAttribute(id, identRef.Name));
    }

    /// <summary>
    ///     planSelect creates an Interpretable with either:
    ///     <ol>
    ///         <li>
    ///             selects a field from a map or proto.
    ///         </li>
    ///         <li>
    ///             creates a field presence test for a select within a has() macro.
    ///         </li>
    ///         <li>
    ///             resolves the select expression to a namespaced identifier.
    ///         </li>
    ///     </ol>
    /// </summary>
    internal IInterpretable? PlanSelect(Expr expr)
    {
        // If the Select id appears in the reference map from the CheckedExpr proto then it is either
        // a namespaced identifier or enum value.
        refMap.TryGetValue(expr.Id, out var identRef);
        if (identRef != null) return PlanCheckedIdent(expr.Id, identRef);

        var sel = expr.SelectExpr;
        // Plan the operand evaluation.
        var op = Plan(sel.Operand);

        // Determine the field type if this is a proto message type.
        FieldType? fieldType = null;
        typeMap.TryGetValue(sel.Operand.Id, out var opType);
        if (opType != null && opType.MessageType.Length > 0)
        {
            var ft = provider.FindFieldType(opType.MessageType, sel.Field);
            if (ft != null) fieldType = ft;
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
        if (sel.TestOnly)
            // Return the test only eval expression.
            return new EvalTestOnly(expr.Id, op!, StringT.StringOf(sel.Field), fieldType);

        // Build a qualifier.
        var qual = attrFactory.NewQualifier(opType, expr.Id, sel.Field);

        // Lastly, create a field selection Interpretable.
        if (op is IInterpretableAttribute)
        {
            var attr = (IInterpretableAttribute)op;
            attr.AddQualifier(qual);
            return attr;
        }

        var relAttr = RelativeAttr(op.Id(), op);
        if (relAttr == null) return null;

        relAttr.AddQualifier(qual);
        return relAttr;
    }

    /// <summary>
    ///     planCall creates a callable Interpretable while specializing for common functions and
    ///     invocation patterns. Specifically, conditional operators &&, ||, ?:, and (in)equality
    ///     functions result in optimized Interpretable values.
    /// </summary>
    internal IInterpretable? PlanCall(Expr expr)
    {
        var call = expr.CallExpr;
        var resolvedFunc = ResolveFunction(expr);
        // target, fnName, oName := p.resolveFunction(expr)
        var argCount = call.Args.Count;
        var offset = 0;
        if (resolvedFunc.target != null)
        {
            argCount++;
            offset++;
        }

        var args = new IInterpretable[argCount];
        if (resolvedFunc.target != null)
        {
            var arg = Plan(resolvedFunc.target);
            if (arg == null) return null;

            args[0] = arg;
        }

        for (var i = 0; i < call.Args.Count; i++)
        {
            var argExpr = call.Args[i];
            var arg = Plan(argExpr)!;
            args[i + offset] = arg;
        }

        // Generate specialized Interpretable operators by function name if possible.
        if (resolvedFunc.fnName.Equals(Operator.LogicalAnd.Id)) return PlanCallLogicalAnd(expr, args);

        if (resolvedFunc.fnName.Equals(Operator.LogicalOr.Id)) return PlanCallLogicalOr(expr, args);

        if (resolvedFunc.fnName.Equals(Operator.Conditional.Id)) return PlanCallConditional(expr, args);

        if (resolvedFunc.fnName.Equals(Operator.Equals.Id)) return PlanCallEqual(expr, args);

        if (resolvedFunc.fnName.Equals(Operator.NotEquals.Id)) return PlanCallNotEqual(expr, args);

        if (resolvedFunc.fnName.Equals(Operator.Index.Id)) return PlanCallIndex(expr, args);

        // Otherwise, generate Interpretable calls specialized by argument count.
        // Try to find the specific function by overload id.
        Overload? fnDef = null;
        if (resolvedFunc.overloadId.Length == 0)
            fnDef = disp.FindOverload(resolvedFunc.overloadId);

        // If the overload id couldn't resolve the function, try the simple function name.
        if (fnDef == null) fnDef = disp.FindOverload(resolvedFunc.fnName);

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
    ///     planCallZero generates a zero-arity callable Interpretable.
    /// </summary>
    internal IInterpretable PlanCallZero(Expr expr, string function, string overload, Overload? impl)
    {
        if (impl == null || impl.FunctionOp == null)
            throw new ArgumentException(string.Format("no such overload: {0}()", function));

        return new EvalZeroArity(expr.Id, function, overload, impl.FunctionOp);
    }

    /// <summary>
    ///     planCallUnary generates a unary callable Interpretable.
    /// </summary>
    internal IInterpretable PlanCallUnary(Expr expr, string function, string overload, Overload? impl,
        IInterpretable[] args)
    {
        UnaryOp? fn = null;
        var trait = Trait.None;
        if (impl != null)
        {
            if (impl.UnaryOp == null)
                throw new InvalidOperationException(string.Format("no such overload: {0}(arg)", function));

            fn = impl.UnaryOp;
            trait = impl.OperandTrait;
        }

        return new EvalUnary(expr.Id, function, overload, args[0], trait, fn);
    }

    /// <summary>
    ///     planCallBinary generates a binary callable Interpretable.
    /// </summary>
    internal IInterpretable PlanCallBinary(Expr expr, string function, string overload, Overload? impl,
        params IInterpretable[] args)
    {
        BinaryOp? fn = null;
        var trait = Trait.None;
        if (impl != null)
        {
            if (impl.BinaryOp == null)
                throw new InvalidOperationException(string.Format("no such overload: {0}(lhs, rhs)",
                    function));

            fn = impl.BinaryOp;
            trait = impl.OperandTrait;
        }

        return new EvalBinary(expr.Id, function, overload, args[0], args[1], trait, fn);
    }

    /// <summary>
    ///     planCallVarArgs generates a variable argument callable Interpretable.
    /// </summary>
    internal IInterpretable PlanCallVarArgs(Expr expr, string function, string overload, Overload? impl,
        params IInterpretable[] args)
    {
        FunctionOp? fn = null;
        var trait = Trait.None;
        if (impl != null)
        {
            if (impl.FunctionOp == null)
                throw new InvalidOperationException(string.Format("no such overload: {0}(...)", function));

            fn = impl.FunctionOp;
            trait = impl.OperandTrait;
        }

        return new EvalVarArgs(expr.Id, function, overload, args, trait, fn);
    }

    /// <summary>
    ///     planCallEqual generates an equals (==) Interpretable.
    /// </summary>
    internal IInterpretable PlanCallEqual(Expr expr, params IInterpretable[] args)
    {
        return new EvalEq(expr.Id, args[0], args[1]);
    }

    /// <summary>
    ///     planCallNotEqual generates a not equals (!=) Interpretable.
    /// </summary>
    internal IInterpretable PlanCallNotEqual(Expr expr, params IInterpretable[] args)
    {
        return new EvalNe(expr.Id, args[0], args[1]);
    }

    /// <summary>
    ///     planCallLogicalAnd generates a logical and (&&) Interpretable.
    /// </summary>
    internal IInterpretable PlanCallLogicalAnd(Expr expr, params IInterpretable[] args)
    {
        return new EvalAnd(expr.Id, args[0], args[1]);
    }

    /// <summary>
    ///     planCallLogicalOr generates a logical or (||) Interpretable.
    /// </summary>
    internal IInterpretable PlanCallLogicalOr(Expr expr, params IInterpretable[] args)
    {
        return new EvalOr(expr.Id, args[0], args[1]);
    }

    /// <summary>
    ///     planCallConditional generates a conditional / ternary (c ? t : f) Interpretable.
    /// </summary>
    internal IInterpretable PlanCallConditional(Expr expr, params IInterpretable[] args)
    {
        var cond = args[0];

        var t = args[1];
        IAttribute tAttr;
        if (t is IInterpretableAttribute)
        {
            var truthyAttr = (IInterpretableAttribute)t;
            tAttr = truthyAttr.Attr();
        }
        else
        {
            tAttr = attrFactory.RelativeAttribute(t.Id(), t);
        }

        var f = args[2];
        IAttribute fAttr;
        if (f is IInterpretableAttribute)
        {
            var falsyAttr = (IInterpretableAttribute)f;
            fAttr = falsyAttr.Attr();
        }
        else
        {
            fAttr = attrFactory.RelativeAttribute(f.Id(), f);
        }

        return new EvalAttr(adapter, attrFactory.ConditionalAttribute(expr.Id, cond, tAttr, fAttr));
    }

    /// <summary>
    ///     planCallIndex either extends an attribute with the argument to the index operation, or
    ///     creates a relative attribute based on the return of a function call or operation.
    /// </summary>
    internal IInterpretable? PlanCallIndex(Expr expr, params IInterpretable[] args)
    {
        var op = args[0];
        var ind = args[1];
        var opAttr = RelativeAttr(op.Id(), op);
        if (opAttr == null) return null;

        var target = expr.CallExpr.Target;
        if (target == null) target = new Expr();
        typeMap.TryGetValue(target.Id, out var opType);
        if (ind is IInterpretableConst)
        {
            var indConst = (IInterpretableConst)ind;
            var qual = attrFactory.NewQualifier(opType, expr.Id, indConst.Value());

            opAttr.AddQualifier(qual);
            return opAttr;
        }

        if (ind is IInterpretableAttribute)
        {
            var indAttr = (IInterpretableAttribute)ind;
            var qual = attrFactory.NewQualifier(opType, expr.Id, indAttr);

            opAttr.AddQualifier(qual);
            return opAttr;
        }

        var indQual = RelativeAttr(expr.Id, ind);
        if (indQual == null) return null;

        opAttr.AddQualifier(indQual);
        return opAttr;
    }

    /// <summary>
    ///     planCreateList generates a list construction Interpretable.
    /// </summary>
    internal IInterpretable? PlanCreateList(Expr expr)
    {
        var list = expr.ListExpr;
        var elems = new IInterpretable[list.Elements.Count];
        for (var i = 0; i < list.Elements.Count; i++)
        {
            var elem = list.Elements[i];
            var elemVal = Plan(elem);
            if (elemVal == null) return null;

            elems[i] = elemVal;
        }

        return new EvalList(expr.Id, elems, adapter);
    }

    /// <summary>
    ///     planCreateStruct generates a map or object construction Interpretable.
    /// </summary>
    internal IInterpretable? PlanCreateStruct(Expr expr)
    {
        var str = expr.StructExpr;
        if (str.MessageName.Length > 0) return PlanCreateObj(expr);

        IList<Expr.Types.CreateStruct.Types.Entry> entries = str.Entries;
        var keys = new IInterpretable[entries.Count];
        var vals = new IInterpretable[entries.Count];
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var keyVal = Plan(entry.MapKey);
            if (keyVal == null) return null;

            keys[i] = keyVal;

            var valVal = Plan(entry.Value);
            if (valVal == null) return null;

            vals[i] = valVal;
        }

        return new EvalMap(expr.Id, keys, vals, adapter);
    }

    /// <summary>
    ///     planCreateObj generates an object construction Interpretable.
    /// </summary>
    internal IInterpretable? PlanCreateObj(Expr expr)
    {
        var obj = expr.StructExpr;
        var typeName = ResolveTypeName(obj.MessageName);
        if (typeName == null)
            throw new InvalidOperationException(string.Format("unknown type: {0}", obj.MessageName));

        IList<Expr.Types.CreateStruct.Types.Entry> entries = obj.Entries;
        var fields = new string[entries.Count];
        var vals = new IInterpretable[entries.Count];
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            fields[i] = entry.FieldKey;
            var val = Plan(entry.Value);
            if (val == null) return null;

            vals[i] = val;
        }

        return new EvalObj(expr.Id, typeName, fields, vals, provider);
    }

    /// <summary>
    ///     planComprehension generates an Interpretable fold operation.
    /// </summary>
    internal IInterpretable? PlanComprehension(Expr expr)
    {
        var fold = expr.ComprehensionExpr;
        var accu = Plan(fold.AccuInit);
        if (accu == null) return null;

        var iterRange = Plan(fold.IterRange);
        if (iterRange == null) return null;

        var cond = Plan(fold.LoopCondition);
        if (cond == null) return null;

        var step = Plan(fold.LoopStep);
        if (step == null) return null;

        var result = Plan(fold.Result);
        if (result == null) return null;

        return new EvalFold(expr.Id, fold.AccuVar, accu, fold.IterVar, iterRange, cond, step, result);
    }

    /// <summary>
    ///     planConst generates a constant valued Interpretable.
    /// </summary>
    internal IInterpretable PlanConst(Expr expr)
    {
        var val = ConstValue(expr.ConstExpr);

        return InterpretableUtils.NewConstValue(expr.Id, val);
    }

    /// <summary>
    ///     constValue converts a proto Constant value to a ref.Val.
    /// </summary>
    internal IVal ConstValue(Constant c)
    {
        switch (c.ConstantKindCase)
        {
            case Constant.ConstantKindOneofCase.BoolValue:
                return Types.BoolOf(c.BoolValue);
            case Constant.ConstantKindOneofCase.BytesValue:
                return BytesT.BytesOf(c.BytesValue);
            case Constant.ConstantKindOneofCase.DoubleValue:
                return DoubleT.DoubleOf(c.DoubleValue);
            case Constant.ConstantKindOneofCase.DurationValue:
                return DurationT.DurationOf(c.DurationValue);
            case Constant.ConstantKindOneofCase.Int64Value:
                return IntT.IntOf(c.Int64Value);
            case Constant.ConstantKindOneofCase.NullValue:
                return NullT.NullValue;
            case Constant.ConstantKindOneofCase.StringValue:
                return StringT.StringOf(c.StringValue);
            case Constant.ConstantKindOneofCase.TimestampValue:
                return TimestampT.TimestampOf(c.TimestampValue);
            case Constant.ConstantKindOneofCase.Uint64Value:
                return UintT.UintOf(c.Uint64Value);
        }

        throw new ArgumentException(string.Format("unknown constant type: '{0}' of kind '{1}'", c,
            c.ConstantKindCase));
    }

    /// <summary>
    ///     resolveTypeName takes a qualified string constructed at parse time, applies the proto
    ///     namespace resolution rules to it in a scan over possible matching types in the TypeProvider.
    /// </summary>
    internal string? ResolveTypeName(string typeName)
    {
        foreach (var qualifiedTypeName in container.ResolveCandidateNames(typeName))
            if (provider.FindType(qualifiedTypeName) != null)
                return qualifiedTypeName;

        return null;
    }

    /// <summary>
    ///     resolveFunction determines the call target, function name, and overload name from a given
    ///     Expr value.
    ///     <para>
    ///         The resolveFunction resolves ambiguities where a function may either be a receiver-style
    ///         invocation or a qualified global function name.
    ///         <ul>
    ///             <li>
    ///                 The target expression may only consist of ident and select expressions.
    ///             </li>
    ///             <li>
    ///                 The function is declared in the environment using its fully-qualified name.
    ///             </li>
    ///             <li>
    ///                 The fully-qualified function name matches the string serialized target value.
    ///             </li>
    ///         </ul>
    ///     </para>
    /// </summary>
    internal ResolvedFunction ResolveFunction(Expr expr)
    {
        // Note: similar logic exists within the `checker/checker.go`. If making changes here
        // please consider the impact on checker.go and consolidate implementations or mirror code
        // as appropriate.
        var call = expr.CallExpr;
        var target = call.Target != null ? call.Target : null;
        var fnName = call.Function;

        // Checked expressions always have a reference map entry, and _should_ have the fully
        // qualified
        // function name as the fnName value.
        refMap.TryGetValue(expr.Id, out var oRef);
        if (oRef != null)
        {
            if (oRef.OverloadId.Count == 1) return new ResolvedFunction(target, fnName, oRef.OverloadId[0]);

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
            foreach (var qualifiedName in container.ResolveCandidateNames(fnName))
                if (disp.FindOverload(qualifiedName) != null)
                    return new ResolvedFunction(target, qualifiedName, "");

            // It's possible that the overload was not found, but this situation is accounted for in
            // the planCall phase; however, the leading dot used for denoting fully-qualified
            // namespaced identifiers must be stripped, as all declarations already use fully-qualified
            // names. This stripping behavior is handled automatically by the ResolveCandidateNames
            // call.
            return new ResolvedFunction(target, StripLeadingDot(fnName), "");
        }

        // Handle the situation where the function target actually indicates a qualified function
        // name.
        var qualifiedPrefix = ToQualifiedName(target);
        if (qualifiedPrefix != null)
        {
            var maybeQualifiedName = qualifiedPrefix + "." + fnName;
            foreach (var qualifiedName in container.ResolveCandidateNames(maybeQualifiedName))
                if (disp.FindOverload(qualifiedName) != null)
                    // Clear the target to ensure the proper arity is used for finding the
                    // implementation.
                    return new ResolvedFunction(null, qualifiedName, "");
        }

        // In the default case, the function is exactly as it was advertised: a receiver call on with
        // an expression-based target with the given simple function name.
        return new ResolvedFunction(target, fnName, "");
    }

    internal IInterpretableAttribute? RelativeAttr(long id, IInterpretable eval)
    {
        IInterpretableAttribute eAttr;
        if (eval is IInterpretableAttribute)
            eAttr = (IInterpretableAttribute)eval;
        else
            eAttr = new EvalAttr(adapter, attrFactory.RelativeAttribute(id, eval));

        var decAttr = Decorate(eAttr);
        if (decAttr == null) return null;

        if (!(decAttr is IInterpretableAttribute))
            throw new InvalidOperationException(string.Format("invalid attribute decoration: {0}({1})",
                decAttr, decAttr.GetType().FullName));

        eAttr = (IInterpretableAttribute)decAttr;
        return eAttr;
    }

    /// <summary>
    ///     toQualifiedName converts an expression AST into a qualified name if possible, with a boolean
    ///     'found' value that indicates if the conversion is successful.
    /// </summary>
    internal string? ToQualifiedName(Expr operand)
    {
        // If the checker identified the expression as an attribute by the type-checker, then it can't
        // possibly be part of qualified name in a namespace.
        if (refMap.ContainsKey(operand.Id)) return "";

        // Since functions cannot be both namespaced and receiver functions, if the operand is not an
        // qualified variable name, return the (possibly) qualified name given the expressions.
        return Container.ToQualifiedName(operand);
    }

    internal string StripLeadingDot(string name)
    {
        return name.StartsWith(".", StringComparison.Ordinal) ? name.Substring(1) : name;
    }

    internal sealed class ResolvedFunction
    {
        internal readonly string fnName;
        internal readonly string overloadId;
        internal readonly Expr? target;

        internal ResolvedFunction(Expr? target, string fnName, string overloadId)
        {
            this.target = target;
            this.fnName = fnName;
            this.overloadId = overloadId;
        }
    }
}