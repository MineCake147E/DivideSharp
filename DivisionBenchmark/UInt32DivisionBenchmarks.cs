using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using DivideSharp;

using OldDivisor = DivisionBenchmark.OldDivisors.UInt32Divisor;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class UInt32DivisionBenchmarks
    {
        private uint a = 0;

        [Params(16, 9, 19, 0x8000_0001u)]
        public uint ValueToDivideBy { [MethodImpl(MethodImplOptions.NoInlining)]get; set; }

        private UInt32Divisor divisorBranching;
        private OldDivisor divisorOld;

        [GlobalSetup]
        public void Setup()
        {
            a = 0;
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt32Divisor(ValueToDivideBy);
            divisorOld = new OldDivisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint ValueToBeDivided() => a += int.MaxValue;

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
        /*  Now the New became with more `Unsafe.As`
         *  |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
            |     DivideNew |               9 | 2.308 ns | 0.0688 ns | 0.0644 ns |  0.49 |    0.02 |
            |     DivideOld |               9 | 2.415 ns | 0.0382 ns | 0.0339 ns |  0.51 |    0.01 |
            | DivideOrdinal |               9 | 4.706 ns | 0.0722 ns | 0.0675 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              16 | 2.427 ns | 0.0748 ns | 0.0699 ns |  0.50 |    0.01 |
            |     DivideOld |              16 | 2.619 ns | 0.0887 ns | 0.1021 ns |  0.55 |    0.02 |
            | DivideOrdinal |              16 | 4.844 ns | 0.0526 ns | 0.0467 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              19 | 2.320 ns | 0.0555 ns | 0.0519 ns |  0.49 |    0.01 |
            |     DivideOld |              19 | 2.716 ns | 0.0854 ns | 0.1169 ns |  0.58 |    0.03 |
            | DivideOrdinal |              19 | 4.696 ns | 0.0522 ns | 0.0488 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |      2147483649 | 1.978 ns | 0.0306 ns | 0.0286 ns |  0.40 |    0.02 |
            |     DivideOld |      2147483649 | 2.346 ns | 0.0562 ns | 0.0526 ns |  0.48 |    0.02 |
            | DivideOrdinal |      2147483649 | 4.887 ns | 0.1312 ns | 0.1562 ns |  1.00 |    0.00 |
         */

        /*  The Old vs New Benchmarks before 2020/08/25 10:41
         *  DivideNew: adjustment code became simpler 64bit adjustment instead of old complex 32bit adjustment
            |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
            |     DivideNew |               9 | 2.715 ns | 0.0872 ns | 0.1781 ns |  0.58 |    0.04 |
            |     DivideOld |               9 | 2.538 ns | 0.0989 ns | 0.1058 ns |  0.53 |    0.02 |
            | DivideOrdinal |               9 | 4.776 ns | 0.1274 ns | 0.1468 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              16 | 2.568 ns | 0.0831 ns | 0.0957 ns |  0.50 |    0.03 |
            |     DivideOld |              16 | 2.794 ns | 0.0894 ns | 0.1224 ns |  0.54 |    0.04 |
            | DivideOrdinal |              16 | 5.238 ns | 0.1387 ns | 0.2429 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |              19 | 2.357 ns | 0.0712 ns | 0.0666 ns |  0.50 |    0.02 |
            |     DivideOld |              19 | 2.759 ns | 0.0902 ns | 0.1456 ns |  0.59 |    0.04 |
            | DivideOrdinal |              19 | 4.718 ns | 0.0819 ns | 0.0726 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |     DivideNew |      2147483649 | 2.442 ns | 0.0819 ns | 0.0841 ns |  0.46 |    0.02 |
            |     DivideOld |      2147483649 | 2.687 ns | 0.0842 ns | 0.0746 ns |  0.50 |    0.02 |
            | DivideOrdinal |      2147483649 | 5.202 ns | 0.1342 ns | 0.1967 ns |  1.00 |    0.00 |
         */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
