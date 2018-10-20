using System;
using Microsoft.Extensions.CommandLineUtils;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Description = "Nature-inspired algorithms in jobs scheduling problems";
        
            app.HelpOption("-?|-h|--help");

            app.Command("get-test-instances", (command) => {
                command.Description = "Download Taillard tests instances";
                command.HelpOption("-?|-h|--help");
    
                command.OnExecute(() => {
                    Console.WriteLine("Downloading finished.");
                    return 0;
                });
            });

            app.Command("calculate", (command) => {
                command.Description = "Solve job scheduling problem using one of the algorithms.";
                command.HelpOption("-?|-h|--help");
                
                var algorithmType = command.Option("-a|--algorithm", "Specify algorithm type. Available options: \"CO\", \"ACO\", \"GA\".", CommandOptionType.SingleValue);
                
                command.OnExecute(() => {
                    if (algorithmType.HasValue()) {
                        Console.WriteLine($"{algorithmType.Value()} algorithm selected.");
                    }
                    else {
                        Console.WriteLine($"You have to specify algorithm type using. Available options: \"CO\", \"ACO\", \"GA\".");
                    }
                    Console.WriteLine("Calculations finished.");
                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
