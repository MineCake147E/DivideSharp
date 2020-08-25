using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using DivideSharp;

namespace DivisionBenchmark
{
    [SimpleJob]
    public class TernaryBenchmarks
    {
        private ulong state = 0;
        private ulong inc = 0;

        [GlobalSetup]
        public void Setup()
        {
            Span<byte> a = stackalloc byte[2 * sizeof(ulong)];
            RandomNumberGenerator.Fill(a);
            var q = MemoryMarshal.Cast<byte, ulong>(a);
            state = q[0];
            inc = q[1] | 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Benchmark(Baseline = true)]
        public uint Overhead()
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

        [Benchmark]
        public uint Ordinal() => unchecked((int)Overhead()) > 0 ? 100000u : 0u;

        [Benchmark]
        public uint Casting()
        {
            unchecked
            {
                var g = (int)Overhead() > 0;
                return 100000u & (uint)-Unsafe.As<bool, sbyte>(ref g);
            }
        }

        #region Results

        /*
            |   Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |--------- |---------:|----------:|----------:|------:|--------:|
            | Overhead | 1.396 ns | 0.0565 ns | 0.0529 ns |  1.00 |    0.00 |
            |  Ordinal | 6.256 ns | 0.0897 ns | 0.0839 ns |  4.49 |    0.20 |
            |  Casting | 2.581 ns | 0.0515 ns | 0.0482 ns |  1.85 |    0.06 |
        */

        #endregion Results
    }
}
