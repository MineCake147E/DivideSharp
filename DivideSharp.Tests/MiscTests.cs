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
        public void MulHiEquals()
        {
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var a = rng.Next();
                var b = rng.Next();
                Assert.AreEqual(MultiplyHigh32(a, b), MultiplyHighCustom32(a, b), $"Testing {a} * {b}");
            }
        }
    }
}
