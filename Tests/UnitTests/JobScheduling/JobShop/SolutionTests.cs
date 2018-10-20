using System.Collections.Generic;
using System.Linq;
using Core.JobScheduling.Base;
using NUnit.Framework;

namespace Tests.UnitTests.JobShop
{
    [TestFixture]
    public class SolutionTests
    {
        [Test, TestCaseSource("creating_random_solutions_test_cases")]
        [Repeat(100)]
        public void test_creating_random_solutions(List<Job> jobs)
        {
            // Arrange
            var machinesCount = jobs[0].Operations.Count;

            // Act
            var solution = new Core.JobScheduling.JobShop.Solution(machinesCount, jobs);

            // Assert
            Assert.True( solution.Machines.All(m => m.Operations.Count == jobs.Count) );
        }

        [Test, TestCaseSource("creating_solution_test_cases")]
        public void test_creating_solution_with_given_operations_scheduling_order(List<Job> jobs, List<(int j, int m)> operationsSchedulingOrder, int expectedTimeSpan)
        {
            // Arrange
            var machinesCount = jobs[0].Operations.Count;
            var operationsList = new List<Operation>();

            foreach (var operation in operationsSchedulingOrder)
                operationsList.Add( jobs[ operation.j ].Operations.Find(o => o.MachineId == operation.m) );

            // Act
            var solution = new Core.JobScheduling.JobShop.Solution(machinesCount, jobs, operationsList);

            // Assert
            Assert.AreEqual(expectedTimeSpan, solution.TimeSpan);
        }
        
        [Test]
        [Repeat(30)]
        public void test_making_cockroach_moves()
        {
            // Arrange
            var jobs = GetJobShopTestSchedulingProblem2();
            var machinesCount = 3;
            var maxStep = 2;
            var solution1 = new Core.JobScheduling.JobShop.Solution(machinesCount, jobs);
            var solution2 = new Core.JobScheduling.JobShop.Solution(machinesCount, jobs);
            var diffDegree = solution1.CalculateDifferenceDegree(solution2);

            // Act
            solution1.MakeMoveTo(solution2, maxStep);
            var diffDegreeAfter = solution1.CalculateDifferenceDegree(solution2);

            // Assert
            Assert.IsTrue(solution1.IsCorrect(jobs));
            Assert.IsTrue(diffDegreeAfter <= diffDegree);

            System.IO.File.AppendAllText(
                System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/JobShop/test_making_cockroach_moves.txt", 
                $"diffDegreeBefore: {diffDegree}, diffDegreeAfter: {diffDegreeAfter}, diff: {diffDegree-diffDegreeAfter}, maxSteps: {maxStep}" + System.Environment.NewLine);
        }

        [Test, TestCaseSource("getting_possible_crossing_positions_test_cases")]
        public void test_getting_possible_crossing_positions(List<Operation> operationsList1, List<Operation> operationsList2, List<int> expectedCrossingPositions)
        {
            // Arrange
            var solution1 = new Core.JobScheduling.JobShop.Solution() {
                OperationsSchedulingOrder = operationsList1
            };
            var solution2 = new Core.JobScheduling.JobShop.Solution() {
                OperationsSchedulingOrder = operationsList2
            };

            // Act
            var result = solution1.GetPossibleCrossingPositions(solution2);

            // Assert
            Assert.AreEqual(expectedCrossingPositions, result);
        }

        private static object[] getting_possible_crossing_positions_test_cases = 
        {
            new object[] {
                new List<Operation>() {
                    new Operation() { JobId = 0, MachineId = 0 }, new Operation() { JobId = 0, MachineId = 1 }, new Operation() { JobId = 0, MachineId = 2 },
                    new Operation() { JobId = 1, MachineId = 0 }, new Operation() { JobId = 1, MachineId = 1 }, new Operation() { JobId = 1, MachineId = 2 }
                },
                new List<Operation>() {
                    new Operation() { JobId = 0, MachineId = 1 }, new Operation() { JobId = 0, MachineId = 0 }, new Operation() { JobId = 1, MachineId = 0 },
                    new Operation() { JobId = 0, MachineId = 2 }, new Operation() { JobId = 1, MachineId = 1 }, new Operation() { JobId = 1, MachineId = 2 }
                },
                new List<int>() { 1, 3, 4 }
            },
            new object[] {
                new List<Operation>() {
                    new Operation() { JobId = 0, MachineId = 0 }, new Operation() { JobId = 0, MachineId = 1 }, new Operation() { JobId = 0, MachineId = 2 },
                    new Operation() { JobId = 1, MachineId = 0 }, new Operation() { JobId = 1, MachineId = 1 }, new Operation() { JobId = 1, MachineId = 2 }
                },
                new List<Operation>() {
                    new Operation() { JobId = 0, MachineId = 0 }, new Operation() { JobId = 1, MachineId = 0 }, new Operation() { JobId = 0, MachineId = 1 },
                    new Operation() { JobId = 0, MachineId = 2 }, new Operation() { JobId = 1, MachineId = 1 }, new Operation() { JobId = 1, MachineId = 2 }
                },
                new List<int>() { 0, 3, 4 }
            }
        };

