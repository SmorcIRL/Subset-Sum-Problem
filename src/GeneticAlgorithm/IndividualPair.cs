namespace GeneticAlgorithm
{
    public class IndividualPair : IComparable<IndividualPair>
    {
        public readonly Individual First;
        public readonly Individual Second;
        public decimal Fitness = Const.MaxFitness;
        public int TotalBitsSet;

        public IndividualPair(int size1, int size2)
        {
            First = new Individual(size1);
            Second = new Individual(size2);
        }

        public int CompareTo(IndividualPair other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        public void SetAll(bool value)
        {
            First.SetAll(value);
            Second.SetAll(value);
        }
    }
}
