namespace GeneticAlgorithm.ManyToMany
{
    public class ManyToManyGeneticOptions
    {
        public Random Random { get; init; }

        public decimal[] FirstSet { get; init; }

        public decimal[] SecondSet { get; init; }

        public decimal FitnessThreshold { get; init; }

        public int GenerationSize { get; init; }

        public double MutationChance { get; init; }

        public int GenerationsMaxCount { get; init; }

        public TimeSpan Timeout { get; init; }

        public ManyToManyInitialSeeding InitialSeeding { get; init; }

        public ManyToManyGenesSorting GenesSorting { get; init; }
    }
}
