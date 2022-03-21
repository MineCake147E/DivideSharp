using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using DivideSharp;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class Int32DivisionBenchmarks
    {
        private int a = 0;

        [Params(16, 9, 15, -16, -9, -15)]
        public int ValueToDivideBy { get; set; }

        [Params(256)]
        public int LoopCount { get; set; }

        private Int32Divisor divisor;

        [GlobalSetup]
        public void Setup()
        {
            a = 0;
            Console.WriteLine($"Setup with value {ValueToDivideBy}");
            divisor = new Int32Divisor(ValueToDivideBy);
        }

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
        /// Current
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int DivideSharp()
        {
            int length = LoopCount;
            var divisor1 = divisor;
            int u = 0;
            for (int i = 0; i < length; i++)
            {
                u += ValueToBeDivided() / divisor1;
            }
            return u;
        }

        /// <summary>
        /// Control
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public int DivideOrdinal()
        {
            int length = LoopCount;
            var divisor1 = ValueToDivideBy;
            int u = 0;
            for (int i = 0; i < length; i++)
            {
                u += ValueToBeDivided() / divisor1;
            }
            return u;
        }

        #region Result

#pragma warning disable S125 // Sections of code should not be commented out

        /*
``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]     : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT
  DefaultJob : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT


```
|        Method | ValueToDivideBy | LoopCount |     Mean |   Error |  StdDev | Ratio | Code Size |
|-------------- |---------------- |---------- |---------:|--------:|--------:|------:|----------:|
|   **DivideSharp** |             **-16** |       **256** | **507.5 ns** | **2.89 ns** | **2.42 ns** |  **0.79** |     **223 B** |
| DivideOrdinal |             -16 |       256 | 639.1 ns | 3.13 ns | 2.93 ns |  1.00 |      49 B |
|               |                 |           |          |         |         |       |           |
|   **DivideSharp** |             **-15** |       **256** | **648.9 ns** | **4.98 ns** | **4.41 ns** |  **0.97** |     **223 B** |
| DivideOrdinal |             -15 |       256 | 668.0 ns | 4.58 ns | 4.06 ns |  1.00 |      49 B |
|               |                 |           |          |         |         |       |           |
|   **DivideSharp** |              **-9** |       **256** | **524.0 ns** | **5.73 ns** | **5.36 ns** |  **0.78** |     **223 B** |
| DivideOrdinal |              -9 |       256 | 667.5 ns | 5.39 ns | 4.78 ns |  1.00 |      49 B |
|               |                 |           |          |         |         |       |           |
|   **DivideSharp** |               **9** |       **256** | **543.0 ns** | **4.72 ns** | **4.19 ns** |  **0.81** |     **223 B** |
| DivideOrdinal |               9 |       256 | 667.1 ns | 5.80 ns | 5.42 ns |  1.00 |      49 B |
|               |                 |           |          |         |         |       |           |
|   **DivideSharp** |              **15** |       **256** | **647.9 ns** | **4.81 ns** | **4.50 ns** |  **0.97** |     **223 B** |
| DivideOrdinal |              15 |       256 | 666.8 ns | 3.89 ns | 3.64 ns |  1.00 |      49 B |
|               |                 |           |          |         |         |       |           |
|   **DivideSharp** |              **16** |       **256** | **508.3 ns** | **6.67 ns** | **5.91 ns** |  **0.80** |     **223 B** |
| DivideOrdinal |              16 |       256 | 637.7 ns | 3.59 ns | 3.36 ns |  1.00 |      49 B |

        */

#pragma warning restore S125 // Sections of code should not be commented out

        #endregion Result
    }
}
