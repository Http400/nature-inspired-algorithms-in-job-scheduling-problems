using System.Collections.Generic;
using System.Linq;
using Core.JobScheduling.Base;

namespace Core.Algorithms
{
    public abstract class Algorithm
    {
        protected int _maxIterations;
        protected int _populationCount;
        protected SchedulingProblem _schedulingProblem;
        protected List<Solution> _population;
        protected Solution _globalOptimum;
        protected List<int> _solutionsDiversities;

        public Algorithm()
        {
            _solutionsDiversities = new List<int>();
        }

        public abstract Solution Perform();

        protected virtual void CalculateSolutionsDiversity()
        {
            var diverseSolutions = new List<Solution>();

            foreach (var solution in _population)
            {
                var sameSolutionFound = false;

                foreach (var dS in diverseSolutions)
                {
                    if ( !solution.IsDifferent(dS) )
                    {
                        sameSolutionFound = true;
                        break;
                    }
                }

                if (!sameSolutionFound)
                {
                    diverseSolutions.Add(solution);
                }
            }

            _solutionsDiversities.Add(diverseSolutions.Count);
        }

        // protected virtual void CalculateSolutionsDiversity()
        // {
        //     var diverseSolutions = new List<Solution>();

        //     foreach (var solution in _population)
        //     {
        //         if ( diverseSolutions.Count == 0 || ListDoesNotContainsSolution(diverseSolutions, solution) )
        //         {
        //             diverseSolutions.Add(solution);
        //         }
        //     }

        //     _solutionsDiversities.Add(diverseSolutions.Count);
        // }

        private bool ListDoesNotContainsSolution(List<Solution> solutionList, Solution solution)
        {
            foreach (var dS in solutionList)
            {
                for (int i = 0; i < solution.Machines.Count; i++)
                {
                    for (int j = 0; j < solution.Machines[i].Operations.Count; j++)
                    {
                        if (solution.TimeSpan != dS.TimeSpan 
                        || solution.Machines[i].Operations[j].StartTime != dS.Machines[i].Operations[j].StartTime 
                        || solution.Machines[i].Operations[j].Operation != dS.Machines[i].Operations[j].Operation)
                            return true;
                    }
                }
            }

            return false;
        }

        protected virtual void GatherAlgorithmEfficiencyInformation(int i)
        {
            // System.IO.File.AppendAllText(
            //     System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestResults/eff.txt", 
            //     "Iteration: " + i + ", solutionsDiversity: " + _solutionsDiversities.Last() + ", globalOpt: " + _globalOptimum.TimeSpan + System.Environment.NewLine
            // );
        }
    }
}