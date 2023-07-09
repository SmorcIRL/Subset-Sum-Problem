namespace GeneticAlgorithm.ManyToMany
{
    public readonly struct ManyToManyIterationResult
    {
        public bool IsCompleted { get; init; }

        public bool IsThresholdSatisfied { get; init; }

        public int Generation { get; init; }

        public (IReadOnlyList<decimal>, IReadOnlyList<decimal>) BestSolution { get; init; }

        public decimal BestFitness { get; init; }

        public decimal IterationBestFitness { get; init; }

        public int IterationBestBitsSet { get; init; }
    }
}
