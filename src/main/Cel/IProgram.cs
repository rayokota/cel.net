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

using Cel.Common.Types.Ref;

namespace Cel;

/// <summary>
///     IProgram is an evaluable view of an Ast.
/// </summary>
public interface IProgram
{

    /// <summary>
    ///     Eval returns the result of an evaluation of the Ast and environment against the input vars.
    ///     <para>
    ///         The vars value may either be an `interpreter.Activation` or a `map[string]interface{}`.
    ///     </para>
    ///     <para>
    ///         If the `OptTrackState` or `OptExhaustiveEval` flags are used, the `details` response will be
    ///         non-nil. Given this caveat on `details`, the return state from evaluation will be:
    ///         <ul>
    ///             <li>
    ///                 `val`, `details`, `nil` - Successful evaluation of a non-error result.
    ///             </li>
    ///             <li>
    ///                 `val`, `details`, `err` - Successful evaluation to an error result.
    ///             </li>
    ///             <li>
    ///                 `nil`, `details`, `err` - Unsuccessful evaluation.
    ///             </li>
    ///         </ul>
    ///     </para>
    ///     <para>
    ///         An unsuccessful evaluation is typically the result of a series of incompatible `EnvOption`
    ///         or `ProgramOption` values used in the creation of the evaluation environment or executable
    ///         program.
    ///     </para>
    /// </summary>
    EvalResult Eval(object vars);
}

public sealed class EvalResult
{
    internal EvalResult(IVal val, EvalDetails evalDetails)
    {
        Val = val;
        EvalDetails = evalDetails;
    }

    public IVal Val { get; }

    public EvalDetails EvalDetails { get; }
}