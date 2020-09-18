using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="long"/> value RAPIDLY.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct Int64Divisor : ISignedDivisor<long>, IEquatable<Int64Divisor>
    {
        #region Static Members

        private static Int64Divisor[] PositiveDivisors { get; } = unchecked(new Int64Divisor[]
        {
            new Int64Divisor(3, 0x5555555555555556, SignedDivisorStrategy.MultiplyShift, 0),
                new Int64Divisor(4, 1, SignedDivisorStrategy.PowerOfTwoPositive, 2),
                new Int64Divisor(5, 0x6666666666666667, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(6, 0x2aaaaaaaaaaaaaab, SignedDivisorStrategy.MultiplyShift, 0),
                new Int64Divisor(7, 0x4924924924924925, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(8, 1, SignedDivisorStrategy.PowerOfTwoPositive, 3),
                new Int64Divisor(9, 0x1c71c71c71c71c72, SignedDivisorStrategy.MultiplyShift, 0),
                new Int64Divisor(10, 0x6666666666666667, SignedDivisorStrategy.MultiplyShift, 2),
                new Int64Divisor(11, 0x2e8ba2e8ba2e8ba3, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(12, 0x2aaaaaaaaaaaaaab, SignedDivisorStrategy.MultiplyShift, 1),
        });

        private static Int64Divisor[] NegativeDivisors { get; } = unchecked(new Int64Divisor[]
        {
            new Int64Divisor(-3, 0x5555555555555555, SignedDivisorStrategy.MultiplySubtractShift, 1),
                new Int64Divisor(-4, 1, SignedDivisorStrategy.PowerOfTwoNegative, 2),
                new Int64Divisor(-5, (long)0x9999999999999999, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(-6, (long)0xd555555555555555, SignedDivisorStrategy.MultiplyShift, 0),
                new Int64Divisor(-7, (long)0xb6db6db6db6db6db, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(-8, 1, SignedDivisorStrategy.PowerOfTwoNegative, 3),
                new Int64Divisor(-9, 0x1c71c71c71c71c71, SignedDivisorStrategy.MultiplySubtractShift, 3),
                new Int64Divisor(-10, (long)0x9999999999999999, SignedDivisorStrategy.MultiplyShift, 2),
                new Int64Divisor(-11, (long)0xd1745d1745d1745d, SignedDivisorStrategy.MultiplyShift, 1),
                new Int64Divisor(-12, (long)0xd555555555555555, SignedDivisorStrategy.MultiplyShift, 1),
        });

        private static (long multiplier, SignedDivisorStrategy strategy, byte shift) GetMagic(long divisor)
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
                    var q = PositiveDivisors[(int)(divisor - 3)];
                    return (q.Multiplier, q.Strategy, q.Shift);
                }
            }
            else
            {
                if (-divisor - 3 < NegativeDivisors.Length)
                {
                    var q = NegativeDivisors[(int)(-divisor - 3)];
                    return (q.Multiplier, q.Strategy, q.Shift);
                }
            }

            const int Bits = sizeof(long) * 8;
            const int BitsMinus1 = Bits - 1;
            const ulong TwoNMinus1 = 1ul << BitsMinus1;

            int p;
            ulong absDivisor;
            ulong absNc;
            ulong delta;
            ulong q1;
            ulong r1;
            ulong r2;
            ulong q2;
            ulong t;
            long resultMagic;

            absDivisor = Utils.Abs(divisor);
            t = TwoNMinus1 + ((ulong)divisor >> BitsMinus1);
            absNc = t - 1 - t % absDivisor; // absolute value of nc
            p = BitsMinus1; // initialize p
            q1 = TwoNMinus1 / absNc; // initialize q1 = 2^p / abs(nc)
            r1 = TwoNMinus1 - q1 * absNc; // initialize r1 = rem(2^p, abs(nc))
            q2 = TwoNMinus1 / absDivisor; // initialize q1 = 2^p / abs(divisor)
            r2 = TwoNMinus1 - q2 * absDivisor; // initialize r1 = rem(2^p, abs(divisor))

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
            } while (q1 < delta || q1 == delta && r1 == 0);

            resultMagic = (long)(q2 + 1); // resulting magic number
            if (divisor < 0)
            {
                resultMagic = -resultMagic;
            }
            byte shift = (byte)(p - Bits); // resulting shift
            return Math.Sign(resultMagic) != Math.Sign(divisor)
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
#pragma warning disable S3358 // Ternary operators should not be nested
                ?
                (resultMagic, divisor > 0 ? SignedDivisorStrategy.MultiplyAddShift : SignedDivisorStrategy.MultiplySubtractShift, shift)
#pragma warning restore S3358 // Ternary operators should not be nested
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
                :
                (resultMagic, SignedDivisorStrategy.MultiplyShift, shift);
        }

        #endregion Static Members

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public long Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public long Multiplier { get; }

        /// <summary>
        /// Gets the strategy of a division.
        /// </summary>
        /// <value>
        /// The strategy of a division.
        /// </value>
        public SignedDivisorStrategy Strategy { get; }

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
        public long Mask { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int64Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private Int64Divisor(long divisor, long multiplier, SignedDivisorStrategy strategy, byte shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
            Mask = (long)~(~0u << shift);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int64Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public Int64Divisor(long divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = SignedDivisorStrategy.PowerOfTwoPositive;
                Shift = 0;
                Mask = 0;
            }
            else if (divisor == -1)
            {
                Multiplier = -1;
                Strategy = SignedDivisorStrategy.PowerOfTwoNegative;
                Shift = 0;
                Mask = 0;
            }
            else if (divisor == long.MinValue)
            {
                Multiplier = 1;
                Strategy = SignedDivisorStrategy.Branch;
                Shift = 0;
                Mask = 0;
            }
            else if ((-divisor & (-divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = SignedDivisorStrategy.PowerOfTwoNegative;
                Shift = (byte)Utils.CountConsecutiveZeros((ulong)divisor);
                Mask = (long)~(~0u << Shift);
            }
            else if ((divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = SignedDivisorStrategy.PowerOfTwoPositive;
                Shift = (byte)Utils.CountConsecutiveZeros((ulong)divisor);
                Mask = (long)~(~0u << Shift);
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
                Mask = (long)~(~0u << Shift);
            }
        }

        /// <summary>
        /// Divides the specified <paramref name="value"/> by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Divide(long value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == long.MinValue;
                return Unsafe.As<bool, byte>(ref al);
            }
            long rax = value;
            ulong r9;
            if ((strategy & 0b100u) > 0)
            {
                long multiplier = Multiplier;
                int shift = Shift;
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b001u) > 0)
                {
                    rax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    rax += value;
                }
                r9 = (ulong)rax;
                r9 >>= 63;
                rax >>= shift;
                return rax + (long)r9;
            }
            else
            {
                rax >>= 63;
                long mask = Mask;
                int shift = Shift;
                strategy &= 0b1u;
                rax &= mask;
                rax += value;
                rax >>= shift;
                return ((strategy & 0b1u) > 0) ? -rax : rax;
            }
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DivRem(long value, out long quotient)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == long.MinValue;
                long q = quotient = Unsafe.As<bool, byte>(ref al);
                return value & --q;
            }
            long rax = value;
            ulong rdx;
            if ((strategy & 0b100u) > 0)
            {
                long multiplier = Multiplier;
                long divisor = Divisor;
                int shift = Shift;
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b001u) > 0)
                {
                    rax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    rax += value;
                }
                rdx = (ulong)rax;
                rdx >>= 63;
                rax >>= shift;
                long r11 = quotient = rax + (long)rdx;
                return value - r11 * divisor;
            }
            else
            {
                rax >>= 63;
                long mask = Mask;
                int shift = Shift;
                rax &= mask;
                rax += value;
                long r11 = rax >> shift;
                mask = ~mask;
                quotient = ((strategy & 0b1u) > 0) ? -r11 : r11;
                mask &= rax;
                return value - mask;
            }
        }

        /// <summary>
        /// Returns a multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>A multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long AbsFloor(long value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == long.MinValue;
                return (long)Unsafe.As<bool, byte>(ref al) << 63;
            }
            long rax = value;
            ulong r9;
            if ((strategy & 0b100u) > 0)
            {
                long divisor = Divisor;
                long multiplier = Multiplier;
                int shift = Shift;
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b001u) > 0)
                {
                    rax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    rax += value;
                }
                r9 = (ulong)rax;
                r9 >>= 63;
                rax >>= shift;
                rax += (long)r9;
                return rax * divisor;
            }
            else
            {
                rax >>= 63;
                long mask = Mask;
                rax &= mask;
                rax += value;
                mask = ~mask;
                mask &= rax;
                return mask;
            }
        }

        /// <summary>
        /// Calculates a multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/> and stores it in <paramref name="largestMultipleOfDivisor"/>, and Returns the difference between <paramref name="largestMultipleOfDivisor"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">A multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.</param>
        /// <returns>The difference between <paramref name="largestMultipleOfDivisor"/> and <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long AbsFloorRem(long value, out long largestMultipleOfDivisor)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == long.MinValue;
                long q = Unsafe.As<bool, byte>(ref al);
                largestMultipleOfDivisor = q << 63;
                return value & --q;
            }
            long rax = value;
            ulong r9;
            if ((strategy & 0b100u) > 0)
            {
                long divisor = Divisor;
                long multiplier = Multiplier;
                int shift = Shift;
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b001u) > 0)
                {
                    rax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    rax += value;
                }
                r9 = (ulong)rax;
                r9 >>= 63;
                rax >>= shift;
                rax += (long)r9;
                var f = largestMultipleOfDivisor = rax * divisor;
                return value - f;
            }
            else
            {
                rax >>= 63;
                long mask = Mask;
                rax &= mask;
                rax += value;
                mask = ~mask;
                mask &= rax;
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
        public long Modulo(long value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value != long.MinValue;
                return value & -Unsafe.As<bool, byte>(ref al);
            }
            long rax = value;
            ulong rdx;
            if ((strategy & 0b100u) > 0)
            {
                long multiplier = Multiplier;
                long divisor = Divisor;
                int shift = Shift;
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b001u) > 0)
                {
                    rax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    rax += value;
                }
                rdx = (ulong)rax;
                rdx >>= 63;
                rax >>= shift;
                long r11 = rax + (long)rdx;
                return value - r11 * divisor;
            }
            else
            {
                rax >>= 63;
                long mask = Mask;
                rax &= mask;
                rax += value;
                mask = ~mask;
                mask &= rax;
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
        public override bool Equals(object obj) => obj is Int64Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Int64Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift && Mask == other.Mask;

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
        /// Rapidly divides the specified <see cref="long"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long operator /(long left, Int64Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="long"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long operator %(long left, Int64Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int64Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int64Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="Int64Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Int64Divisor left, Int64Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int64Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int64Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="Int64Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Int64Divisor left, Int64Divisor right) => !(left == right);

        #endregion Operators
    }
}
