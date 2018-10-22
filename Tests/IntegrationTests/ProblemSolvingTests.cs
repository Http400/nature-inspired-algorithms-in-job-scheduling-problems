using System.IO;
using Core.Algorithms;
using Core.JobScheduling.Base;
using Core.Utils;
using NUnit.Framework;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class ProblemSolvingTests
    {
        [Test, TestCaseSource("perform_ant_colony_optimization_algorithm_test_cases")]
        public void test_perform_ant_colony_optimization_algorithm(string directory, string fileName, SchedulingProblemType schedulingProblemType, int maxIterations, int populationCount, float evaporationRate)
        {
            // Arrange
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestInstances/" + directory;
            var resultsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestResults/ACO";

            // Act
            var inputData = FileHelper.ReadFile(path, fileName);
            var schedulingProblem = SchedulingProblemFactory.Create(schedulingProblemType, inputData);
            var algorithm = new AntColonyOptimizationAlgorithm(maxIterations, populationCount, evaporationRate, schedulingProblem);
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = algorithm.Perform();
            watch.Stop();

            // Assert
            Assert.IsTrue(result.IsCorrect(schedulingProblem.Jobs));

            // Writing result to file
            var executionTime = watch.Elapsed.ToString("hh\\:mm\\:ss");
            FileHelper.WriteToFile(resultsPath + "/" + directory, fileName, 
                $@"maxIterations: {maxIterations}, populationCount: {populationCount}, evaporationRate: {evaporationRate}, timeSpan: {result.TimeSpan}, time: {executionTime}"
            );
        }

        private static object[] perform_ant_colony_optimization_algorithm_test_cases = 
        {
            // new object[] {
            //     "flowshop/tai20_5",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.FlowShop, // schedulingProblemType
            //     50,                            // maxIterations
            //     50,                            // populationCount
            //     0.2F                            // evaporationRate
            // }
            // new object[] {
            //     "jobshop/tai15_15",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.JobShop,  // schedulingProblemType
            //     5,                             // maxIterations
            //     10,                             // populationCount
            //     0.8F                            // evaporationRate
            // }
            new object[] {
                "openshop/tai4_4",             // directory
                "0.txt",                        // fileName
                SchedulingProblemType.OpenShop,  // schedulingProblemType
                20,                             // maxIterations
                50,                             // populationCount
                0.2F                            // evaporationRate
            }
        };

        [Test, TestCaseSource("perform_cockroach_optimization_algorithm_test_cases")]
        public void test_perform_cockroach_optimization_algorithm(string directory, string fileName, SchedulingProblemType schedulingProblemType, int maxIterations, int populationCount, int maxStep, int visual)
        {
            // Arrange
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestInstances/" + directory;
            var resultsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestResults/CO";

            // Act
            var inputData = FileHelper.ReadFile(path, fileName);
            var schedulingProblem = SchedulingProblemFactory.Create(schedulingProblemType, inputData);
            var algorithm = new CockroachAlgorithm(maxStep, visual, maxIterations, populationCount, schedulingProblem);
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = algorithm.Perform();
            watch.Stop();
            var executionTime = watch.Elapsed.ToString("hh\\:mm\\:ss");

            // Assert
            Assert.IsTrue(result.IsCorrect(schedulingProblem.Jobs));

            // Writing result to file
            FileHelper.WriteToFile(resultsPath + "/" + directory, fileName, 
                $@"maxIterations: {maxIterations}, populationCount: {populationCount}, visual: {visual}, maxStep: {maxStep} timeSpan: {result.TimeSpan}, time: {executionTime}"
            );
        }

        private static object[] perform_cockroach_optimization_algorithm_test_cases = 
        {
            // new object[] {
            //     "flowshop/tai20_5",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.FlowShop, // schedulingProblemType
            //     20,                            // maxIterations
            //     50,                            // populationCount
            //     5,                           // maxStep
            //     10                               // visual
            // }
            new object[] {
                "jobshop/tai15_15",             // directory
                "0.txt",                        // fileName
                SchedulingProblemType.JobShop,  // schedulingProblemType
                50,                             // maxIterations
                50,                             // populationCount
                10,                           // maxStep
                20                               // visual
            }
            // new object[] {
            //     "openshop/tai4_4",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.OpenShop,  // schedulingProblemType
            //     100,                             // maxIterations
            //     100,                             // populationCount
            //     4,                           // maxStep
            //     12                             // visual
            // }
        };

        [Test, TestCaseSource("perform_genetic_algorithm_test_cases")]
        public void test_perform_genetic_algorithm(string directory, string fileName, SchedulingProblemType schedulingProblemType, int maxIterations, int populationCount)
        {
            // Arrange
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestInstances/" + directory;
            var resultsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestResults/GA";

            // Act
            var inputData = FileHelper.ReadFile(path, fileName);
            var schedulingProblem = SchedulingProblemFactory.Create(schedulingProblemType, inputData);
            var algorithm = new GeneticAlgorithm(maxIterations, populationCount, schedulingProblem, 0.1F);
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = algorithm.Perform();
            watch.Stop();
            var executionTime = watch.Elapsed.ToString("hh\\:mm\\:ss");

            // Assert
            if (!result.IsCorrect(schedulingProblem.Jobs))
            {
                result.PropagateOperations(schedulingProblem.Jobs);
            }

            Assert.IsTrue(result.IsCorrect(schedulingProblem.Jobs));

            // Writing result to file
            FileHelper.WriteToFile(resultsPath + "/" + directory, fileName, 
                $@"maxIterations: {maxIterations}, populationCount: {populationCount}, timeSpan: {result.TimeSpan}, time: {executionTime}"
            );
        }

        private static object[] perform_genetic_algorithm_test_cases = 
        {
            // new object[] {
            //     "flowshop/tai20_5",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.FlowShop, // schedulingProblemType
            //     100,                            // maxIterations
            //     50,                            // populationCount
            // }
            new object[] {
                "jobshop/tai15_15",             // directory
                "0.txt",                        // fileName
                SchedulingProblemType.JobShop,  // schedulingProblemType
                100,                             // maxIterations
                50,                             // populationCount
            }
            // new object[] {
            //     "openshop/tai4_4",             // directory
            //     "0.txt",                        // fileName
            //     SchedulingProblemType.OpenShop,  // schedulingProblemType
            //     200,                             // maxIterations
            //     100,                             // populationCount
            // }
        };

        [Test, TestCaseSource("creating_scheduling_problem_test_cases")]
        public void test_creating_scheduling_problem_from_file_input_data(string directory, string fileName, SchedulingProblemType schedulingProblemType, int expectedJobsCount, int expectedMachinesCount)
        {
            // Arrange
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestInstances/" + directory;

            // Act
            var inputData = FileHelper.ReadFile(path, fileName);
            var schedulingProblem = SchedulingProblemFactory.Create(schedulingProblemType, inputData);

            // Assert
            Assert.AreEqual(expectedJobsCount, schedulingProblem.JobsCount);
            Assert.AreEqual(expectedJobsCount, schedulingProblem.Jobs.Count);
            Assert.AreEqual(expectedMachinesCount, schedulingProblem.MachinesCount);
        }

        private static object[] creating_scheduling_problem_test_cases = 
        {
            new object[] {
                "flowshop/tai20_5",             // directory
                "0.txt",                        // fileName
                SchedulingProblemType.FlowShop, // schedulingProblemType
                20,                             // jobsCount
                5                               // machinesCount
            },
            new object[] {
                "jobshop/tai15_15",             // directory
                "0.txt",                        // fileName
                SchedulingProblemType.JobShop,  // schedulingProblemType
                15,                             // jobsCount
                15                              // machinesCount
            },
            new object[] {
                "openshop/tai4_4",              // directory
                "0.txt",                        // fileName
                SchedulingProblemType.OpenShop, // schedulingProblemType
                4,                              // jobsCount
                4                               // machinesCount
            }
        };
    }
}