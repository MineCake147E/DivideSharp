using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="short"/> value RAPIDLY.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct Int16Divisor : ISignedDivisor<short>, IEquatable<Int16Divisor>
    {
        #region Static Members

        private static Int16Divisor[] PositiveDivisors { get; } = unchecked(new Int16Divisor[]
        {
            new Int16Divisor(3, 21846, SignedDivisorStrategy.MultiplyShift, 0),
            new Int16Divisor(4, 1, SignedDivisorStrategy.PowerOfTwoPositive, 2),
            new Int16Divisor(5, 26215, SignedDivisorStrategy.MultiplyShift, 1),
            new Int16Divisor(6, 10923, SignedDivisorStrategy.MultiplyShift, 0),
            new Int16Divisor(7, 18725, SignedDivisorStrategy.MultiplyShift, 1),
            new Int16Divisor(8, 1, SignedDivisorStrategy.PowerOfTwoPositive, 3),
            new Int16Divisor(9, 7282, SignedDivisorStrategy.MultiplyShift, 0),
            new Int16Divisor(10, 26215, SignedDivisorStrategy.MultiplyShift, 2),
            new Int16Divisor(11, 5958, SignedDivisorStrategy.MultiplyShift, 0),
            new Int16Divisor(12, 10923, SignedDivisorStrategy.MultiplyShift, 1),
        });

        private static Int16Divisor[] NegativeDivisors { get; } = unchecked(new Int16Divisor[]
        {
            new Int16Divisor(-3, 21845, SignedDivisorStrategy.MultiplySubtractShift, 1),
            new Int16Divisor(-4, 1, SignedDivisorStrategy.PowerOfTwoNegative, 2),
            new Int16Divisor(-5, -26215, SignedDivisorStrategy.MultiplyShift, 1),
            new Int16Divisor(-6, -10923, SignedDivisorStrategy.MultiplyShift, 0),
            new Int16Divisor(-7, -18725, SignedDivisorStrategy.MultiplyShift, 1),
            new Int16Divisor(-8, 1, SignedDivisorStrategy.PowerOfTwoNegative, 3),
            new Int16Divisor(-9, 7281, SignedDivisorStrategy.MultiplySubtractShift, 3),
            new Int16Divisor(-10, -26215, SignedDivisorStrategy.MultiplyShift, 2),
            new Int16Divisor(-11, 17873, SignedDivisorStrategy.MultiplySubtractShift, 3),
            new Int16Divisor(-12, -10923, SignedDivisorStrategy.MultiplyShift, 1),
        });

        private static (short multiplier, SignedDivisorStrategy strategy, byte shift) GetMagic(short divisor)
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

            const int Bits = sizeof(short) * 8;
            const int BitsMinus1 = Bits - 1;
            const ushort TwoNMinus1 = (ushort)(1u << BitsMinus1);

            short p;
            ushort absDivisor;
            ushort absNc;
            ushort delta;
            ushort q1;
            ushort r1;
            ushort r2;
            ushort q2;
            ushort t;
            short resultMagic;

            absDivisor = Utils.Abs(divisor);
            t = (ushort)(TwoNMinus1 + ((ushort)divisor >> BitsMinus1));
            absNc = (ushort)(t - 1 - t % absDivisor); // absolute value of nc
            p = BitsMinus1; // initialize p
            q1 = (ushort)(TwoNMinus1 / absNc); // initialize q1 = 2^p / abs(nc)
            r1 = (ushort)(TwoNMinus1 - q1 * absNc); // initialize r1 = rem(2^p, abs(nc))
            q2 = (ushort)(TwoNMinus1 / absDivisor); // initialize q1 = 2^p / abs(divisor)
            r2 = (ushort)(TwoNMinus1 - q2 * absDivisor); // initialize r1 = rem(2^p, abs(divisor))

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

                delta = (ushort)(absDivisor - r2);
            } while (q1 < delta || q1 == delta && r1 == 0);

            resultMagic = (short)(q2 + 1); // resulting magic number
            if (divisor < 0)
            {
                resultMagic = (short)-resultMagic;
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
        public short Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public short Multiplier { get; }

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
        public short Mask { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int16Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private Int16Divisor(short divisor, short multiplier, SignedDivisorStrategy strategy, byte shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
            Mask = (short)~(~0u << shift);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int16Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public Int16Divisor(short divisor)
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
            else if (divisor == short.MinValue)
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
                Shift = (byte)Utils.CountConsecutiveZeros((ushort)divisor);
                Mask = (short)~(~0u << Shift);
            }
            else if ((divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = SignedDivisorStrategy.PowerOfTwoPositive;
                Shift = (byte)Utils.CountConsecutiveZeros((ushort)divisor);
                Mask = (short)~(~0u << Shift);
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
                Mask = (short)~(~0u << Shift);
            }
        }

        /// <summary>
        /// Divides the specified <paramref name="value"/> by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Divide(short value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == short.MinValue;
                return Unsafe.As<bool, byte>(ref al);
            }
            short ax;
            ushort r9w;
            if ((strategy & 0b100u) > 0)
            {
                int eax = value;
                int multiplier = Multiplier;
                int shift = Shift;
                eax *= multiplier;
                ax = (short)((uint)eax >> 16);
                if ((strategy & 0b001u) > 0)
                {
                    ax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    ax += value;
                }
                r9w = (ushort)ax;
                r9w >>= 15;
                ax >>= shift;
                return (short)(ax + (short)r9w);
            }
            else
            {
                ax = (short)(value >> 15);
                short mask = Mask;
                int shift = Shift;
                strategy &= 0b1u;
                ax &= mask;
                ax += value;
                ax >>= shift;
                return (short)(((strategy & 0b1u) > 0) ? -ax : ax);
            }
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short DivRem(short value, out short quotient)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == short.MinValue;
                short q = quotient = Unsafe.As<bool, byte>(ref al);
                return (short)(value & --q);
            }
            short ax;
            ushort edx;
            if ((strategy & 0b100u) > 0)
            {
                int eax = value;
                int multiplier = Multiplier;
                short divisor = Divisor;
                int shift = Shift;
                eax *= multiplier;
                ax = (short)((uint)eax >> 16);
                if ((strategy & 0b001u) > 0)
                {
                    ax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    ax += value;
                }
                edx = (ushort)ax;
                edx >>= 15;
                ax >>= shift;
                short r11w = quotient = (short)(ax + (short)edx);
                return (short)(value - r11w * divisor);
            }
            else
            {
                ax = (short)(value >> 15);
                short mask = Mask;
                int shift = Shift;
                ax &= mask;
                ax += value;
                short r11w = (short)(ax >> shift);
                mask = (short)~mask;
                quotient = (short)(((strategy & 0b1u) > 0) ? -r11w : r11w);
                mask &= ax;
                return (short)(value - mask);
            }
        }

        /// <summary>
        /// Returns a multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>A multiple of <see cref="Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short AbsFloor(short value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == short.MinValue;
                return (short)(Unsafe.As<bool, byte>(ref al) << 15);
            }
            short ax;
            ushort r9w;
            if ((strategy & 0b100u) > 0)
            {
                int eax = value;
                short divisor = Divisor;
                int multiplier = Multiplier;
                int shift = Shift;
                eax *= multiplier;
                ax = (short)((uint)eax >> 16);
                if ((strategy & 0b001u) > 0)
                {
                    ax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    ax += value;
                }
                r9w = (ushort)ax;
                r9w >>= 15;
                ax >>= shift;
                ax += (short)r9w;
                return (short)(ax * divisor);
            }
            else
            {
                ax = (short)(value >> 15);
                short mask = Mask;
                ax &= mask;
                ax += value;
                mask = (short)~mask;
                mask &= ax;
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
        public short AbsFloorRem(short value, out short largestMultipleOfDivisor)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value == short.MinValue;
                short q = Unsafe.As<bool, byte>(ref al);
                largestMultipleOfDivisor = (short)(q << 15);
                return (short)(value & --q);
            }
            short ax;
            ushort r9w;
            if ((strategy & 0b100u) > 0)
            {
                int eax = value;
                short divisor = Divisor;
                int multiplier = Multiplier;
                int shift = Shift;
                eax *= multiplier;
                ax = (short)((uint)eax >> 16);
                if ((strategy & 0b001u) > 0)
                {
                    ax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    ax += value;
                }
                r9w = (ushort)ax;
                r9w >>= 15;
                ax >>= shift;
                ax += (short)r9w;
                var f = largestMultipleOfDivisor = (short)(ax * divisor);
                return (short)(value - f);
            }
            else
            {
                ax = (short)(value >> 15);
                short mask = Mask;
                ax &= mask;
                ax += value;
                mask = (short)~mask;
                mask &= ax;
                largestMultipleOfDivisor = mask;
                return (short)(value - mask);
            }
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Modulo(short value)
        {
            uint strategy = (uint)Strategy;
            if (strategy == (uint)SignedDivisorStrategy.Branch)
            {
                bool al = value != short.MinValue;
                return (short)(value & -Unsafe.As<bool, byte>(ref al));
            }
            short ax;
            ushort edx;
            if ((strategy & 0b100u) > 0)
            {
                int eax = value;
                int multiplier = Multiplier;
                short divisor = Divisor;
                int shift = Shift;
                eax *= multiplier;
                ax = (short)((uint)eax >> 16);
                if ((strategy & 0b001u) > 0)
                {
                    ax -= value;
                }
                else if ((strategy & 0b010u) > 0)
                {
                    ax += value;
                }
                edx = (ushort)ax;
                edx >>= 15;
                ax >>= shift;
                short r11w = (short)(ax + (short)edx);
                return (short)(value - r11w * divisor);
            }
            else
            {
                ax = (short)(value >> 15);
                short mask = Mask;
                ax &= mask;
                ax += value;
                mask = (short)~mask;
                mask &= ax;
                return (short)(value - mask);
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Int16Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Int16Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift && Mask == other.Mask;

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
        /// Rapidly divides the specified <see cref="short"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short operator /(short left, Int16Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="short"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short operator %(short left, Int16Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int16Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int16Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="Int16Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Int16Divisor left, Int16Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int16Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Int16Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="Int16Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Int16Divisor left, Int16Divisor right) => !(left == right);

        #endregion Operators
    }
}
