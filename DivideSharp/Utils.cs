using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET5_0 || NETCOREAPP3_1

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0

using System.Runtime.Intrinsics.Arm;

#endif

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
#if !NETSTANDARD
                return BitOperations.TrailingZeroCount(value);
#else
                if (value == 0)
                {
                    return 32;
                }
                long v2 = -value;
                var index = (((uint)v2 & value) * 0x077C_B531u) >> 27;

                return Unsafe.AddByteOffset(
                    ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                    (IntPtr)(int)index);
#endif
            }
        }

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountConsecutiveZeros(ulong value)
        {
            unchecked
            {
#if !NETSTANDARD
                return BitOperations.TrailingZeroCount(value);
#else
                return (uint)value > 0 ? CountConsecutiveZeros((uint)value) : 32 + CountConsecutiveZeros((uint)(value >> 32));
#endif
            }
        }

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
                return (long)MultiplyHigh((ulong)x, (ulong)y) - ((x >> 63) & y) - ((y >> 63) & x);
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
#if NET5_0 || NETCOREAPP3_1
            if (Bmi2.X64.IsSupported)
            {
                return Bmi2.X64.MultiplyNoFlags(x, y);
            }
#endif
            //Copied and modified from Shamisen which I own the code.
            ulong a0 = (uint)x;
            var a1 = x >> 32;
            ulong b0 = (uint)y;
            var b1 = y >> 32;
            var z1a = a0 * b1;
            var z1b = a1 * b0;
            var z0 = a0 * b0;
            var z2 = a1 * b1;
            var z1 = z1a + z1b;
            var f = z1a > z1;
            z1b = (ulong)Unsafe.As<bool, byte>(ref f) << 32;
            z1a = z1 + (z0 >> 32);
            z1a >>= 32;
            z2 += z1b;
            z2 += z1a;
            return z2;
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
        /// <summary>
        /// Sets the bits of <paramref name="value"/> higher than specified <paramref name="index"/> to 0.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static uint ZeroHighBits(int index, uint value)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.IsSupported)
            {
                return Bmi2.ZeroHighBits(value, (uint)index);
            }
#endif
            return value & ~(~0u << index);
        }
    }
}
