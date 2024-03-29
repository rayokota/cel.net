﻿using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Parser;
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
namespace Cel.Checker;

/// <summary>
///     Env is the environment for type checking.
///     <para>
///         The Env is comprised of a container, type provider, declarations, and other related objects
///         which can be used to assist with type-checking.
///     </para>
/// </summary>
public sealed class CheckerEnv
{
    internal const int DynElementType = 0;
    internal const int HomogenousElementType = 1;
    internal readonly Container container;
    private readonly Scopes declarations;
    internal readonly ITypeProvider provider;
    internal int aggLitElemType;

    private CheckerEnv(Container container, ITypeProvider provider, Scopes declarations, int aggLitElemType)
    {
        this.container = container;
        this.provider = provider;
        this.declarations = declarations;
        this.aggLitElemType = aggLitElemType;
    }

    /// <summary>
    ///     NewEnv returns a new *Env with the given parameters.
    /// </summary>
    public static CheckerEnv NewCheckerEnv(Container container, ITypeProvider provider)
    {
        var declarations = Scopes.NewScopes();
        // declarations.push(); // TODO why this ??

        return new CheckerEnv(container, provider, declarations, DynElementType);
    }

    /// <summary>
    ///     NewStandardEnv returns a new *Env with the given params plus standard declarations.
    /// </summary>
    public static CheckerEnv NewStandardCheckerEnv(Container container, ITypeProvider provider)
    {
        var e = NewCheckerEnv(container, provider);
        e.Add(Checker.StandardDeclarations);
        // TODO: isolate standard declarations from the custom set which may be provided layer.
        return e;
    }

    /// <summary>
    ///     EnableDynamicAggregateLiterals detmerines whether list and map literals may support mixed
    ///     element types at check-time. This does not preclude the presence of a dynamic list or map
    ///     somewhere in the CEL evaluation process.
    /// </summary>
    public CheckerEnv EnableDynamicAggregateLiterals(bool enabled)
    {
        aggLitElemType = enabled ? DynElementType : HomogenousElementType;
        return this;
    }

    /// <summary>
    ///     Add adds new Decl protos to the Env. Returns an error for identifier redeclarations.
    /// </summary>
    public void Add(params Decl[] decls)
    {
        Add(new List<Decl>(decls));
    }

    /// <summary>
    ///     Add adds new Decl protos to the Env. Returns an error for identifier redeclarations.
    /// </summary>
    public void Add(IList<Decl> decls)
    {
        IList<string> errMsgs = new List<string>();
        foreach (var decl in decls)
            switch (decl.DeclKindCase)
            {
                case Decl.DeclKindOneofCase.Ident:
                    AddIdent(SanitizeIdent(decl), errMsgs);
                    break;
                case Decl.DeclKindOneofCase.Function:
                    AddFunction(SanitizeFunction(decl), errMsgs);
                    break;
            }

        if (errMsgs.Count > 0) throw new ArgumentException(string.Join("\n", errMsgs));
    }

    /// <summary>
    ///     LookupIdent returns a Decl proto for typeName as an identifier in the Env. Returns nil if no
    ///     such identifier is found in the Env.
    /// </summary>
    public Decl? LookupIdent(string name)
    {
        foreach (var candidate in container.ResolveCandidateNames(name))
        {
            var ident = declarations.FindIdent(candidate);
            if (ident != null) return ident;

            // Next try to import the name as a reference to a message type. If found,
            // the declaration is added to the outest (global) scope of the
            // environment, so next time we can access it faster.
            var t = provider.FindType(candidate);
            if (t != null)
            {
                var decl = Decls.NewVar(candidate, t);
                declarations.AddIdent(decl);
                return decl;
            }

            // Next try to import this as an enum value by splitting the name in a type prefix and
            // the enum inside.
            var enumValue = provider.EnumValue(candidate);
            if (enumValue.Type() != Err.ErrType)
            {
                var constant = new Constant();
                constant.Int64Value = enumValue.IntValue();
                var decl = Decls.NewIdent(candidate, Decls.Int, constant);
                declarations.AddIdent(decl);
                return decl;
            }
        }

        return null;
    }

