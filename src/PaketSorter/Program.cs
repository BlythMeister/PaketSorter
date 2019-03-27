using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.CodeAnalysis;

namespace PaketSorter
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    internal class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("-d|--dir <PATH>", "The path to a root of a repository", CommandOptionType.SingleValue)]
        public string Directory { get; }

        [Option("-ac|--auto-close", "Should app auto close", CommandOptionType.NoValue)]
        public bool AutoClose { get; }

        private int OnExecute() => Sorter.Run(Directory, AutoClose);
    }
}
