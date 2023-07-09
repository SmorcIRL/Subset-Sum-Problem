namespace GeneticAlgorithm
{
    public class Individual : IComparable<Individual>
    {
        public readonly bool[] BitMask;
        public decimal Fitness = Const.MaxFitness;
        public int BitsSet;

        public Individual(int bitCount)
        {
            BitMask = new bool[bitCount];
        }

        public int CompareTo(Individual other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        public bool this[int index]
        {
            get => BitMask[index];
            set => BitMask[index] = value;
        }

        public void SetAll(bool value)
        {
            for (var i = 0; i < BitMask.Length; i++)
            {
                this[i] = value;
            }
        }
    }
}
