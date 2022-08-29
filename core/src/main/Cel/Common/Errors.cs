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

namespace Cel.Common
{
    public class Errors
    {
        private readonly IList<CelError> errors = new List<CelError>();
        private readonly Source source;

        public Errors(Source source)
        {
            this.source = source;
        }

        /// <summary>
        /// ReportError records an error at a source location. </summary>
        public virtual void ReportError(Location l, string format, params object[] args)
        {
            CelError err = new CelError(l, String.Format(format, args));
            errors.Add(err);
        }

        /// <summary>
        /// GetErrors returns the list of observed errors. </summary>
        public virtual IList<CelError> GetErrors
        {
            get { return errors; }
        }

        public virtual bool HasErrors()
        {
            return errors.Count > 0;
        }

        /// <summary>
        /// Append takes an Errors object as input creates a new Errors object with the current and input
        /// errors.
        /// </summary>
        public virtual Errors Append(IList<CelError> errors)
        {
            Errors errs = new Errors(source);
            ((List<CelError>)errs.errors).AddRange(this.errors);
            ((List<CelError>)errs.errors).AddRange(errors);
            return errs;
        }

        public override string ToString()
        {
            return ToDisplayString();
        }

        /// <summary>
        /// ToDisplayString returns the error set to a newline delimited string. </summary>
        public virtual string ToDisplayString()
        {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
            return string.Join("\n", errors.OrderBy(c => c)
                .Select(e => e.ToDisplayString(source)));
        }

        public virtual void SyntaxError(Location l, string msg)
        {
            ReportError(l, "Syntax error: {0}", msg);
        }
    }
}