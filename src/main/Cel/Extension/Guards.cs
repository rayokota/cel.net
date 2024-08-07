﻿using Cel.Common.Types;
using Cel.Interpreter.Functions;

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
namespace Cel.Extension;

/// <summary>
///     function invocation guards for common call signatures within extension functions.
/// </summary>
public sealed class Guards
{
    private Guards()
    {
    }

    public static BinaryOp CallInStrIntOutStr(Func<string, int, string> func)
    {
        return (lhs, rhs) =>
        {
            try
            {
                return StringT.StringOf(func((string)lhs.Value(), GetIntValue((IntT)rhs)));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static FunctionOp CallInStrIntIntOutStr(Func<string, int, int, string> func)
    {
        return values =>
        {
            try
            {
                return StringT.StringOf(func((string)values[0].Value(), GetIntValue((IntT)values[1]),
                    GetIntValue((IntT)values[2])));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static BinaryOp CallInStrStrOutInt(Func<string, string, int> func)
    {
        return (lhs, rhs) =>
        {
            try
            {
                return IntT.IntOf(func((string)lhs.Value(), (string)rhs.Value()));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static FunctionOp CallInStrStrIntOutInt(Func<string, string, int, int> func)
    {
        return values =>
        {
            try
            {
                return IntT.IntOf(func((string)values[0].Value(), (string)values[1].Value(),
                    GetIntValue((IntT)values[2])));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static BinaryOp CallInStrStrOutStrArr(Func<string, string, string[]> func)
    {
        return (lhs, rhs) =>
        {
            try
            {
                return ListT.NewStringArrayList(func((string)lhs.Value(), (string)rhs.Value()));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static FunctionOp CallInStrStrIntOutStrArr(Func<string, string, int, string[]> func)
    {
        return values =>
        {
            try
            {
                return ListT.NewStringArrayList(func((string)values[0].Value(), (string)values[1].Value(),
                    GetIntValue((IntT)values[2])));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static FunctionOp CallInStrStrStrOutStr(Func<string, string, string, string> func)
    {
        return values =>
        {
            try
            {
                return StringT.StringOf(func((string)values[0].Value(), (string)values[1].Value(),
                    (string)values[2].Value()));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static FunctionOp CallInStrStrStrIntOutStr(Func<string, string, string, int, string> func)
    {
        return values =>
        {
            try
            {
                return StringT.StringOf(func((string)values[0].Value(), (string)values[1].Value(),
                    (string)values[2].Value(), GetIntValue((IntT)values[3])));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static UnaryOp CallInStrOutStr(Func<string, string> func)
    {
        return val =>
        {
            try
            {
                return StringT.StringOf(func((string)val.Value()));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static UnaryOp CallInStrArrayOutStr(Func<string[], string> func)
    {
        return val =>
        {
            try
            {
                var objects = (Array)val.Value();
                var strings = new string[objects.Length];
                Array.Copy(objects, 0, strings, 0, objects.Length);
                return StringT.StringOf(func(strings));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    public static BinaryOp CallInStrArrayStrOutStr(Func<string[], string, string> func)
    {
        return (lhs, rhs) =>
        {
            try
            {
                var objects = (Array)lhs.Value();
                var strings = new string[objects.Length];
                Array.Copy(objects, 0, strings, 0, objects.Length);
                return StringT.StringOf(func(strings, (string)rhs.Value()));
            }
            catch (Exception e)
            {
                return Err.NewErr(e, "{0}", e.Message);
            }
        };
    }

    private static int GetIntValue(IntT value)
    {
        var longValue = (long)value.Value();
        if (longValue > int.MaxValue || longValue < int.MinValue)
            throw new Exception(string.Format("Integer {0:D} value overflow", longValue));

        return (int)longValue;
    }
}