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

namespace Cel.Interpreter;

/// <summary>
///     ICoster calculates the heuristic cost incurred during evaluation.
/// </summary>
public interface ICoster
{
    Cost Cost();
}

public sealed class Cost
{
    public static readonly Cost Unknown = Of(0, long.MaxValue);
    public static readonly Cost None = Of(0, 0);
    public static readonly Cost OneOne = Of(1, 1);

    internal Cost(long min, long max)
    {
        Min = min;
        Max = max;
    }

    public long Max { get; }

    public long Min { get; }

    /// <summary>
    ///     estimateCost returns the heuristic cost interval for the program.
    /// </summary>
    public static Cost EstimateCost(object i)
    {
        if (i is ICoster) return ((ICoster)i).Cost();

        return Unknown;
    }

    public static Cost Of(long min, long max)
    {
        return new Cost(min, max);
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var cost = (Cost)o;
        return Min == cost.Min && Max == cost.Max;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }

    public override string ToString()
    {
        return "Cost{" + "min=" + Min + ", max=" + Max + '}';
    }

    public Cost Add(Cost c)
    {
        return new Cost(Min + c.Min, Max + c.Max);
    }

    public Cost Multiply(long multiplier)
    {
        return new Cost(Min * multiplier, Max * multiplier);
    }
}