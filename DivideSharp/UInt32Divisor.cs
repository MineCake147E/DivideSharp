using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DivideSharp
{
    /// <summary>
    /// Divides an <see cref="uint"/>
    /// </summary>
    /// <seealso cref="IDivisor{T}" />
    public readonly struct UInt32Divisor : IDivisor<uint>
    {
        #region Static members

        private static ReadOnlySpan<UInt32Divisor> Divisors => new UInt32Divisor[]
        {
            new UInt32Divisor(3, 0xaaaaaaabu, DivisorStrategy.MultiplyShift, 1),
            new UInt32Divisor(4, 1, DivisorStrategy.Shift, 1),
            new UInt32Divisor(5, 0xcccccccdu, DivisorStrategy.MultiplyShift, 2),
            new UInt32Divisor(6, 0xaaaaaaabu, DivisorStrategy.MultiplyShift, 2),
            new UInt32Divisor(7, 0x24924925u, DivisorStrategy.MultiplyAddShift, 3),
            new UInt32Divisor(8, 1, DivisorStrategy.Shift, 3),
            new UInt32Divisor(9, 0x38e38e39u, DivisorStrategy.MultiplyShift, 1),
            new UInt32Divisor(10, 0xcccccccdu, DivisorStrategy.MultiplyShift, 3),
            new UInt32Divisor(11, 0xba2e8ba3u, DivisorStrategy.MultiplyShift, 3),
            new UInt32Divisor(12, 0xaaaaaaabu, DivisorStrategy.MultiplyShift, 3),
        };

        private static (uint multiplier, DivisorStrategy strategy, int shift) GetMagic(uint divisor)
        {
            //Copied from CoreCLR, and modified by MineCake1.4.7
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
            if (divisor - 3 < Divisors.Length)
            {
                var q = Divisors[(int)(divisor - 3)];
                return (q.Multiplier, q.Strategy, q.Shift);
            }

            const int Bits = sizeof(uint) * 8;
            const int BitsMinus1 = Bits - 1;
            const uint TwoNMinus1 = 1u << BitsMinus1;

            var strategy = DivisorStrategy.MultiplyShift;
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
                        strategy = DivisorStrategy.MultiplyAddShift;
                    }

                    q2 = (2 * q2) + 1;
                    r2 = (2 * r2) + 1 - divisor;
                }
                else
                {
                    if (q2 >= TwoNMinus1)
                    {
                        strategy = DivisorStrategy.MultiplyAddShift;
                    }

                    q2 = 2 * q2;
                    r2 = (2 * r2) + 1;
                }

                delta = divisor - 1 - r2;
            } while ((p < (Bits * 2)) && ((q1 < delta) || ((q1 == delta) && (r1 == 0))));

            return (q2 + 1, strategy, strategy == DivisorStrategy.MultiplyAddShift ? p - Bits : p);     // resulting magic number
        }

        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountConsecutiveZeros(uint value)
        {
            if (value == 0)
            {
                return 32;
            }

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)(((value & (uint)-(int)value) * 0x077C_B531u) >> 27)); // Multi-cast mitigates redundant conv.u8
        }

        #endregion Static members

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
        public DivisorStrategy Strategy { get; }

        /// <summary>
        /// Gets the number of bits to shift for actual "division".
        /// </summary>
        /// <value>
        /// The number of bits to shift right.
        /// </value>
        public int Shift { get; }

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
                Strategy = DivisorStrategy.None;
                Shift = 0;
            }
            else if (divisor != 0 && (divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = DivisorStrategy.Shift;
                Shift = CountConsecutiveZeros(divisor);
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
        private UInt32Divisor(uint divisor, uint multiplier, DivisorStrategy strategy, int shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
        }

        /// <summary>
        /// Divides the specified value by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Divide(uint value)
        {
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
        /// Calculates the quotient of two 32-bit signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        public uint DivRem(uint value, out uint quotient)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            int shift = Shift;
            uint divisor = Divisor;
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
            }
            eax = (uint)(rax >> shift);
            quotient = eax;
            eax *= divisor;
            return value - eax;
        }

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        public uint Floor(uint value)
        {
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
            }
            eax = (uint)(rax >> shift);
            return eax * divisor;
        }

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        public uint Modulo(uint value)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            int shift = Shift;
            uint divisor = Divisor;
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
            }
            eax = (uint)(rax >> shift);
            eax *= divisor;
            return value - eax;
        }

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        public uint FloorRem(uint value, out uint largestMultipleOfDivisor)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            int shift = Shift;
            uint divisor = Divisor;
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
            }
            eax = (uint)(rax >> shift);
            eax *= divisor;
            largestMultipleOfDivisor = eax;
            return value - eax;
        }
    }
}
