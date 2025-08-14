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

namespace Cel;

internal sealed class ProgGen : IProgram, ICoster
{
    private readonly ProgFactory factory;

    internal ProgGen(ProgFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        // Use an empty state value since no evaluation is performed.
        var p = factory(Prog.EmptyEvalState);
        return Cel.EstimateCost(p);
    }

    /// <summary>
    ///     Eval implements the Program interface method.
    /// </summary>
    public EvalResult Eval(object input)
    {
        // The factory based Eval() differs from the standard evaluation model in that it generates a
        // new EvalState instance for each call to ensure that unique evaluations yield unique stateful
        // results.
        var state = EvalStateFactory.NewEvalState();

        // Generate a new instance of the interpretable using the factory configured during the call to
        // newProgram(). It is incredibly unlikely that the factory call will generate an error given
        // the factory test performed within the Program() call.
        var p = factory(state);

        // Evaluate the input, returning the result and the 'state' within EvalDetails.
        return p.Eval(input);
    }
}