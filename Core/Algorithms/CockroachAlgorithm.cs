using System;
using System.Collections.Generic;
using Core.JobScheduling.Base;

namespace Core.Algorithms
{
    public class CockroachAlgorithm : Algorithm
    {
        private int _maxStep;
        private int _visual;
        // private int _maxIterations;
        // private int _populationCount;
        // private SchedulingProblem _schedulingProblem;
        //private List<Solution> _population;
        // private Solution _globalOptimum;

        public CockroachAlgorithm(int maxStep, int visual, int maxIterations, int populationCount, SchedulingProblem schedulingProblem) : base()
        {
            _maxStep = maxStep;
            _visual = visual;
            _maxIterations = maxIterations;
            _schedulingProblem = schedulingProblem;
            _populationCount = populationCount;
            _population = CreatePopulation();
            FindGlobalOptimum();
        }

        public override Solution Perform()
        {
            for (int i = 0; i < _maxIterations; i++)
            {
                SwarmChasing();
                Dispersion();
                UpdateCriterionValue();
                FindGlobalOptimum();
                RuthlessBehavior();

                CalculateSolutionsDiversity();
                GatherAlgorithmEfficiencyInformation(i);
            }

            return _globalOptimum;
        }

        public List<Solution> GetPopulation()
        {
            return _population;
        }

        private void SwarmChasing()
        {
            foreach (var cockroach in _population)
            {
                var localOptimum = cockroach.FindLocalOptimum(_population, _visual);

                if (localOptimum.IsNotNull())
                    cockroach.MakeMoveTo(localOptimum, _maxStep);
                else
                    cockroach.MakeMoveTo(_globalOptimum, _maxStep);     
            }
        }

        private void Dispersion()
        {
            foreach (var cockroach in _population)
            {
                cockroach.Disperse();
            }
        }

        private void RuthlessBehavior()
        {
            var random = new Random();
            var randomIndividualIndex = random.Next(0, _population.Count);
            _population[randomIndividualIndex] = _globalOptimum.Clone();
        }

        private void UpdateCriterionValue()
        {
            foreach (var cockroach in _population)
            {
                cockroach.PropagateOperations(_schedulingProblem.Jobs);
                cockroach.CalculateTimeSpan();
            }
        }

        private List<Solution> CreatePopulation()
        {
            return _schedulingProblem.CreateCockroachesPopulation(_populationCount);
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
    }
}