    /// <summary>
    ///     LookupFunction returns a Decl proto for typeName as a function in env. Returns nil if no such
    ///     function is found in env.
    /// </summary>
    public Decl? LookupFunction(string name)
    {
        foreach (var candidate in container.ResolveCandidateNames(name))
        {
            var fn = declarations.FindFunction(candidate);
            if (fn != null) return fn;
        }

        return null;
    }

    /// <summary>
    ///     addOverload adds overload to function declaration f. Returns one or more errorMsg values if the
    ///     overload overlaps with an existing overload or macro.
    /// </summary>
    internal Decl AddOverload(Decl f, Decl.Types.FunctionDecl.Types.Overload overload, IList<string> errMsgs)
    {
        var function = f.Function;
        var emptyMappings = Mapping.NewMapping();
        var overloadFunction = Decls.NewFunctionType(overload.ResultType, overload.Params);
        var overloadErased = Types.Substitute(emptyMappings, overloadFunction, true);
        var hasErr = false;
        foreach (var existing in function.Overloads)
        {
            var existingFunction = Decls.NewFunctionType(existing.ResultType, existing.Params);
            var existingErased = Types.Substitute(emptyMappings, existingFunction, true);
            var overlap = Types.IsAssignable(emptyMappings, overloadErased, existingErased) != null
                          || Types.IsAssignable(emptyMappings, existingErased, overloadErased) != null;
            if (overlap && overload.IsInstanceFunction == existing.IsInstanceFunction)
            {
                errMsgs.Add(OverlappingOverloadError(f.Name, overload.OverloadId, overloadFunction,
                    existing.OverloadId, existingFunction));
                hasErr = true;
            }
        }

        foreach (var macro in Macro.AllMacros)
            if (macro.Function().Equals(f.Name) && macro.ReceiverStyle == overload.IsInstanceFunction &&
                macro.ArgCount() == overload.Params.Count)
            {
                errMsgs.Add(OverlappingMacroError(f.Name, macro.ArgCount()));
                hasErr = true;
            }

        if (hasErr) return f;

        function.Overloads.Add(overload);
        f.Function = function;
        return f;
    }

    /// <summary>
    ///     addFunction adds the function Decl to the Env. Adds a function decl if one doesn't already
    ///     exist, then adds all overloads from the Decl. If overload overlaps with an existing overload,
    ///     adds to the errors in the Env instead.
    /// </summary>
    internal void AddFunction(Decl decl, IList<string> errMsgs)
    {
        var current = declarations.FindFunction(decl.Name);
        if (current == null)
        {
            // Add the function declaration without overloads and check the overloads below.
            current = Decls.NewFunction(decl.Name, new List<Decl.Types.FunctionDecl.Types.Overload>());
            declarations.AddFunction(current);
        }

        foreach (var overload in decl.Function.Overloads) current = AddOverload(current, overload, errMsgs);

        declarations.UpdateFunction(decl.Name, current);
    }

    /// <summary>
    ///     addIdent adds the Decl to the declarations in the Env. Returns a non-empty errorMsg if the
    ///     identifier is already declared in the scope.
    /// </summary>
    internal void AddIdent(Decl decl, IList<string> errMsgs)
    {
        var current = declarations.FindIdentInScope(decl.Name);
        if (current != null)
        {
            errMsgs.Add(OverlappingIdentifierError(decl.Name));
            return;
        }

        declarations.AddIdent(decl);
    }

