using Cel.Common;
using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
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
namespace Cel.Checker;

using ConstantKindCase = Constant.ConstantKindOneofCase;

public sealed class Checker
{
    public static readonly IList<Decl> StandardDeclarations = Standard.MakeStandardDeclarations();
    private readonly TypeErrors errors;
    private readonly IDictionary<long, Reference> references = new Dictionary<long, Reference>();
    private readonly SourceInfo sourceInfo;
    private readonly IDictionary<long, Type> types = new Dictionary<long, Type>();

    private CheckerEnv env;
    private int freeTypeVarCounter;
    private Mapping mappings;

    private Checker(CheckerEnv env, TypeErrors errors, Mapping mappings, int freeTypeVarCounter,
        SourceInfo sourceInfo)
    {
        this.env = env;
        this.errors = errors;
        this.mappings = mappings;
        this.freeTypeVarCounter = freeTypeVarCounter;
        this.sourceInfo = sourceInfo;
    }

    /// <summary>
    ///     Check performs type checking, giving a typed AST. The input is a ParsedExpr proto and an env
    ///     which encapsulates type binding of variables, declarations of built-in functions, descriptions
    ///     of protocol buffers, and a registry for errors. Returns a CheckedExpr proto, which might not be
    ///     usable if there are errors in the error registry.
    /// </summary>
    public static CheckResult Check(Parser.Parser.ParseResult parsedExpr, ISource source, CheckerEnv env)
    {
        var errors = new TypeErrors(source);
        var c = new Checker(env, errors, Mapping.NewMapping(), 0, parsedExpr.SourceInfo);

        var e = parsedExpr.Expr;
        c.Check(e!);

        // Walk over the final type map substituting any type parameters either by their bound value or
        // by DYN.
        IDictionary<long, Type> m = new Dictionary<long, Type>();
        foreach (var entry in c.types) m.Add(entry.Key, Types.Substitute(c.mappings, entry.Value, true));

        var checkedExpr = new CheckedExpr();
        checkedExpr.Expr = e;
        checkedExpr.SourceInfo = parsedExpr.SourceInfo;
        checkedExpr.TypeMap.Add(m);
        checkedExpr.ReferenceMap.Add(c.references);

        return new CheckResult(checkedExpr, errors);
    }

    internal void Check(Expr e)
    {
        switch (e.ExprKindCase)
        {
            case Expr.ExprKindOneofCase.ConstExpr:
                var literal = e.ConstExpr;
                switch (literal.ConstantKindCase)
                {
                    case ConstantKindCase.BoolValue:
                        CheckBoolLiteral(e);
                        return;
                    case ConstantKindCase.BytesValue:
                        CheckBytesLiteral(e);
                        return;
                    case ConstantKindCase.DoubleValue:
                        CheckDoubleLiteral(e);
                        return;
                    case ConstantKindCase.Int64Value:
                        CheckInt64Literal(e);
                        return;
                    case ConstantKindCase.NullValue:
                        CheckNullLiteral(e);
                        return;
                    case ConstantKindCase.StringValue:
                        CheckStringLiteral(e);
                        return;
                    case ConstantKindCase.Uint64Value:
                        CheckUint64Literal(e);
                        return;
                }

                throw new ArgumentException(
                    string.Format("Unrecognized ast type: {0}", e.GetType().FullName));
            case Expr.ExprKindOneofCase.IdentExpr:
                CheckIdent(e);
                return;
            case Expr.ExprKindOneofCase.SelectExpr:
                CheckSelect(e);
                return;
            case Expr.ExprKindOneofCase.CallExpr:
                CheckCall(e);
                return;
            case Expr.ExprKindOneofCase.ListExpr:
                CheckCreateList(e);
                return;
            case Expr.ExprKindOneofCase.StructExpr:
                CheckCreateStruct(e);
                return;
            case Expr.ExprKindOneofCase.ComprehensionExpr:
                CheckComprehension(e);
                return;
            default:
                throw new ArgumentException(
                    string.Format("Unrecognized ast type: {0}", e.GetType().FullName));
        }
    }

    internal void CheckInt64Literal(Expr e)
    {
        SetType(e, Decls.Int);
    }

