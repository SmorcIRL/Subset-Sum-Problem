namespace GeneticAlgorithm
{
    public class Individual128 : IComparable<Individual128>
    {
        public UInt128 BitMask;
        public decimal Fitness = Const.MaxFitness;
        public int BitsSet;

        public int CompareTo(Individual128 other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        public bool this[int index]
        {
            get => (BitMask & UInt128.One << index) != 0;
            set
            {
                if (value)
                {
                    BitMask |= UInt128.One << index;
                }
                else
                {
                    BitMask &= ~(UInt128.One << index);
                }
            }
        }

        public void SetAll(bool value)
        {
            BitMask = value ? UInt128.MaxValue : UInt128.Zero;
        }
    }
}
