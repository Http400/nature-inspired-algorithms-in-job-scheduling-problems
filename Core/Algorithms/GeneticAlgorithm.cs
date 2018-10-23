using System;
using System.Collections.Generic;
using Core.JobScheduling.Base;

namespace Core.Algorithms
{
    public class GeneticAlgorithm : Algorithm
    {
        private float _mutationProbability;

        public GeneticAlgorithm(int maxIterations, int populationCount, SchedulingProblem schedulingProblem, float mutationProbability = 1)
        {
            _maxIterations = maxIterations;
            _populationCount = populationCount;
            _schedulingProblem = schedulingProblem;
            _mutationProbability = mutationProbability;
            _population = CreatePopulation();
        }

        public override void PerformStep()
        {
            CreateNextGeneration();
            Mutation();
            UpdateCriterionValue();
            FindGlobalOptimum();
            // CalculateSolutionsDiversity();
            // GatherAlgorithmEfficiencyInformation();
        }

        public List<Solution> GetPopulation()
        {
            return _population;
        }

        public void UpdateCriterionValue()
        {
            foreach (var genotype in _population)
            {
                genotype.PropagateOperations(_schedulingProblem.Jobs);
                genotype.CalculateTimeSpan();
            }
        }

        public void Mutation()
        {
            var random = new Random();

            foreach (var genotype in _population)
            {
                if ( random.Next(0, 101) <= _mutationProbability * 100 )
                    genotype.Mutate();
            }
        }

        public void CreateNextGeneration()
        {
            var nextGeneration = new List<Solution>();

            while (nextGeneration.Count < _populationCount)
            {
                var firstSelectedGenotype = SelectGenotype();
                var secondSelectedGenotype = SelectGenotype();

                var descendants = firstSelectedGenotype.DoCrossing(secondSelectedGenotype);

                if (descendants.solution1.IsNotNull() && descendants.solution2.IsNotNull())
                {
                    descendants.solution1.PropagateOperations(_schedulingProblem.Jobs);
                    descendants.solution1.CalculateTimeSpan();
                    descendants.solution2.PropagateOperations(_schedulingProblem.Jobs);
                    descendants.solution2.CalculateTimeSpan();

                    nextGeneration.Add(descendants.solution1);
                    nextGeneration.Add(descendants.solution2); 
                }
            }

            _population = nextGeneration;
        }

        private void FindGlobalOptimum()
        {
            _globalOptimum = _population[0];

            foreach (var cockroach in _population)
            {
                if (_globalOptimum.TimeSpan > cockroach.TimeSpan)
                {
                    _globalOptimum = cockroach;
                }
            }
        }

        private Solution SelectGenotype()
        {
            var probabilitySum = GetProbabilitySum();
            float cumulative = 0.0F;
            int random = new Random().Next(1, 101);

            foreach (var genotype in _population)
            {
                cumulative += (1 / (float)genotype.TimeSpan) / probabilitySum * 100;
                if (random - cumulative < 1)
                {
                    return genotype;
                }
            }

            return null;
        }

        private float GetProbabilitySum()
        {
            float sum = 0;

            foreach (var genotype in _population)
            {
                sum += 1 / (float)genotype.TimeSpan;
            }

            return sum;
        }

        private List<Solution> CreatePopulation()
        {
            return _schedulingProblem.CreateGenotypesPopulation(_populationCount);
        }
    }
}