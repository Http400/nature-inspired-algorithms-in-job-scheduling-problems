using System;
using System.Collections.Generic;
using System.Linq;
using Core.JobScheduling.Base;
using Core.Utils;

namespace Core.JobScheduling.OpenShop
{
    public class Solution : Base.Solution
    {
        public List<Operation> OperationsSchedulingOrder { get; set; }


        public Solution() : base()
        {
            this.OperationsSchedulingOrder = new List<Operation>();
        }

        public Solution(int machinesCount) : base(machinesCount) {
            this.OperationsSchedulingOrder = new List<Operation>();
        }
        
        public Solution(int machinesCount, List<Job> jobs) : base(machinesCount)
        {
            GenerateOperationsSchedulingOrder(machinesCount, jobs);
            PropagateOperations(jobs);
            CalculateTimeSpan();
        }

        public Solution(int machinesCount, List<Job> jobs, List<Operation> operationsSchedulingOrder) : base(machinesCount)
        {
            this.OperationsSchedulingOrder = operationsSchedulingOrder;
            PropagateOperations(jobs);
            CalculateTimeSpan();
        }

        // public bool CompareMachinesProcessingOrder(Solution solution)
        // {   
        // }

        private void GenerateOperationsSchedulingOrder(int machinesCount, List<Job> jobs)
        {
            this.OperationsSchedulingOrder = new List<Operation>();

            foreach (var job in jobs)
            {
                foreach (var operation in job.Operations)
                {
                    this.OperationsSchedulingOrder.Add(operation);
                }
            }

            this.OperationsSchedulingOrder.Shuffle();
        }

        public override void PropagateOperations(List<Job> jobs)
        {
            base.PropagateOperations(jobs);
            
            if (this.Machines.Any(m => m.Operations.Count > 0)) {

            }

            foreach (var operation in this.OperationsSchedulingOrder)
            {
                var operationIndex = jobs[operation.JobId].Operations.IndexOf(operation);
                var machine = this.Machines[operation.MachineId];
                var suitableMachineTimeGaps = machine.TimeGaps.Where(tg => tg.Duration >= operation.ProcessingTime).ToList();
                var jobAssignedOperations = this.Machines
                    .Where( m => m.Operations.Any(o => o.Operation.JobId == operation.JobId) )
                    .Select( m => m.Operations.Where(o => o.Operation.JobId == operation.JobId).FirstOrDefault() )
                    .OrderBy( o => o.StartTime )
                    .ToList();

                var admissibleTimeGaps = new List<(int startTime, int endTime)>();
                
                if (   
                        jobAssignedOperations.Count > 0 
                        && (suitableMachineTimeGaps.Count == 0 || suitableMachineTimeGaps.Last().EndTime < jobAssignedOperations.Max(o => o.StartTime))
                        && machine.GetLastOperationEndTime() < jobAssignedOperations.Max(o => o.StartTime)
                    )
                {
                    suitableMachineTimeGaps.Add(new Machine.TimeGap(
                        machine.GetLastOperationEndTime(),
                        jobAssignedOperations.Last().StartTime - machine.GetLastOperationEndTime()
                    ));
                }

                foreach (var timeGap in suitableMachineTimeGaps)
                {
                    for (int t = timeGap.StartTime; t < timeGap.EndTime; t++)
                    {
                        if ( jobAssignedOperations.Any(o => o.StartTime <= t && o.StartTime + o.Operation.ProcessingTime >= t + 1) )
                        {
                            continue;
                        }

                        if (admissibleTimeGaps.Count > 0 && admissibleTimeGaps.Last().endTime == t)
                        {
                            var newTimeGap = ( admissibleTimeGaps.Last().startTime, t + 1 );
                            admissibleTimeGaps.RemoveAt( admissibleTimeGaps.Count - 1 );
                            admissibleTimeGaps.Add( newTimeGap );
                        }
                        else
                        {
                            admissibleTimeGaps.Add( (t, t + 1) );
                        }
                    }
                }

                var firstAcceptableTimeGap = admissibleTimeGaps.Find(tg => tg.endTime - tg.startTime >= operation.ProcessingTime);

                if ( !firstAcceptableTimeGap.Equals(default(ValueTuple<int,int>)) )
                {
                    machine.AddOperation(operation, firstAcceptableTimeGap.startTime);
                }
                else
                {
                    var operationStartTime = 0;
                    var lastJobAssignedOperation = jobAssignedOperations.LastOrDefault();

                    if ( 
                            lastJobAssignedOperation.Equals( default(ValueTuple<Operation,int>) ) 
                            //|| lastJobAssignedOperation.StartTime >= machine.GetLastOperationEndTime() + operation.ProcessingTime
                            || jobAssignedOperations.All(o => o.StartTime >= machine.GetLastOperationEndTime() + operation.ProcessingTime)
                        )
                    {
                        operationStartTime = machine.GetLastOperationEndTime();
                    }
                    else
                    {
                        operationStartTime = Math.Max( 
                            lastJobAssignedOperation.StartTime + lastJobAssignedOperation.Operation.ProcessingTime,
                            machine.GetLastOperationEndTime()
                        );
                    }

                    machine.AddOperation(operation, operationStartTime);
                }

                if (machine.Operations.Count > jobs.Count)
                {
                    
                }
            }
        }

