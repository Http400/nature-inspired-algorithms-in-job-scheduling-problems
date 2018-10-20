using System.Collections.Generic;
using Core.JobScheduling.Base;
using NUnit.Framework;

namespace Tests.UnitTests.FlowShop
{
    [TestFixture]
    public class SolutionTests
    {
        [Test]
        public void test_creating_random_solution()
        {
            // Act
            var solution1 = new Core.JobScheduling.FlowShop.Solution(4, jobs);
            var solution2 = new Core.JobScheduling.FlowShop.Solution(4, jobs);

            // Assert
            Assert.AreNotEqual(solution1, solution2);
        }

        [Test, TestCaseSource("creating_solution_test_cases")]
        public void test_creating_solution_with_given_order(int machinesCount, List<Job> jobs, List<int> jobsOrder, int timeSpan)
        {
            // Act
            var solution = new Core.JobScheduling.FlowShop.Solution(machinesCount, jobs, jobsOrder);

            // Assert
            Assert.AreEqual(solution.TimeSpan, timeSpan);
        }

        [Test, TestCaseSource("getting_possible_crossing_positions_test_cases")]
        public void test_getting_possible_crossing_positions(List<int> jobsList1, List<int> jobsList2, List<int> expectedCrossingPositions)
        {
            // Arrange
            var solution1 = new Core.JobScheduling.FlowShop.Solution() {
                JobsOrder = jobsList1
            };
            var solution2 = new Core.JobScheduling.FlowShop.Solution() {
                JobsOrder = jobsList2
            };

            // Act
            var result = solution1.GetPossibleCrossingPositions(solution2);

            // Assert
            Assert.AreEqual(expectedCrossingPositions, result);
        }

        private static object[] getting_possible_crossing_positions_test_cases = 
        {
            new object[] {
                new List<int>() { 0, 1, 2, 3, 4, 5 },
                new List<int>() { 1, 0, 3, 2, 4, 5},
                new List<int>() { 1, 3, 4 }
            },
            new object[] {
                new List<int>() { 0, 1, 2, 3, 4, 5 },
                new List<int>() { 0, 3, 1, 2, 4, 5 },
                new List<int>() { 0, 3, 4 }
            }
        };

        private static readonly List<Job> jobs = new List<Job> {
            new Job() { 
                Id = 0, Operations = new List<Operation> { 
                    new Operation() { Id = 0, JobId = 0, MachineId = 0, ProcessingTime = 2 },
                    new Operation() { Id = 1, JobId = 0, MachineId = 1, ProcessingTime = 3 },
                    new Operation() { Id = 2, JobId = 0, MachineId = 2, ProcessingTime = 1 },
                    new Operation() { Id = 3, JobId = 0, MachineId = 3, ProcessingTime = 2 }
                }    
            },
            new Job() { 
                Id = 1, Operations = new List<Operation> { 
                    new Operation() { Id = 0, JobId = 1, MachineId = 0, ProcessingTime = 1 },
                    new Operation() { Id = 1, JobId = 1, MachineId = 1, ProcessingTime = 3 },
                    new Operation() { Id = 2, JobId = 1, MachineId = 2, ProcessingTime = 1 },
                    new Operation() { Id = 3, JobId = 1, MachineId = 3, ProcessingTime = 3 }
                }    
            },
            new Job() { 
                Id = 2, Operations = new List<Operation> { 
                    new Operation() { Id = 0, JobId = 2, MachineId = 0, ProcessingTime = 3 },
                    new Operation() { Id = 1, JobId = 2, MachineId = 1, ProcessingTime = 2 },
                    new Operation() { Id = 2, JobId = 2, MachineId = 2, ProcessingTime = 2 },
                    new Operation() { Id = 3, JobId = 2, MachineId = 3, ProcessingTime = 1 }
                }    
            }
        };

        private static object[] creating_solution_test_cases = 
        {
            new object[] {
                4, jobs, new List<int>() { 0, 1, 2 }, 13
            },
            new object[] {
                4, jobs, new List<int>() { 0, 2, 1 }, 14
            },
            new object[] {
                4, jobs, new List<int>() { 1, 0, 2 }, 12
            }
        };
    }
}