using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Exposes some utility functions
    /// </summary>
    public static class Utils
    {
        #region Legacy CountBits

        /// <summary>
        /// same as floor(log2(i))
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountBits(ulong i)
        {
            // Reference: https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
            int r = 0;
            if ((i & 0xFFFF_FFFF_0000_0000uL) != 0)
            {
                i >>= 32;
                r |= 32;
            }
            if ((i & 0xFFFF0000u) != 0)
            {
                i >>= 16;
                r |= 16;
            }
            if ((i & 0xFF00) != 0)
            {
                i >>= 8;
                r |= 8;
            }
            if ((i & 0xF0) != 0)
            {
                i >>= 4;
                r |= 4;
            }
            if ((i & 0xC) != 0)
            {
                i >>= 2;
                r |= 2;
            }
            if ((i & 0x2) != 0)
                r |= 1;
            return r;
        }

        /// <summary>
        /// same as floor(log2(i))
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountBits(uint i)
        {
            // Reference: https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
            int r = 0;
            if ((i & 0xFFFF0000u) != 0)
            {
                i >>= 16;
                r |= 16;
            }
            if ((i & 0xFF00) != 0)
            {
                i >>= 8;
                r |= 8;
            }
            if ((i & 0xF0) != 0)
            {
                i >>= 4;
                r |= 4;
            }
            if ((i & 0xC) != 0)
            {
                i >>= 2;
                r |= 2;
            }
            if ((i & 0x2) != 0)
                r |= 1;
            return r;
        }

        /// <summary>
        /// same as floor(log2(i))
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountBits(ushort i)
        {
            // Reference: https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
            int r = 0;
            if ((i & 0xFF00) != 0)
            {
                i >>= 8;
                r |= 8;
            }
            if ((i & 0xF0) != 0)
            {
                i >>= 4;
                r |= 4;
            }
            if ((i & 0xC) != 0)
            {
                i >>= 2;
                r |= 2;
            }
            if ((i & 0x2) != 0)
                r |= 1;
            return r;
        }

        /// <summary>
        /// same as floor(log2(i))
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountBits(byte i)
        {
            // Reference: https://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
            int r = 0;
            if ((i & 0xF0) != 0)
            {
                i >>= 4;
                r |= 4;
            }
            if ((i & 0xC) != 0)
            {
                i >>= 2;
                r |= 2;
            }
            if ((i & 0x2) != 0)
                r |= 1;
            return r;
        }

        #endregion Legacy CountBits

        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountConsecutiveZeros(uint value)
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

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountConsecutiveZeros(ulong value) => (uint)value > 0 ? CountConsecutiveZeros((uint)value) : 32 + CountConsecutiveZeros((uint)(value >> 32));

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long MultiplyHigh(long x, long y)
        {
            unchecked
            {
                ulong xl = (uint)x;
                ulong yl = (uint)y;
                long xh = x >> 32;
                long yh = y >> 32;
                ulong u = xl * yl;
                long s = (long)(u >> 32);
                long t1 = xh * (long)yl;
                long t2 = yh * (long)xl;
                s += (uint)t1;
                s += (uint)t2;
                s >>= 32;
                s += (t1 >> 32) + (t2 >> 32);
                s += xh * yh;
                return s;
            }
        }

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MultiplyHigh(ulong x, ulong y)
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

            ulong xl = (uint)x;
            ulong yl = (uint)y;
            ulong xh = x >> 32;
            ulong yh = y >> 32;
            ulong mull = xl * yl;
            ulong t = xh * yl + (mull >> 32);
            ulong tl = (uint)t;
            tl += xl * yh;
            return xh * yh + (t >> 32) + (tl >> 32);
        }

        /// <summary>
        /// Returns the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ushort Abs(short value)
        {
            var q = value >> 15;
            return (ushort)((value + q) ^ q);
        }

        /// <summary>
        /// Returns the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static uint Abs(int value)
        {
            var q = value >> 31;
            return (uint)((value + q) ^ q);
        }

        /// <summary>
        /// Returns the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ulong Abs(long value)
        {
            var q = value >> 63;
            return (ulong)((value + q) ^ q);
        }
    }
}