    internal void CheckUint64Literal(Expr e)
    {
        SetType(e, Decls.Uint);
    }

    internal void CheckStringLiteral(Expr e)
    {
        SetType(e, Decls.String);
    }

    internal void CheckBytesLiteral(Expr e)
    {
        SetType(e, Decls.Bytes);
    }

    internal void CheckDoubleLiteral(Expr e)
    {
        SetType(e, Decls.Double);
    }

    internal void CheckBoolLiteral(Expr e)
    {
        SetType(e, Decls.Bool);
    }

    internal void CheckNullLiteral(Expr e)
    {
        SetType(e, Decls.Null);
    }

    internal void CheckIdent(Expr e)
    {
        var identExpr = e.IdentExpr;
        if (identExpr == null)
        {
            identExpr = new Expr.Types.Ident();
            e.IdentExpr = identExpr;
        }

        // Check to see if the identifier is declared.
        var ident = env.LookupIdent(identExpr.Name);
        if (ident != null)
        {
            SetType(e, ident.Ident.Type);
            SetReference(e, NewIdentReference(ident.Name, ident.Ident.Value));
            // Overwrite the identifier with its fully qualified name.
            identExpr.Name = ident.Name;
            return;
        }

        SetType(e, Decls.Error);
        errors.UndeclaredReference(LocationByExpr(e), env.container.Name(), identExpr.Name);
    }

    internal void CheckSelect(Expr e)
    {
        var sel = e.SelectExpr;
        if (sel == null)
        {
            sel = new Expr.Types.Select();
            e.SelectExpr = sel;
        }

        // Before traversing down the tree, try to interpret as qualified name.
        var qname = Container.ToQualifiedName(e);
        if (qname != null)
        {
            var ident = env.LookupIdent(qname);
            if (ident != null)
            {
                if (sel.TestOnly)
                {
                    errors.ExpressionDoesNotSelectField(LocationByExpr(e));
                    SetType(e, Decls.Bool);
                    return;
                }

                // Rewrite the node to be a variable reference to the resolved fully-qualified
                // variable name.
                SetType(e, ident.Ident.Type);
                SetReference(e, NewIdentReference(ident.Name, ident.Ident.Value));
                var identName = ident.Name;
                var identExpr = e.IdentExpr;
                if (identExpr == null)
                {
                    identExpr = new Expr.Types.Ident();
                    e.IdentExpr = identExpr;
                }

                identExpr.Name = identName;
                return;
            }
        }

        // Interpret as field selection, first traversing down the operand.
        if (sel.Operand == null) sel.Operand = new Expr();
        Check(sel.Operand);

        var targetType = GetType(sel.Operand)!;
        // Assume error type by default as most types do not support field selection.
        var resultType = Decls.Error;
        switch (Types.KindOf(targetType))
        {
            case Types.Kind.KindMap:
                // Maps yield their value type as the selection result type.
                var mapType = targetType.MapType;
                resultType = mapType.ValueType;
                break;
            case Types.Kind.KindObject:
                // Objects yield their field type declaration as the selection result type, but only if
                // the field is defined.
                var fieldType = LookupFieldType(LocationByExpr(e), targetType.MessageType, sel.Field);
                if (fieldType != null) resultType = fieldType.Type;

                break;
            case Types.Kind.KindTypeParam:
                // Set the operand type to DYN to prevent assignment to a potentionally incorrect type
                // at a later point in type-checking. The isAssignable call will update the type
                // substitutions for the type param under the covers.
                IsAssignable(Decls.Dyn, targetType);
                // Also, set the result type to DYN.
                resultType = Decls.Dyn;
                break;
            default:
                // Dynamic / error values are treated as DYN type. Errors are handled this way as well
                // in order to allow forward progress on the check.
                if (Types.IsDynOrError(targetType))
                    resultType = Decls.Dyn;
                else
                    errors.TypeDoesNotSupportFieldSelection(LocationByExpr(e), targetType);

                break;
        }

        if (sel.TestOnly) resultType = Decls.Bool;

        SetType(e, resultType);
    }

