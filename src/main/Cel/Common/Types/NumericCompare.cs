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

namespace Cel.Common.Types;

/// <summary>
///     Ordering across the three numeric types, ported from cel-go's
///     common/types/compare.go so that cross-type comparisons answer the same way here as
///     they do there.
///     <para>
///         The integer pairing is decided exactly. A negative long is below every ulong and
///         a ulong above <see cref="long.MaxValue" /> is above every long, so ruling those
///         out leaves two values that both fit in long, where the cast cannot wrap.
///     </para>
///     <para>
///         The pairings involving double range-check the double against the integer type's
///         bounds first — outside them the answer is settled, and the conversion would be
///         undefined anyway — and then compare as doubles, exactly as cel-go does. That last
///         step is not exact above 2^53, where a long has more precision than a double can
///         carry; matching cel-go matters more here than being more precise than it.
///     </para>
/// </summary>
internal static class NumericCompare
{
    /// <summary>
    ///     ulong.MaxValue is not representable as a double; this is the double it rounds to,
    ///     which is 2^64. cel-go compares against math.MaxUint64 and lands on the same value.
    /// </summary>
    private const double MaxULongAsDouble = 18446744073709551615.0;

    internal static int CompareLongDouble(long i, double d)
    {
        return -CompareDoubleLong(d, i);
    }

    internal static int CompareDoubleLong(double d, long i)
    {
        if (d < long.MinValue) return -1;
        if (d > long.MaxValue) return 1;
        return CompareDoubleDouble(d, i);
    }

    internal static int CompareULongDouble(ulong u, double d)
    {
        return -CompareDoubleULong(d, u);
    }

    internal static int CompareDoubleULong(double d, ulong u)
    {
        if (d < 0) return -1;
        if (d > MaxULongAsDouble) return 1;
        return CompareDoubleDouble(d, u);
    }

    internal static int CompareLongULong(long i, ulong u)
    {
        if (i < 0 || u > long.MaxValue) return -1;
        return CompareLongLong(i, (long)u);
    }

    internal static int CompareULongLong(ulong u, long i)
    {
        return -CompareLongULong(i, u);
    }

    internal static int CompareLongLong(long a, long b)
    {
        return a < b ? -1 : a > b ? 1 : 0;
    }

    internal static int CompareULongULong(ulong a, ulong b)
    {
        return a < b ? -1 : a > b ? 1 : 0;
    }

    internal static int CompareDoubleDouble(double a, double b)
    {
        return a < b ? -1 : a > b ? 1 : 0;
    }
}
