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

namespace Cel.Interpreter
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.Dispatcher.newDispatcher;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.InterpretableDecorator.decDisableShortcircuits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.InterpretableDecorator.decObserveEval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.InterpretableDecorator.decOptimize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.InterpretablePlanner.newPlanner;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.InterpretablePlanner.newUncheckedPlanner;

    using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using Container = global::Cel.Common.Containers.Container;
    using TypeAdapter = global::Cel.Common.Types.Ref.TypeAdapter;
    using TypeProvider = global::Cel.Common.Types.Ref.TypeProvider;
    using Overload = global::Cel.Interpreter.Functions.Overload;

    /// <summary>
    /// Interpreter generates a new Interpretable from a checked or unchecked expression. </summary>
    public interface Interpreter
    {
        /// <summary>
        /// NewInterpretable creates an Interpretable from a checked expression and an optional list of
        /// InterpretableDecorator values.
        /// </summary>
        Interpretable NewInterpretable(CheckedExpr @checked, params InterpretableDecorator[] decorators);

        /// <summary>
        /// NewUncheckedInterpretable returns an Interpretable from a parsed expression and an optional
        /// list of InterpretableDecorator values.
        /// </summary>
        Interpretable NewUncheckedInterpretable(Expr expr, params InterpretableDecorator[] decorators);

        /// <summary>
        /// TrackState decorates each expression node with an observer which records the value associated
        /// with the given expression id. EvalState must be provided to the decorator. This decorator is
        /// not thread-safe, and the EvalState must be reset between Eval() calls.
        /// </summary>
        static InterpretableDecorator TrackState(EvalState state)
        {
            return IInterpretableDecorator.DecObserveEval(state.SetValue);
        }

        /// <summary>
        /// ExhaustiveEval replaces operations that short-circuit with versions that evaluate expressions
        /// and couples this behavior with the TrackState() decorator to provide insight into the
        /// evaluation state of the entire expression. EvalState must be provided to the decorator. This
        /// decorator is not thread-safe, and the EvalState must be reset between Eval() calls.
        /// </summary>
        static InterpretableDecorator ExhaustiveEval(EvalState state)
        {
            InterpretableDecorator ex = IInterpretableDecorator.DecDisableShortcircuits();
            InterpretableDecorator obs = TrackState(state);
            return i =>
            {
                Interpretable iDec = ex(i);
                return obs(iDec);
            };
        }

        /// <summary>
        /// Optimize will pre-compute operations such as list and map construction and optimize call
        /// arguments to set membership tests. The set of optimizations will increase over time.
        /// </summary>
        static InterpretableDecorator Optimize()
        {
            return IInterpretableDecorator.DecOptimize();
        }

        /// <summary>
        /// NewInterpreter builds an Interpreter from a Dispatcher and TypeProvider which will be used
        /// throughout the Eval of all Interpretable instances gerenated from it.
        /// </summary>
        static Interpreter NewInterpreter(Dispatcher dispatcher, Container container, TypeProvider provider,
            TypeAdapter adapter, AttributeFactory attrFactory)
        {
            return new Interpreter_ExprInterpreter(dispatcher, container, provider, adapter, attrFactory);
        }

        /// <summary>
        /// NewStandardInterpreter builds a Dispatcher and TypeProvider with support for all of the CEL
        /// builtins defined in the language definition.
        /// </summary>
        static Interpreter NewStandardInterpreter(Container container, TypeProvider provider, TypeAdapter adapter,
            AttributeFactory resolver)
        {
            Dispatcher dispatcher = Dispatcher.NewDispatcher();
            dispatcher.Add(Overload.StandardOverloads());
            return NewInterpreter(dispatcher, container, provider, adapter, resolver);
        }
    }

    public sealed class Interpreter_ExprInterpreter : Interpreter
    {
        internal readonly Dispatcher dispatcher;
        internal readonly Container container;
        internal readonly TypeProvider provider;
        internal readonly TypeAdapter adapter;
        internal readonly AttributeFactory attrFactory;

        internal Interpreter_ExprInterpreter(Dispatcher dispatcher, Container container, TypeProvider provider,
            TypeAdapter adapter, AttributeFactory attrFactory)
        {
            this.dispatcher = dispatcher;
            this.container = container;
            this.provider = provider;
            this.adapter = adapter;
            this.attrFactory = attrFactory;
        }

        /// <summary>
        /// NewIntepretable implements the Interpreter interface method. </summary>
        public Interpretable NewInterpretable(CheckedExpr @checked, params InterpretableDecorator[] decorators)
        {
            InterpretablePlanner p = InterpretablePlanner.NewPlanner(dispatcher, provider, adapter, attrFactory,
                container, @checked, decorators);
            return p.Plan(@checked.Expr);
        }

        /// <summary>
        /// NewUncheckedIntepretable implements the Interpreter interface method. </summary>
        public Interpretable NewUncheckedInterpretable(Expr expr, params InterpretableDecorator[] decorators)
        {
            InterpretablePlanner p = InterpretablePlanner.NewUncheckedPlanner(dispatcher, provider, adapter,
                attrFactory, container, decorators);
            return p.Plan(expr);
        }
    }
}