    internal void CheckCall(Expr e)
    {
        // Note: similar logic exists within the `interpreter/planner.go`. If making changes here
        // please consider the impact on planner.go and consolidate implementations or mirror code
        // as appropriate.
        var call = e.CallExpr;
        if (call == null)
        {
            call = new Expr.Types.Call();
            e.CallExpr = call;
        }

        IList<Expr> args = call.Args;
        var fnName = call.Function;

        // Traverse arguments.
        foreach (var arg in args) Check(arg);

        // Regular static call with simple name.
        Decl? fn;
        if (call.Target == null || Equals(call.Target, new Expr()))
        {
            // Check for the existence of the function.
            fn = env.LookupFunction(fnName);
            if (fn == null)
            {
                errors.UndeclaredReference(LocationByExpr(e), env.container.Name(), fnName);
                SetType(e, Decls.Error);
                return;
            }

            // Overwrite the function name with its fully qualified resolved name.
            call.Function = fn.Name;
            // Check to see whether the overload resolves.
            ResolveOverloadOrError(LocationByExpr(e), e, fn, null, args);
            return;
        }

        // If a receiver 'target' is present, it may either be a receiver function, or a namespaced
        // function, but not both. Given a.b.c() either a.b.c is a function or c is a function with
        // target a.b.
        //
        // Check whether the target is a namespaced function name.
        var target = call.Target;
        if (target == null)
        {
            target = new Expr();
            call.Target = target;
        }

        var qualifiedPrefix = Container.ToQualifiedName(target);
        if (qualifiedPrefix != null)
        {
            var maybeQualifiedName = qualifiedPrefix + "." + fnName;
            fn = env.LookupFunction(maybeQualifiedName);
            if (fn != null)
            {
                // The function name is namespaced and so preserving the target operand would
                // be an inaccurate representation of the desired evaluation behavior.
                // Overwrite with fully-qualified resolved function name sans receiver target.
                call.Target = null;
                call.Function = fn.Name;
                ResolveOverloadOrError(LocationByExpr(e), e, fn, null, args);
                return;
            }
        }

        // Regular instance call.
        Check(target);
        // Overwrite with fully-qualified resolved function name sans receiver target.
        fn = env.LookupFunction(fnName);
        // Function found, attempt overload resolution.
        if (fn != null)
        {
            ResolveOverloadOrError(LocationByExpr(e), e, fn, target, args);
            return;
        }

        // Function name not declared, record error.
        errors.UndeclaredReference(LocationByExpr(e), env.container.Name(), fnName);
    }

    internal void ResolveOverloadOrError(ILocation loc, Expr e, Decl fn, Expr? target, IList<Expr> args)
    {
        // Attempt to resolve the overload.
        var resolution = ResolveOverload(loc, fn, target, args);
        // No such overload, error noted in the resolveOverload call, type recorded here.
        if (resolution == null)
        {
            SetType(e, Decls.Error);
            return;
        }

        // Overload found.
        SetType(e, resolution.type);
        SetReference(e, resolution.reference);
    }

    internal OverloadResolution? ResolveOverload(ILocation loc, Decl fn, Expr? target, IList<Expr> args)
    {
        IList<Type> argTypes = new List<Type>();
        if (target != null)
        {
            var argType = GetType(target);
            if (argType == null) throw new Err.ErrException("Could not resolve type for target '{0}'", target);

            argTypes.Add(argType);
        }

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            var argType = GetType(arg);
            if (argType == null) throw new Err.ErrException("Could not resolve type for argument %d '{0}'", i, arg);

            argTypes.Add(argType);
        }