    // sanitizeFunction replaces well-known types referenced by message name with their equivalent
    // CEL built-in type instances.
    internal Decl SanitizeFunction(Decl decl)
    {
        var fn = decl.Function;
        // Determine whether the declaration requires replacements from proto-based message type
        // references to well-known CEL type references.
        var needsSanitizing = false;
        foreach (var o in fn.Overloads)
        {
            if (IsObjectWellKnownType(o.ResultType))
            {
                needsSanitizing = true;
                break;
            }

            foreach (var p in o.Params)
                if (IsObjectWellKnownType(p))
                {
                    needsSanitizing = true;
                    break;
                }
        }

        // Early return if the declaration requires no modification.
        if (!needsSanitizing) return decl;

        // Sanitize all of the overloads if any overload requires an update to its type references.
        IList<Decl.Types.FunctionDecl.Types.Overload> overloads =
            new List<Decl.Types.FunctionDecl.Types.Overload>(fn.Overloads);
        foreach (var o in fn.Overloads)
        {
            var sanitized = false;
            var rt = o.ResultType;
            if (IsObjectWellKnownType(rt))
            {
                rt = GetObjectWellKnownType(rt);
                sanitized = true;
            }

            IList<Type> @params = new List<Type>(o.Params);
            foreach (var p in o.Params)
                if (IsObjectWellKnownType(p))
                {
                    @params.Add(GetObjectWellKnownType(p)!);
                    sanitized = true;
                }
                else
                {
                    @params.Add(p);
                }

            // If sanitized, replace the overload definition.
            Decl.Types.FunctionDecl.Types.Overload ov;
            if (sanitized)
            {
                if (o.IsInstanceFunction)
                    ov = Decls.NewInstanceOverload(o.OverloadId, @params, rt);
                else
                    ov = Decls.NewOverload(o.OverloadId, @params, rt);
            }
            else
            {
                // Otherwise, preserve the original overload.
                ov = o;
            }

            overloads.Add(ov);
        }

        return Decls.NewFunction(decl.Name, overloads);
    }

    /// <summary>
    ///     sanitizeIdent replaces the identifier's well-known types referenced by message name with
    ///     references to CEL built-in type instances.
    /// </summary>
    internal Decl SanitizeIdent(Decl decl)
    {
        var id = decl.Ident;
        var t = id.Type;
        if (!IsObjectWellKnownType(t)) return decl;

        return Decls.NewIdent(decl.Name, GetObjectWellKnownType(t)!, id.Value);
    }

    /// <summary>
    ///     isObjectWellKnownType returns true if the input type is an OBJECT type with a message name that
    ///     corresponds the message name of a built-in CEL type.
    /// </summary>
    internal static bool IsObjectWellKnownType(Type t)
    {
        if (Types.KindOf(t) != Types.Kind.KindObject) return false;

        return Checked.CheckedWellKnowns.TryGetValue(t.MessageType, out var type);
    }

    /// <summary>
    ///     getObjectWellKnownType returns the built-in CEL type declaration for input type's message name.
    /// </summary>
    internal static Type? GetObjectWellKnownType(Type t)
    {
        Checked.CheckedWellKnowns.TryGetValue(t.MessageType, out var type);
        return type;
    }

    /// <summary>
    ///     enterScope creates a new Env instance with a new innermost declaration scope.
    /// </summary>
    internal CheckerEnv EnterScope()
    {
        var childDecls = declarations.Push();
        return new CheckerEnv(container, provider, childDecls, aggLitElemType);
    }

    // exitScope creates a new Env instance with the nearest outer declaration scope.
    internal CheckerEnv ExitScope()
    {
        var parentDecls = declarations.Pop();
        return new CheckerEnv(container, provider, parentDecls, aggLitElemType);
    }

    // errorMsg is a type alias meant to represent error-based return values which
    // may be accumulated into an error at a later point in execution.
    //  type errorMsg string

    internal string OverlappingIdentifierError(string name)
    {
        return string.Format("overlapping identifier for name '{0}'", name);
    }

    internal string OverlappingOverloadError(string name, string overloadId1, Type f1, string overloadId2, Type f2)
    {
        return string.Format(
            "overlapping overload for name '{0}' (type '{1}' with overloadId: '{2}' " +
            "cannot be distinguished from '{3}' with overloadId: '{4}')", name, Types.FormatCheckedType(f1),
            overloadId1, Types.FormatCheckedType(f2), overloadId2);
    }

    internal string OverlappingMacroError(string name, int argCount)
    {
        return string.Format("overlapping macro for name '{0}' with {1:D} args", name, argCount);
    }
}