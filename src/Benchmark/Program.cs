using Benchmark.Benchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    internal class Program
    {
        private static void Main()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.JoinSummary)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
                .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()));

            BenchmarkRunner.Run(new[]
            {
                BenchmarkConverter.TypeToBenchmarks(typeof(OneToManyBenchmark), config),
                BenchmarkConverter.TypeToBenchmarks(typeof(OneToManyBenchmark128), config),
                //BenchmarkConverter.TypeToBenchmarks(typeof(ManyToManyBenchmark), config),
            });
        }
    }
}
