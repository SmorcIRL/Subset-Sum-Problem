using GeneticAlgorithm.OneToMany;

namespace Benchmark.Benchmarks
{
    public class OneToManyBenchmark : OneToManyBenchmarkBase
    {
        protected override IOneToManyGeneticAlgorithm CreateAlgorithm(OneToManyGeneticOptions options)
        {
            return new OneToManyGeneticAlgorithm(options);
        }
    }
}