        Type? resultType = null;
        Reference? checkedRef = null;
        foreach (var overload in fn.Function.Overloads)
        {
            if ((target == null && overload.IsInstanceFunction) || (target != null && !overload.IsInstanceFunction))
                // not a compatible call style.
                continue;

            var overloadType = Decls.NewFunctionType(overload.ResultType, overload.Params);
            if (overload.TypeParams.Count > 0)
            {
                // Instantiate overload's type with fresh type variables.
                var substitutions = Mapping.NewMapping();
                foreach (var typePar in overload.TypeParams)
                    substitutions.Add(Decls.NewTypeParamType(typePar), NewTypeVar());

                overloadType = Types.Substitute(substitutions, overloadType, false);
            }

            IList<Type> candidateArgTypes = overloadType.Function.ArgTypes;
            if (IsAssignableList(argTypes, candidateArgTypes))
            {
                if (checkedRef == null)
                {
                    checkedRef = NewFunctionReference(new List<string> { overload.OverloadId });
                }
                else
                {
                    var reference = new Reference();
                    reference.MergeFrom(checkedRef.ToByteArray());
                    reference.OverloadId.Add(overload.OverloadId);
                    checkedRef = reference;
                }

                // First matching overload, determines result type.
                var fnResultType = Types.Substitute(mappings, overloadType.Function.ResultType, false);
                if (resultType == null)
                    resultType = fnResultType;
                else if (!Types.IsDyn(resultType) && !fnResultType.Equals(resultType)) resultType = Decls.Dyn;
            }
        }

        if (resultType == null)
        {
            errors.NoMatchingOverload(loc, fn.Name, argTypes, target != null);
            // resultType = Decls.Error;
            return null;
        }

