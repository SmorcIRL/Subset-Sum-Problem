using Benchmark.Helpers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using GeneticAlgorithm.ManyToMany;

namespace Benchmark.Benchmarks
{
    [SimpleJob(RunStrategy.Monitoring, 1, 1, 20, 5)]
    public class ManyToManyBenchmark
    {
        #region Params

        [Params(0.1, Priority = 0)]
        public double FitnessThreshold { get; set; }

        [Params
        (
            //500,
            1000,
            //10000,
            Priority = 1
        )]
        public int N { get; set; }

        [Params
        (
            //ManyToManyInitialSeeding.FullRandom,
            //ManyToManyInitialSeeding.AllUnset,
            //ManyToManyInitialSeeding.AllSet,
            ManyToManyInitialSeeding.RandomSum,
            //ManyToManyInitialSeeding.RandomSubArraySum,
            //ManyToManyInitialSeeding.RandomSubArraySumHalf,
            ManyToManyInitialSeeding.RandomSumUntilExceedingMinHalfSum,
            //ManyToManyInitialSeeding.RandomSubArraySumUntilExceedingMinHalfSubSum,
            Priority = 2
        )]
        public ManyToManyInitialSeeding FillingType { get; set; }

        [Params
        (
            ManyToManyGenesSorting.Increasing,
            ManyToManyGenesSorting.Hump,
            Priority = 3
        )]
        public ManyToManyGenesSorting Sorting { get; set; }

        [Params
        (
            //50,
            100,
            //200,
            Priority = 4
        )]
        public int GenSize { get; set; }

        [Params
        (
            //1,
            2,
            Priority = 5
        )]
        public int SetGenerationMode { get; set; }

        #endregion

        private decimal[] _initialSet;
        private decimal[] _set1;
        private decimal[] _set2;
        private ManyToManyGeneticAlgorithm _algorithm;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _initialSet = InitialSetProvider.GetInitialSet();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var random = new Random();

            switch (SetGenerationMode)
            {
                case 1:
                    _set1 = SetGenerator.TakeN(_initialSet, N / 2, random);
                    _set2 = SetGenerator.TakeN(_initialSet, N / 2, random);
                    break;
                case 2:
                    (_set1, _set2) = SetGenerator.TakeNWithoutIntersection(_initialSet, N / 2, random);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _algorithm = new ManyToManyGeneticAlgorithm(new ManyToManyGeneticOptions
            {
                Random = random,
                FirstSet = _set1,
                SecondSet = _set2,
                FitnessThreshold = (decimal)FitnessThreshold,
                GenerationSize = GenSize,
                MutationChance = 1d / GenSize,

                GenerationsMaxCount = int.MaxValue,
                Timeout = TimeSpan.FromSeconds(30),

                InitialSeeding = FillingType,
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
    }
}
