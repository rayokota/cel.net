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
///     Coster calculates the heuristic cost incurred during evaluation.
/// </summary>
public interface Coster
{
    Cost Cost();

    static Cost CostOf(long min, long max)
    {
        return new Cost(min, max);
    }
}

public sealed class Cost
{
    public static readonly Cost Unknown = Coster.CostOf(0, long.MaxValue);
    public static readonly Cost None = Coster.CostOf(0, 0);
    public static readonly Cost OneOne = Coster.CostOf(1, 1);
    public readonly long max;
    public readonly long min;

    internal Cost(long min, long max)
    {
        this.min = min;
        this.max = max;
    }

    /// <summary>
    ///     estimateCost returns the heuristic cost interval for the program.
    /// </summary>
    public static Cost EstimateCost(object i)
    {
        if (i is Coster) return ((Coster)i).Cost();

        return Unknown;
    }

    public override bool Equals(object o)
    {
        if (this == o) return true;

        if (o == null || GetType() != o.GetType()) return false;

        var cost = (Cost)o;
        return min == cost.min && max == cost.max;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(min, max);
    }

    public override string ToString()
    {
        return "Cost{" + "min=" + min + ", max=" + max + '}';
    }

    public Cost Add(Cost c)
    {
        return new Cost(min + c.min, max + c.max);
    }

    public Cost Multiply(long multiplier)
    {
        return new Cost(min * multiplier, max * multiplier);
    }
}