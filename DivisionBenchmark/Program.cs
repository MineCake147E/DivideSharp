using System;

using BenchmarkDotNet.Running;

namespace DivisionBenchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<Int32DivisionBenchmarks>();
        }
    }
}
