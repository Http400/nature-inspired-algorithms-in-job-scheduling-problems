using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.JobScheduling.Base;

namespace Core.JobScheduling.JobShop
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

            for (int i = 0; i < lines.Length; i++)
            {
                bool containsLetter = Regex.IsMatch(lines[i], "[a-zA-Z]");
                if (containsLetter)
                {
                    continue;
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
                }
                else if (this.Jobs.Count == 0)
                {
                    int j = 0;
                    while (j < this.JobsCount)
                    {
                        this.Jobs.Add(new Job() { Id = j });

                        string[] processingTimes = lines[i + j].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        for (int m = 0; m < processingTimes.Length; m++)
                        {
                            this.Jobs.LastOrDefault().Operations.Add(new Operation() {
                                Id = m,
                                JobId = j,
                                ProcessingTime = Int32.Parse(processingTimes[m])
                            });
                        }
                        
                        j++;
                    }

                    i = i + j;
                }
                else
                {
                    int j = 0;
                    while (j < this.JobsCount)
                    {
                        string[] machinesOrder = lines[i + j].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        for (int m = 0; m < machinesOrder.Length; m++)
                        {
                            this.Jobs[j].Operations[m].MachineId = Int32.Parse(machinesOrder[m]) - 1;
                        }

                        j++;
                    }

                    i = i + j;
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
                        int l = (k == i) ? j + 1 : 0;
                        while (l < this.MachinesCount)
                        {
                            paths.Add(new Path() {
                                StartingOperation = (this.Jobs[i].Operations[j].JobId, this.Jobs[i].Operations[j].MachineId),
                                EndingOperation = (this.Jobs[k].Operations[l].JobId, this.Jobs[k].Operations[l].MachineId),
                                Pheromone = pheromone
                            });

                            l++;
                        }
                    }
                }
            }
            
            return paths;
        }

        // public override List<Path> GeneratePaths(float pheromone)
        // {
        //     var paths = new List<Path>();
        //     var operations = new List<Operation>();

        //     for (int i = 0; i < this.JobsCount; i++)
        //     {
        //         for (int j = 0; j < this.MachinesCount; j++)
        //         {
        //             operations.Add(this.Jobs[i].Operations[j]);
        //         }
        //     }

        //     var operationsCount = operations.Count;

        //     for (int i = 0; i < operationsCount; i++)
        //     {
        //         for (int j = 0; j < operationsCount; j++)
        //         {
        //             if (operations[i].JobId == operations[j].JobId && i >= j)
        //             {
        //                 i += i - j + 1;
        //             }

        //             paths.Add(new Path() {
        //                 StartingOperation = (operations[i].JobId, operations[i].MachineId),
        //                 EndingOperation = (operations[j].JobId, operations[j].MachineId),
        //                 Pheromone = pheromone
        //             });
        //         }
        //     }

        //     return paths;
        // }

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