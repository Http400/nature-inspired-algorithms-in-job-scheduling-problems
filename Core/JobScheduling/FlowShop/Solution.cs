using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;

namespace Core.JobScheduling.FlowShop
{
    public class Solution : Base.Solution
    {
        public List<int> JobsOrder { get; set; }


        public Solution() : base()
        {
            this.JobsOrder = new List<int>();
        }

        public Solution(int machinesCount) : base(machinesCount) 
        {
            this.JobsOrder = new List<int>();
        }

        public Solution(int machinesCount, List<Base.Job> jobs) : base(machinesCount)
        {
            GenerateRandomMachinesProcessingOrder(jobs.Count);
            PropagateOperations(jobs);
            CalculateTimeSpan();
        }
        
        public Solution(int machinesCount, List<Base.Job> jobs, List<int> jobsOrder) : base(machinesCount)
        {
            this.JobsOrder = jobsOrder;

            PropagateOperations(jobs);
            CalculateTimeSpan();
        }

        private void GenerateRandomMachinesProcessingOrder(int jobsCount)
        {
            this.JobsOrder = new List<int>();

            for (int j = 0; j < jobsCount; j++)
            {
                this.JobsOrder.Add(j);
            }

            this.JobsOrder.Shuffle();
        }

        public override void PropagateOperations(List<Base.Job> jobs)
        {
            base.PropagateOperations(jobs);

            foreach (var jobId in this.JobsOrder)
            {
                foreach (var machine in this.Machines)
                {
                    var previousMachine = GetMachineById(machine.Id - 1);
                    var startTime = Math.Max( 
                        previousMachine != null ? previousMachine.GetLastOperationEndTime() : 0, 
                        machine.GetLastOperationEndTime() 
                    );

                    machine.AddOperation( jobs[jobId].Operations[machine.Id], startTime );
                }
            }
        }

        private Base.Machine GetMachineById(int id)
        {
            return this.Machines.Find(m => m.Id == id);
        }

        public override List<(int jobId, int? machineId)> GetNextFeasibleGraphNodes(List<Base.Job> jobs)
        {
            var nextFeasibleGraphNodes = new List<(int jobId, int? machineId)>();

            foreach (var job in jobs)
            {
                if ( this.JobsOrder.Count > 0 && this.JobsOrder.Contains(job.Id) )
                    continue;

                nextFeasibleGraphNodes.Add( (job.Id, null) );
            }

            return nextFeasibleGraphNodes;
        }

        public override void AddGraphNode((int jobId, int? machineId) graphNode, List<Base.Job> jobs)
        {
            this.JobsOrder.Add(graphNode.jobId);
        }

        public override (int jobId, int? machineId) GetGraphNodeByIndex(int index)
        {
            var job = this.JobsOrder[index];
            return (job, null);
        }

        public override Base.Solution Clone()
        {
            return new Solution() {
                TimeSpan = this.TimeSpan,
                Machines = this.Machines.Select(m => m.Clone()).ToList(),
                JobsOrder = new List<int>(this.JobsOrder) 
            };
        }

        public override void Reset()
        {
            base.Reset();
            this.TimeSpan = 0;
            this.JobsOrder = new List<int>();
        }

        public override Base.Solution FindLocalOptimum(List<Base.Solution> population, int visual)
        {
            var localOptimum = new Solution();

            for (int i = 0; i < population.Count; i++)
            {
                var comparedSolution = (Solution)population[i];

                if (IsInVisualScope(comparedSolution, visual) && comparedSolution.TimeSpan < this.TimeSpan)
                {
                    localOptimum = comparedSolution;
                }
            }

            return localOptimum;
        }

        private bool IsInVisualScope(Solution s, int visual)
        {
            var jobsOrder = new List<int>(this.JobsOrder);
            int swaps = 0;

            for (int j = 0; j < jobsOrder.Count; j++)
            {
                if (jobsOrder[j] != s.JobsOrder[j])
                {
                    if (swaps == visual)
                        return false;

                    for (int k = j + 1; k < jobsOrder.Count; k++)
                    {
                        if (jobsOrder[k] == s.JobsOrder[j])
                        {
                            jobsOrder.Swap(j, k);
                            swaps++;
                            break;
                        }
                    }
                }
            }

            return true;
        }

