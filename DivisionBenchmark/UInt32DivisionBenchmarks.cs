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
    public class UInt32DivisionBenchmarks
    {
        private uint a = 0;

        [Params(16, 9, 19, 0x8000_0001u)]
        public uint ValueToDivideBy { [MethodImpl(MethodImplOptions.NoInlining)]get; set; }

        private UInt32Divisor divisorBranching;
        private DelegateDispatchedUInt32Divisor divisorJumping;
        private SingleSwitchUInt32Divisor divisorSwitching;

        [GlobalSetup]
        public void Setup()
        {
            a = 0;
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt32Divisor(ValueToDivideBy);
            divisorJumping = new DelegateDispatchedUInt32Divisor(ValueToDivideBy);
            divisorSwitching = new SingleSwitchUInt32Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint ValueToBeDivided() => a += int.MaxValue;

        /*
        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint EchoValue() => ValueToBeDivided();
        */

        /// <summary>
        /// Current(Winner)
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideSharp() => divisorBranching.Divide(ValueToBeDivided());

        /*
        /// <summary>
        /// Switch Only Divisor (Loser)
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideSwitching() => divisorSwitching.Divide(ValueToBeDivided());

        /// <summary>
        /// Old Loser
        /// </summary>
        /// <returns></returns>
        //[Benchmark]
        public uint DivideJumping() => divisorJumping.Divide(ValueToBeDivided());
        */

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public uint DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out

        /*
            |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
            |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
            |   DivideSharp |               9 | 2.533 ns | 0.0525 ns | 0.0491 ns |  0.51 |    0.02 |
            | DivideOrdinal |               9 | 4.930 ns | 0.1189 ns | 0.1112 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |              16 | 2.230 ns | 0.0426 ns | 0.0399 ns |  0.44 |    0.01 |
            | DivideOrdinal |              16 | 5.049 ns | 0.0987 ns | 0.0923 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |              19 | 2.709 ns | 0.0808 ns | 0.0756 ns |  0.55 |    0.02 |
            | DivideOrdinal |              19 | 4.937 ns | 0.1046 ns | 0.0928 ns |  1.00 |    0.00 |
            |               |                 |          |           |           |       |         |
            |   DivideSharp |      2147483649 | 2.054 ns | 0.0333 ns | 0.0312 ns |  0.42 |    0.01 |
            | DivideOrdinal |      2147483649 | 4.895 ns | 0.1301 ns | 0.1548 ns |  1.00 |    0.00 |
        */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
