using System;
using System.Numerics;
using NodaTime;

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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.maxUnixTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TimestampT.minUnixTime;


    public sealed class Overflow
    {
        public sealed class OverflowException : Exception
        {
            internal OverflowException() : base("overflow")
            {
            }
        }

        public static readonly OverflowException overflowException = new OverflowException();

        /// <summary>
        /// addInt64Checked performs addition with overflow detection of two int64, returning the result of
        /// the addition if no overflow occurred as the first return value and a bool indicating whether no
        /// overflow occurred as the second return value.
        /// </summary>
        public static long AddInt64Checked(long x, long y)
        {
            if ((y > 0 && x > long.MaxValue - y) || (y < 0 && x < long.MinValue - y))
            {
                throw overflowException;
            }

            return x + y;
        }

        /// <summary>
        /// subtractInt64Checked performs subtraction with overflow detection of two int64, returning the
        /// result of the subtraction if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static long SubtractInt64Checked(long x, long y)
        {
            if ((y < 0 && x > long.MaxValue + y) || (y > 0 && x < long.MinValue + y))
            {
                throw overflowException;
            }

            return x - y;
        }

        /// <summary>
        /// negateInt64Checked performs negation with overflow detection of an int64, returning the result
        /// of the negation if no overflow occurred as the first return value and a bool indicating whether
        /// no overflow occurred as the second return value.
        /// </summary>
        public static long NegateInt64Checked(long x)
        {
            // In twos complement, negating MinInt64 would result in a valid of MaxInt64+1.
            if (x == long.MinValue)
            {
                throw overflowException;
            }

            return -x;
        }

        /// <summary>
        /// multiplyInt64Checked performs multiplication with overflow detection of two int64, returning
        /// the result of the multiplication if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static long MultiplyInt64Checked(long x, long y)
        {
            // Detecting multiplication overflow is more complicated than the others. The first two detect
            // attempting to negate MinInt64, which would result in MaxInt64+1. The other four detect normal
            // overflow conditions.
            if ((x == -1 && y == long.MinValue) || (y == -1 && x == long.MinValue) ||
                (x > 0 && y > 0 && x > long.MaxValue / y) || (x > 0 && y < 0 && y < long.MinValue / x) ||
                (x < 0 && y > 0 && x < long.MinValue / y) || (x < 0 && y < 0 && y < long.MaxValue / x))
            {
                throw overflowException;
            }

            return x * y;
        }

        /// <summary>
        /// divideInt64Checked performs division with overflow detection of two int64, returning the result
        /// of the division if no overflow occurred as the first return value and a bool indicating whether
        /// no overflow occurred as the second return value.
        /// </summary>
        public static long DivideInt64Checked(long x, long y)
        {
            // In twos complement, negating MinInt64 would result in a valid of MaxInt64+1.
            if (x == long.MinValue && y == -1)
            {
                throw overflowException;
            }

            return x / y;
        }

        /// <summary>
        /// moduloInt64Checked performs modulo with overflow detection of two int64, returning the result
        /// of the modulo if no overflow occurred as the first return value and a bool indicating whether
        /// no overflow occurred as the second return value.
        /// </summary>
        public static long ModuloInt64Checked(long x, long y)
        {
            // In twos complement, negating MinInt64 would result in a valid of MaxInt64+1.
            if (x == long.MinValue && y == -1)
            {
                throw overflowException;
            }

            return x % y;
        }

        /// <summary>
        /// addUint64Checked performs addition with overflow detection of two uint64, returning the result
        /// of the addition if no overflow occurred as the first return value and a bool indicating whether
        /// no overflow occurred as the second return value.
        /// </summary>
        public static long AddUint64Checked(long x, long y)
        {
            // hopefully faster than using BigInteger...
            long xU = (long)((ulong)x >> 32);
            long xL = x & 0xffffffffL;
            long yU = (long)((ulong)y >> 32);
            long yL = y & 0xffffffffL;

            long rL = xL + yL;
            long rU = xU + yU;
            if (rL > 0xffffffffL)
            {
                // carry
                rU++;
            }

            if (rU > 0xffffffffL)
            {
                throw overflowException;
            }

            return rU << 32 | (rL & 0xffffffffL);
        }

        /// <summary>
        /// subtractUint64Checked performs subtraction with overflow detection of two uint64, returning the
        /// result of the subtraction if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static long SubtractUint64Checked(long x, long y)
        {
            // hopefully faster than using BigInteger...
            long xU = (long)((ulong)x >> 32);
            long xL = x & 0xffffffffL;
            long yU = (long)((ulong)y >> 32);
            long yL = y & 0xffffffffL;

            long rU = xU - yU;
            long rL = xL - yL;
            if (rL < 0L)
            {
                rU--;
            }

            if (rU < 0L)
            {
                throw overflowException;
            }

            return rU << 32 | (rL & 0xffffffffL);
        }

        /// <summary>
        /// multiplyUint64Checked performs multiplication with overflow detection of two uint64, returning
        /// the result of the multiplication if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static long MultiplyUint64Checked(long x, long y)
        {
            // Sloooow, but works.
            BigInteger r = x * y;
            if (r.GetBitLength() > 64)
            {
                throw overflowException;
            }

            return Convert.ToInt64(r);
        }

        /// <summary>
        /// addDurationChecked performs addition with overflow detection of two time.Duration, returning
        /// the result of the addition if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static Period AddDurationChecked(Period x, Period y)
        {
            try
            {
                return Period.Add(x, y);
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// subtractDurationChecked performs subtraction with overflow detection of two time.Duration,
        /// returning the result of the subtraction if no overflow occurred as the first return value and a
        /// bool indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static Period SubtractDurationChecked(Period x, Period y)
        {
            try
            {
                return Period.Subtract(x, y);
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// negateDurationChecked performs negation with overflow detection of a time.Duration, returning
        /// the result of the negation if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static Period NegateDurationChecked(Period x)
        {
            try
            {
                // TODO RAY check
                return Period.Subtract(Period.Zero, x);
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// addDurationChecked performs addition with overflow detection of a time.Time and time.Duration,
        /// returning the result of the addition if no overflow occurred as the first return value and a
        /// bool indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static ZonedDateTime AddTimeDurationChecked(ZonedDateTime x, Period y)
        {
            try
            {
                return CheckTimeOverflow(ZonedDateTime.Add(x, y.ToDuration()));
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// subtractTimeChecked performs subtraction with overflow detection of two time.Time, returning
        /// the result of the subtraction if no overflow occurred as the first return value and a bool
        /// indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static Period SubtractTimeChecked(ZonedDateTime x, ZonedDateTime y)
        {
            try
            {
                Period d = Period.FromSeconds(x.ToInstant().ToUnixTimeSeconds());
                d = Period.Add(d, Period.FromNanoseconds(x.NanosecondOfSecond));
                d = Period.Subtract(d, Period.FromSeconds(y.ToInstant().ToUnixTimeSeconds()));
                d = Period.Subtract(d, Period.FromNanoseconds(y.NanosecondOfSecond));
                return d;
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// subtractTimeDurationChecked performs subtraction with overflow detection of a time.Time and
        /// time.Duration, returning the result of the subtraction if no overflow occurred as the first
        /// return value and a bool indicating whether no overflow occurred as the second return value.
        /// </summary>
        public static ZonedDateTime SubtractTimeDurationChecked(ZonedDateTime x, Period y)
        {
            try
            {
                return CheckTimeOverflow(ZonedDateTime.Subtract(x, y.ToDuration()));
            }
            catch (ArithmeticException)
            {
                throw overflowException;
            }
        }

        /// <summary>
        /// Checks whether the given timestamp overflowed in the bounds of "Go", that is less than {@link
        /// TimestampT#minUnixTime} or greater than <seealso cref="TimestampT.maxUnixTime"/>.
        /// </summary>
        public static ZonedDateTime CheckTimeOverflow(ZonedDateTime x)
        {
            long s = x.ToInstant().ToUnixTimeSeconds();
            if (s < TimestampT.minUnixTime || s > TimestampT.maxUnixTime)
            {
                throw overflowException;
            }

            return x;
        }
    }
}