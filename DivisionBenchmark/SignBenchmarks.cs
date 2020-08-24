using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class SignBenchmarks
    {
        private int a = 0;

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public int ValueToBeTested() => a += int.MaxValue;

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
    }
}
