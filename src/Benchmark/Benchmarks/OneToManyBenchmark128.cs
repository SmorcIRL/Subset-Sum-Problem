using Benchmark.Helpers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using GeneticAlgorithm.OneToMany;

namespace Benchmark.Benchmarks
{
    [SimpleJob(RunStrategy.Monitoring, 1, 1, 20, 5)]
    public class OneToManyBenchmark128
    {
        #region Params

        [Params(0.1, Priority = 0)]
        public double FitnessThreshold { get; set; }

        [Params(128, Priority = 1)]
        public int N { get; set; }

        [Params
        (
            //OneToManyInitialSeeding.FullRandom,
            //OneToManyInitialSeeding.AllUnset,
            //OneToManyInitialSeeding.AllSet,
            OneToManyInitialSeeding.RandomSumUntilExceeding,
            //OneToManyInitialSeeding.RandomSubArraySumUntilExceeding,
            Priority = 2
        )]
        public OneToManyInitialSeeding Filling { get; set; }

        [Params
        (
            OneToManyGenesSorting.Increasing,
            OneToManyGenesSorting.Hump,
            Priority = 3
        )]
        public OneToManyGenesSorting Sorting { get; set; }

        [Params
        (
            //50,
            //100,
            //200,
            //500,
            10000,
            Priority = 4
        )]
        public int GenSize { get; set; }

        [Params
        (
            //1,
            5,
            //10,
            Priority = 5
        )]
        public int MeanScale { get; set; }

        #endregion

        private decimal[] _initialSet;
        private decimal[] _set;
        private decimal _mean;
        private OneToManyGeneticAlgorithm128 _algorithm128;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _initialSet = InitialSetProvider.GetInitialSet();
            _mean = Math.Round(_initialSet.Sum() / _initialSet.Length, 2);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var random = new Random();

            var subsetSum = MeanScale * _mean;

            _set = SetGenerator.TakeN(_initialSet, N, random, x => x <= subsetSum);

            _algorithm128 = new OneToManyGeneticAlgorithm128(new OneToManyGeneticOptions
            {
                Random = random,
                Set = _set,
                SubsetSum = subsetSum,
                FitnessThreshold = (decimal)FitnessThreshold,
                GenerationSize = GenSize,
                MutationChance = 1d / GenSize,

                GenerationsMaxCount = int.MaxValue,
                Timeout = TimeSpan.FromSeconds(30),

                InitialSeeding = Filling,
                GenesSorting = Sorting,
            });
        }

        [Benchmark]
        public void Test128()
        {
            _algorithm128.Reset();

            while (true)
            {
                var result = _algorithm128.NextIteration();

                if (result.IsCompleted || result.IsThresholdSatisfied)
                {
                    break;
                }
            }
        }
    }
}
