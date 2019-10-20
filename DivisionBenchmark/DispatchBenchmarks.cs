using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using DivideSharp;

namespace DivisionBenchmark
{
    [CoreJob]
    [CoreRtJob]
    public class DispatchBenchmarks
    {
        private uint a = 0;

        [Params(16, 9, 19, 0x8000_0001u)]
        public uint ValueToDivideBy { get; set; }

        private UInt32Divisor divisorBranching;
        private DelegateDispatchedUInt32Divisor divisorJumping;

        [GlobalSetup]
        public void Setup()
        {
            a = 0;
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt32Divisor(ValueToDivideBy);
            divisorJumping = new DelegateDispatchedUInt32Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint ValueToBeDivided() => a += int.MaxValue;

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public uint EchoValue() => ValueToBeDivided();

        /// <summary>
        /// Winner
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideSharp() => divisorBranching.Divide(ValueToBeDivided());

        /// <summary>
        /// Loser
        /// </summary>
        /// <returns></returns>
        //[Benchmark]
        public uint DivideJumping() => divisorJumping.Divide(ValueToBeDivided());

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public uint DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out

        /*
            |        Method |    Job | Runtime | ValueToDivideBy |      Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |------- |-------- |---------------- |----------:|----------:|----------:|------:|--------:|
            |     EchoValue |   Core |    Core |              16 | 0.9074 ns | 0.0227 ns | 0.0212 ns |  1.00 |    0.00 |
            |   DivideSharp |   Core |    Core |              16 | 2.2799 ns | 0.0424 ns | 0.0396 ns |  2.51 |    0.07 |
            | DivideOrdinal |   Core |    Core |              16 | 4.4957 ns | 0.0333 ns | 0.0312 ns |  4.96 |    0.12 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue | CoreRT |  CoreRT |              16 | 0.5587 ns | 0.0141 ns | 0.0125 ns |  1.00 |    0.00 |
            |   DivideSharp | CoreRT |  CoreRT |              16 | 1.5716 ns | 0.0301 ns | 0.0281 ns |  2.82 |    0.08 |
            | DivideOrdinal | CoreRT |  CoreRT |              16 | 4.1325 ns | 0.0956 ns | 0.0847 ns |  7.40 |    0.19 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue |   Core |    Core |              19 | 0.8039 ns | 0.0521 ns | 0.0659 ns |  1.00 |    0.00 |
            |   DivideSharp |   Core |    Core |              19 | 2.4196 ns | 0.0612 ns | 0.0572 ns |  2.95 |    0.24 |
            | DivideOrdinal |   Core |    Core |              19 | 5.0880 ns | 0.1322 ns | 0.1470 ns |  6.28 |    0.48 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue | CoreRT |  CoreRT |              19 | 0.5515 ns | 0.0256 ns | 0.0239 ns |  1.00 |    0.00 |
            |   DivideSharp | CoreRT |  CoreRT |              19 | 2.1835 ns | 0.0822 ns | 0.1280 ns |  3.98 |    0.34 |
            | DivideOrdinal | CoreRT |  CoreRT |              19 | 4.1086 ns | 0.0937 ns | 0.0877 ns |  7.46 |    0.39 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue |   Core |    Core |      2147483649 | 0.9398 ns | 0.0483 ns | 0.0475 ns |  1.00 |    0.00 |
            |   DivideSharp |   Core |    Core |      2147483649 | 2.2920 ns | 0.0470 ns | 0.0440 ns |  2.45 |    0.13 |
            | DivideOrdinal |   Core |    Core |      2147483649 | 4.2667 ns | 0.0451 ns | 0.0422 ns |  4.55 |    0.22 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue | CoreRT |  CoreRT |      2147483649 | 0.5775 ns | 0.0273 ns | 0.0242 ns |  1.00 |    0.00 |
            |   DivideSharp | CoreRT |  CoreRT |      2147483649 | 1.9447 ns | 0.0572 ns | 0.0535 ns |  3.37 |    0.16 |
            | DivideOrdinal | CoreRT |  CoreRT |      2147483649 | 4.4261 ns | 0.1266 ns | 0.2149 ns |  7.74 |    0.57 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue |   Core |    Core |               9 | 0.8009 ns | 0.0332 ns | 0.0311 ns |  1.00 |    0.00 |
            |   DivideSharp |   Core |    Core |               9 | 2.2079 ns | 0.0651 ns | 0.0609 ns |  2.76 |    0.15 |
            | DivideOrdinal |   Core |    Core |               9 | 5.0975 ns | 0.0780 ns | 0.0651 ns |  6.40 |    0.27 |
            |               |        |         |                 |           |           |           |       |         |
            |     EchoValue | CoreRT |  CoreRT |               9 | 0.5640 ns | 0.0322 ns | 0.0301 ns |  1.00 |    0.00 |
            |   DivideSharp | CoreRT |  CoreRT |               9 | 1.9183 ns | 0.0406 ns | 0.0380 ns |  3.41 |    0.17 |
            | DivideOrdinal | CoreRT |  CoreRT |               9 | 4.1994 ns | 0.0980 ns | 0.0916 ns |  7.47 |    0.45 |
        */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
