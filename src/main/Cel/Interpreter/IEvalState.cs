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

namespace Cel.Interpreter;

/// <summary>
///     IEvalState tracks the values associated with expression ids during execution.
/// </summary>
public interface IEvalState
{
    /// <summary>
    ///     IDs returns the list of ids with recorded values.
    /// </summary>
    long[] Ids();

    /// <summary>
    ///     Value returns the observed value of the given expression id if found, and a nil false result if
    ///     not.
    /// </summary>
    IVal? Value(long id);

    /// <summary>
    ///     SetValue sets the observed value of the expression id.
    /// </summary>
    void SetValue(long id, IVal v);

    /// <summary>
    ///     Reset clears the previously recorded expression values.
    /// </summary>
    void Reset();

    /// <summary>
    ///     NewEvalState returns an EvalState instanced used to observe the intermediate evaluations of an
    ///     expression.
    /// </summary>
    static IEvalState NewEvalState()
    {
        return new EvalStateImpl();
    }

}

/// <summary>
///     evalState permits the mutation of evaluation state for a given expression id.
/// </summary>
public sealed class EvalStateImpl : IEvalState
{
    private readonly IDictionary<long, IVal> values = new Dictionary<long, IVal>();

    /// <summary>
    ///     IDs implements the EvalState interface method.
    /// </summary>
    public long[] Ids()
    {
        return values.Keys.Select(l => l).ToArray();
    }

    /// <summary>
    ///     Value is an implementation of the EvalState interface method.
    /// </summary>
    public IVal? Value(long id)
    {
        values.TryGetValue(id, out var v);
        return v;
    }

    /// <summary>
    ///     SetValue is an implementation of the EvalState interface method.
    /// </summary>
    public void SetValue(long id, IVal v)
    {
        values[id] = v;
    }

    /// <summary>
    ///     Reset implements the EvalState interface method.
    /// </summary>
    public void Reset()
    {
        values.Clear();
    }
}