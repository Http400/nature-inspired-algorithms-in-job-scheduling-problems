using System.Collections.Generic;
using System.Linq;

namespace Core.JobScheduling.Base
{
    public abstract class Solution
    {
        public int TimeSpan { get; set; }
        public List<Machine> Machines { get; set; }
        public int MachinesCount { get; set; }
        public List<Path> Paths { get; set; }

        public Solution() {
            this.TimeSpan = 0;
            this.Machines = new List<Machine>();
        }

        public Solution(int machinesCount)
        {
            this.TimeSpan = 0;
            this.Paths = new List<Path>();
            this.Machines = new List<Machine>();
            this.MachinesCount = machinesCount;

            for (int i = 0; i < machinesCount; i++)
            {
                this.Machines.Add( new Machine(id: i) );
            }
        }

        public void CalculateTimeSpan()
        {
            // this.TimeSpan = this.Machines.Max( m => 
            //     m.Operations.LastOrDefault().StartTime + m.Operations.LastOrDefault().Operation.ProcessingTime
            // );
            
            this.TimeSpan = 0;

            foreach (var machine in this.Machines)
            {
                if (machine.Operations.Count == 0)
                    continue;

                var lastOperationEndTime = machine.Operations.Last().StartTime + machine.Operations.Last().Operation.ProcessingTime;
                if ( lastOperationEndTime > this.TimeSpan)
                    this.TimeSpan = lastOperationEndTime;
            }
        }

        public virtual void PropagateOperations(List<Job> jobs)
        {
            this.MachinesCount = this.Machines.Count;
            this.Machines = new List<Machine>();

            for (int i = 0; i < this.MachinesCount; i++)
            {
                this.Machines.Add( new Machine(id: i) );
            }
        }

        public abstract List<(int jobId, int? machineId)> GetNextFeasibleGraphNodes(List<Job> jobs);

        public abstract void AddGraphNode((int jobId, int? machineId) graphNode, List<Job> jobs);

        public abstract (int jobId, int? machineId) GetGraphNodeByIndex(int index);

        public abstract Solution Clone();

        public virtual void Reset()
        {
            this.Machines.ForEach(m => m = new Machine());
        }

        public abstract Solution FindLocalOptimum(List<Solution> population, int visual);

        public virtual bool IsNotNull()
        {
            return this.Machines != null && this.Machines.Count > 0;
        }

        public abstract void MakeMoveTo(Solution solution, int maxStep);
        
        public abstract bool IsCorrect(List<Job> jobs);

        public virtual bool IsOrderFeasible() => true;

        public virtual void AddPath(Path path)
        {
            this.Paths.Add(path);
        }

        public abstract (Solution solution1, Solution solution2) DoCrossing(Solution solution);
        public abstract void Disperse();
        public virtual void Mutate()
        {
            Disperse();
        }
        public abstract bool IsDifferent(Solution solution);
        public abstract int GetDistanceTo(Solution solution);
    }
}