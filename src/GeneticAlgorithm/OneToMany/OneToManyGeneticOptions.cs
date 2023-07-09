namespace GeneticAlgorithm.OneToMany
{
    public class OneToManyGeneticOptions
    {
        public Random Random { get; init; }

        public decimal[] Set { get; init; }

        public decimal SubsetSum { get; init; }

        public decimal FitnessThreshold { get; init; }

        public int GenerationSize { get; init; }

        public double MutationChance { get; init; }

        public int GenerationsMaxCount { get; init; }

        public TimeSpan Timeout { get; init; }

        public OneToManyInitialSeeding InitialSeeding { get; init; }

        public OneToManyGenesSorting GenesSorting { get; init; }
    }
}
