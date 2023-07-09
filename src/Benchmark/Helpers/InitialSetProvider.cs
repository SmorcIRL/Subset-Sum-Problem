using System.Text.Json;

namespace Benchmark.Helpers
{
    public static class InitialSetProvider
    {
        private static decimal[] _initialSet;

        public static decimal[] GetInitialSet()
        {
            if (_initialSet != null)
            {
                return _initialSet;
            }

            var assembly = typeof(InitialSetProvider).Assembly;

            using var stream = assembly.GetManifestResourceStream($"{nameof(Benchmark)}.InitialSet.json");
            using var reader = new StreamReader(stream!);

            var json = reader.ReadToEnd();

            _initialSet = JsonSerializer.Deserialize<decimal[]>(json);

            return _initialSet;
        }
    }
}
