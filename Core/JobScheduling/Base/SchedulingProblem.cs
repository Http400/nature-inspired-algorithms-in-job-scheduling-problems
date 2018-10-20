using System.Collections.Generic;

namespace Core.JobScheduling.Base
{
    public abstract class SchedulingProblem
    {
        public int JobsCount { get; set; }
        public int MachinesCount { get; set; }
        public List<Job> Jobs { get; set; }


        public SchedulingProblem() 
        {
            this.Jobs = new List<Job>();
        }

        public abstract List<(int jobId, int? machineId)> GenerateGraphNodes();
        public abstract List<Path> GeneratePaths(float pheromone);

        public abstract List<Solution> CreateAntsPopulation(int populationCount);
        public abstract List<Solution> CreateCockroachesPopulation(int populationCount);
        public abstract List<Solution> CreateGenotypesPopulation(int populationCount);

        // public SchedulingProblem()
        // {
        //     Jobs = new List<Job>();
        //     Solutions = new List<Solution>();
        // }

        public bool IsEmpty()
        {
            return JobsCount == 0 && MachinesCount == 0 && Jobs.Count == 0;
        }

        public abstract int GetDistance(Solution solution, (int jobId, int? machineId) node);
    }
}