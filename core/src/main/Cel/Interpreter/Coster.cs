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

namespace Cel.Interpreter
{
    /// <summary>
    /// Coster calculates the heuristic cost incurred during evaluation. </summary>
    public interface Coster
    {
        Coster_Cost Cost();

        static Coster_Cost CostOf(long min, long max)
        {
            return new Coster_Cost(min, max);
        }
    }

    public sealed class Coster_Cost
    {
        public static readonly Coster_Cost Unknown = Coster.CostOf(0, long.MaxValue);
        public static readonly Coster_Cost None = Coster.CostOf(0, 0);
        public static readonly Coster_Cost OneOne = Coster.CostOf(1, 1);
        public readonly long min;
        public readonly long max;

        internal Coster_Cost(long min, long max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// estimateCost returns the heuristic cost interval for the program. </summary>
        public static Coster_Cost EstimateCost(object i)
        {
            if (i is Coster)
            {
                return ((Coster)i).Cost();
            }

            return Unknown;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }

            Coster_Cost cost = (Coster_Cost)o;
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

        public Coster_Cost Add(Coster_Cost c)
        {
            return new Coster_Cost(min + c.min, max + c.max);
        }

        public Coster_Cost Multiply(long multiplier)
        {
            return new Coster_Cost(min * multiplier, max * multiplier);
        }
    }
}