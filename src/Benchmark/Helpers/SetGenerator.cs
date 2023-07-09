namespace Benchmark.Helpers
{
    public static class SetGenerator
    {
        public static decimal[] TakeN(IReadOnlyList<decimal> initialSet, int count, Random random, Func<decimal, bool> filter = default)
        {
            var result = new decimal[count];

            for (var i = 0; i < count; i++)
            {
                while (true)
                {
                    var value = initialSet[random.Next(initialSet.Count)];
                    if (filter == null || filter(value))
                    {
                        result[i] = value;
                        break;
                    }
                }
            }

            return result;
        }

        public static (decimal[], decimal[]) TakeNWithoutIntersection(IReadOnlyList<decimal> initialSet, int count, Random random)
        {
            var set1 = new decimal[count];
            var set2 = new decimal[count];

            var hashSet1 = new HashSet<decimal>();
            var hashSet2 = new HashSet<decimal>();

            for (var i = 0; i < count; i++)
            {
                while (true)
                {
                    var element = initialSet[random.Next(initialSet.Count)];

                    if (!hashSet2.Contains(element))
                    {
                        hashSet1.Add(element);
                        set1[i] = element;
                        break;
                    }
                }

                while (true)
                {
                    var element = initialSet[random.Next(initialSet.Count)];

                    if (!hashSet1.Contains(element))
                    {
                        hashSet2.Add(element);
                        set2[i] = element;
                        break;
                    }
                }
            }

            return (set1, set2);
        }
    }
}
