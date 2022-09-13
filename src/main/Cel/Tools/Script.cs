using Cel.Common.Types;
using Cel.Common.Types.Ref;

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
namespace Cel.Tools;

public sealed class Script
{
    private readonly Env env;
    private readonly IProgram prg;

    internal Script(Env env, IProgram prg)
    {
        this.env = env;
        this.prg = prg;
    }

    public T Execute<T>(IDictionary<string, object> arguments)
    {
        var evalResult = prg.Eval(arguments);

        var result = evalResult.Val;

        if (Err.IsError(result))
        {
            var err = (Err)result;
            throw new ScriptExecutionException(err.ToString(), err.Cause);
        }

        if (UnknownT.IsUnknown(result))
        {
            if (typeof(T) == typeof(IVal) || typeof(T) == typeof(object)) return (T)result;
            throw new ScriptExecutionException(string.Format(
                "script returned unknown {0}, but expected result type is {1}", result, typeof(T).FullName));
        }

        return (T)result.ConvertToNative(typeof(T));
    }
}