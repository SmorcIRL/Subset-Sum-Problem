namespace GeneticAlgorithm.ManyToMany
{
    public enum ManyToManyInitialSeeding
    {
        FullRandom = 0,
        AllUnset = 1,
        AllSet = 2,
        RandomSum = 3,
        RandomSubArraySum = 4,
        RandomSubArraySumHalf = 5, 
        RandomSumUntilExceedingMinHalfSum = 6,
        RandomSubArraySumUntilExceedingMinHalfSubSum = 7,
    }
}
