using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace DivisionBenchmark
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    public class CNegBenchmarks
    {
        private int a = 0;

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public int ValueToBeTested() => a += int.MaxValue;

        //[MethodImpl(MethodImplOptions.NoInlining)]
        [Params(false, true)]
        public bool DoNegate { get; set; }

        [Benchmark(Baseline = true)]
        public int Branch()
        {
            var v = ValueToBeTested();
            var c = DoNegate;
            return c ? -v : v;
        }

        [Benchmark]
        public int NoBranch()
        {
            var v = ValueToBeTested();
            var c = DoNegate;
            var f = (int)Unsafe.As<bool, byte>(ref c);
            return f + (v ^ -f);
        }
    }
}
