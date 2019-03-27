using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PaketSorter
{
    internal class Sorter
    {
        internal static int Run(string directory, bool autoClose)
        {
            Console.WriteLine($"Starting at {DateTime.UtcNow:u}");

            try
            {
                var rootDir = string.IsNullOrWhiteSpace(directory) ? Environment.CurrentDirectory : directory;
                ValidatePaths(rootDir);
                Console.WriteLine($"Running against: {rootDir}");
                RunPaketCommand(rootDir, "update");
                SortReferences(rootDir);
                SortDependencies(rootDir);
                RunPaketCommand(rootDir, "simplify");
                RunPaketCommand(rootDir, "install");

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
                if (!autoClose)
                {
                    Console.WriteLine("Press Enter To Close...");
                    Console.ReadLine();
                }
            }
        }

        private static void ValidatePaths(string rootDir)
        {
            if (!Directory.Exists(rootDir))
            {
                throw new DirectoryNotFoundException($"Unable to locate directory {rootDir}");
            }

            if (!Directory.Exists(Path.Combine(rootDir, ".paket")))
            {
                throw new DirectoryNotFoundException($"Unable to locate .paket directory in {rootDir}");
            }

            if (!File.Exists(Path.Combine(rootDir, ".paket", "paket.exe")))
            {
                throw new FileNotFoundException($"Unable to locate paket.exe directory in {rootDir}\\.paket");
            }
        }

        private static void RunPaketCommand(string rootDir, string command)
        {
            Console.WriteLine($"Running paket command: {command}");
            var paketProcess = new ProcessStartInfo(Path.Combine(rootDir, ".paket", "paket.exe"), command)
            {
                WorkingDirectory = rootDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(paketProcess);
            while (process != null && !process.StandardOutput.EndOfStream)
            {
                Console.WriteLine(process.StandardOutput.ReadLine());
            }
        }

        private static void SortReferences(string rootDir)
        {
            foreach (var file in Directory.GetFiles(rootDir, "paket.references", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Sorting reference file: {file}");
                var content = File.ReadAllLines(file);

                var newContent = new List<string>();
                var nugetBlock = new List<string>();
                var onNuget = false;
                var previousLineBreak = false;

                foreach (var line in content)
                {
                    if (line.Trim().StartsWith("group"))
                    {
                        onNuget = false;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        onNuget = true;
                        previousLineBreak = false;
                    }
                    else
                    {
                        previousLineBreak = true;
                    }

                    if (onNuget && !string.IsNullOrWhiteSpace(line))
                    {
                        nugetBlock.Add(line);
                    }

                    if (!onNuget)
                    {
                        if (nugetBlock.Any())
                        {
                            newContent.AddRange(nugetBlock.OrderBy(x => x));
                            nugetBlock.Clear();
                        }

                        if (previousLineBreak)
                        {
                            newContent.Add("");
                            previousLineBreak = false;
                        }

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            newContent.Add(line);
                        }
                    }
                }

                if (nugetBlock.Any())
                {
                    newContent.AddRange(nugetBlock.OrderBy(x => x));
                }

                newContent.Add("");

                File.WriteAllLines(file, newContent);
            }
        }

        private static void SortDependencies(string rootDir)
        {
            foreach (var file in Directory.GetFiles(rootDir, "paket.dependencies", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Sorting dependencies file: {file}");
                var content = File.ReadAllLines(file);

                var newContent = new List<string>();
                var nugetBlock = new List<string>();
                var onNuget = false;
                var previousLineBreak = false;

                foreach (var line in content)
                {
                    if (line.Trim().StartsWith("nuget"))
                    {
                        onNuget = true;
                        previousLineBreak = false;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        onNuget = false;
                    }
                    else
                    {
                        previousLineBreak = true;
                    }

                    if (onNuget && !string.IsNullOrWhiteSpace(line))
                    {
                        nugetBlock.Add(line);
                    }

                    if (!onNuget)
                    {
                        if (nugetBlock.Any())
                        {
                            newContent.AddRange(nugetBlock.OrderBy(x => x));
                            nugetBlock.Clear();
                        }

                        if (previousLineBreak)
                        {
                            newContent.Add("");
                            previousLineBreak = false;
                        }

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            newContent.Add(line);
                        }
                    }
                }

                if (nugetBlock.Any())
                {
                    newContent.AddRange(nugetBlock.OrderBy(x => x));
                }

                newContent.Add("");

                File.WriteAllLines(file, newContent);
            }
        }
    }
}
