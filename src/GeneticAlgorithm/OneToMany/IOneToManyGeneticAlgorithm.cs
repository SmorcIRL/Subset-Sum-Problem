namespace GeneticAlgorithm.OneToMany;

public interface IOneToManyGeneticAlgorithm
{
    void Reset();
    OneToManyIterationResult NextIteration();
}