namespace GeneticAlgorithm.OneToMany
{
    public class OneToManyGeneticAlgorithm
    {
        private readonly Random _random;
        private readonly decimal[] _set;  
        private readonly int _setSize;
        private readonly decimal _subsetSum;
        private readonly decimal _fitnessThreshold;
        private readonly int _generationSize;  
        private readonly double _mutationChance;
        private readonly int _generationsMaxCount;
        private readonly TimeSpan _timeout;
        private readonly OneToManyInitialSeeding _initialSeeding;

        private DateTime _stopDateTime;
        private int _generationCounter;
        private Individual[] _currentGeneration;
        private Individual[] _nextGeneration;

        private readonly List<decimal> _bestSolution;
        private decimal _bestFitness;

        public OneToManyGeneticAlgorithm(OneToManyGeneticOptions options)
        {
            if (options.SubsetSum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.SubsetSum), "Subset sum should be positive");
            }

            if (options.GenerationSize <= 0 || options.GenerationSize % 2 == 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options.GenerationSize), "Generation size must be a positive even number");
            }

            if (options.GenerationsMaxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.GenerationsMaxCount));
            }

            if (options.MutationChance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MutationChance));
            }

            if (options.Set.Any(x => x <= 0 || options.SubsetSum < x))
            {
                throw new ArgumentOutOfRangeException(nameof(options.Set), "Set elements should be x > 0 and <= target");
            }

            _random = options.Random ?? throw new ArgumentNullException(nameof(options.Random));
            _set = SortValues(options.Set, _random, options.GenesSorting);
            _setSize = _set.Length;
            _subsetSum = options.SubsetSum;
            _fitnessThreshold = options.FitnessThreshold;
            _mutationChance = options.MutationChance;
            _generationSize = options.GenerationSize;
            _timeout = options.Timeout;
            _generationsMaxCount = options.GenerationsMaxCount;
            _initialSeeding = options.InitialSeeding;

            _currentGeneration = new Individual[_generationSize];
            for (var i = 0; i < _generationSize; i++)
            {
                _currentGeneration[i] = new Individual(_setSize);
            }

            _nextGeneration = new Individual[_generationSize];
            for (var i = 0; i < _generationSize; i++)
            {
                _nextGeneration[i] = new Individual(_setSize);
            }

            _bestSolution = new List<decimal>();

            Reset();
        }

        public void Reset()
        {
            _generationCounter = 0;
            _stopDateTime = DateTime.UtcNow + _timeout;
            _bestSolution.Clear();
            _bestFitness = Const.MaxFitness;

            FillGeneration(_currentGeneration, _initialSeeding);
            FillGeneration(_nextGeneration, OneToManyInitialSeeding.AllUnset);
        }

        public OneToManyIterationResult NextIteration()
        {
            if (ShouldStop())
            {
                return CreateIterationResult(true);
            }

            _generationCounter++;

            Crossover();

            Mutation();

            UpdateBestValues();

            return CreateIterationResult(_bestFitness <= _fitnessThreshold);
        }

        #region Steps

        private void Crossover()
        {
            for (var i = 0; i < _generationSize; i += 2)
            {
                var parent1 = _currentGeneration[i];
                var parent2 = _currentGeneration[i + 1];

                var child1 = _nextGeneration[i];
                var child2 = _nextGeneration[i + 1];
                
                var countPart1 = _random.Next(1, _setSize - 1);
                var countPart2 = _setSize - countPart1;

                Array.Copy(parent1.BitMask, child1.BitMask, countPart1);
                Array.Copy(parent2.BitMask, child1.BitMask, countPart1);

                Array.Copy(parent2.BitMask, countPart1, child1.BitMask, countPart1, countPart2);
                Array.Copy(parent1.BitMask, countPart1, child1.BitMask, countPart1, countPart2);

                UpdateFitness(child1);
                UpdateFitness(child2);

                var (best1, best2) = GeneralHelper.GetMaxTwo(parent1, parent2, child1, child2);

                if ((best1, best2) == (child1, child2) || (best1, best2) == (child2, child1))
                {
                    continue;
                }

                if (best1 != child1)
                {
                    Array.Copy(best1.BitMask, child1.BitMask, _setSize);

                    child1.Fitness = best1.Fitness;
                    child1.BitsSet = best1.BitsSet;
                }

                if (best2 != child2)
                {
                    Array.Copy(best2.BitMask, child2.BitMask, _setSize);

                    child2.Fitness = best2.Fitness;
                    child2.BitsSet = best2.BitsSet;
                }
            }

            (_currentGeneration, _nextGeneration) = (_nextGeneration, _currentGeneration);
        }

        private void Mutation()
        {
            for (var i = 0; i < _generationSize; i++)
            {
                if (_random.GetBool(_mutationChance))
                {   
                    var individual = _currentGeneration[i];

                    var bitIndex = GetRandomBitIndex();
                    individual[bitIndex] = !individual[bitIndex];

                    UpdateFitness(individual);
                }
            }
        }

        private void UpdateBestValues()
        {
            Array.Sort(_currentGeneration);

            var iterationBest = _currentGeneration[0];
            if (iterationBest.Fitness < _bestFitness || (iterationBest.Fitness == _bestFitness && iterationBest.BitsSet > _bestSolution.Count))
            {
                _bestFitness = iterationBest.Fitness;

                _bestSolution.Clear();
                for (var i = 0; i < _setSize; i++)
                {
                    if (iterationBest[i])
                    {
                        _bestSolution.Add( _set[i]);
                    }
                }
            }
        }

        private OneToManyIterationResult CreateIterationResult(bool isCompleted)
        {
            return new OneToManyIterationResult
            {
                IsCompleted = isCompleted,
                IsThresholdSatisfied = _bestFitness <= _fitnessThreshold,
                Generation = _generationCounter,
                BestFitness = _bestFitness,
                BestSolution = _bestSolution,
                IterationBestFitness = _currentGeneration[0].Fitness,
                IterationBestBitsSet = _currentGeneration[0].BitsSet,
            };
        }

        #endregion

        #region Support

        private void FillGeneration(Individual[] generation, OneToManyInitialSeeding filling)
        {
            switch (filling)
            {
                case OneToManyInitialSeeding.FullRandom:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];

                        for (var j = 0; j < _setSize; j++)
                        {
                            individual[j] = _random.GetBool(_mutationChance);
                        }
                    }

                    break;
                }
                case OneToManyInitialSeeding.AllUnset:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        generation[i].SetAll(false);
                    }

                    break;
                }
                case OneToManyInitialSeeding.AllSet:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        generation[i].SetAll(true);
                    }

                    break;
                }
                case OneToManyInitialSeeding.RandomSumUntilExceeding:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var sum = 0m;

                        for (var j = 0; j < _setSize; j++)
                        {
                            var index = GetRandomBitIndex();

                            if (!individual[index])
                            {
                                individual[index] = true;
                                sum += _set[index];
                            }

                            if (sum >= _subsetSum)
                            {
                                break;
                            }
                        }
                    }

                    break;
                }
                case OneToManyInitialSeeding.RandomSubArraySumUntilExceeding:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var sum = 0m;
                        var count = 0;

                        for (var j = GetRandomBitIndex(); j < _setSize && count < _setSize; j++, count++)
                        {
                            individual[j] = true;
                            sum += _set[j];

                            if (sum >= _subsetSum)
                            {
                                break;
                            }

                            if (j == _setSize - 1)
                            {
                                j = 0;
                            }
                        }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(filling));
            }

            foreach (var individual in generation)
            {
                UpdateFitness(individual);
            }

            Array.Sort(generation);
        }

        private static decimal[] SortValues(IEnumerable<decimal> values, Random random, OneToManyGenesSorting sorting)
        {
            return sorting switch
            {
                OneToManyGenesSorting.NoSorting => values.ToArray(),
                OneToManyGenesSorting.Random => values.OrderBy(_ => random.Next()).ToArray(),
                OneToManyGenesSorting.Increasing => values.OrderBy(x => x).ToArray(),
                OneToManyGenesSorting.Hump => GeneralHelper.HumpSort(random, values),
                _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null),
            };
        }
        
        private void UpdateFitness(Individual individual)
        {
            individual.Fitness = CalculateFitness(individual, out var countSet);
            individual.BitsSet = countSet;
        }

        private decimal CalculateFitness(Individual individual, out int bitsSet)
        {
            var sum = _subsetSum;
            bitsSet = 0;

            for (var i = 0; i < _setSize; i++)
            {
                if (individual[i])
                {
                    bitsSet++;
                    sum -= _set[i];
                }
            }

            return bitsSet == 0
                ? Const.MaxFitness
                : Math.Abs(sum);
        }

        private int GetRandomBitIndex()
        {
            return _random.Next(_setSize);
        }

        private bool ShouldStop()
        {
            return _generationCounter >= _generationsMaxCount ||
                   DateTime.UtcNow > _stopDateTime ||
                   _bestFitness <= _fitnessThreshold;
        }

        #endregion
    }
}
