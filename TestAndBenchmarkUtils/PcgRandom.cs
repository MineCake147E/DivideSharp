using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TestAndBenchmarkUtils
{
    public sealed class PcgRandom
    {
        private ulong state = 0;
        private ulong inc = 0;

        public PcgRandom(ulong initialState, ulong streamId)
        {
            state = initialState;
            inc = streamId | 1ul;
        }

        public PcgRandom()
        {
            Span<byte> a = stackalloc byte[2 * sizeof(ulong)];
            RandomNumberGenerator.Fill(a);
            (ulong state, ulong inc) q = Unsafe.As<byte, (ulong state, ulong inc)>(ref a[0]);
            state = q.state;
            inc = q.inc | 1;
        }

        public uint Next()
        {
            // *Really* minimal PCG32 code / (c) 2014 M.E. O'Neill / pcg-random.org
            // Licensed under Apache License 2.0 (NO WARRANTY, etc. see website)
            unchecked
            {
                var old = state;
                state = old * 6364136223846793005UL + inc;
                uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
                int rot = (int)(old >> 59);
                return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
            }
        }
    }
}
