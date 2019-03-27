using McMaster.Extensions.CommandLineUtils;
using System;

namespace PaketSorter
{
    internal class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("-d|--dir <PATH>", "The path to a root of a repository", CommandOptionType.SingleValue)]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Directory { get; }

        [Option("-ac|--auto-close", "Should app auto close", CommandOptionType.NoValue)]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool AutoClose { get; }

        // ReSharper disable once UnusedMember.Local
        private int OnExecute()
        {
            Console.WriteLine($"Starting at {DateTime.UtcNow:u}");
            var sorter = new Sorter();

            try
            {
                if (string.IsNullOrWhiteSpace(Directory))
                {
                    var directory = Prompt.GetString("Please enter path to root of a repository: ");
                    if (directory == null)
                    {
                        throw new Exception("Directory is not provided");
                    }
                    sorter.Run(directory);
                }
                else
                {
                    sorter.Run(Directory);
                }

                Console.WriteLine($"Done at {DateTime.UtcNow:u}");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed at {DateTime.UtcNow:u}");
                Console.WriteLine(e);
                return -1;
            }
            finally
            {
                if (!AutoClose)
                {
                    Console.WriteLine("Press Enter To Close...");
                    Console.ReadLine();
                }
            }
        }
    }
}
