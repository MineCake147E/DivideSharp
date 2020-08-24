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
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static long MultiplyHigh(long x, long y)
        {
            ulong u = (uint)x * (ulong)(uint)y;
            long s = (long)(u >> 32);
            long xhi = x >> 32;
            long yhi = y >> 32;
            s += xhi * (uint)y;
            s += yhi * (uint)x;
            s >>= 32;
            s += xhi * yhi;
            return s;
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
    }
}
