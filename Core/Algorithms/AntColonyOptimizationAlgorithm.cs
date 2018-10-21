using System;
using System.Collections.Generic;
using Core.JobScheduling.Base;

namespace Core.Algorithms
{
    public class AntColonyOptimizationAlgorithm : Algorithm
    {
        private float _evaporationRate;
        private float _Q;
        private List<(int jobId, int? machineId)> _nodes;
        private List<Path> _paths;

        public AntColonyOptimizationAlgorithm(int maxIterations, int populationCount, float evaporationRate, SchedulingProblem schedulingProblem, float Q = 1F) : base()
        {
            _maxIterations = maxIterations;
            _schedulingProblem = schedulingProblem;
            _populationCount = populationCount;
            _evaporationRate = evaporationRate;
            _Q = Q;
            _population = schedulingProblem.CreateAntsPopulation(_populationCount);
            _nodes = schedulingProblem.GenerateGraphNodes();
            _paths = schedulingProblem.GeneratePaths(0.5F);
        }

        public override void PerformStep()
        {
            // System.IO.File.AppendAllText(
            //     System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestResults/iteration.txt", 
            //     "Iteration: " + i + Environment.NewLine
            // );

            PathSelecting();
            PheromoneEvaporation();
            UpdateCriterionValue();
            LocalPheromoneUpdate();
            FindGlobalOptimum();
        
            CalculateSolutionsDiversity();
            GatherAlgorithmEfficiencyInformation();
            ResetAntsPaths();
        }

        public List<Solution> GetPopulation()
        {
            return _population;
        }

        public List<(int jobId, int? machineId)> GetNodes()
        {
            return _nodes;
        }

        public List<Path> GetPaths()
        {
            return _paths;
        }

        private void ResetAntsPaths()
        {
            foreach (var ant in _population)
            {
                ant.Reset();
            }
        }

        private void UpdateCriterionValue()
        {
            foreach (var ant in _population)
            {
                ant.PropagateOperations(_schedulingProblem.Jobs);
                ant.CalculateTimeSpan();
            }
        }

        private void PathSelecting()
        {
            foreach (var ant in _population)
            {
                var startingFeasibleGraphNodes = ant.GetNextFeasibleGraphNodes(_schedulingProblem.Jobs);
                var random = new System.Random();
                var randomIndex = random.Next(0, startingFeasibleGraphNodes.Count);
                ant.AddGraphNode(startingFeasibleGraphNodes[randomIndex], _schedulingProblem.Jobs);
            }

            for (int m = 0; m < _nodes.Count - 1; m++)
            {
                foreach (var ant in _population)
                {
                    var lastGraphNode = ant.GetGraphNodeByIndex(m);
                    var nextFeasibleGraphNodes = ant.GetNextFeasibleGraphNodes(_schedulingProblem.Jobs);
                    var nextFeasiblePaths = GetNextFeasiblePaths(_paths, lastGraphNode, nextFeasibleGraphNodes);

                    var probabilitySum = GetPathsProbabilitySum(ant, nextFeasiblePaths);

                    float cumulative = 0.0F;
                    int random = new Random().Next(1, 101);
                    var selectedPath = new Path();

                    foreach (var path in nextFeasiblePaths)
                    {
                        cumulative += path.Pheromone / probabilitySum * 100;
                        if (random - cumulative < 1)
                        {
                            selectedPath = path;
                            break;
                        }
                    }

                    ant.AddGraphNode(selectedPath.EndingOperation, _schedulingProblem.Jobs);
                    ant.AddPath(selectedPath);
                }
            }
        }

        private void PheromoneEvaporation()
        {
            foreach (var path in _paths)
            {
                path.Pheromone = (1 - _evaporationRate) * path.Pheromone;
            }
        }

        private void LocalPheromoneUpdate()
        {
            foreach (var ant in _population)
            {
                foreach (var path in ant.Paths)
                {
                    path.Pheromone += _Q / ant.TimeSpan;
                }
            }
        }

        private float GetPathsProbabilitySum(Solution ant, List<Path> paths)
        {
            float sum = 0F;

            foreach (var path in paths)
            {
                sum += path.Pheromone;
            }

            return sum;
        }

        private void FindGlobalOptimum()
        {
            foreach (var ant in _population)
            {
                if (_globalOptimum == null || _globalOptimum.TimeSpan > ant.TimeSpan)
                    _globalOptimum = ant.Clone();
            }
        }

        private List<Path> GetNextFeasiblePaths(List<Path> paths, (int jobId, int? machineId) lastGraphNode, List<(int jobId, int? machineId)> nextFeasibleGraphNodes)
        {
            var nextFeasiblePaths = new List<Path>();

            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].StartingOperation.machineId != lastGraphNode.machineId || paths[i].StartingOperation.jobId != lastGraphNode.jobId)
                    continue;

                for (int j = 0; j < nextFeasibleGraphNodes.Count; j++)
                {
                    if (nextFeasibleGraphNodes[j].machineId == paths[i].EndingOperation.machineId && nextFeasibleGraphNodes[j].jobId == paths[i].EndingOperation.jobId)
                    {
                        nextFeasiblePaths.Add(paths[i]);
                        break;
                    }
                }
            }

            return nextFeasiblePaths;
        }

        private List<Path> GeneratePaths()
        {
            var paths = new List<Path>();
            
            foreach (var startingNode in _nodes)
            {
                foreach (var endingNode in _nodes)
                {
                    if (startingNode.jobId == endingNode.jobId && startingNode.machineId == endingNode.machineId)
                        continue;

                    paths.Add( new Path() {
                        StartingOperation = (startingNode.jobId, startingNode.machineId),
                        EndingOperation = (endingNode.jobId, endingNode.machineId),
                        Pheromone = 0.5F
                    } );
                }
            }

            return paths;
        }
    }
}