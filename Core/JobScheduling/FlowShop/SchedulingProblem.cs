using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.JobScheduling.Base;

namespace Core.JobScheduling.FlowShop
{
    public class SchedulingProblem : Base.SchedulingProblem
    {
        public ComparativeData ComparativeData { get; set; }
        public List<Solution> Solutions { get; set; }
        

        public SchedulingProblem() {}

        public SchedulingProblem(string dataString) : base()
        {
            string[] lines = dataString.Split("\n");
            this.JobsCount = 0; this.MachinesCount = 0;

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
                        InitialSeed = Int32.Parse(data[2]),
                        UpperBound = Int32.Parse(data[3]),
                        LowerBound = Int32.Parse(data[4])
                    };
                }
                else
                {
                    string[][] processingTimes = new string[this.MachinesCount][];

                    int j = 0;
                    while (j < this.MachinesCount)
                    {
                        processingTimes[j] = lines[i + j].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        j++;
                    }

                    i = i + j;

                    //this.Jobs = new List<Entities.Base.Job>();

                    for (int n = 0; n < this.JobsCount; n++)
                    {
                        var job = new Base.Job() { Id = n };

                        for (int m = 0; m < this.MachinesCount; m++)
                        {
                            job.Operations.Add(new Base.Operation() {
                                Id = m,
                                JobId = n,
                                MachineId = m,
                                ProcessingTime = Int32.Parse( processingTimes[m][n] )
                            });
                        }

                        this.Jobs.Add(job);
                    }
                }
            }
        }

        public override List<(int jobId, int? machineId)> GenerateGraphNodes()
        {
            var graphNodes = new List<(int jobId, int? machineId)>();
            
            foreach (var job in this.Jobs)
            {
                graphNodes.Add( (job.Id, null) );
            }

            return graphNodes;
        }

        public override List<Path> GeneratePaths(float pheromone)
        {
            var paths = new List<Path>();

            for (int i = 0; i < this.JobsCount; i++)
            {
                for (int j = 0; j < this.JobsCount; j++)
                {
                    if (this.Jobs[i].Id == this.Jobs[j].Id)
                        continue;

                    paths.Add(new Path() {
                        StartingOperation = (this.Jobs[i].Id, null),
                        EndingOperation = (this.Jobs[j].Id, null),
                        Pheromone = pheromone
                    });    
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