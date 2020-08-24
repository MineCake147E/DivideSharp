using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using DivideSharp;

namespace DivisionBenchmark
{
    public readonly struct SingleSwitchUInt32Divisor : IDivisor<uint>, IEquatable<SingleSwitchUInt32Divisor>
    {
        #region Divisions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Divide(uint value)
        {
            switch (Strategy)
            {
                case UInt32DivisorStrategy.Branch:
                    return value >= Divisor ? 1u : 0u;
                case UInt32DivisorStrategy.Shift:
                    {
                        int shift = Shift;
                        return value >> shift;
                    }
                case UInt32DivisorStrategy.MultiplyShift:
                    {
                        ulong rax = value;
                        uint eax;
                        ulong multiplier = Multiplier;
                        int shift = Shift;
                        rax *= multiplier;
                        eax = (uint)(rax >> shift);
                        return eax;
                    }
                case UInt32DivisorStrategy.MultiplyAddShift:
                    {
                        ulong rax = value;
                        uint eax;
                        ulong multiplier = Multiplier;
                        int shift = Shift;
                        rax *= multiplier;
                        eax = (uint)(rax >> 32);
                        value -= eax;
                        value >>= 1;
                        eax += value;
                        rax = eax;
                        eax = (uint)(rax >> shift);
                        return eax;
                    }
                default:
                    return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DivRem(uint value, out uint quotient)
        {
            uint divisor = Divisor;
            uint r9d = value;
            ulong rax = value;
            uint eax;
            var strategy = Strategy;
            ulong multiplier = Multiplier;
            int shift = Shift;    //ecx
            switch (strategy)
            {
                case UInt32DivisorStrategy.Branch:
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
                case UInt32DivisorStrategy.Shift:
                    {
                        r9d >>= shift;
                        quotient = r9d;
                        r9d <<= shift;
                        return value ^ r9d;
                    }
                case UInt32DivisorStrategy.MultiplyShift:
                    {
                        rax *= multiplier;
                        eax = (uint)(rax >> shift);
                        quotient = eax;
                        eax *= divisor;
                        return value - eax;
                    }
                case UInt32DivisorStrategy.MultiplyAddShift:
                    {
                        rax *= multiplier;
                        eax = (uint)(rax >> 32);
                        value -= eax;
                        value >>= 1;
                        eax += value;
                        rax = eax;
                        eax = (uint)(rax >> shift);
                        quotient = eax;
                        eax *= divisor;
                        return value - eax;
                    }
                default:
                    quotient = value;
                    return 0;
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Modulo(uint value)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = Multiplier;
            uint strategy = (uint)Strategy;
            uint divisor = Divisor;
            int shift = Shift; //ecx
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

        #region Static members

        private static ReadOnlySpan<SingleSwitchUInt32Divisor> Divisors => new SingleSwitchUInt32Divisor[] {
        new SingleSwitchUInt32Divisor (3, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 1),
        new SingleSwitchUInt32Divisor (4, 1, UInt32DivisorStrategy.Shift, 1),
        new SingleSwitchUInt32Divisor (5, 0xcccccccdu, UInt32DivisorStrategy.MultiplyShift, 32 + 2),
        new SingleSwitchUInt32Divisor (6, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 2),
        new SingleSwitchUInt32Divisor (7, 0x24924925u, UInt32DivisorStrategy.MultiplyAddShift, 32 + 3),
        new SingleSwitchUInt32Divisor (8, 1, UInt32DivisorStrategy.Shift, 3),
        new SingleSwitchUInt32Divisor (9, 0x38e38e39u, UInt32DivisorStrategy.MultiplyShift, 32 + 1),
        new SingleSwitchUInt32Divisor (10, 0xcccccccdu, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
        new SingleSwitchUInt32Divisor (11, 0xba2e8ba3u, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
        new SingleSwitchUInt32Divisor (12, 0xaaaaaaabu, UInt32DivisorStrategy.MultiplyShift, 32 + 3),
    };

        private static (uint multiplier, UInt32DivisorStrategy strategy, int shift) GetMagic(uint divisor)
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
            return (q2 + 1, strategy, strategy == UInt32DivisorStrategy.MultiplyAddShift ? p - Bits - 1 : p); // resulting magic number
        }

        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32] {
        //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
        0,
        1,
        28,
        2,
        29,
        14,
        24,
        3,
        30,
        22,
        20,
        15,
        25,
        17,
        4,
        8,
        31,
        27,
        13,
        23,
        21,
        19,
        16,
        7,
        26,
        12,
        18,
        6,
        11,
        5,
        10,
        9
    };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountConsecutiveZeros(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
                if (value == 0)
                {
                    return 32;
                }
                long v2 = -value;
                var index = (((uint)v2 & value) * 0x077C_B531u) >> 27;

                return Unsafe.AddByteOffset(
                    ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                    (IntPtr)(int)index);
            }
        }

        #endregion Static members

        #region Properties

        public uint Divisor { get; }

        public uint Multiplier { get; }

        public UInt32DivisorStrategy Strategy { get; }

        public int Shift { get; }

        #endregion Properties

        #region Constructors

        public SingleSwitchUInt32Divisor(uint divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            Divisor = divisor;
            if (divisor == 1)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.None;
                Shift = 0;
            }
            else if (divisor != 0 && (divisor & (divisor - 1)) == 0)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.Shift;
                Shift = CountConsecutiveZeros(divisor);
            }
            else if (divisor > int.MaxValue)
            {
                Multiplier = 1;
                Strategy = UInt32DivisorStrategy.Branch;
                Shift = 0;
            }
            else
            {
                (Multiplier, Strategy, Shift) = GetMagic(divisor);
            }
        }

        private SingleSwitchUInt32Divisor(uint divisor, uint multiplier, UInt32DivisorStrategy strategy, int shift)
        {
            Divisor = divisor;
            Multiplier = multiplier;
            Strategy = strategy;
            Shift = shift;
        }

        #endregion Constructors

        public override bool Equals(object obj) => obj is SingleSwitchUInt32Divisor divisor && Equals(divisor);

        public bool Equals(SingleSwitchUInt32Divisor other) => Divisor == other.Divisor && Multiplier == other.Multiplier && Strategy == other.Strategy && Shift == other.Shift;

        public override int GetHashCode()
        {
            var hashCode = 976573318;
            hashCode = (hashCode * -1521134295) + Divisor.GetHashCode();
            hashCode = (hashCode * -1521134295) + Multiplier.GetHashCode();
            hashCode = (hashCode * -1521134295) + Strategy.GetHashCode();
            hashCode = (hashCode * -1521134295) + Shift.GetHashCode();
            return hashCode;
        }

        #endregion Divisions

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint operator /(uint left, SingleSwitchUInt32Divisor right) => right.Divide(left);

        public static uint operator %(uint left, SingleSwitchUInt32Divisor right) => right.Modulo(left);

        public static bool operator ==(SingleSwitchUInt32Divisor left, SingleSwitchUInt32Divisor right) => left.Equals(right);

        public static bool operator !=(SingleSwitchUInt32Divisor left, SingleSwitchUInt32Divisor right) => !(left == right);

        #endregion Operators
    }
}
