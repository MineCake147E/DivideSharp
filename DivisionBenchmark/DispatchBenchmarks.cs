using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using DivideSharp;

namespace DivisionBenchmark
{
    [CoreJob]
    public class DispatchBenchmarks
    {
        private uint a = 0;

        [Params(16, 9, 19)]
        public uint ValueToDivideBy { get; set; }

        private UInt32Divisor divisorBranching;
        private DelegateDispatchedUInt32Divisor divisorJumping;

        [GlobalSetup]
        public void Setup()
        {
            divisorBranching = new UInt32Divisor(ValueToDivideBy);
            divisorJumping = new DelegateDispatchedUInt32Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public uint ValueToBeDivided() => a++;

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint EchoValue() => ValueToBeDivided();

        /// <summary>
        /// Winner
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideSwitch() => divisorBranching.Divide(ValueToBeDivided());

        /// <summary>
        /// Loser
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public uint DivideJumping() => divisorJumping.Divide(ValueToBeDivided());

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public uint DivideOrdinal() => ValueToBeDivided() / ValueToDivideBy;

        #region Result

        /*
            |        Method | ValueToDivideBy |      Mean |     Error |    StdDev |
            |-------------- |---------------- |----------:|----------:|----------:|
            |     EchoValue |               9 | 0.8905 ns | 0.0130 ns | 0.0115 ns |
            |  DivideSwitch |               9 | 2.0447 ns | 0.0117 ns | 0.0098 ns |
            | DivideJumping |               9 | 4.7681 ns | 0.0199 ns | 0.0186 ns |
            | DivideOrdinal |               9 | 4.5541 ns | 0.0271 ns | 0.0240 ns |
            |     EchoValue |              16 | 0.7680 ns | 0.0097 ns | 0.0091 ns |
            |  DivideSwitch |              16 | 1.6670 ns | 0.0196 ns | 0.0184 ns |
            | DivideJumping |              16 | 4.7538 ns | 0.0477 ns | 0.0446 ns |
            | DivideOrdinal |              16 | 4.7445 ns | 0.0530 ns | 0.0496 ns |
            |     EchoValue |              19 | 0.8917 ns | 0.0109 ns | 0.0102 ns |
            |  DivideSwitch |              19 | 2.0428 ns | 0.0196 ns | 0.0174 ns |
            | DivideJumping |              19 | 4.7544 ns | 0.0618 ns | 0.0516 ns |
            | DivideOrdinal |              19 | 4.5824 ns | 0.1163 ns | 0.1087 ns |
         */

        #endregion Result
    }
}
