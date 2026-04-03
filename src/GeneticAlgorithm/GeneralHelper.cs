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

        public static decimal[] HumpSort(IEnumerable<decimal> values)
        {
            var sorted = values.OrderBy(x => x).ToArray();
            var result = new decimal[sorted.Length];

            var left = 0;
            var right = result.Length - 1;

            for (var i = 0; i < sorted.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    result[left++] = sorted[i];
                }
                else
                {
                    result[right--] = sorted[i];
                }
            }

            return result;
        }

        public static bool GetBool(this Random random, double probability = 0.5)
        {
            return random.NextDouble() < probability;
        }
    }
}
