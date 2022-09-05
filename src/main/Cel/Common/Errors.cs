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

namespace Cel.Common;

public class Errors
{
    private readonly IList<CelError> errors = new List<CelError>();
    private readonly Source source;

    public Errors(Source source)
    {
        this.source = source;
    }

    /// <summary>
    ///     GetErrors returns the list of observed errors.
    /// </summary>
    public virtual IList<CelError> GetErrors => errors;

    /// <summary>
    ///     ReportError records an error at a source location.
    /// </summary>
    public virtual void ReportError(Location l, string format, params object[] args)
    {
        var err = new CelError(l, string.Format(format, args));
        errors.Add(err);
    }

    public virtual bool HasErrors()
    {
        return errors.Count > 0;
    }

    /// <summary>
    ///     Append takes an Errors object as input creates a new Errors object with the current and input
    ///     errors.
    /// </summary>
    public virtual Errors Append(IList<CelError> errors)
    {
        var errs = new Errors(source);
        ((List<CelError>)errs.errors).AddRange(this.errors);
        ((List<CelError>)errs.errors).AddRange(errors);
        return errs;
    }

    public override string ToString()
    {
        return ToDisplayString();
    }

    /// <summary>
    ///     ToDisplayString returns the error set to a newline delimited string.
    /// </summary>
    public virtual string ToDisplayString()
    {
        return string.Join("\n", errors.OrderBy(c => c)
            .Select(e => e.ToDisplayString(source)));
    }

    public virtual void SyntaxError(Location l, string msg)
    {
        ReportError(l, "Syntax error: {0}", msg);
    }
}