/*
 * Copyright (C) 2021 The Authors of CEL-Java
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

namespace Cel
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.CEL.estimateCost;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Prog.emptyEvalState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.EvalState.newEvalState;

    using Coster = global::Cel.Interpreter.Coster;
    using EvalState = global::Cel.Interpreter.EvalState;

    internal sealed class ProgGen : Program, Coster
    {
        private readonly ProgFactory factory;

        internal ProgGen(ProgFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Eval implements the Program interface method. </summary>
        public Program_EvalResult Eval(object input)
        {
            // The factory based Eval() differs from the standard evaluation model in that it generates a
            // new EvalState instance for each call to ensure that unique evaluations yield unique stateful
            // results.
            EvalState state = EvalState.NewEvalState();

            // Generate a new instance of the interpretable using the factory configured during the call to
            // newProgram(). It is incredibly unlikely that the factory call will generate an error given
            // the factory test performed within the Program() call.
            Program p = factory(state);

            // Evaluate the input, returning the result and the 'state' within EvalDetails.
            return p.Eval(input);
        }

        /// <summary>
        /// Cost implements the Coster interface method. </summary>
        public Interpreter.Coster_Cost Cost()
        {
            // Use an empty state value since no evaluation is performed.
            Program p = factory(Prog.EmptyEvalState);
            return Cel.EstimateCost(p);
        }
    }
}