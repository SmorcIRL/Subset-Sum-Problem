namespace GeneticAlgorithm.ManyToMany
{
    public class ManyToManyGeneticAlgorithm
    {  
        private readonly Random _random; 
        private readonly decimal[] _set1;
        private readonly decimal[] _set2;  
        private readonly int _setSize1;
        private readonly int _setSize2;
        private readonly int _setSizesSum; 
        private readonly double _chanceToMutateFirst;
        private readonly decimal _fitnessThreshold; 
        private readonly int _generationSize;
        private readonly double _mutationChance;
        private readonly int _generationsMaxCount;
        private readonly ManyToManyInitialSeeding _initialSeeding;
        private readonly TimeSpan _timeout;

        private DateTime _stopDateTime;
        private int _generationCounter;
        private IndividualPair[] _currentGeneration;
        private IndividualPair[] _nextGeneration;

        private readonly (List<decimal> SubSet1, List<decimal> SubSet2) _bestSolution;
        private decimal _bestFitness;

        public ManyToManyGeneticAlgorithm(ManyToManyGeneticOptions options)
        {
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

            if (options.FirstSet is not {Length: > 0} || options.SecondSet is not {Length: > 0})
            {
                throw new ArgumentOutOfRangeException();
            }

            if (options.FirstSet.Any(x => x <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(options.FirstSet), "Set elements should be >= 0");
            }

            if (options.SecondSet.Any(x => x <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(options.SecondSet), "Set elements should be >= 0");
            }

            _random = options.Random ?? throw new ArgumentNullException(nameof(options.Random));

            (_set1, _set2) = SortValues(options.FirstSet, options.SecondSet, _random, options.GenesSorting);
            _setSize1 = _set1.Length;
            _setSize2 = _set2.Length;
            _setSizesSum = _setSize1 + _setSize2;
            _chanceToMutateFirst = (double)_setSize1 / _setSizesSum;
            _fitnessThreshold = options.FitnessThreshold;
            _mutationChance = options.MutationChance;
            _generationSize = options.GenerationSize;
            _timeout = options.Timeout;
            _generationsMaxCount = options.GenerationsMaxCount;
            _initialSeeding = options.InitialSeeding;

            _currentGeneration = new IndividualPair[_generationSize];
            for (var i = 0; i < _generationSize; i++)
            {
                _currentGeneration[i] = new IndividualPair(_setSize1, _setSize2);
            }

            _nextGeneration = new IndividualPair[_generationSize];
            for (var i = 0; i < _generationSize; i++)
            {
                _nextGeneration[i] = new IndividualPair(_setSize1, _setSize2);
            }

            _bestSolution = (new List<decimal>(), new List<decimal>());

            Reset();
        }

        public void Reset()
        {
            _generationCounter = 0;
            _stopDateTime = DateTime.UtcNow + _timeout;
            _bestSolution.SubSet1.Clear();
            _bestSolution.SubSet2.Clear();
            _bestFitness = Const.MaxFitness;

            FillGeneration(_currentGeneration, _initialSeeding);
            FillGeneration(_nextGeneration, ManyToManyInitialSeeding.AllUnset);
        }

        public ManyToManyIterationResult NextIteration()
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

                var countPart1 = _random.Next(1, _setSizesSum - 1);
                var countPart2 = _setSizesSum - countPart1;

                if (countPart1 >= _setSize1)
                {
                    var countOver = countPart1 - _setSize1;

                    Array.Copy(parent1.First.BitMask, child1.First.BitMask, _setSize1);
                    Array.Copy(parent1.Second.BitMask, child1.Second.BitMask, countOver);
                    Array.Copy(parent2.Second.BitMask, countOver, child1.Second.BitMask, countOver, countPart2);

                    Array.Copy(parent2.First.BitMask, child2.First.BitMask, _setSize1);
                    Array.Copy(parent2.Second.BitMask, child2.Second.BitMask, countOver);
                    Array.Copy(parent1.Second.BitMask, countOver, child2.Second.BitMask, countOver, countPart2);
                }
                else
                {
                    var countUnder = _setSize1 - countPart1;

                    Array.Copy(parent1.First.BitMask, child1.First.BitMask, countPart1);
                    Array.Copy(parent2.First.BitMask, countPart1, child1.First.BitMask, countPart1, countUnder);
                    Array.Copy(parent2.Second.BitMask, child1.Second.BitMask, _setSize2);

                    Array.Copy(parent2.First.BitMask, child2.First.BitMask, countPart1);
                    Array.Copy(parent1.First.BitMask, countPart1, child2.First.BitMask, countPart1, countUnder);
                    Array.Copy(parent1.Second.BitMask, child2.Second.BitMask, _setSize2);
                }

                UpdateFitness(child1);
                UpdateFitness(child2);

                var (best1, best2) = GeneralHelper.GetMaxTwo(parent1, parent2, child1, child2);

                if ((best1, best2) == (child1, child2) || (best1, best2) == (child2, child1))
                {
                    continue;
                }

                if (best1 != child1)
                {
                    Array.Copy(best1.First.BitMask, child1.First.BitMask, _setSize1);
                    Array.Copy(best1.Second.BitMask, child1.Second.BitMask, _setSize2);

                    child1.Fitness = best1.Fitness;
                    child1.TotalBitsSet = best1.TotalBitsSet;
                }

                if (best2 != child2)
                {
                    Array.Copy(best2.First.BitMask, child2.First.BitMask, _setSize1);
                    Array.Copy(best2.Second.BitMask, child2.Second.BitMask, _setSize2);

                    child2.Fitness = best2.Fitness;
                    child2.TotalBitsSet = best2.TotalBitsSet;
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

                    if (_random.GetBool(_chanceToMutateFirst))
                    {
                        var bitIndex = GetRandomBitIndex1();
                        individual.First[bitIndex] = !individual.First[bitIndex];
                    }
                    else
                    {
                        var bitIndex = GetRandomBitIndex2();
                        individual.Second[bitIndex] = !individual.Second[bitIndex];
                    }

                    UpdateFitness(individual);
                }
            }
        }
        
        private void UpdateBestValues()
        {
            Array.Sort(_currentGeneration);

            var iterationBest = _currentGeneration[0];
            if (iterationBest.Fitness < _bestFitness
                || (iterationBest.Fitness == _bestFitness && iterationBest.TotalBitsSet > _bestSolution.SubSet1.Count + _bestSolution.SubSet2.Count))
            {
                _bestFitness = iterationBest.Fitness;

                _bestSolution.SubSet1.Clear();
                for (var i = 0; i < _setSize1; i++)
                {
                    if (iterationBest.First[i])
                    {
                        _bestSolution.SubSet1.Add(_set1[i]);
                    }
                }

                _bestSolution.SubSet2.Clear();
                for (var i = 0; i < _setSize2; i++)
                {
                    if (iterationBest.Second[i])
                    {
                        _bestSolution.SubSet2.Add(_set2[i]);
                    }
                }
            }
        }

        private ManyToManyIterationResult CreateIterationResult(bool isCompleted)
        {
            return new ManyToManyIterationResult
            {
                IsCompleted = isCompleted,
                IsThresholdSatisfied = _bestFitness <= _fitnessThreshold,
                Generation = _generationCounter,
                BestFitness = _bestFitness,
                BestSolution = _bestSolution,
                IterationBestFitness = _currentGeneration[0].Fitness,
                IterationBestBitsSet = _currentGeneration[0].TotalBitsSet,
            };
        }

        #endregion


        #region Support

        private void FillGeneration(IndividualPair[] generation, ManyToManyInitialSeeding fillingType)
        {
            switch (fillingType)
            {
                case ManyToManyInitialSeeding.FullRandom:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];

                        for (var j = 0; j < _setSize1; j++)
                        {
                            individual.First[j] = _random.GetBool();
                        }

                        for (var j = 0; j < _setSize2; j++)
                        {
                            individual.Second[j] = _random.GetBool();
                        }
                    }

                    break;
                }
                case ManyToManyInitialSeeding.AllUnset:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        generation[i].SetAll(false);
                    }

                    break;
                }
                case ManyToManyInitialSeeding.AllSet:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        generation[i].SetAll(true);
                    }

                    break;
                }
                case ManyToManyInitialSeeding.RandomSum:
                {
                    var halfSum1 = _set1.Sum() / 2;
                    var halfSum2 = _set2.Sum() / 2;
                    var minHalfSum = halfSum1 < halfSum2
                        ? halfSum1
                        : halfSum2;

                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var randomSum = _random.Next((int)minHalfSum);

                        var sum = 0m;

                        while (true)
                        {
                            var j = GetRandomBitIndex1();

                            if (individual.First[j])
                            {
                                continue;
                            }

                            individual.First[j] = true;
                            sum += _set1[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }
                        }

                        sum = 0m;

                        while (true)
                        {
                            var j = GetRandomBitIndex2();

                            if (individual.Second[j])
                            {
                                continue;
                            }

                            individual.Second[j] = true;
                            sum += _set2[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }
                        }
                    }

                    break;
                }
                case ManyToManyInitialSeeding.RandomSubArraySum:
                {
                    var sum1 = _set1.Sum();
                    var sum2 = _set2.Sum();
                    var minSum = sum1 < sum2
                        ? sum1
                        : sum2;

                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var randomSum = _random.Next((int)minSum);

                        var sum = 0m;
                        var count = 0;

                        for (var j = GetRandomBitIndex1(); j < _setSize1 && count < _setSize1; j++, count++)
                        {
                            individual.First[j] = true;
                            sum += _set1[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }

                            if (j == _setSize1 - 1)
                            {
                                j = 0;
                            }
                        }

                        sum = 0m;
                        count = 0;

                        for (var j = GetRandomBitIndex2(); j < _setSize2 && count < _setSize2; j++, count++)
                        {
                            individual.Second[j] = true;
                            sum += _set2[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }

                            if (j == _setSize2 - 1)
                            {
                                j = 0;
                            }
                        }
                    }

                    break;
                }
                case ManyToManyInitialSeeding.RandomSubArraySumHalf:
                {
                    var sum1 = _set1.Sum() / 2;
                    var sum2 = _set2.Sum() / 2;
                    var minSum = sum1 < sum2
                        ? sum1
                        : sum2;

                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var randomSum = _random.Next((int)minSum);

                        var sum = 0m;
                        var count = 0;

                        for (var j = GetRandomBitIndex1(); j < _setSize1 && count < _setSize1; j++, count++)
                        {
                            individual.First[j] = true;
                            sum += _set1[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }

                            if (j == _setSize1 - 1)
                            {
                                j = 0;
                            }
                        }

                        sum = 0m;
                        count = 0;

                        for (var j = GetRandomBitIndex2(); j < _setSize2 && count < _setSize2; j++, count++)
                        {
                            individual.Second[j] = true;
                            sum += _set2[j];

                            if (sum >= randomSum)
                            {
                                break;
                            }

                            if (j == _setSize2 - 1)
                            {
                                j = 0;
                            }
                        }
                    }

                    break;
                }
                case ManyToManyInitialSeeding.RandomSumUntilExceedingMinHalfSum:
                {
                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var halfSum1 = _set1.Sum() / 2;
                        var halfSum2 = _set2.Sum() / 2;
                        var minHalfSum = halfSum1 < halfSum2
                            ? halfSum1
                            : halfSum2;
                        var sum = 0m;

                        for (var j = 0; j < _setSize1; j++)
                        {
                            var index = GetRandomBitIndex1();

                            if (!individual.First[index])
                            {
                                individual.First[index] = true;
                                sum += _set1[index];
                            }

                            if (sum >= minHalfSum)
                            {
                                break;
                            }
                        }

                        sum = 0m;

                        for (var j = 0; j < _setSize2; j++)
                        {
                            var index = GetRandomBitIndex2();

                            if (!individual.Second[index])
                            {
                                individual.Second[index] = true;
                                sum += _set2[index];
                            }

                            if (sum >= minHalfSum)
                            {
                                break;
                            }
                        }
                    }

                    break;
                }
                case ManyToManyInitialSeeding.RandomSubArraySumUntilExceedingMinHalfSubSum:
                {
                    var halfSum1 = _set1.Sum() / 2;
                    var halfSum2 = _set2.Sum() / 2;
                    var minHalfSum = halfSum1 < halfSum2
                        ? halfSum1
                        : halfSum2;

                    for (var i = 0; i < _generationSize; i++)
                    {
                        var individual = generation[i];
                        individual.SetAll(false);

                        var sum = 0m;
                        var count = 0;

                        for (var j = GetRandomBitIndex1(); j < _setSize1 && count < _setSize1; j++, count++)
                        {
                            individual.First[j] = true;
                            sum += _set1[j];

                            if (sum >= minHalfSum)
                            {
                                break;
                            }

                            if (j == _setSize1 - 1)
                            {
                                j = 0;
                            }
                        }

                        sum = 0m;
                        count = 0;

                        for (var j = GetRandomBitIndex2(); j < _setSize2 && count < _setSize2; j++, count++)
                        {
                            individual.Second[j] = true;
                            sum += _set2[j];

                            if (sum >= minHalfSum)
                            {
                                break;
                            }

                            if (j == _setSize2 - 1)
                            {
                                j = 0;
                            }
                        }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(fillingType));
            }

            foreach (var individual in generation)
            {
                UpdateFitness(individual);
            }

            Array.Sort(generation);
        }

        private static (decimal[], decimal[]) SortValues(IEnumerable<decimal> values1, IEnumerable<decimal> values2, Random random, ManyToManyGenesSorting sorting)
        {
            return sorting switch
            {
                ManyToManyGenesSorting.NoSorting => 
                (
                    values1.ToArray(),
                    values2.ToArray()
                ),
                ManyToManyGenesSorting.Random => 
                (
                    values1.OrderBy(_ => random.Next()).ToArray(),
                    values2.OrderBy(_ => random.Next()).ToArray()
                ),
                ManyToManyGenesSorting.Increasing => 
                (
                    values1.OrderBy(x => x).ToArray(),
                    values2.OrderBy(x => x).ToArray()
                ),
                ManyToManyGenesSorting.Hump => 
                (
                    values1.OrderBy(x => x).ToArray(),
                    values2.OrderByDescending(x => x).ToArray()
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null),
            };
        }
        
        private void UpdateFitness(IndividualPair individual)
        {
            individual.Fitness = CalculateFitness(individual, out var bitsSet);
            individual.TotalBitsSet = bitsSet;
        }

        private decimal CalculateFitness(IndividualPair individual, out int countSet)
        {
            var sum = 0m;
            var countSet1 = 0;
            var countSet2 = 0;

            for (var i = 0; i < _setSize1; i++)
            {
                if (individual.First[i])
                {
                    sum += _set1[i];
                    countSet1++;
                }
            }

            for (var i = 0; i < _setSize2; i++)
            {
                if (individual.Second[i])
                {
                    sum -= _set2[i];
                    countSet2++;
                }
            }

            countSet = countSet1 + countSet2;

            if (countSet1 == 0 || countSet2 == 0)
            {
                return Const.MaxFitness;
            }

            return Math.Abs(sum);
        }

        private int GetRandomBitIndex1()
        {
            return _random.Next(_setSize1);
        }

        private int GetRandomBitIndex2()
        {
            return _random.Next(_setSize2);
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
