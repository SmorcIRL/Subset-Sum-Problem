using Benchmark.Helpers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using GeneticAlgorithm.OneToMany;

namespace Benchmark.Benchmarks
{
    [SimpleJob(RunStrategy.Monitoring, 1, 1, 20, 5)]
    public abstract class OneToManyBenchmarkBase
    {
        #region Params

        [Params(0.1, Priority = 0)]
        public double FitnessThreshold { get; set; }

        [Params
        (
            128,
            //500,
            //1000,
            //10000,
            Priority = 1
        )]
        public int N { get; set; }

        [Params
        (
            OneToManyInitialSeeding.FullRandom,
            //OneToManyInitialSeeding.AllUnset,
            //OneToManyInitialSeeding.AllSet,
            OneToManyInitialSeeding.RandomSumUntilExceeding,
            OneToManyInitialSeeding.RandomSubArraySumUntilExceeding,
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
            200,
            //500,
            //10000,
            Priority = 4
        )]
        public int GenSize { get; set; }

        [Params
        (
            //0.1,
            //0.3,
            0.5,
            //0.7,
            //0.9,
            Priority = 5
        )]
        public double Goal { get; set; }

        [Params
        (
            //1,
            //3,
            5,
            //10,
            Priority = 6
        )]
        public double MutationCoefficient { get; set; }

        #endregion

        private static readonly Random _random = new Random();

        private decimal[] _initialSet;
        private decimal[] _set;
        private IOneToManyGeneticAlgorithm _algorithm;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _initialSet = InitialSetProvider.GetInitialSet();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _set = SetGenerator.TakeN(_initialSet, N, _random);

            var subsetSum = (decimal)Math.Round(Goal * (double)_set.Sum(), 2);

            _algorithm = CreateAlgorithm(new OneToManyGeneticOptions
            {
                Random = _random,
                Set = _set,
                SubsetSum = subsetSum,
                FitnessThreshold = (decimal)FitnessThreshold,
                GenerationSize = GenSize,
                MutationChance = MutationCoefficient / GenSize,

                GenerationsMaxCount = int.MaxValue,
                Timeout = TimeSpan.FromSeconds(30),

                InitialSeeding = Filling,
                GenesSorting = Sorting,
            });
        }

        [Benchmark]
        public void Test()
        {
            _algorithm.Reset();

            while (true)
            {
                var result = _algorithm.NextIteration();

                if (result.IsCompleted || result.IsThresholdSatisfied)
                {
                    break;
                }
            }
        }

        protected abstract IOneToManyGeneticAlgorithm CreateAlgorithm(OneToManyGeneticOptions  options);
    }
}
