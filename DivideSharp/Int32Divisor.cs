using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="int"/> value RAPIDLY.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct Int32Divisor : IDivisor<int>, IEquatable<Int32Divisor>
    {
        #region Static Members

        private static ReadOnlySpan<Int32Divisor> PositiveDivisors => unchecked(new Int32Divisor[]
        {
            new Int32Divisor(3, 0x55555556, Int32DivisorStrategy.MultiplyShift, 0),
            new Int32Divisor(4, 1, Int32DivisorStrategy.PowerOfTwoPositive, 2),
            new Int32Divisor(5, 0x66666667, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(6, 0x2aaaaaab, Int32DivisorStrategy.MultiplyShift, 0),
            new Int32Divisor(7, (int)0x92492493, Int32DivisorStrategy.MultiplyAddShift, 2),
            new Int32Divisor(8, 1, Int32DivisorStrategy.PowerOfTwoPositive, 3),
            new Int32Divisor(9, 0x38e38e39, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(10, 0x66666667, Int32DivisorStrategy.MultiplyShift, 2),
            new Int32Divisor(11, 0x2e8ba2e9, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(12, 0x2aaaaaab, Int32DivisorStrategy.MultiplyShift, 1),
        });

        private static ReadOnlySpan<Int32Divisor> NegativeDivisors => unchecked(new Int32Divisor[]
        {
            new Int32Divisor(-3, 0x55555555, Int32DivisorStrategy.MultiplySubtractShift, 1),
            new Int32Divisor(-4, 1, Int32DivisorStrategy.PowerOfTwoNegative, 2),
            new Int32Divisor(-5, (int)0x99999999, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(-6, (int)0xd5555555, Int32DivisorStrategy.MultiplyShift, 0),
            new Int32Divisor(-7, 0x6db6db6d, Int32DivisorStrategy.MultiplySubtractShift, 2),
            new Int32Divisor(-8, 1, Int32DivisorStrategy.PowerOfTwoNegative, 3),
            new Int32Divisor(-9, (int)0xc71c71c7, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(-10, (int)0x99999999, Int32DivisorStrategy.MultiplyShift, 2),
            new Int32Divisor(-11, (int)0xd1745d17, Int32DivisorStrategy.MultiplyShift, 1),
            new Int32Divisor(-12, (int)0xd5555555, Int32DivisorStrategy.MultiplyShift, 1),
        });

        private static (int multiplier, Int32DivisorStrategy strategy, byte shift) GetMagic(int divisor)
        {
            //Copied from CoreCLR, and modified by MineCake1.4.7

            #region License Notice

            /*
             The MIT License (MIT)
            Copyright (c) .NET Foundation and Contributors
            All rights reserved.
            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the "Software"), to deal
            in the Software without restriction, including without limitation the rights
            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
            copies of the Software, and to permit persons to whom the Software is
            furnished to do so, subject to the following conditions:
            The above copyright notice and this permission notice shall be included in all
            copies or substantial portions of the Software.
            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
            SOFTWARE.
             */

            #endregion License Notice

            if (divisor > 0)
            {
                if (divisor - 3 < PositiveDivisors.Length)
                {
                    var q = PositiveDivisors[divisor - 3];
                    return (q.Multiplier, q.Strategy, q.Shift);
                }
            }
            else
            {
                if (-divisor - 3 < NegativeDivisors.Length)
                {
                    var q = NegativeDivisors[-divisor - 3];
                    return (q.Multiplier, q.Strategy, q.Shift);
                }
            }

            const int Bits = sizeof(int) * 8;
            const int BitsMinus1 = Bits - 1;
            const uint TwoNMinus1 = 1u << BitsMinus1;

            int p;
            uint absDivisor;
            uint absNc;
            uint delta;
            uint q1;
            uint r1;
            uint r2;
            uint q2;
            uint t;
            int resultMagic;

            absDivisor = Utils.Abs(divisor);
            t = TwoNMinus1 + ((uint)(divisor) >> BitsMinus1);
            absNc = t - 1 - (t % absDivisor); // absolute value of nc
            p = BitsMinus1; // initialize p
            q1 = TwoNMinus1 / absNc; // initialize q1 = 2^p / abs(nc)
            r1 = TwoNMinus1 - (q1 * absNc); // initialize r1 = rem(2^p, abs(nc))
            q2 = TwoNMinus1 / absDivisor; // initialize q1 = 2^p / abs(divisor)
            r2 = TwoNMinus1 - (q2 * absDivisor); // initialize r1 = rem(2^p, abs(divisor))

            do
            {
                p++;
                q1 *= 2; // update q1 = 2^p / abs(nc)
                r1 *= 2; // update r1 = rem(2^p / abs(nc))

                if (r1 >= absNc)
                { // must be unsigned comparison
                    q1++;
                    r1 -= absNc;
                }

                q2 *= 2; // update q2 = 2^p / abs(divisor)
                r2 *= 2; // update r2 = rem(2^p / abs(divisor))

                if (r2 >= absDivisor)
                { // must be unsigned comparison
                    q2++;
                    r2 -= absDivisor;
                }

                delta = absDivisor - r2;
            } while (q1 < delta || (q1 == delta && r1 == 0));

            resultMagic = (int)(q2 + 1); // resulting magic number
            if (divisor < 0)
            {
                resultMagic = -resultMagic;
            }
            byte shift = (byte)(p - Bits); // resulting shift
            return Math.Sign(resultMagic) != Math.Sign(divisor)
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
#pragma warning disable S3358 // Ternary operators should not be nested
                ? (resultMagic, divisor > 0 ? Int32DivisorStrategy.MultiplyAddShift : Int32DivisorStrategy.MultiplySubtractShift, shift)
#pragma warning restore S3358 // Ternary operators should not be nested
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
                : (resultMagic, Int32DivisorStrategy.MultiplyShift, shift);
        }

        #endregion Static Members

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public int Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public int Multiplier { get; }

        /// <summary>
        /// Gets the strategy of a division.
        /// </summary>
        /// <value>
        /// The strategy of a division.
        /// </value>
        public Int32DivisorStrategy Strategy { get; }

        /// <summary>
        /// Gets the number of bits to shift for actual "division".
        /// </summary>
        /// <value>
        /// The number of bits to shift right.
        /// </value>
        public byte Shift { get; }

        /// <summary>
        /// Gets the mask to divide.
        /// </summary>
        /// <value>
        /// The mask.
        /// </value>
        public int Mask { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private Int32Divisor(int divisor, int multiplier, Int32DivisorStrategy strategy, byte shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
            Mask = (int)~(~0u << shift);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public Int32Divisor(int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = Int32DivisorStrategy.PowerOfTwoPositive;
                Shift = 0;
                Mask = 0;
            }
            else if (divisor == -1)
            {
                Multiplier = -1;
                Strategy = Int32DivisorStrategy.PowerOfTwoNegative;
                Shift = 0;
                Mask = 0;
            }
            else if (divisor == int.MinValue)
            {
                Multiplier = 1;
                Strategy = Int32DivisorStrategy.Branch;
                Shift = 0;
                Mask = 0;
            }
            else if ((-divisor & (-divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = Int32DivisorStrategy.PowerOfTwoNegative;
                Shift = (byte)Utils.CountConsecutiveZeros((uint)divisor);
                Mask = (int)~(~0u << Shift);
            }
            else if ((divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = Int32DivisorStrategy.PowerOfTwoPositive;
                Shift = (byte)Utils.CountConsecutiveZeros((uint)divisor);
                Mask = (int)~(~0u << Shift);
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
                Mask = (int)~(~0u << Shift);
            }
        }

        /// <summary>
        /// Divides the specified <paramref name="value"/> by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Divide(int value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)Int32DivisorStrategy.Branch)
            {
                bool al = value == int.MinValue;
                return Unsafe.As<bool, byte>(ref al);
            }
            int eax;
            uint r9d;
            if ((strategy & 0b100u) > 0)
            {
                long rax = value;
                long multiplier = Multiplier;
                int shift = Shift;
                rax *= multiplier;
                eax = (int)((ulong)rax >> 32);
                if ((strategy & 0b001u) > 0)
                {
                    eax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    eax += value;
                }
                r9d = (uint)eax;
                r9d >>= 31;
                eax >>= shift;
                return eax + (int)r9d;
            }
            else
            {
                eax = value >> 31;
                int mask = Mask;
                int shift = Shift;
                strategy &= 0b1u;
                eax &= mask;
                eax += value;
                eax >>= shift;
                return ((strategy & 0b1u) > 0) ? -eax : eax;
            }
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DivRem(int value, out int quotient)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)Int32DivisorStrategy.None)
            {
                quotient = value;
                return 0;
            }
            else if (strategy == (uint)Int32DivisorStrategy.Negate)
            {
                quotient = -value;
                return 0;
            }
            else if (strategy == (uint)Int32DivisorStrategy.Branch)
            {
                bool al = value == int.MinValue;
                int q = quotient = Unsafe.As<bool, byte>(ref al);
                return value & --q;
            }
            int eax;
            uint edx;
            if ((strategy & 0b100u) > 0)
            {
                long rax = value;
                long multiplier = Multiplier;
                int divisor = Divisor;
                int shift = Shift;
                rax *= multiplier;
                eax = (int)((ulong)rax >> 32);
                if ((strategy & 0b001u) > 0)
                {
                    eax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    eax += value;
                }
                edx = (uint)eax;
                edx >>= 31;
                eax >>= shift;
                int r11d = quotient = eax + (int)edx;
                return value - r11d * divisor;
            }
            else
            {
                eax = value >> 31;
                int mask = Mask;
                int shift = Shift;
                eax &= mask;
                eax += value;
                int r11d = eax >> shift;
                mask = ~mask;
                quotient = ((strategy & 0b1u) > 0) ? -r11d : r11d;
                mask &= eax;
                return value - mask;
            }
        }

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Floor(int value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)Int32DivisorStrategy.None || strategy == (uint)Int32DivisorStrategy.Negate)
            {
                return value;
            }
            else if (strategy == (uint)Int32DivisorStrategy.Branch)
            {
                bool al = value == int.MinValue;
                return Unsafe.As<bool, byte>(ref al) << 31;
            }
            int eax;
            uint r9d;
            if ((strategy & 0b100u) > 0)
            {
                long rax = value;
                int divisor = Divisor;
                long multiplier = Multiplier;
                int shift = Shift;
                rax *= multiplier;
                eax = (int)((ulong)rax >> 32);
                if ((strategy & 0b001u) > 0)
                {
                    eax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    eax += value;
                }
                r9d = (uint)eax;
                r9d >>= 31;
                eax >>= shift;
                eax += (int)r9d;
                return eax * divisor;
            }
            else
            {
                eax = value >> 31;
                int mask = Mask;
                eax &= mask;
                eax += value;
                mask = ~mask;
                mask &= eax;
                return mask;
            }
        }

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FloorRem(int value, out int largestMultipleOfDivisor)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)Int32DivisorStrategy.None || strategy == (uint)Int32DivisorStrategy.Negate)
            {
                largestMultipleOfDivisor = value;
                return 0;
            }
            else if (strategy == (uint)Int32DivisorStrategy.Branch)
            {
                bool al = value == int.MinValue;
                int q = Unsafe.As<bool, byte>(ref al);
                largestMultipleOfDivisor = q << 31;
                return value & --q;
            }
            int eax;
            uint r9d;
            if ((strategy & 0b100u) > 0)
            {
                long rax = value;
                int divisor = Divisor;
                long multiplier = Multiplier;
                int shift = Shift;
                rax *= multiplier;
                eax = (int)((ulong)rax >> 32);
                if ((strategy & 0b001u) > 0)
                {
                    eax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    eax += value;
                }
                r9d = (uint)eax;
                r9d >>= 31;
                eax >>= shift;
                eax += (int)r9d;
                var f = largestMultipleOfDivisor = eax * divisor;
                return value - f;
            }
            else
            {
                eax = value >> 31;
                int mask = Mask;
                eax &= mask;
                eax += value;
                mask = ~mask;
                mask &= eax;
                largestMultipleOfDivisor = mask;
                return value - mask;
            }
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Modulo(int value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)Int32DivisorStrategy.None || strategy == (uint)Int32DivisorStrategy.Negate)
            {
                return 0;
            }
            else if (strategy == (uint)Int32DivisorStrategy.Branch)
            {
                bool al = value != int.MinValue;
                return value & -Unsafe.As<bool, byte>(ref al);
            }
            int eax;
            uint edx;
            if ((strategy & 0b100u) > 0)
            {
                long rax = value;
                long multiplier = Multiplier;
                int divisor = Divisor;
                int shift = Shift;
                rax *= multiplier;
                eax = (int)((ulong)rax >> 32);
                if ((strategy & 0b001u) > 0)
                {
                    eax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    eax += value;
                }
                edx = (uint)eax;
                edx >>= 31;
                eax >>= shift;
                int r11d = eax + (int)edx;
                return value - r11d * divisor;
            }
            else
            {
                eax = value >> 31;
                int mask = Mask;
                eax &= mask;
                eax += value;
                mask = ~mask;
                mask &= eax;
                return value - mask;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Int32Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Int32Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift && Mask == other.Mask;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 1864125951;
            hashCode = hashCode * -1521134295 + Divisor.GetHashCode();
            hashCode = hashCode * -1521134295 + Multiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + Strategy.GetHashCode();
            hashCode = hashCode * -1521134295 + Shift.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask.GetHashCode();
            return hashCode;
        }

        #region Operators

        /// <summary>
        /// Rapidly divides the specified <see cref="int"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(int left, Int32Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="int"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(int left, Int32Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int32Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int32Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="Int32Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Int32Divisor left, Int32Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int32Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int32Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="Int32Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Int32Divisor left, Int32Divisor right) => !(left == right);

        #endregion Operators
    }
}
