using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="ushort"/> value RAPIDLY.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct UInt16Divisor : IUnsignedDivisor<ushort>, IEquatable<UInt16Divisor>
    {
        #region Static members

        private static UInt16Divisor[] Divisors { get; } = new UInt16Divisor[]
        {
            new UInt16Divisor(3, 43691, UnsignedIntegerDivisorStrategy.MultiplyShift, 17),
            new UInt16Divisor(4, 1, UnsignedIntegerDivisorStrategy.Shift, 1),   //not even refereed
            new UInt16Divisor(5, 52429, UnsignedIntegerDivisorStrategy.MultiplyShift, 18),
            new UInt16Divisor(6, 43691, UnsignedIntegerDivisorStrategy.MultiplyShift, 18),
            new UInt16Divisor(7, 9363, UnsignedIntegerDivisorStrategy.MultiplyAddShift, 2),
            new UInt16Divisor(8, 1, UnsignedIntegerDivisorStrategy.Shift, 3),
            new UInt16Divisor(9, 58255, UnsignedIntegerDivisorStrategy.MultiplyShift, 19),
            new UInt16Divisor(10, 52429, UnsignedIntegerDivisorStrategy.MultiplyShift, 19),
            new UInt16Divisor(11, 47663, UnsignedIntegerDivisorStrategy.MultiplyShift, 19),
            new UInt16Divisor(12, 43691, UnsignedIntegerDivisorStrategy.MultiplyShift, 19),
        };

        private static (ushort multiplier, UnsignedIntegerDivisorStrategy strategy, byte shift) GetMagic(ushort divisor)
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

            if (divisor - 3 < Divisors.Length)
            {
                var q = Divisors[(divisor - 3)];
                return (q.Multiplier, q.Strategy, q.Shift);
            }

            const int Bits = sizeof(ushort) * 8;
            const int BitsMinus1 = Bits - 1;
            const ushort TwoNMinus1 = (ushort)(1u << BitsMinus1);

            var strategy = UnsignedIntegerDivisorStrategy.MultiplyShift;
            ushort nc = (ushort)(-1 - (-divisor % divisor));

            int p = BitsMinus1;
            ushort q1 = (ushort)(BitsMinus1 / nc);
            ushort r1 = (ushort)(TwoNMinus1 - q1 * nc);
            ushort q2 = (ushort)((TwoNMinus1 - 1) / divisor);
            ushort r2 = (ushort)(TwoNMinus1 - 1 - q2 * divisor);
            ushort delta;

            do
            {
                p++;

                if (r1 >= nc - r1)
                {
                    q1 = (ushort)(2 * q1 + 1);
                    r1 = (ushort)(2 * r1 - nc);
                }
                else
                {
                    q1 = (ushort)(2 * q1);
                    r1 = (ushort)(2 * r1);
                }

                if (r2 + 1 >= divisor - r2)
                {
                    if (q2 >= TwoNMinus1 - 1)
                    {
                        strategy = UnsignedIntegerDivisorStrategy.MultiplyAddShift;
                    }

                    q2 = (ushort)(2 * q2 + 1);
                    r2 = (ushort)(2 * r2 + 1 - divisor);
                }
                else
                {
                    if (q2 >= TwoNMinus1)
                    {
                        strategy = UnsignedIntegerDivisorStrategy.MultiplyAddShift;
                    }

                    q2 = (ushort)(2 * q2);
                    r2 = (ushort)(2 * r2 + 1);
                }

                delta = (ushort)(divisor - 1 - r2);
            } while (p < Bits * 2 && (q1 < delta || q1 == delta && r1 == 0));
            return (unchecked((ushort)(q2 + 1)), strategy, (byte)(strategy == UnsignedIntegerDivisorStrategy.MultiplyAddShift ? p - Bits - 1 : p)); // resulting magic number
        }

        #endregion Static members

        #region Properties

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public ushort Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public ushort Multiplier { get; }

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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt16Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public UInt16Divisor(ushort divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = UnsignedIntegerDivisorStrategy.None;
                Shift = 0;
            }
            else if (divisor > short.MaxValue)
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
        /// Initializes a new instance of the <see cref="UInt16Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private UInt16Divisor(ushort divisor, ushort multiplier, UnsignedIntegerDivisorStrategy strategy, byte shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
        }

        #endregion Constructors

        #region Divisions

        /// <summary>
        /// Divides the specified <paramref name="value"/> by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Divide(ushort value)
        {
            uint strategy = (uint)Strategy;
            ushort divisor = Divisor;
            if (strategy == (uint)UnsignedIntegerDivisorStrategy.Branch)
            {
                bool v = value >= divisor;
                return Unsafe.As<bool, byte>(ref v);
            }
            uint rax = value;
            ushort eax;
            uint multiplier = Multiplier;
            int shift = Shift;
            if ((strategy & 0b10u) > 0)
            {
                rax *= multiplier;
                if ((strategy & 0b01u) > 0)
                {
                    eax = (ushort)(rax >> 16);
                    value -= eax;
                    value >>= 1;
                    eax += value;
                    rax = eax;
                }
            }
            eax = (ushort)(rax >> shift);
            return eax;
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit unsigned integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort DivRem(ushort value, out ushort quotient)
        {
            uint rax = value;
            ushort eax;
            ushort r9d = value;
            uint multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ushort divisor = Divisor;
            int shift = Shift; //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        eax = (ushort)(rax >> 16);
                        r9d -= eax;
                        r9d >>= 1;
                        eax += r9d;
                        rax = eax;
                    }
                    eax = (ushort)(rax >> shift);
                    quotient = eax;
                    eax *= divisor;
                    return (ushort)(value - eax);
                }
                else
                {
                    r9d >>= shift;
                    quotient = r9d;
                    r9d <<= shift;
                    return (ushort)(value ^ r9d);
                }
            }
            else
            {
                bool v = value >= Divisor;
                byte v1 = Unsafe.As<bool, byte>(ref v);
                quotient = v1;
                return (ushort)(value - (divisor & (uint)-Unsafe.As<bool, sbyte>(ref v)));
            }
        }

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Floor(ushort value)
        {
            ushort divisor = Divisor;
            uint strategy = (uint)Strategy;
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
            if (strategy == (uint)UnsignedIntegerDivisorStrategy.Branch)
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
            {
                bool v = value >= divisor;
                return (ushort)(divisor & (ushort)-Unsafe.As<bool, sbyte>(ref v));
            }
            uint rax = value;
            ushort eax;
            uint multiplier = Multiplier;
            int shift = Shift;
            if ((strategy & 0b10u) > 0)
            {
                rax *= multiplier;
                if ((strategy & 0b01u) > 0)
                {
                    eax = (ushort)(rax >> 16);
                    value -= eax;
                    value >>= 1;
                    eax += value;
                    rax = eax;
                }
                eax = (ushort)(rax >> shift);
                return (ushort)(eax * divisor);
            }
            else
            {
                eax = (ushort)(~ushort.MinValue << shift);
                return (ushort)(rax & eax);
            }
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Modulo(ushort value)
        {
            uint rax = value;
            ushort eax;
            uint multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ushort divisor = Divisor;
            int shift = Shift; //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        ushort r9d = value;
                        eax = (ushort)(rax >> 16);
                        r9d -= eax;
                        r9d >>= 1;
                        eax += r9d;
                        rax = eax;
                    }
                    eax = (ushort)(rax >> shift);
                    eax *= divisor;
                    return (ushort)(value - eax);
                }
                else
                {
                    return (ushort)(value & ~(~0u << shift));
                }
            }
            else
            {
                bool v = value >= divisor;
                return (ushort)(value - (divisor & (uint)-Unsafe.As<bool, sbyte>(ref v)));
            }
        }

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort FloorRem(ushort value, out ushort largestMultipleOfDivisor)
        {
            uint rax = value;
            ushort eax;
            uint multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            ushort divisor = Divisor;
            int shift = Shift;
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        ushort ecx = value;
                        eax = (ushort)(rax >> 16);
                        ecx -= eax;
                        ecx >>= 1;
                        eax += ecx;
                        rax = eax;
                    }
                    eax = (ushort)(rax >> shift);
                    eax *= divisor;
                    largestMultipleOfDivisor = eax;
                    return (ushort)(value - eax);
                }
                else
                {
                    var r9d = ~0u << shift;
                    largestMultipleOfDivisor = (ushort)(value & r9d);
                    return (ushort)(value & ~r9d);
                }
            }
            else
            {
                largestMultipleOfDivisor = (ushort)(value >= divisor ? divisor : 0u);
                return (ushort)(value - largestMultipleOfDivisor);
            }
        }

        #endregion Divisions

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => obj is UInt16Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(UInt16Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = 976573318;
            hashCode = hashCode * -1521134295 + Divisor.GetHashCode();
            hashCode = hashCode * -1521134295 + Multiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + Strategy.GetHashCode();
            hashCode = hashCode * -1521134295 + Shift.GetHashCode();
            return hashCode;
        }

        #region Operators

        /// <summary>
        /// Rapidly divides the specified <see cref="ushort"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort operator /(ushort left, UInt16Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="ushort"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort operator %(ushort left, UInt16Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt16Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt16Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt16Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(UInt16Divisor left, UInt16Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt16Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt16Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="UInt16Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UInt16Divisor left, UInt16Divisor right) => !(left == right);

        #endregion Operators
    }
}