        public override void MakeMoveTo(Base.Solution solution, int maxStep)
        {
            var s = (Solution)solution;
            Random r = new Random();
            var steps = r.Next(0, maxStep + 1);

            for (int i = 0; i < this.JobsOrder.Count; i++)
            {
                if (this.JobsOrder[i] != s.JobsOrder[i])
                {
                    for (int j = i + 1; j < this.JobsOrder.Count; j++)
                    {
                        if (this.JobsOrder[j] == s.JobsOrder[i])
                        {
                            this.JobsOrder.Swap(i, j);
                            steps--;

                            if (steps == 0)
                                return;
                        }
                    }
                }
            }
        }

        public override void Disperse()
        {
            Random r = new Random();
            var dispersePosition = r.Next(0, this.JobsOrder.Count - 2);
            this.JobsOrder.Swap(dispersePosition, dispersePosition + 1);
        }

        public override bool IsCorrect(List<Base.Job> jobs)
        {
            foreach (var job in jobs)
            {
                var scheduledOperations = this.Machines
                    .Select(m => m.Operations.Find(o => o.Operation.JobId == job.Id))
                    .OrderBy(o => o.StartTime)
                    .ToList();

                for (int i = 0; i < job.Operations.Count; i++)
                {
                    if (job.Operations[i].MachineId != scheduledOperations[i].Operation.MachineId)
                        return false;

                    if (i > 0)
                    {
                        int previousOperationEndTime = scheduledOperations[i-1].StartTime + scheduledOperations[i-1].Operation.ProcessingTime;
                        if (scheduledOperations[i].StartTime < previousOperationEndTime)
                            return false;
                    }
                }
            }

            return true;
        }

        public List<int> GetPossibleCrossingPositions(Solution solution)
        {
            var crossingPositions = new List<int>();
            var jobsCount = this.JobsOrder.Count;

            for (int i = 0; i < jobsCount - 1; i++)
            {
                var equivalentPositions = 0;

                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        if (this.JobsOrder[j] == solution.JobsOrder[k])
                        {
                            equivalentPositions++;
                            break;
                        }
                    }
                }

                if (equivalentPositions == i + 1)
                {
                    crossingPositions.Add(i);
                }
            }

            return crossingPositions;
        }

        public override (Base.Solution solution1, Base.Solution solution2) DoCrossing(Base.Solution s)
        {
            var solution = (Solution)s;
            var operationsCount = this.JobsOrder.Count;
            var crossingPositionsCount = Math.Max( 2, Convert.ToInt32( Math.Round(0.3 * operationsCount) ) );
            var crossingPositions = new List<int>();

            for (int i = 0; i < operationsCount - 1; i++)
            {
                var equivalentPositions = 0;

                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        if (this.JobsOrder[j] == solution.JobsOrder[k])
                        {
                            equivalentPositions++;
                            break;
                        }
                    }
                }

                if (equivalentPositions == i + 1)
                {
                    crossingPositions.Add(i);

                    if (crossingPositions.Last() + 1 >= crossingPositionsCount)
                        break;
                }
            }

            (Solution s1, Solution s2) result = (new Solution(this.MachinesCount), new Solution(this.MachinesCount));

            if (crossingPositions.Count == 0)
                return result;

            for (int i = 0; i <= crossingPositions.Last(); i++)
            {
                result.s1.JobsOrder.Add(this.JobsOrder[i]);
                result.s2.JobsOrder.Add(solution.JobsOrder[i]);
            }
            for (int j = crossingPositions.Last() + 1; j < operationsCount; j++)
            {
                result.s1.JobsOrder.Add(solution.JobsOrder[j]);
                result.s2.JobsOrder.Add(this.JobsOrder[j]);
            }

            return result;
        }

        public override bool IsNotNull()
        {
            base.IsNotNull();

            return this.JobsOrder.Count == 0 ? false : true;
        }

        public override bool IsDifferent(Base.Solution solution)
        {
            var s = (Solution) solution;

            for (int i = 0; i < this.JobsOrder.Count; i++)
            {
                if (this.JobsOrder[i] != s.JobsOrder[i])
                    return true;
            }

            return false;
        }

        public override int GetDistanceTo(Base.Solution solution)
        {
            var s = (Solution) solution;
            int distance = 0;
            var jobsOrderClone = new List<int>(this.JobsOrder);

            for (int i = 0; i < jobsOrderClone.Count - 1; i++)
            {
                if (jobsOrderClone[i] != s.JobsOrder[i])
                {
                    for (int j = i; j < jobsOrderClone.Count; j++)
                    {
                        if (jobsOrderClone[i] == jobsOrderClone[j])
                        {
                            jobsOrderClone.Swap(i, j);
                            distance++;
                            break;
                        }
                    }
                }
            }

            return distance;
        }
    }
}