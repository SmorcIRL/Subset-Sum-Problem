using System.Diagnostics;
using System.Reflection;
using GeneticAlgorithm.OneToMany;
using Newtonsoft.Json;

namespace Example
{
    public class Program
    {
        private static void Main()
        {
            var options = GetOptions();

            var algorithm = new OneToManyGeneticAlgorithm(options);

            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                var result = algorithm.NextIteration();

                Console.WriteLine($"[{result.Generation:D5}] Fitness: {result.IterationBestFitness} Subset size: {result.IterationBestBitsSet}");

                if (result.IsCompleted)
                {
                    Console.WriteLine("==============================================");
                    Console.WriteLine($"Fitness: {result.BestFitness} Subset size: {result.BestSolution.Count}");
                    Console.WriteLine($"Subset: {string.Join(", ", result.BestSolution)}");

                    break;
                }
            }

            Console.WriteLine($"Total: {stopwatch.Elapsed.TotalSeconds:F5} (s)");

            Console.ReadLine();
        }

        private static OneToManyGeneticOptions GetOptions()
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var configPath = Path.Combine(basePath, "Config.json");
            var setPath = Path.Combine(basePath, "Set.json");

            var configJson = File.ReadAllText(configPath);
            var setJson = File.ReadAllText(setPath);

            var options = JsonConvert.DeserializeObject<Options>(configJson);
            var set = JsonConvert.DeserializeObject<decimal[]>(setJson);

            return new OneToManyGeneticOptions
            {
                Random = new Random(options.Seed),
                Set = set,
                SubsetSum = options.SubsetSum,
                FitnessThreshold = options.FitnessThreshold,
                GenerationSize = options.GenerationSize,
                MutationChance = options.MutationChance,
                GenerationsMaxCount = options.GenerationsMaxCount ?? int.MaxValue,
                Timeout = options.Timeout ?? TimeSpan.MaxValue,
                InitialSeeding = options.InitialSeeding,
                GenesSorting = options.GenesSorting,
            };
        }
    }
}