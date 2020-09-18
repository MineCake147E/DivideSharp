using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class MiscTests
    {
        private const ulong RandomTestCount = 0x1_0000ul;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MultiplyHighCustom32(uint a, uint b)
        {
            uint al = (ushort)a;
            uint ah = a >> 16;
            uint bl = (ushort)b;
            uint bh = b >> 16;
            uint u = al * bl;
            uint t = ah * bl + (u >> 16);
            uint tl = (ushort)t;
            uint th = t >> 16;
            tl += al * bh;
            return ah * bh + th + (tl >> 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MultiplyHigh32(uint a, uint b)
        {
            ulong h = a;
            h *= b;
            return (uint)(h >> 32);
        }

        [Test]
        public void MulHiEqualsUnsigned()
        {
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var a = rng.Next();
                var b = rng.Next();
                Assert.AreEqual(MultiplyHigh32(a, b), MultiplyHighCustom32(a, b), $"Testing {a} * {b}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MultiplyHighCustom32(int x, int y)
        {
            unchecked
            {
                uint xl = (ushort)x;
                uint yl = (ushort)y;
                int xh = x >> 16;
                int yh = y >> 16;
                uint u = xl * yl;
                int s = (int)(u >> 16);
                int t1 = xh * (int)yl;
                int t2 = yh * (int)xl;
                s += (ushort)t1;
                s += (ushort)t2;
                s >>= 16;
                s += (t1 >> 16) + (t2 >> 16);
                s += xh * yh;
                return s;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MultiplyHigh32(int a, int b)
        {
            long h = a;
            h *= b;
            return (int)(h >> 32);
        }

        [Test]
        public void MulHiEqualsSigned()
        {
            unchecked
            {
                var rng = new PcgRandom();
                for (ulong i = 0; i < RandomTestCount; i++)
                {
                    var a = (int)rng.Next();
                    var b = (int)rng.Next();
                    Assert.AreEqual(MultiplyHigh32(a, b), MultiplyHighCustom32(a, b), $"Testing {a} * {b}");
                }
            }
        }
    }
}
