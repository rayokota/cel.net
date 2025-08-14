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

using Cel.Interpreter;
using Cel.Interpreter.Functions;

namespace Cel;

public delegate Prog ProgramOption(Prog prog);

public interface IProgramOption { }

public static class ProgramOptions
{
    /// <summary>
    ///     CustomDecorator appends an InterpreterDecorator to the program.
    ///     <para>
    ///         InterpretableDecorators can be used to inspect, alter, or replace the Program plan.
    ///     </para>
    /// </summary>
    public static ProgramOption CustomDecorator(InterpretableDecorator dec)
    {
        return p =>
        {
            p.decorators.Add(dec);
            return p;
        };
    }

    /// <summary>
    ///     Functions adds function overloads that extend or override the set of CEL built-ins.
    /// </summary>
    public static ProgramOption Functions(params Overload[] funcs)
    {
        return p =>
        {
            p.dispatcher.Add(funcs);
            return p;
        };
    }

    /// <summary>
    ///     Globals sets the global variable values for a given program. These values may be shadowed by
    ///     variables with the same name provided to the Eval() call.
    ///     <para>
    ///         The vars value may either be an `interpreter.Activation` instance or a
    ///         `map[string]interface{}`.
    ///     </para>
    /// </summary>
    public static ProgramOption Globals(object vars)
    {
        return p =>
        {
            p.defaultVars = ActivationFactory.NewActivation(vars);
            return p;
        };
    }

    /// <summary>
    ///     EvalOptions sets one or more evaluation options which may affect the evaluation or Result.
    /// </summary>
    public static ProgramOption EvalOptions(params EvalOption[] opts)
    {
        return p =>
        {
            p.evalOpts.UnionWith(opts);
            return p;
        };
    }
}