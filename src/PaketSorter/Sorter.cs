using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace PaketSorter
{
    internal class Sorter
    {
        public static int Run(string rootDir, bool noPrompt, bool simplify, bool update, string updateArgs, string installArgs, string simplifyArgs)
        {
            Console.WriteLine($"Starting at {DateTime.UtcNow:u}");

            try
            {
                CheckForNewVersion(noPrompt);
                rootDir = string.IsNullOrWhiteSpace(rootDir) ? Environment.CurrentDirectory : rootDir;
                ValidatePaths(rootDir);
                Console.WriteLine($"Running against: {rootDir}");
                if (update) RunPaketCommand(rootDir, "update", updateArgs);
                SortReferences(rootDir);
                SortDependencies(rootDir);
                if (simplify) RunPaketCommand(rootDir, "simplify", simplifyArgs);
                RunPaketCommand(rootDir, "install", installArgs);

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
                if (!noPrompt)
                {
                    Console.WriteLine("Press Enter To Close...");
                    Console.ReadLine();
                }
            }
        }

        private static void CheckForNewVersion(bool noPrompt)
        {
            if (!Debugger.IsAttached)
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("User-Agent", "PaketSorter");
                        var data = webClient.DownloadString("https://api.github.com/repos/BlythMeister/PaketSorter/releases/latest");
                        dynamic latestRelease = JObject.Parse(data);

                        var latestVersion = Version.Parse(latestRelease.tag_name.Value.Substring(1));

                        if (latestVersion.CompareTo(currentVersion) > 0)
                        {
                            Console.WriteLine("There is a new version of Paket Sorter, visit https://github.com/BlythMeister/PaketSorter/releases/latest to download");
                            if (!noPrompt)
                            {
                                Console.WriteLine($"Press Enter to continue with version {currentVersion}");
                                Console.ReadLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("You are already running the latest version of Paket Sorter");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to check for latest version of Paket Sorter");
                    Console.WriteLine(e);
                    if (!noPrompt)
                    {
                        Console.WriteLine($"Press Enter to continue with version {currentVersion}");
                        Console.ReadLine();
                    }
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

        private static void RunPaketCommand(string rootDir, string command, string args)
        {
            var commandPlusArgs = $"{command} {args ?? string.Empty}".Trim();
            Console.WriteLine($"Running paket command: {commandPlusArgs}");
            var paketProcess = new ProcessStartInfo(Path.Combine(rootDir, ".paket", "paket.exe"), commandPlusArgs)
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
