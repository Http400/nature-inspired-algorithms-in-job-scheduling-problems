using System;
using System.Collections.Generic;
using System.Linq;
using Core.JobScheduling.Base;
using Core.Utils;

namespace Core.JobScheduling.JobShop
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

        // public bool CompareOperationsSchedulingOrder(Solution solution)
        // {   
        // }

        private void GenerateOperationsSchedulingOrder(int machinesCount, List<Job> jobs)
        {
            var operationsMatrix = new List<List<Operation>>();

            foreach (var job in jobs)
            {
                var machineOperations = new List<Operation>();
                
                foreach (var operation in job.Operations)
                {
                    machineOperations.Add(operation);
                }

                operationsMatrix.Add(machineOperations);
            }

            this.OperationsSchedulingOrder = new List<Operation>();
            var random = new Random();

            while (operationsMatrix.Count > 0)
            {
                var r = random.Next(0, operationsMatrix.Count);
                this.OperationsSchedulingOrder.Add(operationsMatrix[r][0]);
                operationsMatrix[r].RemoveAt(0);

                if (operationsMatrix[r].Count == 0)
                    operationsMatrix.RemoveAt(r);
            }
        }

        public override void PropagateOperations(List<Job> jobs)
        {
            base.PropagateOperations(jobs);
            
            foreach (var operation in this.OperationsSchedulingOrder)
            {
                var operationIndex = jobs[operation.JobId].Operations.IndexOf(operation);
                var machine = this.Machines[operation.MachineId];

                if (operationIndex == 0)
                {
                    // var suitableTimeGaps = machine.TimeGaps
                    //     .Where(tg => tg.Duration >= operation.ProcessingTime)
                    //     .ToList();

                    var suitableTimeGaps = new List<Machine.TimeGap>();
                    foreach (var timeGap in machine.TimeGaps)
                    {
                        if (timeGap.Duration >= operation.ProcessingTime)
                            suitableTimeGaps.Add(timeGap);
                    }

                    if (suitableTimeGaps.Count == 0)
                    {
                        machine.AddOperation( operation, machine.GetLastOperationEndTime() );
                    }
                    else
                    {
                        machine.AddOperation( operation, suitableTimeGaps.First().StartTime );
                    }
                }
                else
                {
                    var jobPreviousOperation = jobs[operation.JobId].Operations[operationIndex - 1];
                    var previousOperationMachine = this.Machines[jobPreviousOperation.MachineId];
                    var previousOperationEndTime = previousOperationMachine.GetOperationEndTime(jobPreviousOperation.JobId);
                    // var suitableTimeGaps = machine.TimeGaps
                    //     .Where(tg => 
                    //         tg.EndTime > previousOperationEndTime && 
                    //         tg.EndTime - Math.Max(tg.StartTime, previousOperationEndTime) >= operation.ProcessingTime 
                    //     )
                    //     .ToList();

                    var suitableTimeGaps = new List<Machine.TimeGap>();
                    foreach (var timeGap in machine.TimeGaps)
                    {
                        if (timeGap.EndTime > previousOperationEndTime && timeGap.EndTime - Math.Max(timeGap.StartTime, previousOperationEndTime) >= operation.ProcessingTime)
                            suitableTimeGaps.Add(timeGap);
                    }
                    
                    if (suitableTimeGaps.Count == 0)
                    {
                        var operationStartTime = Math.Max( previousOperationEndTime, machine.GetLastOperationEndTime() );
                        machine.AddOperation( operation, operationStartTime);
                    }
                    else
                    {
                        var operationStartTime = Math.Max( previousOperationEndTime, suitableTimeGaps.First().StartTime );
                        machine.AddOperation( operation, operationStartTime);
                    }
                }
            }
        }

        // public override List<(int jobId, int? machineId)> GetNextFeasibleGraphNodes(List<Job> jobs)
        // {
        //     var nextFeasibleGraphNodes = new List<(int jobId, int? machineId)>();

        //     foreach (var job in jobs)
        //     {
        //         foreach (var operation in job.Operations)
        //         {
        //             if ( this.OperationsSchedulingOrder.Contains(operation) )
        //                 continue;

        //             var previousOperations = job.Operations
        //                 .Where( o => job.Operations.IndexOf(o) < job.Operations.IndexOf(operation) )
        //                 .ToList();

        //             if ( previousOperations.Count == 0 || previousOperations.All(o => this.OperationsSchedulingOrder.Contains(o)) )
        //             {
        //                 nextFeasibleGraphNodes.Add( (operation.JobId, operation.MachineId) );
        //             }
        //         }
        //     }

        //     return nextFeasibleGraphNodes;
        // }

        public override List<(int jobId, int? machineId)> GetNextFeasibleGraphNodes(List<Job> jobs)
        {
            var nextFeasibleGraphNodes = new List<(int jobId, int? machineId)>();

            // for (int i = 0; i < jobs.Count; i++)
            // {
            //     for (int j = 0; j < jobs[i].Operations.Count; j++)
            //     {
            //         if ( this.OperationsSchedulingOrder.Contains(jobs[i].Operations[j]) )
            //             continue;

            //         if ( j > 0 && !this.OperationsSchedulingOrder.Contains(jobs[i].Operations[j-1]) )
            //             continue;

            //         nextFeasibleGraphNodes.Add( (jobs[i].Operations[j].JobId, jobs[i].Operations[j].MachineId) );
            //     }
            // }

            for (int i = 0; i < jobs.Count; i++)
            {
                for (int j = 0; j < jobs[i].Operations.Count; j++)
                {
                    bool operationAlreadyScheduled = false;
                    bool previousOperationScheduled = false;

                    for (int k = 0; k < this.OperationsSchedulingOrder.Count; k++)
                    {
                        if (this.OperationsSchedulingOrder[k] == jobs[i].Operations[j])
                        {
                            operationAlreadyScheduled = true;
                            break;
                        }
                        if (j > 0 && this.OperationsSchedulingOrder[k] == jobs[i].Operations[j-1])
                        {
                            previousOperationScheduled = true;
                        }
                    }

                    if (operationAlreadyScheduled == false && (j == 0 || previousOperationScheduled == true))
                    {
                        nextFeasibleGraphNodes.Add( (jobs[i].Operations[j].JobId, jobs[i].Operations[j].MachineId) );
                    }
                }
            }

            return nextFeasibleGraphNodes;
        }

        public override void AddGraphNode((int jobId, int? machineId) graphNode, List<Job> jobs)
        {
            var machinesCount = jobs[0].Operations.Count;

            for (int i = 0; i < jobs.Count; i++)
            {
                for (int j = 0; j < machinesCount; j++)
                {
                    if ( jobs[i].Operations[j].JobId == graphNode.jobId && jobs[i].Operations[j].MachineId == graphNode.machineId )
                    {
                        this.OperationsSchedulingOrder.Add(jobs[i].Operations[j]);
                        return;
                    }
                }
            }
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
                        if (operatonSchedulingOrder[j] == s.OperationsSchedulingOrder[i])
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

        public int CalculateDifferenceDegree(Solution solution)
        {
            var operatonSchedulingOrder = new List<Operation>();

            foreach (var operation in this.OperationsSchedulingOrder)
            {
                operatonSchedulingOrder.Add( (Operation)operation.Clone() );
            }

            int differenceDegree = 0;

            for (int i = 0; i < operatonSchedulingOrder.Count; i++)
            {
                if (operatonSchedulingOrder[i] != solution.OperationsSchedulingOrder[i])
                {
                    for (int j = i + 1; j < operatonSchedulingOrder.Count; j++)
                    {
                        if (operatonSchedulingOrder[j] == solution.OperationsSchedulingOrder[i])
                        {
                            operatonSchedulingOrder.Swap(i, j);
                            differenceDegree++;
                            break;
                        }
                    }
                }
            }

            return differenceDegree;
        }

        public override void MakeMoveTo(Base.Solution solution, int maxStep)
        {
            var s = (Solution)solution;
            Random r = new Random();
            var steps = r.Next(0, maxStep + 1);

            while (steps > 0)
            {
                bool isHardMoveNeeded = true;
                for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
                {
                    if (this.OperationsSchedulingOrder[i] != s.OperationsSchedulingOrder[i])
                    {
                        for (int j = i + 1; j < this.OperationsSchedulingOrder.Count; j++)
                        {
                            if (this.OperationsSchedulingOrder[j] == s.OperationsSchedulingOrder[i] && IsSwapPossible(this.OperationsSchedulingOrder, i, j))
                            {
                                this.OperationsSchedulingOrder.Swap(i, j);
                                steps--;
                                isHardMoveNeeded = false;

                                if (steps == 0) return;
                            }
                        }
                    }
                }
                if (isHardMoveNeeded)
                {
                    MakeHardMoveTo(s);
                    steps--;
                }
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

        // public bool IsOrderFeasible(List<Job> jobs)
        // {
        //     var operationsMatrix = new List<List<int>>() {};

        //     foreach (var job in jobs)
        //     {
        //         operationsMatrix.Add(new List<int>());
        //     }

        //     for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
        //     {
        //         var operationJobId = this.OperationsSchedulingOrder[i].JobId;
        //         if (operationsMatrix[operationJobId].Count > 0 && operationsMatrix[operationJobId].Last() != this.OperationsSchedulingOrder[i].Id-1)
        //         {
        //             return false;
        //         }
        //         else
        //         {
        //             operationsMatrix[operationJobId].Add(this.OperationsSchedulingOrder[i].Id);
        //         }
        //     }

        //     return true;
        // }

        public override bool IsOrderFeasible()
        {
            var operationsMatrix = new List<(int jobId, List<int> operations)>();

            for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
            {
                var operationJobId = this.OperationsSchedulingOrder[i].JobId;
                var operationId = this.OperationsSchedulingOrder[i].Id;
                int? index = null;

                for (int j = 0; j < operationsMatrix.Count; j++)
                {
                    if (operationsMatrix[j].jobId == operationJobId)
                    {
                        index = j;
                        break;
                    }
                }

                if (index.HasValue)
                {
                    if (operationsMatrix[index.Value].operations.Last() == operationId - 1)
                    {
                        operationsMatrix[index.Value].operations.Add(operationId);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    operationsMatrix.Add( (operationJobId, new List<int>() { operationId }) );
                }
            }

            return true;
        }

        private void MakeHardMoveTo(Solution solution)
        {
            for (int i = 0; i < this.OperationsSchedulingOrder.Count; i++)
            {
                if (this.OperationsSchedulingOrder[i] != solution.OperationsSchedulingOrder[i])
                {
                    for (int j = i + 1; j < this.OperationsSchedulingOrder.Count; j++)
                    {
                        if (this.OperationsSchedulingOrder[j] == solution.OperationsSchedulingOrder[i])
                        {
                            this.OperationsSchedulingOrder.Swap(i, j);
                            var jobId = this.OperationsSchedulingOrder[j].JobId;
                            var operations = new List<Operation>();
                            for (int k = 0; k < this.OperationsSchedulingOrder.Count; k++)
                            {
                                 if (this.OperationsSchedulingOrder[k].JobId == jobId)
                                 {
                                     operations.Add((Operation)this.OperationsSchedulingOrder[k].Clone());
                                 }
                            }
                            var correctOrder = operations.OrderBy(o => o.Id).ToList();

                            int l = 0;
                            while (l < this.OperationsSchedulingOrder.Count && correctOrder.Count > 0)
                            {
                                if (this.OperationsSchedulingOrder[l].JobId == jobId)
                                {
                                    if (this.OperationsSchedulingOrder[l].Id != correctOrder[0].Id)
                                    {
                                        this.OperationsSchedulingOrder[l] = correctOrder[0];
                                    }
                                    correctOrder.RemoveAt(0);
                                }
                                l++;
                            }  

                            return;
                        }
                    }
                }
            }
        }

        public override void Disperse()
        {
            Random r = new Random();
            var dispersePosition = r.Next(0, this.OperationsSchedulingOrder.Count - 2);
            if ( IsSwapPossible(this.OperationsSchedulingOrder, dispersePosition, dispersePosition + 1) ) {
                this.OperationsSchedulingOrder.Swap(dispersePosition, dispersePosition + 1);
            }
        }

        private bool IsSwapPossible(List<Operation> operations, int a, int b)
        {
            var operationsClone = operations.Clone();
            operationsClone.Swap(a, b);

            var firstJobId = operationsClone[a].JobId;
            var secondJobId = operationsClone[b].JobId;

            if (firstJobId == secondJobId)
            {
                var jobOperations = new List<Operation>();
                
                foreach (var operation in operationsClone)
                {
                    if (operation.JobId == firstJobId)
                        jobOperations.Add(operation);
                }

                for (int i = 0; i < this.Machines.Count; i++)
                {
                    if (jobOperations[i].Id != i)
                        return false;
                }
            }
            else
            {
                var firstJobOperations = new List<Operation>();
                var secondJobOperations = new List<Operation>();
                
                foreach (var operation in operationsClone)
                {
                    if (operation.JobId == firstJobId)
                        firstJobOperations.Add(operation);
                    else if (operation.JobId == secondJobId)
                        secondJobOperations.Add(operation);
                }

                for (int i = 0; i < this.Machines.Count; i++)
                {
                    if (firstJobOperations[i].Id != i || secondJobOperations[i].Id != i)
                        return false;
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