        [Test, TestCaseSource("doing_the_crossing_test_cases")]
        public void test_doing_the_crossing(List<Operation> operationsList1, List<Operation> operationsList2, List<Operation> expectedResultOperationList1, List<Operation> expectedResultOperationList2)
        {
            // Arrange
            var solution1 = new Core.JobScheduling.JobShop.Solution() { OperationsSchedulingOrder = operationsList1 };
            var solution2 = new Core.JobScheduling.JobShop.Solution() { OperationsSchedulingOrder = operationsList2 };

            // Act
            var result = solution1.DoCrossing(solution2);

            // Assert
            Assert.IsTrue( result.solution1.IsOrderFeasible() );
            Assert.IsTrue( result.solution2.IsOrderFeasible() );

            for (int i = 0; i < solution1.OperationsSchedulingOrder.Count; i++)
            {

                Assert.AreEqual( expectedResultOperationList1[i], ((Core.JobScheduling.JobShop.Solution)result.solution1).OperationsSchedulingOrder[i] );
                Assert.AreEqual( expectedResultOperationList2[i], ((Core.JobScheduling.JobShop.Solution)result.solution2).OperationsSchedulingOrder[i] );
            }
        }

        private static object[] doing_the_crossing_test_cases = 
        {
            new object[] {
                new List<Operation>() {
                    new Operation() { Id = 0, JobId = 1, MachineId = 0 }, new Operation() { Id = 0, JobId = 0, MachineId = 1 }, new Operation() { Id = 1, JobId = 0, MachineId = 0 },
                    new Operation() { Id = 1, JobId = 1, MachineId = 1 }, new Operation() { Id = 0, JobId = 2, MachineId = 1 }, new Operation() { Id = 1, JobId = 2, MachineId = 0 }
                },
                new List<Operation>() {
                    new Operation() { Id = 0, JobId = 0, MachineId = 1 }, new Operation() { Id = 0, JobId = 1, MachineId = 0 }, new Operation() { Id = 0, JobId = 2, MachineId = 1 },
                    new Operation() { Id = 1, JobId = 1, MachineId = 1 }, new Operation() { Id = 1, JobId = 2, MachineId = 0 }, new Operation() { Id = 1, JobId = 0, MachineId = 0 }
                },
                new List<Operation>() {
                    new Operation() { Id = 0, JobId = 1, MachineId = 0 }, new Operation() { Id = 0, JobId = 0, MachineId = 1 }, new Operation() { Id = 0, JobId = 2, MachineId = 1 },
                    new Operation() { Id = 1, JobId = 1, MachineId = 1 }, new Operation() { Id = 1, JobId = 2, MachineId = 0 }, new Operation() { Id = 1, JobId = 0, MachineId = 0 }
                },
                new List<Operation>() {
                    new Operation() { Id = 0, JobId = 0, MachineId = 1 }, new Operation() { Id = 0, JobId = 1, MachineId = 0 }, new Operation() { Id = 1, JobId = 0, MachineId = 0 },
                    new Operation() { Id = 1, JobId = 1, MachineId = 1 }, new Operation() { Id = 0, JobId = 2, MachineId = 1 }, new Operation() { Id = 1, JobId = 2, MachineId = 0 }
                }
            }
        };

        private static object[] creating_random_solutions_test_cases = 
        {
            new object[] {
                GetThreeJobs()
            }
        };

        private static object[] creating_solution_test_cases = 
        {
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (2,1), (0,1), (1,0), (0,0), (2,0), (1,1) },
                9
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (2,1), (2,0), (0,1), (1,0), (1,1), (0,0) },
                11
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (2,1), (1,0), (2,0), (1,1), (0,1), (0,0) },
                12
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (1,0), (1,1), (0,1), (0,0), (2,1), (2,0) },
                12
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (1,0), (0,1), (0,0), (1,1), (2,1), (2,0) },
                12
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (2,1), (2,0), (0,1), (0,0), (1,0), (1,1) },
                14
            },
            new object[] {
                GetThreeJobs(),
                new List<(int,int)> { (1,0), (1,1), (2,1), (0,1), (2,0), (0,0) },
                12
            }
        };

        private static List<Job> GetThreeJobs()
        {
            return new List<Job> {
                new Job() { 
                    Id = 0, Operations = new List<Operation> { 
                        new Operation() { Id = 0, JobId = 0, MachineId = 1, ProcessingTime = 4 },
                        new Operation() { Id = 1, JobId = 0, MachineId = 0, ProcessingTime = 2 }
                    }    
                },
                new Job() { 
                    Id = 1, Operations = new List<Operation> { 
                        new Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 3 },
                        new Operation() { Id = 1, JobId = 1, MachineId = 1, ProcessingTime = 3 }
                    }    
                },
                new Job() { 
                    Id = 2, Operations = new List<Operation> { 
                        new Operation() { Id = 0, JobId = 2, MachineId = 1, ProcessingTime = 2 },
                        new Operation() { Id = 1, JobId = 2, MachineId = 0, ProcessingTime = 3 }
                    }    
                }
            };
        }

        private static List<Job> GetJobShopTestSchedulingProblem2()
        {
            return new List<Job>() {
                new Job() { 
                    Id = 0, Operations = new List<Operation>() {
                        new Operation() { Id = 0, JobId = 0, MachineId = 0, ProcessingTime = 3 },
                        new Operation() { Id = 1, JobId = 0, MachineId = 1, ProcessingTime = 2 },
                        new Operation() { Id = 2, JobId = 0, MachineId = 2, ProcessingTime = 2 }
                    }
                },
                new Job() {
                    Id = 1, Operations = new List<Operation>() {
                        new Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 2 },
                        new Operation() { Id = 1, JobId = 1, MachineId = 2, ProcessingTime = 1 },
                        new Operation() { Id = 2, JobId = 1, MachineId = 1, ProcessingTime = 4 }
                    }
                },
                new Job() {
                    Id = 2, Operations = new List<Operation>() {
                        new Operation() { Id = 0, JobId = 2, MachineId = 1, ProcessingTime = 4 },
                        new Operation() { Id = 1, JobId = 2, MachineId = 0, ProcessingTime = 1 },
                        new Operation() { Id = 2, JobId = 2, MachineId = 2, ProcessingTime = 3 }
                    }
                }
            };
        }
    }
}