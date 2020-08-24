using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using DivideSharp;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class Int32DivisionBenchmarks
    {
        private int a = 0;

        [Params(16, 9, 15, -16, -9, -15)]
        public int ValueToDivideBy { [MethodImpl(MethodImplOptions.NoInlining)]get; set; }

        private Int32Divisor divisor;

        [GlobalSetup]
        public void Setup()
        {
            a = 0;
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisor = new Int32Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int ValueToBeDivided() => a += int.MaxValue;

        /*
        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int EchoValue() => ValueToBeDivided();
        */

        /// <summary>
        /// Current(Winner)
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int DivideSharp() => divisor.Divide(ValueToBeDivided());

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public int DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out

        /*
            |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
            |   DivideSharp |             -16 | 2.844 ns | 0.0448 ns | 0.0419 ns |  0.62 |    0.03 |
            | DivideOrdinal |             -16 | 4.540 ns | 0.1248 ns | 0.1944 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |             -15 | 2.887 ns | 0.0585 ns | 0.0456 ns |  0.62 |    0.03 |
            | DivideOrdinal |             -15 | 4.582 ns | 0.1255 ns | 0.1840 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |              -9 | 3.609 ns | 0.1048 ns | 0.1165 ns |  0.82 |    0.03 |
            | DivideOrdinal |              -9 | 4.374 ns | 0.0625 ns | 0.0585 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |               9 | 3.441 ns | 0.0834 ns | 0.0696 ns |  0.78 |    0.04 |
            | DivideOrdinal |               9 | 4.290 ns | 0.1196 ns | 0.1677 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |              15 | 3.196 ns | 0.0974 ns | 0.2296 ns |  0.77 |    0.06 |
            | DivideOrdinal |              15 | 4.324 ns | 0.0873 ns | 0.0774 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |              16 | 2.835 ns | 0.0495 ns | 0.0463 ns |  0.64 |    0.02 |
            | DivideOrdinal |              16 | 4.424 ns | 0.1087 ns | 0.0848 ns |  1.00 |    0.00 |
        */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