        return NewResolution(checkedRef!, resultType);
    }

    internal void CheckCreateList(Expr e)
    {
        var create = e.ListExpr;
        if (create == null)
        {
            create = new Expr.Types.CreateList();
            e.ListExpr = create;
        }

        Type? elemType = null;
        for (var i = 0; i < create.Elements.Count; i++)
        {
            var el = create.Elements[i];
            Check(el);
            elemType = JoinTypes(LocationByExpr(el), elemType, GetType(el));
        }

        if (elemType == null)
            // If the list is empty, assign free type var to elem type.
            elemType = NewTypeVar();

        SetType(e, Decls.NewListType(elemType));
    }

    internal void CheckCreateStruct(Expr e)
    {
        var str = e.StructExpr;
        if (str == null)
        {
            str = new Expr.Types.CreateStruct();
            e.StructExpr = str;
        }

        if (str.MessageName.Length > 0)
            CheckCreateMessage(e);
        else
            CheckCreateMap(e);
    }

    internal void CheckCreateMap(Expr e)
    {
        var mapVal = e.StructExpr;
        if (mapVal == null)
        {
            mapVal = new Expr.Types.CreateStruct();
            e.StructExpr = mapVal;
        }

        Type? keyType = null;
        Type? valueType = null;
        foreach (var ent in mapVal.Entries)
        {
            var key = ent.MapKey;
            if (key == null)
            {
                key = new Expr();
                ent.MapKey = key;
            }

            Check(key);
            keyType = JoinTypes(LocationByExpr(key), keyType, GetType(key));

            var val = ent.Value;
            if (val == null)
            {
                val = new Expr();
                ent.Value = val;
            }

            Check(val);
            valueType = JoinTypes(LocationByExpr(val), valueType, GetType(val));
        }

        if (keyType == null)
        {
            // If the map is empty, assign free type variables to typeKey and value type.
            keyType = NewTypeVar();
            valueType = NewTypeVar();
        }

        SetType(e, Decls.NewMapType(keyType, valueType));
    }

    internal void CheckCreateMessage(Expr e)
    {
        var msgVal = e.StructExpr;
        if (msgVal == null)
        {
            msgVal = new Expr.Types.CreateStruct();
            e.StructExpr = msgVal;
        }

        // Determine the type of the message.
        var messageType = Decls.Error;
        var decl = env.LookupIdent(msgVal.MessageName);
        if (decl == null)
        {
            errors.UndeclaredReference(LocationByExpr(e), env.container.Name(), msgVal.MessageName);
            return;
        }

        // Ensure the type name is fully qualified in the AST.
        msgVal.MessageName = decl.Name;
        SetReference(e, NewIdentReference(decl.Name, null));
        var ident = decl.Ident;
        var identKind = Types.KindOf(ident.Type);
        if (identKind != Types.Kind.KindError)
        {
            if (identKind != Types.Kind.KindType)
            {
                errors.NotAType(LocationByExpr(e), ident.Type);
            }
            else
            {
                messageType = ident.Type.Type_;
                if (Types.KindOf(messageType) != Types.Kind.KindObject)
                {
                    errors.NotAMessageType(LocationByExpr(e), messageType);
                    messageType = Decls.Error;
                }
            }
        }

        if (CheckerEnv.IsObjectWellKnownType(messageType))
            SetType(e, CheckerEnv.GetObjectWellKnownType(messageType)!);
        else
            SetType(e, messageType);

        // Check the field initializers.
        foreach (var ent in msgVal.Entries)
        {
            var field = ent.FieldKey;
            var value = ent.Value;
            if (value == null)
            {
                value = new Expr();
                ent.Value = value;
            }

            Check(value);

            var fieldType = Decls.Error;
            var t = LookupFieldType(LocationById(ent.Id), messageType.MessageType, field);
            if (t != null) fieldType = t.Type;

            if (!IsAssignable(fieldType, GetType(value)!))
                errors.FieldTypeMismatch(LocationById(ent.Id), field, fieldType, GetType(value));
        }
    }

    internal void CheckComprehension(Expr e)
    {
        var comp = e.ComprehensionExpr;
        if (comp == null)
        {
            comp = new Expr.Types.Comprehension();
            e.ComprehensionExpr = comp;
        }

        if (comp.IterRange == null) comp.IterRange = new Expr();
        if (comp.AccuInit == null) comp.AccuInit = new Expr();
        Check(comp.IterRange);
        Check(comp.AccuInit);
        var accuType = GetType(comp.AccuInit)!;
        var rangeType = GetType(comp.IterRange)!;
        Type varType;

        switch (Types.KindOf(rangeType))
        {
            case Types.Kind.KindList:
                varType = rangeType.ListType.ElemType;
                break;
            case Types.Kind.KindMap:
                // Ranges over the keys.
                varType = rangeType.MapType.KeyType;
                break;
            case Types.Kind.KindDyn:
            case Types.Kind.KindError:
            case Types.Kind.KindTypeParam:
                // Set the range type to DYN to prevent assignment to a potentionally incorrect type
                // at a later point in type-checking. The isAssignable call will update the type
                // substitutions for the type param under the covers.
                IsAssignable(Decls.Dyn, rangeType);
                // Set the range iteration variable to type DYN as well.
                varType = Decls.Dyn;
                break;
            default:
                errors.NotAComprehensionRange(LocationByExpr(comp.IterRange), rangeType);
                varType = Decls.Error;
                break;
        }

        // Create a scope for the comprehension since it has a local accumulation variable.
        // This scope will contain the accumulation variable used to compute the result.
        env = env.EnterScope();
        env.Add(Decls.NewVar(comp.AccuVar, accuType));
        // Create a block scope for the loop.
        env = env.EnterScope();
        env.Add(Decls.NewVar(comp.IterVar, varType));
        // Check the variable references in the condition and step.
        if (comp.LoopCondition == null) comp.LoopCondition = new Expr();
        if (comp.LoopStep == null) comp.LoopStep = new Expr();
        Check(comp.LoopCondition);
        AssertType(comp.LoopCondition, Decls.Bool);
        Check(comp.LoopStep);
        AssertType(comp.LoopStep, accuType);
        // Exit the loop's block scope before checking the result.
        env = env.ExitScope();
        if (comp.Result == null) comp.Result = new Expr();
        Check(comp.Result);
        // Exit the comprehension scope.
        env = env.ExitScope();
        SetType(e, GetType(comp.Result)!);
    }

    /// <summary>
    ///     Checks compatibility of joined types, and returns the most general common type.
    /// </summary>
    internal Type? JoinTypes(ILocation loc, Type? previous, Type? current)
    {
        if (previous == null) return current;

        if (IsAssignable(previous, current!)) return Types.MostGeneral(previous, current!);

        if (DynAggregateLiteralElementTypesEnabled()) return Decls.Dyn;

        errors.TypeMismatch(loc, previous, current);
        return Decls.Error;
    }

    internal bool DynAggregateLiteralElementTypesEnabled()
    {
        return env.aggLitElemType == CheckerEnv.DynElementType;
    }

    internal Type NewTypeVar()
    {
        var id = freeTypeVarCounter;
        freeTypeVarCounter++;
        return Decls.NewTypeParamType(string.Format("_var{0:D}", id));
    }

    internal bool IsAssignable(Type t1, Type t2)
    {
        var subs = Types.IsAssignable(mappings, t1, t2);
        if (subs != null)
        {
            mappings = subs;
            return true;
        }

        return false;
    }

    internal bool IsAssignableList(IList<Type> l1, IList<Type> l2)
    {
        var subs = Types.IsAssignableList(mappings, l1, l2);
        if (subs != null)
        {
            mappings = subs;
            return true;
        }

        return false;
    }

    internal FieldType? LookupFieldType(ILocation l, string messageType, string fieldName)
    {
        if (env.provider.FindType(messageType) == null)
        {
            // This should not happen, anyway, report an error.
            errors.UnexpectedFailedResolution(l, messageType);
            return null;
        }

        var ft = env.provider.FindFieldType(messageType, fieldName);
        if (ft != null) return ft;

        errors.UndefinedField(l, fieldName);
        return null;
    }

    internal void SetType(Expr e, Type t)
    {
        types.TryGetValue(e.Id, out var old);
        if (old != null && !old.Equals(t))
            throw new InvalidOperationException(string.Format(
                "(Incompatible) Type already exists for expression: {0}({1:D}) old:{2}, new:{3}", e, e.Id, old, t));

        types[e.Id] = t;
    }

    internal Type? GetType(Expr e)
    {
        types.TryGetValue(e.Id, out var type);
        return type;
    }

    internal void SetReference(Expr e, Reference r)
    {
        references.TryGetValue(e.Id, out var old);
        if (old != null && !old.Equals(r))
            throw new InvalidOperationException(string.Format(
                "Reference already exists for expression: {0}({1:D}) old:{2}, new:{3}", e, e.Id, old, r));

        references[e.Id] = r;
    }

    internal void AssertType(Expr e, Type t)
    {
        if (!IsAssignable(t, GetType(e)!)) errors.TypeMismatch(LocationByExpr(e), t, GetType(e));
    }

    internal static OverloadResolution NewResolution(Reference checkedRef, Type t)
    {
        return new OverloadResolution(checkedRef, t);
    }

    internal ILocation LocationByExpr(Expr e)
    {
        return LocationById(e.Id);
    }

    internal ILocation LocationById(long id)
    {
        IDictionary<long, int> positions = sourceInfo.Positions;
        var line = 1;
        var offset = -1;
        positions.TryGetValue(id, out offset);
        if (offset >= 0)
        {
            var col = offset;
            foreach (int? lineOffset in sourceInfo.LineOffsets)
                if (lineOffset < offset)
                {
                    line++;
                    col = offset - lineOffset.Value;
                }
                else
                {
                    break;
                }

            return ILocation.NewLocation(line, col);
        }

        return ILocation.NoLocation;
    }

    internal static Reference NewIdentReference(string name, Constant? value)
    {
        var refBuilder = new Reference();
        refBuilder.Name = name;
        if (value != null && value.ConstantKindCase != ConstantKindCase.None) refBuilder.Value = value;

        return refBuilder;
    }

    internal static Reference NewFunctionReference(IList<string> overloads)
    {
        var reference = new Reference();
        reference.OverloadId.Add(overloads);
        return reference;
    }

    public sealed class CheckResult
    {
        internal CheckResult(CheckedExpr expr, TypeErrors errors)
        {
            this.CheckedExpr = expr;
            this.Errors = errors;
        }

        public CheckedExpr CheckedExpr { get; }

        public TypeErrors Errors { get; }

        public bool HasErrors()
        {
            return Errors.HasErrors();
        }

        public override string ToString()
        {
            return "CheckResult{" + "expr=" + CheckedExpr + ", errors=" + Errors + '}';
        }
    }

    internal sealed class OverloadResolution
    {
        internal readonly Reference reference;
        internal readonly Type type;

        public OverloadResolution(Reference reference, Type type)
        {
            this.reference = reference;
            this.type = type;
        }
    }
}