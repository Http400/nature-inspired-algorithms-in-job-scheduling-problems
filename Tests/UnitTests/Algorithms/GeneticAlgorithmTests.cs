using System.Collections.Generic;
using Core.Algorithms;
using NUnit.Framework;

namespace Tests.UnitTests.Algorithms
{
    [TestFixture]
    public class GeneticAlgorithmTests
    {
        [Test, TestCaseSource("test_cases")]
        public void test_initializing_algorithm(int maxIterations, int populationCount, Core.JobScheduling.Base.SchedulingProblem schedulingProblem)
        {
            // Act
            var algorithm = new GeneticAlgorithm(maxIterations, populationCount, schedulingProblem);

            // Assert
            Assert.AreEqual(populationCount, algorithm.GetPopulation().Count);
        }

        [Test, TestCaseSource("test_cases")]
        [Repeat(1)]
        public void test_performing_algorithm(int maxIterations, int populationCount, Core.JobScheduling.Base.SchedulingProblem schedulingProblem)
        {
            // Arrange
            var algorithm = new GeneticAlgorithm(maxIterations, populationCount, schedulingProblem);

            // Act
            var result = algorithm.Perform();

            // Assert
            Assert.IsTrue( result.IsCorrect(schedulingProblem.Jobs) );
        }

        private static object[] test_cases = {
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                GetFlowShopTestSchedulingProblem()
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                GetJobShopTestSchedulingProblem2()
            },
            new object[] {
                10,     // maxIterations
                10,     // populationCount
                GetOpenShopTestSchedulingProblem()
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

        private static Core.JobScheduling.JobShop.SchedulingProblem GetJobShopTestSchedulingProblem2()
        {
            return new Core.JobScheduling.JobShop.SchedulingProblem() {
                JobsCount = 3,
                MachinesCount = 3,
                Jobs = new List<Core.JobScheduling.Base.Job>() {
                    new Core.JobScheduling.Base.Job() {
                        Id = 0, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 0, MachineId = 0, ProcessingTime = 3 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 0, MachineId = 1, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 0, MachineId = 2, ProcessingTime = 2 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 1, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 2 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 1, MachineId = 2, ProcessingTime = 1 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 1, MachineId = 1, ProcessingTime = 4 }
                        }
                    },
                    new Core.JobScheduling.Base.Job() {
                        Id = 2, Operations = new List<Core.JobScheduling.Base.Operation>() {
                            new Core.JobScheduling.Base.Operation() { Id = 0, JobId = 2, MachineId = 1, ProcessingTime = 4 },
                            new Core.JobScheduling.Base.Operation() { Id = 1, JobId = 2, MachineId = 0, ProcessingTime = 1 },
                            new Core.JobScheduling.Base.Operation() { Id = 2, JobId = 2, MachineId = 2, ProcessingTime = 3 }
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