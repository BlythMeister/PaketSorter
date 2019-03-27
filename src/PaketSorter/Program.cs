using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.CodeAnalysis;

namespace PaketSorter
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    internal class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("-d|--dir <PATH>", "The path to a root of a repository, defaults to current directory if not provided (Note: <PATH> should be in quotes)", CommandOptionType.SingleValue)]
        public string Directory { get; }

        [Option("-ua|--update-args <ARGS>", "Args to pass to paket update (Note: <ARGS> should be in quotes)", CommandOptionType.SingleValue)]
        public string UpdateArgs { get; }

        [Option("-ia|--install-args <ARGS>", "Args to pass to paket install (Note: <ARGS> should be in quotes)", CommandOptionType.SingleValue)]
        public string InstallArgs { get; }

        [Option("-sa|--simplify-args <ARGS>", "Args to pass to paket simplify (Note: <ARGS> should be in quotes)", CommandOptionType.SingleValue)]
        public string SimplifyArgs { get; }

        [Option("-s|--simplify", "Include a paket simplify", CommandOptionType.NoValue)]
        public bool Simplify { get; }

        [Option("-u|--update", "Include a paket update", CommandOptionType.NoValue)]
        public bool Update { get; }

        [Option("-np|--no-prompt", "Never prompt user input", CommandOptionType.NoValue)]
        public bool NoPrompt { get; }

        private int OnExecute() => Sorter.Run(Directory, NoPrompt, Simplify, Update, UpdateArgs, InstallArgs, SimplifyArgs);
    }
}
