using System.Collections.Generic;
using System.Linq;
using Core.Algorithms;
using NUnit.Framework;

namespace Tests.UnitTests.Algorithms
{
    [TestFixture]
    public class AntColonyOptimizationAlgorithmTests
    {
        [Test, TestCaseSource("initializing_algorithm_test_cases")]
        public void test_initializing_algorithm(int maxIterations, int populationCount, float evaporationRate, Core.JobScheduling.Base.SchedulingProblem schedulingProblem, int expectedNodesCount, int expectedPathsCount)
        {
            // Act
            var algorithm = new AntColonyOptimizationAlgorithm(maxIterations, populationCount, evaporationRate, schedulingProblem);

            // Assert
            Assert.AreEqual(populationCount, algorithm.GetPopulation().Count);
            Assert.AreEqual(expectedNodesCount, algorithm.GetNodes().Count);
            Assert.AreEqual(expectedPathsCount, algorithm.GetPaths().Count);
        }

        [Test, TestCaseSource("performing_algorithm_test_cases")]
        [Repeat(1)]
        public void test_performing_algorithm(int maxIterations, int populationCount, float evaporationRate, Core.JobScheduling.Base.SchedulingProblem schedulingProblem, int expectedResultTimeSpan)
        {
            // Arrange
            var algorithm = new AntColonyOptimizationAlgorithm(maxIterations, populationCount, evaporationRate, schedulingProblem);
        
            // Act
            var result = algorithm.Perform();

            // Assert
            Assert.AreEqual(expectedResultTimeSpan, result.TimeSpan);

            if (schedulingProblem is Core.JobScheduling.OpenShop.SchedulingProblem)
            {
                return;
            }

            foreach (var job in schedulingProblem.Jobs)
            {
                var scheduledOperations = result.Machines
                    .Select(m => m.Operations.FindLast(o => o.Operation.JobId == job.Id))
                    .OrderBy(o => o.StartTime)
                    .ToList();

                for (int i = 0; i < job.Operations.Count; i++)
                {
                    Assert.AreEqual(job.Operations[i].MachineId, scheduledOperations[i].Operation.MachineId);

                    if (i > 0)
                    {
                        int previousOperationEndTime = scheduledOperations[i-1].StartTime + scheduledOperations[i-1].Operation.ProcessingTime;
                        Assert.IsTrue( scheduledOperations[i].StartTime >= previousOperationEndTime );
                    }
                }
            }
        }

        private static object[] initializing_algorithm_test_cases = {
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetFlowShopTestSchedulingProblem(),
                3,       // expectedNodesCount
                6       // expectedPathsCount
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetJobShopTestSchedulingProblem(),
                6,       // expectedNodesCount
                27       // expectedPathsCount
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetOpenShopTestSchedulingProblem(),
                6,       // expectedNodesCount
                30       // expectedPathsCount
            }
        };

        private static object[] performing_algorithm_test_cases = {
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetFlowShopTestSchedulingProblem(),
                12      // expectedResultTimeSpan
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetJobShopTestSchedulingProblem(),
                9       // expectedResultTimeSpan
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                0.8F,   // evaporationRate
                GetOpenShopTestSchedulingProblem(),
                9       // expectedResultTimeSpan
            }
        };

        private static Core.JobScheduling.FlowShop.SchedulingProblem GetFlowShopTestSchedulingProblem()
        {
            return new Core.JobScheduling.FlowShop.SchedulingProblem() {
                JobsCount = 3,
                MachinesCount = 4,
                Jobs = new List<Core.JobScheduling.Base.Job>() {
                    new Core.JobScheduling.Base.Job() {
                        Id = 0, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 0, MachineId = 0, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 0, MachineId = 1, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 0, MachineId = 2, ProcessingTime = 1 },
                            new Core.JobScheduling.Base.Operation() { Id = 3, JobId = 0, MachineId = 3, ProcessingTime = 2 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 1, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 1 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 1, MachineId = 1, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 1, MachineId = 2, ProcessingTime = 1 },
                            new Core.JobScheduling.Base.Operation() { Id = 3, JobId = 1, MachineId = 3, ProcessingTime = 3 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 2, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 2, MachineId = 0, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 2, MachineId = 1, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 2, MachineId = 2, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 3, JobId = 2, MachineId = 3, ProcessingTime = 1 }
                        }
                    }
                }
            };
        }

        private static Core.JobScheduling.JobShop.SchedulingProblem GetJobShopTestSchedulingProblem()
        {
            return new Core.JobScheduling.JobShop.SchedulingProblem() {
                JobsCount = 3,
                MachinesCount = 2,
                Jobs = new List<Core.JobScheduling.Base.Job>() {
                    new Core.JobScheduling.Base.Job() {
                        Id = 0, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 0, MachineId = 1, ProcessingTime = 4 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 0, MachineId = 0, ProcessingTime = 2 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 1, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 1, MachineId = 1, ProcessingTime = 3 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 2, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 2, MachineId = 1, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 2, MachineId = 0, ProcessingTime = 3 }
                        }
                    }
                }
            };
        }

        private static Core.JobScheduling.OpenShop.SchedulingProblem GetOpenShopTestSchedulingProblem()
        {
            return new Core.JobScheduling.OpenShop.SchedulingProblem() {
                JobsCount = 3,
                MachinesCount = 2,
                Jobs = new List<Core.JobScheduling.Base.Job>() {
                    new Core.JobScheduling.Base.Job() {
                        Id = 0, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 0, MachineId = 1, ProcessingTime = 4 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 0, MachineId = 0, ProcessingTime = 2 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 1, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 1, MachineId = 1, ProcessingTime = 3 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 2, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 2, MachineId = 1, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 2, MachineId = 0, ProcessingTime = 3 }
                        }
                    }
                }
            };
        }
    }
}