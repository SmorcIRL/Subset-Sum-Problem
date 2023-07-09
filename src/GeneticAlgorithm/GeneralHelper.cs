namespace GeneticAlgorithm
{
    public static class GeneralHelper
    {
        public static (T, T) GetMaxTwo<T>(T a, T b, T c, T d) where T : IComparable<T>
        {
            if (a.CompareTo(b) < 0)
            {
                if (b.CompareTo(c) < 0)
                {
                    return d.CompareTo(b) < 0 ? (a, d) : (a, b);
                }

                if (a.CompareTo(c) < 0)
                {
                    return d.CompareTo(c) < 0 ? (a, d) : (a, c);
                }

                return d.CompareTo(a) < 0 ? (c, d) : (c, a);
            }

            if (a.CompareTo(c) < 0)
            {
                return d.CompareTo(a) < 0 ? (b, d) : (b, a);
            }

            if (b.CompareTo(c) < 0)
            {
                return d.CompareTo(c) < 0 ? (b, d) : (b, c);
            }

            return d.CompareTo(b) < 0 ? (c, d) : (c, b);
        } 
        
        public static decimal[] HumpSort(Random random, IEnumerable<decimal> values)
        {
            var array = values.OrderBy(_ => random.Next()).ToArray();

            return Enumerable.Empty<decimal>()
                .Concat(array.Take(array.Length / 2).OrderBy(x => x))
                .Concat(array.Skip(array.Length / 2).OrderByDescending(x => x))
                .ToArray();
        }

        public static bool GetBool(this Random random, double probability = 0.5)
        {
            return random.NextDouble() < probability;
        }
    }
}
