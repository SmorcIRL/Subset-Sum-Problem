using GeneticAlgorithm.OneToMany;

namespace Benchmark.Benchmarks;

public class OneToManyBenchmark128 : OneToManyBenchmarkBase
{
    protected override IOneToManyGeneticAlgorithm CreateAlgorithm(OneToManyGeneticOptions options)
    {
        return new OneToManyGeneticAlgorithm128(options);
    }
}
