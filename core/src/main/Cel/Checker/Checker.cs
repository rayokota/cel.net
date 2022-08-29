using System.Collections.Generic;
using Google.Protobuf;

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
namespace Cel.Checker
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.CheckerEnv.dynElementType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.CheckerEnv.getObjectWellKnownType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.CheckerEnv.isObjectWellKnownType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Mapping.newMapping;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.isDyn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.isDynOrError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.kindOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.mostGeneral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.substitute;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Location.NoLocation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Location.newLocation;

    using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
    using Constant = Google.Api.Expr.V1Alpha1.Constant;
    using ConstantKindCase = Google.Api.Expr.V1Alpha1.Constant.ConstantKindOneofCase;
    using Decl = Google.Api.Expr.V1Alpha1.Decl;
    using Overload = Google.Api.Expr.V1Alpha1.Decl.Types.FunctionDecl.Types.Overload;
    using IdentDecl = Google.Api.Expr.V1Alpha1.Decl.Types.IdentDecl;
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
    using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Types.Comprehension;
    using CreateList = Google.Api.Expr.V1Alpha1.Expr.Types.CreateList;
    using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct;
    using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
    using Ident = Google.Api.Expr.V1Alpha1.Expr.Types.Ident;
    using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
    using Reference = Google.Api.Expr.V1Alpha1.Reference;
    using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
    using Type = Google.Api.Expr.V1Alpha1.Type;
    using MapType = Google.Api.Expr.V1Alpha1.Type.Types.MapType;
    using Kind = global::Cel.Checker.Types.Kind;
    using Location = global::Cel.Common.Location;
    using Source = global::Cel.Common.Source;
    using Container = global::Cel.Common.Containers.Container;
    using ErrException = global::Cel.Common.Types.Err.ErrException;
    using FieldType = global::Cel.Common.Types.Ref.FieldType;
    using ParseResult = global::Cel.Parser.Parser.ParseResult;

    public sealed class Checker
    {
        public static readonly IList<Decl> StandardDeclarations = Standard.MakeStandardDeclarations();

        private CheckerEnv env;
        private readonly TypeErrors errors;
        private Mapping mappings;
        private int freeTypeVarCounter;
        private readonly SourceInfo sourceInfo;
        private readonly IDictionary<long, Type> types = new Dictionary<long, Type>();
        private readonly IDictionary<long, Reference> references = new Dictionary<long, Reference>();

        private Checker(CheckerEnv env, TypeErrors errors, Mapping mappings, int freeTypeVarCounter,
            SourceInfo sourceInfo)
        {
            this.env = env;
            this.errors = errors;
            this.mappings = mappings;
            this.freeTypeVarCounter = freeTypeVarCounter;
            this.sourceInfo = sourceInfo;
        }

        public sealed class CheckResult
        {
            internal readonly CheckedExpr expr;
            internal readonly TypeErrors errors;

            internal CheckResult(CheckedExpr expr, TypeErrors errors)
            {
                this.expr = expr;
                this.errors = errors;
            }

            public CheckedExpr CheckedExpr
            {
                get { return expr; }
            }

            public TypeErrors Errors
            {
                get { return errors; }
            }

            public bool HasErrors()
            {
                return errors.HasErrors();
            }

            public override string ToString()
            {
                return "CheckResult{" + "expr=" + expr + ", errors=" + errors + '}';
            }
        }

        /// <summary>
        /// Check performs type checking, giving a typed AST. The input is a ParsedExpr proto and an env
        /// which encapsulates type binding of variables, declarations of built-in functions, descriptions
        /// of protocol buffers, and a registry for errors. Returns a CheckedExpr proto, which might not be
        /// usable if there are errors in the error registry.
        /// </summary>
        public static CheckResult Check(ParseResult parsedExpr, Source source, CheckerEnv env)
        {
            TypeErrors errors = new TypeErrors(source);
            Checker c = new Checker(env, errors, Mapping.NewMapping(), 0, parsedExpr.SourceInfo);

            Expr e = new Expr();
            MessageExtensions.MergeFrom(e, parsedExpr.Expr.ToByteArray());
            c.Check(e);

            // Walk over the final type map substituting any type parameters either by their bound value or
            // by DYN.
            IDictionary<long, Type> m = new Dictionary<long, Type>();
            foreach (var entry in m)
            {
                m.Add(entry.Key, Types.Substitute(c.mappings, entry.Value, true));
            }

            CheckedExpr checkedExpr = new CheckedExpr();
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
                    Constant literal = e.ConstExpr;
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

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                    throw new System.ArgumentException(
                        String.Format("Unrecognized ast type: {0}", e.GetType().FullName));
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
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                    throw new System.ArgumentException(
                        String.Format("Unrecognized ast type: {0}", e.GetType().FullName));
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
            Ident identExpr = e.IdentExpr;
            // Check to see if the identifier is declared.
            Decl ident = env.LookupIdent(identExpr.Name);
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
            Select sel = e.SelectExpr;
            // Before traversing down the tree, try to interpret as qualified name.
            string qname = Container.ToQualifiedName(e);
            if (!string.ReferenceEquals(qname, null))
            {
                Decl ident = env.LookupIdent(qname);
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
                    string identName = ident.Name;
                    e.IdentExpr.Name = identName;
                    return;
                }
            }

            // Interpret as field selection, first traversing down the operand.
            Check(sel.Operand);

            Type targetType = GetType(sel.Operand);
            // Assume error type by default as most types do not support field selection.
            Type resultType = Decls.Error;
            switch (Types.KindOf(targetType))
            {
                case Kind.kindMap:
                    // Maps yield their value type as the selection result type.
                    Type.Types.MapType mapType = targetType.MapType;
                    resultType = mapType.ValueType;
                    break;
                case Kind.kindObject:
                    // Objects yield their field type declaration as the selection result type, but only if
                    // the field is defined.
                    FieldType fieldType = LookupFieldType(LocationByExpr(e), targetType.MessageType, sel.Field);
                    if (fieldType != null)
                    {
                        resultType = fieldType.type;
                    }

                    break;
                case Kind.kindTypeParam:
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
                    {
                        resultType = Decls.Dyn;
                    }
                    else
                    {
                        errors.TypeDoesNotSupportFieldSelection(LocationByExpr(e), targetType);
                    }

                    break;
            }

            if (sel.TestOnly)
            {
                resultType = Decls.Bool;
            }

            SetType(e, resultType);
        }

        internal void CheckCall(Expr e)
        {
            // Note: similar logic exists within the `interpreter/planner.go`. If making changes here
            // please consider the impact on planner.go and consolidate implementations or mirror code
            // as appropriate.
            Call call = e.CallExpr;
            IList<Expr> args = call.Args;
            string fnName = call.Function;

            // Traverse arguments.
            foreach (Expr arg in args)
            {
                Check(arg);
            }

            // Regular static call with simple name.
            Decl fn;
            if (Object.Equals(call.Target, new Expr()))
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
            Expr target = call.Target;
            string qualifiedPrefix = Container.ToQualifiedName(target);
            if (!string.ReferenceEquals(qualifiedPrefix, null))
            {
                string maybeQualifiedName = qualifiedPrefix + "." + fnName;
                fn = env.LookupFunction(maybeQualifiedName);
                if (fn != null)
                {
                    // The function name is namespaced and so preserving the target operand would
                    // be an inaccurate representation of the desired evaluation behavior.
                    // Overwrite with fully-qualified resolved function name sans receiver target.
                    call.Target = new Expr();
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

        internal void ResolveOverloadOrError(Location loc, Expr e, Decl fn, Expr target, IList<Expr> args)
        {
            // Attempt to resolve the overload.
            OverloadResolution resolution = ResolveOverload(loc, fn, target, args);
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

        internal OverloadResolution ResolveOverload(Location loc, Decl fn, Expr target, IList<Expr> args)
        {
            IList<Type> argTypes = new List<Type>();
            if (target != null)
            {
                Type argType = GetType(target);
                if (argType == null)
                {
                    throw new ErrException("Could not resolve type for target '{0}'", target);
                }

                argTypes.Add(argType);
            }

            for (int i = 0; i < args.Count; i++)
            {
                Expr arg = args[i];
                Type argType = GetType(arg);
                if (argType == null)
                {
                    throw new ErrException("Could not resolve type for argument %d '{0}'", i, arg);
                }

                argTypes.Add(argType);
            }

            Type resultType = null;
            Reference checkedRef = null;
            foreach (Decl.Types.FunctionDecl.Types.Overload overload in fn.Function.Overloads)
            {
                if ((target == null && overload.IsInstanceFunction) || (target != null && !overload.IsInstanceFunction))
                {
                    // not a compatible call style.
                    continue;
                }

                Type overloadType = Decls.NewFunctionType(overload.ResultType, overload.Params);
                if (overload.TypeParams.Count > 0)
                {
                    // Instantiate overload's type with fresh type variables.
                    Mapping substitutions = Mapping.NewMapping();
                    foreach (string typePar in overload.TypeParams)
                    {
                        substitutions.Add(Decls.NewTypeParamType(typePar), NewTypeVar());
                    }

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
                        Reference reference = new Reference();
                        MessageExtensions.MergeFrom(reference, checkedRef.ToByteArray());
                        reference.OverloadId.Add(overload.OverloadId);
                        checkedRef = reference;
                    }

                    // First matching overload, determines result type.
                    Type fnResultType = Types.Substitute(mappings, overloadType.Function.ResultType, false);
                    if (resultType == null)
                    {
                        resultType = fnResultType;
                    }
                    else if (!Types.IsDyn(resultType) && !fnResultType.Equals(resultType))
                    {
                        resultType = Decls.Dyn;
                    }
                }
            }

            if (resultType == null)
            {
                errors.NoMatchingOverload(loc, fn.Name, argTypes, target != null);
                // resultType = Decls.Error;
                return null;
            }

            return NewResolution(checkedRef, resultType);
        }

        internal void CheckCreateList(Expr e)
        {
            CreateList create = e.ListExpr;
            Type elemType = null;
            for (int i = 0; i < create.Elements.Count; i++)
            {
                Expr el = create.Elements[i];
                Check(el);
                elemType = JoinTypes(LocationByExpr(el), elemType, GetType(el));
            }

            if (elemType == null)
            {
                // If the list is empty, assign free type var to elem type.
                elemType = NewTypeVar();
            }

            SetType(e, Decls.NewListType(elemType));
        }

        internal void CheckCreateStruct(Expr e)
        {
            CreateStruct str = e.StructExpr;
            if (str.MessageName.Length == 0)
            {
                CheckCreateMessage(e);
            }
            else
            {
                CheckCreateMap(e);
            }
        }

        internal void CheckCreateMap(Expr e)
        {
            CreateStruct mapVal = e.StructExpr;
            Type keyType = null;
            Type valueType = null;
            foreach (CreateStruct.Types.Entry ent in mapVal.Entries)
            {
                Expr key = ent.MapKey;
                Check(key);
                keyType = JoinTypes(LocationByExpr(key), keyType, GetType(key));

                Expr val = ent.Value;
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
            CreateStruct msgVal = e.StructExpr;
            // Determine the type of the message.
            Type messageType = Decls.Error;
            Decl decl = env.LookupIdent(msgVal.MessageName);
            if (decl == null)
            {
                errors.UndeclaredReference(LocationByExpr(e), env.container.Name(), msgVal.MessageName);
                return;
            }

            // Ensure the type name is fully qualified in the AST.
            msgVal.MessageName = decl.Name;
            SetReference(e, NewIdentReference(decl.Name, null));
            Decl.Types.IdentDecl ident = decl.Ident;
            Types.Kind identKind = Types.KindOf(ident.Type);
            if (identKind != Kind.kindError)
            {
                if (identKind != Kind.kindType)
                {
                    errors.NotAType(LocationByExpr(e), ident.Type);
                }
                else
                {
                    messageType = ident.Type.Type_;
                    if (Types.KindOf(messageType) != Kind.kindObject)
                    {
                        errors.NotAMessageType(LocationByExpr(e), messageType);
                        messageType = Decls.Error;
                    }
                }
            }

            if (CheckerEnv.IsObjectWellKnownType(messageType))
            {
                SetType(e, CheckerEnv.GetObjectWellKnownType(messageType));
            }
            else
            {
                SetType(e, messageType);
            }

            // Check the field initializers.
            foreach (CreateStruct.Types.Entry ent in msgVal.Entries)
            {
                string field = ent.FieldKey;
                Expr value = ent.Value;
                Check(value);

                Type fieldType = Decls.Error;
                FieldType t = LookupFieldType(LocationByID(ent.Id), messageType.MessageType, field);
                if (t != null)
                {
                    fieldType = t.type;
                }

                if (!IsAssignable(fieldType, GetType(value)))
                {
                    errors.FieldTypeMismatch(LocationByID(ent.Id), field, fieldType, GetType(value));
                }
            }
        }

        internal void CheckComprehension(Expr e)
        {
            Comprehension comp = e.ComprehensionExpr;
            Check(comp.IterRange);
            Check(comp.AccuInit);
            Type accuType = GetType(comp.AccuInit);
            Type rangeType = GetType(comp.IterRange);
            Type varType;

            switch (Types.KindOf(rangeType))
            {
                case Kind.kindList:
                    varType = rangeType.ListType.ElemType;
                    break;
                case Kind.kindMap:
                    // Ranges over the keys.
                    varType = rangeType.MapType.KeyType;
                    break;
                case Kind.kindDyn:
                case Kind.kindError:
                case Kind.kindTypeParam:
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
            Check(comp.LoopCondition);
            AssertType(comp.LoopCondition, Decls.Bool);
            Check(comp.LoopStep);
            AssertType(comp.LoopStep, accuType);
            // Exit the loop's block scope before checking the result.
            env = env.ExitScope();
            Check(comp.Result);
            // Exit the comprehension scope.
            env = env.ExitScope();
            SetType(e, GetType(comp.Result));
        }

        /// <summary>
        /// Checks compatibility of joined types, and returns the most general common type. </summary>
        internal Type JoinTypes(Location loc, Type previous, Type current)
        {
            if (previous == null)
            {
                return current;
            }

            if (IsAssignable(previous, current))
            {
                return Types.MostGeneral(previous, current);
            }

            if (DynAggregateLiteralElementTypesEnabled())
            {
                return Decls.Dyn;
            }

            errors.TypeMismatch(loc, previous, current);
            return Decls.Error;
        }

        internal bool DynAggregateLiteralElementTypesEnabled()
        {
            return env.aggLitElemType == CheckerEnv.DynElementType;
        }

        internal Type NewTypeVar()
        {
            int id = freeTypeVarCounter;
            freeTypeVarCounter++;
            return Decls.NewTypeParamType(String.Format("_var{0:D}", id));
        }

        internal bool IsAssignable(Type t1, Type t2)
        {
            Mapping subs = Types.IsAssignable(mappings, t1, t2);
            if (subs != null)
            {
                mappings = subs;
                return true;
            }

            return false;
        }

        internal bool IsAssignableList(IList<Type> l1, IList<Type> l2)
        {
            Mapping subs = Types.IsAssignableList(mappings, l1, l2);
            if (subs != null)
            {
                mappings = subs;
                return true;
            }

            return false;
        }

        internal FieldType LookupFieldType(Location l, string messageType, string fieldName)
        {
            if (env.provider.FindType(messageType) == null)
            {
                // This should not happen, anyway, report an error.
                errors.UnexpectedFailedResolution(l, messageType);
                return null;
            }

            FieldType ft = env.provider.FindFieldType(messageType, fieldName);
            if (ft != null)
            {
                return ft;
            }

            errors.UndefinedField(l, fieldName);
            return null;
        }

        internal void SetType(Expr e, Type t)
        {
            Type old = types[e.Id];
            if (old != null && !old.Equals(t))
            {
                throw new System.InvalidOperationException(String.Format(
                    "(Incompatible) Type already exists for expression: {0}({1:D}) old:{2}, new:{3}", e, e.Id, old, t));
            }

            types[e.Id] = t;
        }

        internal Type GetType(Expr e)
        {
            return types[e.Id];
        }

        internal void SetReference(Expr e, Reference r)
        {
            Reference old = references[e.Id];
            if (old != null && !old.Equals(r))
            {
                throw new System.InvalidOperationException(String.Format(
                    "Reference already exists for expression: {0}({1:D}) old:{2}, new:{3}", e, e.Id, old, r));
            }

            references[e.Id] = r;
        }

        internal void AssertType(Expr e, Type t)
        {
            if (!IsAssignable(t, GetType(e)))
            {
                errors.TypeMismatch(LocationByExpr(e), t, GetType(e));
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

        internal static OverloadResolution NewResolution(Reference checkedRef, Type t)
        {
            return new OverloadResolution(checkedRef, t);
        }

        internal Location LocationByExpr(Expr e)
        {
            return LocationByID(e.Id);
        }

        internal Location LocationByID(long id)
        {
            IDictionary<long, int> positions = sourceInfo.Positions;
            int line = 1;
            int? offset = positions[id];
            if (offset != null)
            {
                int col = offset.Value;
                foreach (int? lineOffset in sourceInfo.LineOffsets)
                {
                    if (lineOffset < offset)
                    {
                        line++;
                        col = offset.Value - lineOffset.Value;
                    }
                    else
                    {
                        break;
                    }
                }

                return Location.NewLocation(line, col);
            }

            return Location.NoLocation;
        }

        internal static Reference NewIdentReference(string name, Constant value)
        {
            Reference refBuilder = new Reference();
            refBuilder.Name = name;
            if (value != null && value.ConstantKindCase != ConstantKindCase.None)
            {
                refBuilder.Value = value;
            }

            return refBuilder;
        }

        internal static Reference NewFunctionReference(IList<string> overloads)
        {
            Reference reference = new Reference();
            reference.OverloadId.Add(overloads);
            return reference;
        }
    }
}