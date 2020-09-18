using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using TestAndBenchmarkUtils;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class SignBenchmarks
    {
        private PcgRandom rng;

        [GlobalSetup]
        public void Setup()
        {
            rng = new PcgRandom();
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public int ValueToBeTested() => unchecked((int)rng.Next());

        [Benchmark(Baseline = true)]
        public int Standard() => Math.Sign(ValueToBeTested());

        [Benchmark]
        public int JustRShift31()
        {
            var v = ValueToBeTested();
            return v >> 31;
        }

        [Benchmark]
        public int CompareSetSubtract()
        {
            var v = ValueToBeTested();
            var cl = v > 0;
            var ch = v < 0;
            return Unsafe.As<bool, byte>(ref cl) - Unsafe.As<bool, byte>(ref ch);
        }

        [Benchmark]
        public int ShiftOr()
        {
            var v = ValueToBeTested();
            var cl = v != 0;
            return Unsafe.As<bool, byte>(ref cl) | (v >> 31);
        }

        /*
        // * Summary *

        BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
        Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
        .NET Core SDK=3.1.402
          [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
          DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

        |             Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
        |------------------- |---------:|----------:|----------:|------:|--------:|
        |           Standard | 2.691 ns | 0.0854 ns | 0.0949 ns |  1.00 |    0.00 |
        |       JustRShift31 | 2.492 ns | 0.0809 ns | 0.1284 ns |  0.92 |    0.05 |
        | CompareSetSubtract | 3.005 ns | 0.1057 ns | 0.2385 ns |  1.12 |    0.15 |
        |            ShiftOr | 2.843 ns | 0.0922 ns | 0.1540 ns |  1.06 |    0.07 |
         */
    }
}
