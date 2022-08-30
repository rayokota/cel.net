using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Interpreter;

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
namespace Cel;

/// <summary>
///     prog is the internal implementation of the Program interface.
/// </summary>
public sealed class Prog : Program, Coster
{
    internal static readonly EvalState EmptyEvalState = EvalState.NewEvalState();
    internal readonly IList<InterpretableDecorator> decorators = new List<InterpretableDecorator>();
    internal readonly Dispatcher dispatcher;

    internal readonly Env e;
    internal readonly ISet<EvalOption> evalOpts = new HashSet<EvalOption>();
    internal readonly EvalState state;
    internal AttributeFactory attrFactory;
    internal Activation defaultVars;
    internal Interpretable interpretable;
    internal Interpreter.Interpreter interpreter;

    internal Prog(Env e, Dispatcher dispatcher)
    {
        this.e = e;
        this.dispatcher = dispatcher;
        state = EvalState.NewEvalState();
    }

    internal Prog(Env e, ISet<EvalOption> evalOpts, Activation defaultVars, Dispatcher dispatcher,
        Interpreter.Interpreter interpreter, EvalState state)
    {
        this.e = e;
        this.evalOpts.UnionWith(evalOpts);
        this.defaultVars = defaultVars;
        this.dispatcher = dispatcher;
        this.interpreter = interpreter;
        this.state = state;
    }

    // Cost implements the Coster interface method.
    public Cost Cost()
    {
        return Cel.EstimateCost(interpretable);
    }

    /// <summary>
    ///     Eval implements the Program interface method.
    /// </summary>
    public EvalResult Eval(object input)
    {
        Val v;

        var evalDetails = new EvalDetails(state);

        try
        {
            // Build a hierarchical activation if there are default vars set.
            var vars = Activation.NewActivation(input);

            if (defaultVars != null) vars = Activation.NewHierarchicalActivation(defaultVars, vars);

            v = interpretable.Eval(vars);
        }
        catch (Err.ErrException e)
        {
            v = e.Err;
        }
        catch (Exception e)
        {
            throw new Exception(string.Format("internal error: {0}", e.Message), e);
        }

        // The output of an internal Eval may have a value (`v`) that is a types.Err. This step
        // translates the CEL value to a Go error response. This interface does not quite match the
        // RPC signature which allows for multiple errors to be returned, but should be sufficient.
        // NOTE: Unlike the Go implementation, errors are handled differently in the Java
        // implementation.
        //    if (isError(v)) {
        //      throw new EvalException(v);
        //    }

        return Program.NewEvalResult(v, evalDetails);
    }
}