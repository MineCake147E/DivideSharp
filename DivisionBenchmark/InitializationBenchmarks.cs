using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DivideSharp;
using TestAndBenchmarkUtils;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class InitializationBenchmarks
    {
        private PcgRandom rng;
        private uint random;
        private UInt32Divisor u32D;
        private Int32Divisor i32D;
        private UInt64Divisor u64D;

        [GlobalSetup]
        public void Setup()
        {
            rng = new PcgRandom();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint Divisor() => rng.Next();

        [Benchmark]
        public uint GenerateRandom() => random = Divisor();

        [Benchmark]
        public UInt32Divisor UInt32Divisor() => u32D = new UInt32Divisor(Divisor());

        [Benchmark]
        public Int32Divisor Int32Divisor() => i32D = new Int32Divisor(unchecked((int) Divisor()));

        [Benchmark]
        public UInt64Divisor UInt64Divisor() => u64D = new UInt64Divisor(Divisor() | ((ulong) Divisor() << 32));

        #region Results

        /*
         *  |         Method |       Mean |     Error |    StdDev |
            |--------------- |-----------:|----------:|----------:|
            | GenerateRandom |   4.097 ns | 0.1165 ns | 0.2216 ns |
            |  UInt32Divisor | 100.215 ns | 1.7585 ns | 1.4684 ns |
            |   Int32Divisor | 170.136 ns | 2.8221 ns | 2.2033 ns |
            |  UInt64Divisor | 181.202 ns | 3.6365 ns | 4.9776 ns |
        */

        #endregion Results
    }
}