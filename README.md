# Subset-Sum-Problem
[Subset sum problem (SSP)](https://en.wikipedia.org/wiki/Subset_sum_problem) is an NP problem that can arise in several scenarios. The main question is, given a set of numbers (integer/decimal/floating point), how to find a subset of a given sum. Since there are **N^2** possible subsets, brute force is impossible in most cases when **N > ~30**. There are some deterministic algorithms such as dynamic or graph-based, but they also have some limitations (for example, the dynamic one uses a lot of memory and relies heavily on the minimum distance between two numbers and is more suitable for integers).

The problem has several variations, for example [Partition problem](https://en.wikipedia.org/wiki/Partition_problem). This implementation covers the standard scenario - finding a subset of given sum (*1-Many*) and a variant with 2 sets, where we want to find subsets of the same sum (*Many-Many*). In my case I had to work with positive decimal numbers, but the solution can be adapted to negative numbers as well.

## Genetic alogoritm
If the exact solution is not absolutely necessary, we can try using the genetic algorithm. The algorithm is non-determenistic, so results can vary on each run for the same input. On each iteration it tries to give a solution that is better than on the previous iteration. It also relies heavily on the data itself (and the distribution when we're talking about SSP), so its parameters should be adjusted for each case. The most important parameters are: solution fitness threshold, how we seed the first generation, generation size and mutation chance. An iteration of the algorithm consists of several steps called operators (selection/crossover/mutation), which can also vary (in my case standard ones like single-point crossover work just fine).

## Notes
- I've found [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) to be very useful for parameter tuning. You can't really call it benchmarking if you're using a non-deterministic algorithm, but it gives us a way to easily do a run with different parameter values on different problem sizes and compare the results.
- For the *1-Many* scenario I've also created an additional implementation for the case where **N <= 128**. It uses [Int128](https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0) for bitmasks, which requires .NET 7. If that's not an option - just use the implementation with classes.
