using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="ulong"/> value rapidly.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct UInt64Divisor : IUnsignedDivisor<ulong>, IEquatable<UInt64Divisor>
    {
        #region Static members

        private static UInt64Divisor[] Divisors { get; } = new UInt64Divisor[]
        {
            new UInt64Divisor(3, 0xaaaaaaaaaaaaaaabul, UnsignedIntegerDivisorStrategy.MultiplyShift, 1),
            new UInt64Divisor(4, 1, UnsignedIntegerDivisorStrategy.Shift, 1),
            new UInt64Divisor(5, 0xcccccccccccccccdul, UnsignedIntegerDivisorStrategy.MultiplyShift, 2),
            new UInt64Divisor(6, 0xaaaaaaaaaaaaaaabul, UnsignedIntegerDivisorStrategy.MultiplyShift, 2),
            new UInt64Divisor(7, 0x2492492492492493ul, UnsignedIntegerDivisorStrategy.MultiplyAddShift, 2),
            new UInt64Divisor(8, 1, UnsignedIntegerDivisorStrategy.Shift, 3),
            new UInt64Divisor(9, 0xe38e38e38e38e38ful, UnsignedIntegerDivisorStrategy.MultiplyShift, 3),
            new UInt64Divisor(10, 0xcccccccccccccccdul, UnsignedIntegerDivisorStrategy.MultiplyShift, 3),
            new UInt64Divisor(11, 0x2e8ba2e8ba2e8ba3ul, UnsignedIntegerDivisorStrategy.MultiplyShift, 1),
            new UInt64Divisor(12, 0xaaaaaaaaaaaaaaabul, UnsignedIntegerDivisorStrategy.MultiplyShift, 3),
        };

        private static (ulong multiplier, UnsignedIntegerDivisorStrategy strategy, byte shift) GetMagic(ulong divisor)
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

            if (divisor < (ulong)Divisors.Length + 3)
            {
                var q = Divisors[(int)(divisor - 3)];
                return (q.Multiplier, q.Strategy, q.Shift);
            }

            const int Bits = sizeof(ulong) * 8;
            const int BitsMinus1 = Bits - 1;
            const ulong TwoNMinus1 = 1ul << BitsMinus1;

            var strategy = UnsignedIntegerDivisorStrategy.MultiplyShift;
            ulong nc = (ulong)(-1L - (-(long)divisor % (long)divisor));

            int p = BitsMinus1;
            ulong q1 = BitsMinus1 / nc;
            ulong r1 = TwoNMinus1 - q1 * nc;
            ulong q2 = (TwoNMinus1 - 1) / divisor;
            ulong r2 = TwoNMinus1 - 1 - q2 * divisor;
            ulong delta;

            do
            {
                p++;

                if (r1 >= nc - r1)
                {
                    q1 = 2 * q1 + 1;
                    r1 = 2 * r1 - nc;
                }
                else
                {
                    q1 = 2 * q1;
                    r1 = 2 * r1;
                }

                if (r2 + 1 >= divisor - r2)
                {
                    if (q2 >= TwoNMinus1 - 1)
                    {
                        strategy = UnsignedIntegerDivisorStrategy.MultiplyAddShift;
                    }

                    q2 = 2 * q2 + 1;
                    r2 = 2 * r2 + 1 - divisor;
                }
                else
                {
                    if (q2 >= TwoNMinus1)
                    {
                        strategy = UnsignedIntegerDivisorStrategy.MultiplyAddShift;
                    }

                    q2 = 2 * q2;
                    r2 = 2 * r2 + 1;
                }

                delta = divisor - 1 - r2;
            } while (p < Bits * 2 && (q1 < delta || q1 == delta && r1 == 0));
            return (q2 + 1, strategy, (byte)(strategy == UnsignedIntegerDivisorStrategy.MultiplyAddShift ? p - Bits - 1 : p - Bits)); // resulting magic number
        }

        #endregion Static members

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public ulong Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public ulong Multiplier { get; }

        /// <summary>
        /// Gets the strategy of a division.
        /// </summary>
        /// <value>
        /// The strategy of a division.
        /// </value>
        public UnsignedIntegerDivisorStrategy Strategy { get; }

        /// <summary>
        /// Gets the number of bits to shift for actual "division".
        /// </summary>
        /// <value>
        /// The number of bits to shift right.
        /// </value>
        public byte Shift { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private UInt64Divisor(ulong divisor, ulong multiplier, UnsignedIntegerDivisorStrategy strategy, byte shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public UInt64Divisor(ulong divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = UnsignedIntegerDivisorStrategy.None;
                Shift = 0;
            }
            else if (divisor > long.MaxValue)
            {
                Multiplier = 1;
                Strategy = UnsignedIntegerDivisorStrategy.Branch;
                Shift = 0;
            }
            else if (divisor != 0 && (divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = UnsignedIntegerDivisorStrategy.Shift;
                Shift = (byte)Utils.CountConsecutiveZeros(divisor);
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
            }
        }

        /// <summary>
        /// Divides the specified value by <see cref="Divisor" />.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>
        /// The value divided by <see cref="Divisor" />.
        /// </returns>
        public ulong Divide(ulong value)
        {
            uint strategy = (uint)Strategy;
            ulong divisor = Divisor;
            if (strategy == (uint)UnsignedIntegerDivisorStrategy.Branch)
            {
                bool v = value >= divisor;
                return Unsafe.As<bool, byte>(ref v);
            }
            ulong rax = value;
            ulong r8 = value;
            ulong multiplier = Multiplier;
            int shift = Shift;
            if ((strategy & 0b10u) > 0)
            {
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b01u) > 0)
                {
                    r8 -= rax;
                    r8 >>= 1;
                    rax += r8;
                }
            }
            rax >>= shift;
            return rax;
        }

        /// <summary>
        /// Calculates the quotient of two 64-bit unsigned integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong DivRem(ulong value, out ulong quotient)
        {
            ulong rax = value;
            ulong r8;
            ulong r9 = value;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ulong divisor = Divisor;
            int shift = Shift; //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax = Utils.MultiplyHigh(rax, multiplier);
                    if ((strategy & 0b01u) > 0)
                    {
                        r8 = value;
                        r8 -= rax;
                        r8 >>= 1;
                        rax += r8;
                    }
                    r8 = rax >> shift;
                    quotient = r8;
                    r8 *= divisor;
                    return value - r8;
                }
                else
                {
                    r9 >>= shift;
                    quotient = r9;
                    r9 <<= shift;
                    return value ^ r9;
                }
            }
            else
            {
                bool v = value >= divisor;
                byte v1 = Unsafe.As<bool, byte>(ref v);
                quotient = v1;
                return value - (divisor & (ulong)-Unsafe.As<bool, sbyte>(ref v));
            }
        }

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Floor(ulong value)
        {
            ulong divisor = Divisor;
            uint strategy = (uint)Strategy;
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
            if (strategy == (uint)UnsignedIntegerDivisorStrategy.Branch)
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
            {
                bool v = value >= divisor;
                return divisor & (ulong)-Unsafe.As<bool, sbyte>(ref v);
            }
            ulong rax = value;
            ulong r8;
            ulong multiplier = Multiplier;
            int shift = Shift;
            if ((strategy & 0b10u) == 0)
            {
                r8 = ~0ul << shift;
                return rax & r8;
            }
            else
            {
                rax = Utils.MultiplyHigh(rax, multiplier);
                if ((strategy & 0b01u) > 0)
                {
                    r8 = value;
                    r8 -= rax;
                    r8 >>= 1;
                    rax += r8;
                }
                r8 = rax >> shift;
                return r8 * divisor;
            }
        }

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong FloorRem(ulong value, out ulong largestMultipleOfDivisor)
        {
            ulong rax = value;
            ulong r8;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ulong divisor = Divisor;
            int shift = Shift;
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax = Utils.MultiplyHigh(rax, multiplier);
                    if ((strategy & 0b01u) > 0)
                    {
                        r8 = value;
                        r8 -= rax;
                        r8 >>= 1;
                        rax += r8;
                    }
                    r8 = rax >> shift;
                    r8 *= divisor;
                    largestMultipleOfDivisor = r8;
                    return value - r8;
                }
                else
                {
                    var r9 = ~0ul << shift;
                    largestMultipleOfDivisor = value & r9;
                    return value & ~r9;
                }
            }
            else
            {
                bool v = value >= divisor;
                r8 = (divisor & (ulong)-Unsafe.As<bool, sbyte>(ref v));
                largestMultipleOfDivisor = r8;
                return value - r8;
            }
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor" /> of the specified <paramref name="value" />.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>
        /// The remainder.
        /// </returns>
        public ulong Modulo(ulong value)
        {
            ulong rax = value;
            ulong r8 = value;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ulong divisor = Divisor;
            int shift = Shift; //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax = Utils.MultiplyHigh(rax, multiplier);
                    if ((strategy & 0b01u) > 0)
                    {
                        r8 -= rax;
                        r8 >>= 1;
                        rax += r8;
                    }
                    r8 = rax >> shift;
                    r8 *= divisor;
                    return value - r8;
                }
                else
                {
                    return value & ~(~0u << shift);
                }
            }
            else
            {
                bool v = value >= divisor;
                return value - (divisor & (ulong)-Unsafe.As<bool, sbyte>(ref v));
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is UInt64Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(UInt64Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 976573318;
            hashCode = hashCode * -1521134295 + Divisor.GetHashCode();
            hashCode = hashCode * -1521134295 + Multiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + Strategy.GetHashCode();
            hashCode = hashCode * -1521134295 + Shift.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Rapidly divides the specified <see cref="ulong"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(ulong left, UInt64Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="ulong"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(ulong left, UInt64Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt64Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt64Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt64Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(UInt64Divisor left, UInt64Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt64Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt64Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="UInt64Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UInt64Divisor left, UInt64Divisor right) => !(left == right);
    }
}
