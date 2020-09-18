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
    public class UInt16DivisionBenchmarks
    {
        private ushort a = 0;
        private PcgRandom rng;

        [Params((ushort)16, (ushort)9, (ushort)21, (ushort)0x8001u)]
        public ushort ValueToDivideBy
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get;
            set;
        }

        private UInt16Divisor divisorBranching;

        [GlobalSetup]
        public void Setup()
        {
            rng = new PcgRandom();
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisorBranching = new UInt16Divisor(ValueToDivideBy);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ushort ValueToBeDivided() => unchecked((ushort)rng.Next());

        [Benchmark]
        public ushort EchoValue() => ValueToBeDivided();

        /// <summary>
        /// The New Divisor
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public ushort DivideNew() => divisorBranching.Divide(ValueToBeDivided());

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public ushort DivideOrdinal() => unchecked((ushort)(ValueToBeDivided() / ValueToDivideBy));

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out
        /*
        // * Summary *

        BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
        Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
        .NET Core SDK=3.1.402
            [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
            DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

        |        Method | ValueToDivideBy |     Mean |     Error |    StdDev | Ratio | RatioSD |
        |-------------- |---------------- |---------:|----------:|----------:|------:|--------:|
        |     EchoValue |              16 | 2.977 ns | 0.1043 ns | 0.1960 ns |  0.41 |    0.03 |
        |     DivideNew |              16 | 5.165 ns | 0.0711 ns | 0.0630 ns |  0.69 |    0.02 |
        | DivideOrdinal |              16 | 7.529 ns | 0.1820 ns | 0.2096 ns |  1.00 |    0.00 |
        |               |                 |          |           |           |       |         |
        |     EchoValue |              21 | 3.229 ns | 0.0412 ns | 0.0385 ns |  0.47 |    0.01 |
        |     DivideNew |              21 | 5.993 ns | 0.0488 ns | 0.0456 ns |  0.87 |    0.01 |
        | DivideOrdinal |              21 | 6.900 ns | 0.0763 ns | 0.0714 ns |  1.00 |    0.00 |
        |               |                 |          |           |           |       |         |
        |     EchoValue |           32769 | 3.128 ns | 0.0942 ns | 0.1381 ns |  0.41 |    0.02 |
        |     DivideNew |           32769 | 4.575 ns | 0.1217 ns | 0.1449 ns |  0.61 |    0.02 |
        | DivideOrdinal |           32769 | 7.529 ns | 0.0877 ns | 0.0777 ns |  1.00 |    0.00 |
        |               |                 |          |           |           |       |         |
        |     EchoValue |               9 | 3.218 ns | 0.0304 ns | 0.0284 ns |  0.46 |    0.00 |
        |     DivideNew |               9 | 4.827 ns | 0.0314 ns | 0.0294 ns |  0.70 |    0.01 |
        | DivideOrdinal |               9 | 6.942 ns | 0.0569 ns | 0.0532 ns |  1.00 |    0.00 |

        */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
