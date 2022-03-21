using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DivisionBenchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher
                        .FromAssembly(typeof(SignBenchmarks).Assembly)
                        .Run(args, DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(256)));
            Console.Write("Press any key to exit:");
            Console.ReadKey();
        }
    }
}
