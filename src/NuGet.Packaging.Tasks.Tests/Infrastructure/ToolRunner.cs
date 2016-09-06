using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class ToolRunner
    {
        private static readonly object ToolRunnerTasksLock = new object();
        private static readonly Dictionary<string, Task> ToolRunnerTasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

        public static Task Run(string command, string arguments, string workingDirectory)
        {
            Task toolRunnerTask;
            lock (ToolRunnerTasksLock)
            {
                if (!ToolRunnerTasks.TryGetValue(workingDirectory, out toolRunnerTask))
                {
                    toolRunnerTask = RunTool(command, arguments, workingDirectory);
                    ToolRunnerTasks[command + workingDirectory] = toolRunnerTask;
                }
            }

            return toolRunnerTask;
        }

        private static Task<int> RunTool(string fileName, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            var outputLines = new List<string>();

            process.ErrorDataReceived += (s, e) =>
            {
                lock (outputLines)
                    outputLines.Add(e.Data);
            };
            process.OutputDataReceived += (s, e) =>
            {
                lock (outputLines)
                    outputLines.Add(e.Data);
            };

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                try
                {
                    if (process.ExitCode != 0)
                    {
                        var output = String.Join(Environment.NewLine, outputLines);
                        var message = String.Format("{0} {1} failed.{2}{3}",
                            Path.GetFileName(fileName),
                            arguments,
                            Environment.NewLine,
                            output);
                        tcs.SetException(new Exception(message));
                    }
                    else
                    {
                        tcs.SetResult(process.ExitCode);
                    }
                }
                finally
                {
                    process.Dispose();
                }
            };

            if (process.Start())
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }

            return tcs.Task;
        }
    }
}
