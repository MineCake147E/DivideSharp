using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="uint"/> value RAPIDLY.
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct UInt32Divisor : IUnsignedDivisor<uint>, IEquatable<UInt32Divisor>
    {
        #region Static members

        private static ReadOnlySpan<UInt32Divisor> Divisors => new UInt32Divisor[]
        {
            new UInt32Divisor(3, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 1),
            new UInt32Divisor(4, 1, UInt32DivisorStrategy.Shift, 1),
            new UInt32Divisor(5, 0xcccccccdu, UInt32DivisorStrategy.MultiplyShift, 32 + 2),
            new UInt32Divisor(6, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 2),
            new UInt32Divisor(7, 0x24924925u, UInt32DivisorStrategy.MultiplyAddShift, 2),
            new UInt32Divisor(8, 1, UInt32DivisorStrategy.Shift, 3),
            new UInt32Divisor(9, 0x38e38e39u, UInt32DivisorStrategy.MultiplyShift, 32 + 1),
            new UInt32Divisor(10, 0xcccccccdu, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
            new UInt32Divisor(11, 0xba2e8ba3u, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
            new UInt32Divisor(12, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
        };

        private static (uint multiplier, UInt32DivisorStrategy strategy, byte shift) GetMagic(uint divisor)
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
                var q = Divisors[(int)(divisor - 3)];
                return (q.Multiplier, q.Strategy, q.Shift);
            }

            const int Bits = sizeof(uint) * 8;
            const int BitsMinus1 = Bits - 1;
            const uint TwoNMinus1 = 1u << BitsMinus1;

            var strategy = UInt32DivisorStrategy.MultiplyShift;
            uint nc = (uint)(-1 - (-divisor % divisor));

            int p = BitsMinus1;
            uint q1 = BitsMinus1 / nc;
            uint r1 = TwoNMinus1 - (q1 * nc);
            uint q2 = (TwoNMinus1 - 1) / divisor;
            uint r2 = TwoNMinus1 - 1 - (q2 * divisor);
            uint delta;

            do
            {
                p++;

                if (r1 >= (nc - r1))
                {
                    q1 = (2 * q1) + 1;
                    r1 = (2 * r1) - nc;
                }
                else
                {
                    q1 = 2 * q1;
                    r1 = 2 * r1;
                }

                if ((r2 + 1) >= (divisor - r2))
                {
                    if (q2 >= (TwoNMinus1 - 1))
                    {
                        strategy = UInt32DivisorStrategy.MultiplyAddShift;
                    }

                    q2 = (2 * q2) + 1;
                    r2 = (2 * r2) + 1 - divisor;
                }
                else
                {
                    if (q2 >= TwoNMinus1)
                    {
                        strategy = UInt32DivisorStrategy.MultiplyAddShift;
                    }

                    q2 = 2 * q2;
                    r2 = (2 * r2) + 1;
                }

                delta = divisor - 1 - r2;
            } while ((p < (Bits * 2)) && ((q1 < delta) || ((q1 == delta) && (r1 == 0))));
            return (q2 + 1, strategy, (byte)(strategy == UInt32DivisorStrategy.MultiplyAddShift ? p - Bits - 1 : p));     // resulting magic number
        }

        #endregion Static members

        #region Properties

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public uint Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public uint Multiplier { get; }

        /// <summary>
        /// Gets the strategy of a division.
        /// </summary>
        /// <value>
        /// The strategy of a division.
        /// </value>
        public UInt32DivisorStrategy Strategy { get; }

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
        /// Initializes a new instance of the <see cref="UInt32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <exception cref="DivideByZeroException"></exception>
        public UInt32Divisor(uint divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.None;
                Shift = 0;
            }
            else if (divisor > int.MaxValue)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.Branch;
                Shift = 0;
            }
            else if (divisor != 0 && (divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.Shift;
                Shift = (byte)Utils.CountConsecutiveZeros(divisor);
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt32Divisor"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="shift">The shift.</param>
        private UInt32Divisor(uint divisor, uint multiplier, UInt32DivisorStrategy strategy, byte shift)
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
        public uint Divide(uint value)
        {
            if (Strategy == UInt32DivisorStrategy.Branch)
            {
                return value >= Divisor ? 1u : 0u;
            }
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            int shift = Shift;
            if ((strategy & 0b10u) > 0)
            {
                rax *= multiplier;
                if ((strategy & 0b01u) > 0)
                {
                    eax = (uint)(rax >> 32);
                    value -= eax;
                    value >>= 1;
                    eax += value;
                    rax = eax;
                }
            }
            eax = (uint)(rax >> shift);
            return eax;
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit unsigned integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DivRem(uint value, out uint quotient)
        {
            ulong rax = value;
            uint eax;
            uint r9d = value;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            uint divisor = Divisor;
            int shift = Shift;    //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        eax = (uint)(rax >> 32);
                        r9d -= eax;
                        r9d >>= 1;
                        eax += r9d;
                        rax = eax;
                    }
                    eax = (uint)(rax >> shift);
                    quotient = eax;
                    eax *= divisor;
                    return value - eax;
                }
                else
                {
                    r9d >>= shift;
                    quotient = r9d;
                    r9d <<= shift;
                    return value ^ r9d;
                }
            }
            else
            {
                if (value >= divisor)
                {
                    quotient = 1u;
                    return value - divisor;
                }
                else
                {
                    quotient = 0;
                    return value;
                }
            }
        }

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Floor(uint value)
        {
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
            if ((Strategy & UInt32DivisorStrategy.Branch) > 0)
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
            {
                return value >= Divisor ? Divisor : 0u;
            }
            ulong rax = value;
            uint eax;
            uint divisor = Divisor;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            int shift = Shift;
            if ((strategy & 0b10u) > 0)
            {
                rax *= multiplier;
                if ((strategy & 0b01u) > 0)
                {
                    eax = (uint)(rax >> 32);
                    value -= eax;
                    value >>= 1;
                    eax += value;
                    rax = eax;
                }
                eax = (uint)(rax >> shift);
                return eax * divisor;
            }
            else
            {
                eax = ~0u << shift;
                return (uint)rax & eax;
            }
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Modulo(uint value)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            uint divisor = Divisor;
            int shift = Shift;    //ecx
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        uint r9d = value;
                        eax = (uint)(rax >> 32);
                        r9d -= eax;
                        r9d >>= 1;
                        eax += r9d;
                        rax = eax;
                    }
                    eax = (uint)(rax >> shift);
                    eax *= divisor;
                    return value - eax;
                }
                else
                {
                    return value & ~(~0u << shift);
                }
            }
            else
            {
                if (value >= divisor)
                {
                    return value - divisor;
                }
                else
                {
                    return value;
                }
            }
        }

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint FloorRem(uint value, out uint largestMultipleOfDivisor)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            uint divisor = Divisor;
            int shift = Shift;
            if ((strategy & 0b100) == 0)
            {
                if ((strategy & 0b10u) > 0)
                {
                    rax *= multiplier;
                    if ((strategy & 0b01u) > 0)
                    {
                        uint ecx = value;
                        eax = (uint)(rax >> 32);
                        ecx -= eax;
                        ecx >>= 1;
                        eax += ecx;
                        rax = eax;
                    }
                    eax = (uint)(rax >> shift);
                    eax *= divisor;
                    largestMultipleOfDivisor = eax;
                    return value - eax;
                }
                else
                {
                    var r9d = (~0u << shift);
                    largestMultipleOfDivisor = value & r9d;
                    return value & ~r9d;
                }
            }
            else
            {
                largestMultipleOfDivisor = value >= Divisor ? Divisor : 0u;
                return value - largestMultipleOfDivisor;
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
        public override bool Equals(object obj) => obj is UInt32Divisor divisor && Equals(divisor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(UInt32Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = 976573318;
            hashCode = (hashCode * -1521134295) + Divisor.GetHashCode();
            hashCode = (hashCode * -1521134295) + Multiplier.GetHashCode();
            hashCode = (hashCode * -1521134295) + Strategy.GetHashCode();
            hashCode = (hashCode * -1521134295) + Shift.GetHashCode();
            return hashCode;
        }

        #region Operators

        /// <summary>
        /// Rapidly divides the specified <see cref="uint"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The result of dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint operator /(uint left, UInt32Divisor right) => right.Divide(left);

        /// <summary>
        /// Rapidly returns the remainder resulting from dividing the specified <see cref="uint"/> value with <paramref name="right"/>'s <see cref="Divisor"/>.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint operator %(uint left, UInt32Divisor right) => right.Modulo(left);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt32Divisor"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt32Divisor"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt32Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(UInt32Divisor left, UInt32Divisor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt32Divisor"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt32Divisor"/> to compare.</param>
        /// <param name="right">The second  <see cref="UInt32Divisor"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UInt32Divisor left, UInt32Divisor right) => !(left == right);

        #endregion Operators
    }
}
