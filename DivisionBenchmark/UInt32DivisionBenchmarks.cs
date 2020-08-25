using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using DivideSharp;

using TestAndBenchmarkUtils;

using OldDivisor = DivisionBenchmark.OldDivisors.UInt32Divisor;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class UInt32DivisionBenchmarks
    {
        private uint a = 0;
        private PcgRandom rng;

        [Params(16, 9, 19, 0x8000_0001u)]
        public uint ValueToDivideBy { [MethodImpl(MethodImplOptions.NoInlining)]get; set; }

        private UInt32Divisor divisorBranching;
        private OldDivisor divisorOld;

        [GlobalSetup]
        public void Setup()
        {
            rng = new PcgRandom();
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt32Divisor(ValueToDivideBy);
            divisorOld = new OldDivisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint ValueToBeDivided() => rng.Next();

        /// <summary>
        /// The New Divisor
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideNew() => divisorBranching.Divide(ValueToBeDivided());

        /// <summary>
        /// Present Divisor
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideOld() => divisorOld.Divide(ValueToBeDivided());

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public uint DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out
        /* New: adopted `byte Unsafe.As<bool, byte>(ref bool)`
         *  |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
            |     DivideNew |               9 | 5.177 ns | 0.1183 ns | 0.0988 ns |  0.65 |    0.02 |
            |     DivideOld |               9 | 5.177 ns | 0.1144 ns | 0.1070 ns |  0.65 |    0.02 |
            | DivideOrdinal |               9 | 7.968 ns | 0.1868 ns | 0.1918 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              16 | 4.866 ns | 0.1482 ns | 0.2926 ns |  0.61 |    0.04 |
            |     DivideOld |              16 | 5.158 ns | 0.1347 ns | 0.1603 ns |  0.60 |    0.02 |
            | DivideOrdinal |              16 | 8.523 ns | 0.1996 ns | 0.1867 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              19 | 5.296 ns | 0.0693 ns | 0.0579 ns |  0.65 |    0.02 |
            |     DivideOld |              19 | 5.602 ns | 0.1500 ns | 0.1330 ns |  0.68 |    0.02 |
            | DivideOrdinal |              19 | 8.188 ns | 0.1551 ns | 0.1451 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |      2147483649 | 4.465 ns | 0.1139 ns | 0.1066 ns |  0.57 |    0.02 |
            |     DivideOld |      2147483649 | 9.398 ns | 0.4377 ns | 0.5843 ns |  1.22 |    0.11 |
            | DivideOrdinal |      2147483649 | 7.839 ns | 0.1916 ns | 0.1792 ns |  1.00 |    0.00 |
         */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
