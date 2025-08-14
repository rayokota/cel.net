﻿/*
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

using Cel.Common.Containers;
using Cel.Common.Types.Ref;
using Cel.Interpreter.Functions;
using Google.Api.Expr.V1Alpha1;

namespace Cel.Interpreter;

/// <summary>
///     IInterpreter generates a new Interpretable from a checked or unchecked expression.
/// </summary>
public interface IInterpreter
{
    /// <summary>
    ///     NewInterpretable creates an Interpretable from a checked expression and an optional list of
    ///     InterpretableDecorator values.
    /// </summary>
    IInterpretable? NewInterpretable(CheckedExpr @checked, params InterpretableDecorator[] decorators);

    /// <summary>
    ///     NewUncheckedInterpretable returns an Interpretable from a parsed expression and an optional
    ///     list of InterpretableDecorator values.
    /// </summary>
    IInterpretable? NewUncheckedInterpretable(Expr expr, params InterpretableDecorator[] decorators);

    /// <summary>
    ///     TrackState decorates each expression node with an observer which records the value associated
    ///     with the given expression id. EvalState must be provided to the decorator. This decorator is
    ///     not thread-safe, and the EvalState must be reset between Eval() calls.
    /// </summary>
    

    /// <summary>
    ///     ExhaustiveEval replaces operations that short-circuit with versions that evaluate expressions
    ///     and couples this behavior with the TrackState() decorator to provide insight into the
    ///     evaluation state of the entire expression. EvalState must be provided to the decorator. This
    ///     decorator is not thread-safe, and the EvalState must be reset between Eval() calls.
    /// </summary>
    

    /// <summary>
    ///     Optimize will pre-compute operations such as list and map construction and optimize call
    ///     arguments to set membership tests. The set of optimizations will increase over time.
    /// </summary>
    

    /// <summary>
    ///     NewInterpreter builds an Interpreter from a Dispatcher and TypeProvider which will be used
    ///     throughout the Eval of all Interpretable instances gerenated from it.
    /// </summary>
    

    /// <summary>
    ///     NewStandardInterpreter builds a Dispatcher and TypeProvider with support for all of the CEL
    ///     builtins defined in the language definition.
    /// </summary>
}

public static class InterpreterUtils
{
    public static InterpretableDecorator TrackState(IEvalState state)
    {
        return InterpretableDecoratorUtils.DecObserveEval(state.SetValue);
    }

    public static InterpretableDecorator ExhaustiveEval(IEvalState state)
    {
        var ex = InterpretableDecoratorUtils.DecDisableShortcircuits();
        var obs = TrackState(state);
        return i =>
        {
            var iDec = ex(i);
            return obs(iDec);
        };
    }

    public static InterpretableDecorator Optimize()
    {
        return InterpretableDecoratorUtils.DecOptimize();
    }

    public static IInterpreter NewInterpreter(IDispatcher dispatcher, Container container, ITypeProvider provider,
        TypeAdapter adapter, IAttributeFactory attrFactory)
    {
        return new ExprInterpreter(dispatcher, container, provider, adapter, attrFactory);
    }

    public static IInterpreter NewStandardInterpreter(Container container, ITypeProvider provider, TypeAdapter adapter,
        IAttributeFactory resolver)
    {
        var dispatcher = DispatcherFactory.NewDispatcher();
        dispatcher.Add(Overload.StandardOverloads());
        return NewInterpreter(dispatcher, container, provider, adapter, resolver);
    }
}

public sealed class ExprInterpreter : IInterpreter
{
    private readonly TypeAdapter adapter;
    private readonly IAttributeFactory attrFactory;
    private readonly Container container;
    private readonly IDispatcher dispatcher;
    private readonly ITypeProvider provider;

    internal ExprInterpreter(IDispatcher dispatcher, Container container, ITypeProvider provider,
        TypeAdapter adapter, IAttributeFactory attrFactory)
    {
        this.dispatcher = dispatcher;
        this.container = container;
        this.provider = provider;
        this.adapter = adapter;
        this.attrFactory = attrFactory;
    }

    /// <summary>
    ///     NewIntepretable implements the Interpreter interface method.
    /// </summary>
    public IInterpretable? NewInterpretable(CheckedExpr @checked, params InterpretableDecorator[] decorators)
    {
        var p = InterpretablePlannerFactory.NewPlanner(dispatcher, provider, adapter, attrFactory,
            container, @checked, decorators);
        return p.Plan(@checked.Expr);
    }

    /// <summary>
    ///     NewUncheckedIntepretable implements the Interpreter interface method.
    /// </summary>
    public IInterpretable? NewUncheckedInterpretable(Expr expr, params InterpretableDecorator[] decorators)
    {
        var p = InterpretablePlannerFactory.NewUncheckedPlanner(dispatcher, provider, adapter,
            attrFactory, container, decorators);
        return p.Plan(expr);
    }
}