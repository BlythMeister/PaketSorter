using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace PaketSorter
{
    internal class Sorter
    {
        public static int Run(Runner runner, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting at {DateTime.UtcNow:u}");

            try
            {
                if (!CheckForNewVersion(runner.NoPrompt, runner.Verbose))
                {
                    return -1;
                }

                var rootDir = string.IsNullOrWhiteSpace(runner.Directory) ? Environment.CurrentDirectory : runner.Directory;
                ValidatePaths(rootDir);
                Console.WriteLine($"Running against: {rootDir}");
                Console.WriteLine("-----------------------------------------------------");
                if (cancellationToken.IsCancellationRequested) return -2;

                if (!runner.SkipSort)
                {
                    SortReferences(rootDir);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                    SortDependencies(rootDir);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }

                if (runner.ClearCache)
                {
                    RunPaketCommand(rootDir, "clear-cache", "--clear-local", cancellationToken);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }

                if (runner.CleanObj)
                {
                    CleanObjFiles(rootDir);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }

                if (runner.Reinstall)
                {
                    if (runner.Update)
                    {
                        Console.WriteLine("Skipping Update as reinstall install newest versions");
                    }

                    Console.WriteLine("Deleting paket.lock file");
                    File.Delete(Path.Combine(rootDir, "paket.lock"));
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }
                else if (runner.Update)
                {
                    RunPaketCommand(rootDir, "update", runner.UpdateArgs, cancellationToken);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }

                if (runner.Simplify)
                {
                    RunPaketCommand(rootDir, "simplify", runner.SimplifyArgs, cancellationToken);
                    Console.WriteLine("-----------------------------------------------------");
                    if (cancellationToken.IsCancellationRequested) return -2;
                }

                RunPaketCommand(rootDir, "install", runner.InstallArgs, cancellationToken);
                Console.WriteLine("-----------------------------------------------------");
                if (cancellationToken.IsCancellationRequested) return -2;

                Console.WriteLine($"Done at {DateTime.UtcNow:u}");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine($"Failed at {DateTime.UtcNow:u}");
                if (runner.Verbose)
                {
                    Console.WriteLine(e);
                }
                else
                {
                    Console.WriteLine(e.Message);
                }

                return -1;
            }
            finally
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Execution Cancelled");
                }

                if (!runner.NoPrompt)
                {
                    Console.WriteLine("Press Enter To Close...");
                    Console.ReadLine();
                }
            }
        }

        private static void CleanObjFiles(string rootDir)
        {
            foreach (var paketProps in new DirectoryInfo(rootDir).GetFiles("*.paket.props", SearchOption.AllDirectories))
            {
                if (paketProps.Directory != null)
                {
                    Console.WriteLine($"Cleaning files in {paketProps.Directory.FullName}");
                    foreach (var file in paketProps.Directory?.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
        }

        private static bool CheckForNewVersion(bool noPrompt, bool verbose)
        {
            if (!Debugger.IsAttached)
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                try
                {
                    using var webClient = new WebClient();
                    webClient.Headers.Add("User-Agent", "PaketSorter");
                    var data = webClient.DownloadString("https://api.github.com/repos/BlythMeister/PaketSorter/releases/latest");
                    dynamic latestRelease = JObject.Parse(data);

                    var latestVersion = Version.Parse(latestRelease.tag_name.Value.Substring(1));

                    if (latestVersion.CompareTo(currentVersion) > 0)
                    {
                        Console.WriteLine("There is a new version of Paket Sorter, visit https://github.com/BlythMeister/PaketSorter/releases/latest to download");
                        if (!noPrompt)
                        {
                            Console.WriteLine($"Press type 'oldver' to continue with version {currentVersion}");
                            if (Console.ReadLine()?.ToLower() != "oldver")
                            {
                                Console.WriteLine("Stopping as you have an old version");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("You are already running the latest version of Paket Sorter");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to check for latest version of Paket Sorter");
                    if (verbose)
                    {
                        Console.WriteLine(e);
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            return true;
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

        private static void RunPaketCommand(string rootDir, string command, string args, CancellationToken cancellationToken)
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
            if (process != null)
            {
                cancellationToken.Register(() => process.Kill(true));
                while (!process.StandardOutput.EndOfStream && !process.HasExited)
                {
                    Console.WriteLine(process.StandardOutput.ReadLine());
                }
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

                foreach (var line in content)
                {
                    if (line.Trim().StartsWith("group"))
                    {
                        onNuget = false;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        onNuget = true;
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
                            newContent.Add("");
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
                    newContent.Add("");
                }

                if (newContent.Count == 0)
                {
                    newContent.Add("");
                }

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

                foreach (var line in content)
                {
                    if (line.Trim().StartsWith("nuget") || line.Trim().StartsWith("clitool"))
                    {
                        onNuget = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        onNuget = false;
                    }

                    if (onNuget && !string.IsNullOrWhiteSpace(line))
                    {
                        nugetBlock.Add(line);
                    }

                    if (!onNuget)
                    {
                        if (nugetBlock.Any())
                        {
                            newContent.Add("");
                            newContent.AddRange(nugetBlock.Where(x => x.Trim().StartsWith("nuget")).OrderBy(x => x));
                            newContent.AddRange(nugetBlock.Where(x => x.Trim().StartsWith("clitool")).OrderBy(x => x));
                            newContent.Add("");
                            nugetBlock.Clear();
                        }

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            newContent.Add(line);
                        }
                    }
                }

                if (nugetBlock.Any())
                {
                    newContent.Add("");
                    newContent.AddRange(nugetBlock.Where(x => x.Trim().StartsWith("nuget")).OrderBy(x => x));
                    newContent.AddRange(nugetBlock.Where(x => x.Trim().StartsWith("clitool")).OrderBy(x => x));
                    newContent.Add("");
                }

                if (newContent.Count == 0)
                {
                    newContent.Add("");
                }

                File.WriteAllLines(file, newContent);
            }
        }
    }
}
