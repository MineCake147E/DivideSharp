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
    public class UInt64DivisionBenchmarks
    {
        private ulong a = 0;
        private PcgRandom rng;

        [Params(16, 9, 19, 0x8000_0000_0000_0001ul)]
        public ulong ValueToDivideBy
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get;
            set;
        }

        private UInt64Divisor divisorBranching;

        [GlobalSetup]
        public void Setup()
        {
            rng = new PcgRandom();
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt64Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ulong ValueToBeDivided() => rng.Next() | ((ulong)rng.Next() << 32);

        [Benchmark]
        public ulong EchoValue() => ValueToBeDivided();

        /// <summary>
        /// The New Divisor
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public ulong DivideNew() => divisorBranching.Divide(ValueToBeDivided());

        /*
                /// <summary>
                /// Present Divisor
                /// </summary>
                /// <returns></returns>
                [Benchmark]
                public ulong DivideOld() => divisorOld.Divide(ValueToBeDivided());
        */

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public ulong DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out
        /*  Now this benchmark has proven that we can accelerate the division even we have to emulate 128bit multiplication manually.
            |        Method |     ValueToDivideBy |      Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |-------------------- |----------:|----------:|----------:|------:|--------:|
            |     EchoValue |                   9 |  6.928 ns | 0.1675 ns | 0.1793 ns |  0.40 |    0.01 |
            |     DivideNew |                   9 | 10.631 ns | 0.1398 ns | 0.1239 ns |  0.61 |    0.01 |
            | DivideOrdinal |                   9 | 17.333 ns | 0.2821 ns | 0.2639 ns |  1.00 |    0.00 |
            |               |                     |           |           |           |       |         |
            |     EchoValue |                  16 |  7.157 ns | 0.1281 ns | 0.1198 ns |  0.42 |    0.01 |
            |     DivideNew |                  16 | 10.030 ns | 0.2252 ns | 0.2504 ns |  0.59 |    0.02 |
            | DivideOrdinal |                  16 | 17.005 ns | 0.2099 ns | 0.1964 ns |  1.00 |    0.00 |
            |               |                     |           |           |           |       |         |
            |     EchoValue |                  19 |  6.691 ns | 0.1635 ns | 0.1529 ns |  0.39 |    0.01 |
            |     DivideNew |                  19 | 10.626 ns | 0.1457 ns | 0.1363 ns |  0.62 |    0.01 |
            | DivideOrdinal |                  19 | 17.263 ns | 0.1918 ns | 0.1794 ns |  1.00 |    0.00 |
            |               |                     |           |           |           |       |         |
            |     EchoValue | 9223372036854775809 |  7.152 ns | 0.1360 ns | 0.1272 ns |  0.45 |    0.01 |
            |     DivideNew | 9223372036854775809 |  9.599 ns | 0.1019 ns | 0.0903 ns |  0.61 |    0.01 |
            | DivideOrdinal | 9223372036854775809 | 15.771 ns | 0.2889 ns | 0.2703 ns |  1.00 |    0.00 |

         */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