        public override List<(int jobId, int? machineId)> GetNextFeasibleGraphNodes(List<Job> jobs)
        {
            var nextFeasibleGraphNodes = new List<(int jobId, int? machineId)>();

            foreach (var job in jobs)
            {
                foreach (var operation in job.Operations)
                {
                    if ( this.OperationsSchedulingOrder.Count > 0 && this.OperationsSchedulingOrder.Contains(operation) )
                        continue;

                    nextFeasibleGraphNodes.Add( (operation.JobId, operation.MachineId) );
                }
            }

            return nextFeasibleGraphNodes;
        }

        public override void AddGraphNode((int jobId, int? machineId) graphNode, List<Job> jobs)
        {
            var nextOperation = jobs.Find(j => j.Id == graphNode.jobId).Operations.Find(o => o.MachineId == graphNode.machineId);
            this.OperationsSchedulingOrder.Add(nextOperation);
        }

        public override (int jobId, int? machineId) GetGraphNodeByIndex(int index)
        {
            var operation = this.OperationsSchedulingOrder[index];
            return (operation.JobId, operation.MachineId);
        }

        public override Base.Solution Clone()
        {
            return new Solution() {
                TimeSpan = this.TimeSpan,
                Machines = this.Machines.Select(m => m.Clone()).ToList(),
                OperationsSchedulingOrder = this.OperationsSchedulingOrder.Select(o => (Operation)o.Clone()).ToList()
            };
        }

        public override void Reset()
        {
            base.Reset();
            this.TimeSpan = 0;
            this.OperationsSchedulingOrder = new List<Operation>();
            this.Paths = new List<Path>();
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
            var operatonSchedulingOrder = new List<Operation>();

            foreach (var operation in this.OperationsSchedulingOrder)
            {
                operatonSchedulingOrder.Add( (Operation)operation.Clone() );
            }

            int swap = 0;

            for (int i = 0; i < operatonSchedulingOrder.Count; i++)
            {
                if (operatonSchedulingOrder[i] != s.OperationsSchedulingOrder[i])
                {
                    if (swap == visual)
                        return false;

                    for (int j = i + 1; j < operatonSchedulingOrder.Count; j++)
                    {
                        if (operatonSchedulingOrder[i] == s.OperationsSchedulingOrder[j])
                        {
                            operatonSchedulingOrder.Swap(i, j);
                            swap++;
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

            for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
            {
                if (this.OperationsSchedulingOrder[i] != s.OperationsSchedulingOrder[i])
                {
                    for (int j = i + 1; j < this.OperationsSchedulingOrder.Count; j++)
                    {
                        if (this.OperationsSchedulingOrder[j] == s.OperationsSchedulingOrder[i])
                        {
                            this.OperationsSchedulingOrder.Swap(i, j);
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
            var disperses = 1; //r.Next(0, this.OperationsSchedulingOrder.Count/2);
            while (disperses > 0)
            {
                var dispersePosition = r.Next(0, this.OperationsSchedulingOrder.Count - 2);
                this.OperationsSchedulingOrder.Swap(dispersePosition, dispersePosition + 1);
                disperses--;
            }
            
        }

        public override bool IsCorrect(List<Job> jobs)
        {
            foreach (var job in jobs)
            {
                var scheduledOperations = this.Machines
                    .Select(m => m.Operations.Find(o => o.Operation.JobId == job.Id))
                    .OrderBy(o => o.StartTime)
                    .ToList();

                for (int i = 0; i < job.Operations.Count; i++)
                {
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
            var operationsCount = this.OperationsSchedulingOrder.Count;

            for (int i = 0; i < operationsCount - 1; i++)
            {
                var equivalentPositions = 0;

                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        if (this.OperationsSchedulingOrder[j] == solution.OperationsSchedulingOrder[k])
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
            var operationsCount = this.OperationsSchedulingOrder.Count;
            var crossingPositionsCount = Math.Max( 2, Convert.ToInt32( Math.Round(0.3 * operationsCount) ) );
            var crossingPositions = new List<int>();

            for (int i = 0; i < operationsCount - 1; i++)
            {
                var equivalentPositions = 0;

                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        if (this.OperationsSchedulingOrder[j] == solution.OperationsSchedulingOrder[k])
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
                result.s1.OperationsSchedulingOrder.Add(this.OperationsSchedulingOrder[i]);
                result.s2.OperationsSchedulingOrder.Add(solution.OperationsSchedulingOrder[i]);
            }
            for (int j = crossingPositions.Last() + 1; j < operationsCount; j++)
            {
                result.s1.OperationsSchedulingOrder.Add(solution.OperationsSchedulingOrder[j]);
                result.s2.OperationsSchedulingOrder.Add(this.OperationsSchedulingOrder[j]);
            }

            return result;
        }

        public override bool IsNotNull()
        {
            base.IsNotNull();

            return this.OperationsSchedulingOrder.Count == 0 ? false : true;
        }

        public override bool IsDifferent(Base.Solution solution)
        {
            var s = (Solution)solution;

            for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
            {
                if (this.OperationsSchedulingOrder[i] != s.OperationsSchedulingOrder[i])
                    return true;
            }

            return false;
        }

        public override int GetDistanceTo(Base.Solution solution)
        {
            var s = (Solution) solution;
            int distance = 0;
            var operationsClone = s.OperationsSchedulingOrder.Clone();

            for (int i = 0; i < operationsClone.Count - 1; i++)
            {
                if (operationsClone[i] != s.OperationsSchedulingOrder[i])
                {
                    for (int j = i; j < operationsClone.Count; j++)
                    {
                        if (operationsClone[i] == operationsClone[j])
                        {
                            operationsClone.Swap(i, j);
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