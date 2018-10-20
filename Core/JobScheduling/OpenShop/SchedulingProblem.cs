using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.JobScheduling.Base;

namespace Core.JobScheduling.OpenShop
{
    public class SchedulingProblem : Base.SchedulingProblem
    {
        public ComparativeData ComparativeData { get; set; }


        public SchedulingProblem() {}

        public SchedulingProblem(string dataString) : base()
        {
            string[] lines = dataString.Split("\n");
            this.JobsCount = 0;
            this.MachinesCount = 0;

            int i = 0;
            while (i < lines.Length)
            {
                bool containsLetter = Regex.IsMatch(lines[i], "[a-zA-Z]");
                if (containsLetter)
                {
                    i++;
                }
                else if (this.JobsCount == 0 && this.MachinesCount == 0)
                {
                    string[] data = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    this.JobsCount = Int32.Parse(data[0]);
                    this.MachinesCount = Int32.Parse(data[1]);

                    this.ComparativeData = new ComparativeData() {
                        TimeSeed = Int32.Parse(data[2]),
                        MachineSeed = Int32.Parse(data[3]),
                        UpperBound = Int32.Parse(data[4]),
                        LowerBound = Int32.Parse(data[5])
                    };

                    i++;
                }
                else if (this.Jobs.Count == 0)
                {
                    int j = 0;
                    while (j < this.JobsCount)
                    {
                        var job = new Job() {
                            Id = j
                        };

                        string[] processingTimes = lines[i + j].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        string[] machinesOrder = lines[i + j + this.JobsCount + 1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        
                        for (int m = 0; m < processingTimes.Length; m++)
                        {
                            job.Operations.Add(new Operation() {
                                Id = m,
                                JobId = j,
                                ProcessingTime = Int32.Parse(processingTimes[m]),
                                MachineId = Int32.Parse(machinesOrder[m]) - 1
                            });
                        }

                        this.Jobs.Add(job);
                        
                        j++;
                    }

                    i = i + 2*j + 1;
                }
            }
        }

        public override List<(int jobId, int? machineId)> GenerateGraphNodes()
        {
            var graphNodes = new List<(int jobId, int? machineId)>();
            
            foreach (var job in this.Jobs)
            {
                foreach (var operation in job.Operations)
                {
                    graphNodes.Add( (operation.JobId, operation.MachineId) );
                }
            }

            return graphNodes;
        }

        public override List<Path> GeneratePaths(float pheromone)
        {
            var paths = new List<Path>();

            for (int i = 0; i < this.JobsCount; i++)
            {
                for (int j = 0; j < this.MachinesCount; j++)
                {
                    for (int k = 0; k < this.JobsCount; k++)
                    {
                        for (int l = 0; l < this.MachinesCount; l++)
                        {
                            if (this.Jobs[i].Operations[j] == this.Jobs[k].Operations[l])
                                continue;

                            paths.Add(new Path() {
                                StartingOperation = (this.Jobs[i].Operations[j].JobId, this.Jobs[i].Operations[j].MachineId),
                                EndingOperation = (this.Jobs[k].Operations[l].JobId, this.Jobs[k].Operations[l].MachineId),
                                Pheromone = pheromone
                            });    
                        }
                    }
                }
            }

            return paths;
        }

        public override List<Base.Solution> CreateAntsPopulation(int populationCount)
        {
            var population = new List<Base.Solution>();

            for (int i = 0; i < populationCount; i++)
            {
                population.Add( new Solution(this.MachinesCount) );
            }

            return population;
        }

        public override List<Base.Solution> CreateCockroachesPopulation(int populationCount)
        {
            var population = new List<Base.Solution>();

            for (int i = 0; i < populationCount; i++)
            {
                population.Add( new Solution(this.MachinesCount, this.Jobs) );
            }

            return population;
        }

        public override List<Base.Solution> CreateGenotypesPopulation(int populationCount)
        {
            var population = new List<Base.Solution>();

            for (int i = 0; i < populationCount; i++)
            {
                population.Add( new Solution(this.MachinesCount, this.Jobs) );
            }

            return population;
        }

        public override int GetDistance(Base.Solution solution, (int jobId, int? machineId) node)
        {
            var solutionClone = solution.Clone();
            solutionClone.AddGraphNode(node, this.Jobs);
            solutionClone.PropagateOperations(this.Jobs);
            solutionClone.CalculateTimeSpan();

            return solutionClone.TimeSpan - solution.TimeSpan;
        }
    }
}