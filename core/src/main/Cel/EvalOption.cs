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
///     EvalOption indicates an evaluation option that may affect the evaluation behavior or information
///     in the output result.
/// </summary>
public sealed class EvalOption
{
    public enum InnerEnum
    {
        OptTrackState,
        OptExhaustiveEval,
        OptOptimize,
        OptPartialEval
    }

    /// <summary>
    ///     OptTrackState will cause the runtime to return an immutable EvalState value in the Result.
    /// </summary>
    public static readonly EvalOption OptTrackState = new("OptTrackState", InnerEnum.OptTrackState, 1);

    /// <summary>
    ///     OptExhaustiveEval causes the runtime to disable short-circuits and track state.
    /// </summary>
    public static readonly EvalOption OptExhaustiveEval =
        new("OptExhaustiveEval", InnerEnum.OptExhaustiveEval, 2 | OptTrackState.Mask);

    /// <summary>
    ///     OptOptimize precomputes functions and operators with constants as arguments at program creation
    ///     time. This flag is useful when the expression will be evaluated repeatedly against a series of
    ///     different inputs.
    /// </summary>
    public static readonly EvalOption OptOptimize = new("OptOptimize", InnerEnum.OptOptimize, 4);

    /// <summary>
    ///     OptPartialEval enables the evaluation of a partial state where the input data that may be known
    ///     to be missing, either as top-level variables, or somewhere within a variable's object member
    ///     graph.
    ///     <para>
    ///         By itself, OptPartialEval does not change evaluation behavior unless the input to the
    ///         Program Eval is an PartialVars.
    ///     </para>
    /// </summary>
    public static readonly EvalOption
        OptPartialEval = new("OptPartialEval", InnerEnum.OptPartialEval, 8);

    private static readonly List<EvalOption> valueList = new();
    private static int nextOrdinal;

    public readonly InnerEnum innerEnumValue;

    private readonly string nameValue;
    private readonly int ordinalValue;

    static EvalOption()
    {
        valueList.Add(OptTrackState);
        valueList.Add(OptExhaustiveEval);
        valueList.Add(OptOptimize);
        valueList.Add(OptPartialEval);
    }

    internal EvalOption(string name, InnerEnum innerEnum, int mask)
    {
        Mask = mask;

        nameValue = name;
        ordinalValue = nextOrdinal++;
        innerEnumValue = innerEnum;
    }

    public int Mask { get; }

    public static EvalOption[] values()
    {
        return valueList.ToArray();
    }

    public int ordinal()
    {
        return ordinalValue;
    }

    public override string ToString()
    {
        return nameValue;
    }

    public static EvalOption valueOf(string name)
    {
        foreach (var enumInstance in valueList)
            if (enumInstance.nameValue == name)
                return enumInstance;

        throw new ArgumentException(name);
    }
}