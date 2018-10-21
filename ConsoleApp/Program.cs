using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Algorithms;
using Core.JobScheduling.Base;
using Core.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace ConsoleApp
{
    class Program
    {
        private static string _inputDataDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/../../../InputData";
        private static string _resultsDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/../../../Results";

        static void Main(string[] args)
        {
            var runner = new Runner();
            var app = new CommandLineApplication();
            app.Description = "Nature-inspired algorithms in jobs scheduling problems";
        
            app.HelpOption("-?|-h|--help");

            app.Command("get-test-instances", (command) => {
                command.Description = "Download Taillard tests instances";
                command.HelpOption("-?|-h|--help");
    
                command.OnExecute(() => {
                    Console.WriteLine("Downloading...");
                    runner.DownloadTestInstances();
                    Console.WriteLine("Downloading finished.");
                    return 0;
                });
            });

            app.Command("co", (command) => {
                command.Description = "Solve job scheduling problem using Cockroach Optimization algorithm.";
                command.HelpOption("-?|-h|--help");
                
                var populationSizeOption    = command.Option("-s|--size",       "Specify population size",                              CommandOptionType.SingleValue);
                var iterationsNumberOption  = command.Option("-i|--iterations", "Specify number of iterations",                         CommandOptionType.SingleValue);
                var visualOption            = command.Option("-v|--visual",     "Specify visual parameter",                             CommandOptionType.SingleValue);
                var maxStepOption           = command.Option("-m|--max-step",   "Specify max step size",                                CommandOptionType.SingleValue);
                var jobTypeOption           = command.Option("-j|--job-type",   "Specify job scheduling type: \"f\", \"j\" or \"o\"",   CommandOptionType.SingleValue);
                var inputFileOption         = command.Option("-f|--input-file", "Specify input file",                                   CommandOptionType.SingleValue);
                var saveResultOption        = command.Option("-r|--save-result","Specify if save result",                               CommandOptionType.NoValue);
                var executionsNumberOption  = command.Option("-e|--executions", "Specify number of executions",                         CommandOptionType.SingleValue);

                command.OnExecute(() => {

                    var populationSize  = populationSizeOption.HasValue()   ? Int16.Parse( populationSizeOption.Value() )   : 10;
                    var iterationNumber = iterationsNumberOption.HasValue() ? Int16.Parse( iterationsNumberOption.Value() ) : 20;
                    var visual          = visualOption.HasValue()           ? Int16.Parse( visualOption.Value() )           : 10;
                    var maxStep         = maxStepOption.HasValue()          ? Int16.Parse( maxStepOption.Value() )          : 4;
                    var jobType         = jobTypeOption.HasValue()          ? jobTypeOption.Value()                         : "f";
                    var inputFile       = inputFileOption.HasValue()        ? inputFileOption.Value()                       : "TestInstances/flowshop/tai20_5/0.txt";
                    var saveResult      = saveResultOption.HasValue()       ? true                                          : false;
                    var executions      = executionsNumberOption.HasValue() ? Int16.Parse( executionsNumberOption.Value() ) : 1;

                    var optionsString = 
                          $" - population size:\t {populationSize}, {Environment.NewLine}"
                        + $" - iterations number:\t {iterationNumber}. {Environment.NewLine}"
                        + $" - visual parameter:\t {visual}, {Environment.NewLine}"
                        + $" - max step paramter:\t {maxStep}, {Environment.NewLine}"
                        + $" - job scheduling type:\t {jobType}, {Environment.NewLine}"
                        + $" - input file:\t\t {inputFile} {Environment.NewLine}"
                        + $" - save results:\t {saveResult} {Environment.NewLine}"
                        + $" - executions number:\t {executions} {Environment.NewLine}";

                    Console.WriteLine($"{Environment.NewLine}"
                        + $"Starting perform CO algorithm with given options: {Environment.NewLine}"
                        + optionsString
                    );

                    var inputData = FileHelper.ReadFile(_inputDataDirectory, inputFile);
                    var schedulingProblem = SchedulingProblemFactory.Create(jobType, inputData);
                    var algorithm = new CockroachAlgorithm(maxStep, visual, iterationNumber, populationSize, schedulingProblem);                 
                    var stringResult = optionsString.Replace("\t", "") + Environment.NewLine;
                    
                    for (int i = 0; i < executions; i++)
                    {
                        if (executions > 0)
                            Console.WriteLine("Execution number: " + (i + 1));

                        var result = algorithm.Perform();
                        stringResult += Environment.NewLine + result.TimeSpan;
                        Console.WriteLine(Environment.NewLine + "Result: " + result.TimeSpan);
                    }

                    if (saveResult)
                    {
                        DateTime time = DateTime.Now;
                        string format = "dd.MM.yyyy_HH:mm:ss";
                        var fileName = "CO_" + time.ToString(format) + ".txt";
                        Console.WriteLine("Saving results to " + fileName);
                        FileHelper.CreateFile(_resultsDirectory, fileName, stringResult);
                    }

                    Console.WriteLine(Environment.NewLine + "Calculations finished.");
                    return 0;
                });
            });

            app.Command("aco", (command) => {
                command.Description = "Solve job scheduling problem using Ant Colony Optimization algorithm.";
                command.HelpOption("-?|-h|--help");

                var populationSizeOption    = command.Option("-s|--size",       "Specify population size",                              CommandOptionType.SingleValue);
                var iterationsNumberOption  = command.Option("-i|--iterations", "Specify number of iterations",                         CommandOptionType.SingleValue);
                var evaporationRateOption   = command.Option("-p|--evaporation","Specify value of evaporation rate parameter",          CommandOptionType.SingleValue);
                var laidPheromoneOption     = command.Option("-q",              "Specify value of laid pheromone parameter Q",          CommandOptionType.SingleValue);
                var jobTypeOption           = command.Option("-j|--job-type",   "Specify job scheduling type: \"f\", \"j\" or \"o\"",   CommandOptionType.SingleValue);
                var inputFileOption         = command.Option("-f|--input-file", "Specify input file",                                   CommandOptionType.SingleValue);
                var saveResultOption        = command.Option("-r|--save-result","Specify if save result",                               CommandOptionType.NoValue);
                var executionsNumberOption  = command.Option("-e|--executions", "Specify number of executions",                         CommandOptionType.SingleValue);

                command.OnExecute(() => {
                    var populationSize  = populationSizeOption.HasValue()   ? Int16.Parse( populationSizeOption.Value() )   : 10;
                    var iterationNumber = iterationsNumberOption.HasValue() ? Int16.Parse( iterationsNumberOption.Value() ) : 5;
                    var evaporationRate = evaporationRateOption.HasValue()  ? float.Parse( evaporationRateOption.Value() )  : 0.2F;
                    var laidPheromone   = laidPheromoneOption.HasValue()    ? float.Parse( laidPheromoneOption.Value() )    : 1F;
                    var jobType         = jobTypeOption.HasValue()          ? jobTypeOption.Value()                         : "f";
                    var inputFile       = inputFileOption.HasValue()        ? inputFileOption.Value()                       : "TestInstances/flowshop/tai20_5/0.txt";
                    var saveResult      = saveResultOption.HasValue()       ? true                                          : false;
                    var executions      = executionsNumberOption.HasValue() ? Int16.Parse( executionsNumberOption.Value() ) : 1;

                    var optionsString = 
                          $" - population size:\t {populationSize}, {Environment.NewLine}"
                        + $" - iterations number:\t {iterationNumber}. {Environment.NewLine}"
                        + $" - evaporation rate:\t {evaporationRate}, {Environment.NewLine}"
                        + $" - laid pheromone parameter Q:\t {laidPheromone}, {Environment.NewLine}"
                        + $" - job scheduling type:\t {jobType}, {Environment.NewLine}"
                        + $" - input file:\t\t {inputFile} {Environment.NewLine}"
                        + $" - save results:\t {saveResult} {Environment.NewLine}"
                        + $" - executions number:\t {executions} {Environment.NewLine}";

                    Console.WriteLine($"{Environment.NewLine}"
                        + $"Starting perform ACO algorithm with given options: {Environment.NewLine}"
                        + optionsString
                    );

                    var inputData = FileHelper.ReadFile(_inputDataDirectory, inputFile);
                    var schedulingProblem = SchedulingProblemFactory.Create(jobType, inputData);
                    var algorithm = new AntColonyOptimizationAlgorithm(iterationNumber, populationSize, evaporationRate, schedulingProblem, laidPheromone);                 
                    var stringResult = optionsString.Replace("\t", "") + Environment.NewLine;

                    for (int i = 0; i < executions; i++)
                    {
                        if (executions > 0)
                            Console.WriteLine("Execution number: " + (i + 1));

                        var result = algorithm.Perform();
                        stringResult += Environment.NewLine + result.TimeSpan;
                        Console.WriteLine(Environment.NewLine + "Result: " + result.TimeSpan);
                    }
                    
                    if (saveResult)
                    {
                        DateTime time = DateTime.Now;
                        string format = "dd.MM.yyyy_HH:mm:ss";
                        var fileName = "ACO_" + time.ToString(format) + ".txt";
                        Console.WriteLine("Saving results to " + fileName);
                        FileHelper.CreateFile(_resultsDirectory, fileName, stringResult);
                    }

                    Console.WriteLine(Environment.NewLine + "Calculations finished.");

                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
