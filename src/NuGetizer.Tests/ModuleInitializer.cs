using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace NuGetizer.Tests
{
    static class ModuleInitializer
    {
        static readonly string logFile = Path.Combine(Path.GetTempPath(), "NuGetizer.txt");
        static string sdkPath;

        [ModuleInitializer]
        internal static void Run()
        {
            // Get the .NET SDK path by running 'dotnet --info'
            sdkPath = GetDotNetSdkPath();
            
            File.AppendAllText(logFile, $"Initializing MSBuild from SDK at {sdkPath}\r\n");

            // Set up assembly resolution to prefer SDK assemblies
            AssemblyLoadContext.Default.Resolving += OnAssemblyResolving;

            // Set environment variables so SDKs can be resolved
            var msbuildExe = Path.Combine(sdkPath, "MSBuild.dll");
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuildExe, EnvironmentVariableTarget.Process);
        }

        static Assembly OnAssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            // Resolve MSBuild, NuGet and related assemblies from the SDK directory
            if (assemblyName.Name != null && 
                (assemblyName.Name.StartsWith("Microsoft.Build") || 
                 assemblyName.Name.StartsWith("Microsoft.CodeAnalysis") ||
                 assemblyName.Name.StartsWith("NuGet.") ||
                 assemblyName.Name.StartsWith("System.Collections.Immutable") ||
                 assemblyName.Name.StartsWith("System.Reflection.Metadata")))
            {
                var assemblyPath = Path.Combine(sdkPath, assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    File.AppendAllText(logFile, $"Loading {assemblyName.Name} from SDK at {assemblyPath}\r\n");
                    return context.LoadFromAssemblyPath(assemblyPath);
                }
            }

            return null;
        }

        static string GetDotNetSdkPath()
        {
            try
            {
                // Use 'dotnet --list-sdks' to find the SDK path
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--list-sdks",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Parse the output to get SDK paths, use the latest SDK
                // Output format: "8.0.100 [/usr/share/dotnet/sdk]"
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Use the latest SDK (last line)
                if (lines.Length > 0)
                {
                    var lastLine = lines.Last();
                    var startIndex = lastLine.IndexOf('[');
                    var endIndex = lastLine.IndexOf(']');
                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        var sdkBase = lastLine.Substring(startIndex + 1, endIndex - startIndex - 1);
                        var version = lastLine.Substring(0, startIndex).Trim();
                        var sdkPath = Path.Combine(sdkBase, version);
                        File.AppendAllText(logFile, $"Using latest SDK at {sdkPath}\r\n");
                        return sdkPath;
                    }
                }

                // Fallback: try to use DOTNET_ROOT or common paths
                var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                if (!string.IsNullOrEmpty(dotnetRoot))
                {
                    var sdkDir = Path.Combine(dotnetRoot, "sdk");
                    if (Directory.Exists(sdkDir))
                    {
                        var versions = Directory.GetDirectories(sdkDir)
                            .OrderByDescending(d => d)
                            .FirstOrDefault();
                        if (versions != null)
                            return versions;
                    }
                }

                throw new InvalidOperationException("Could not locate .NET SDK path");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"Error finding SDK path: {ex}\r\n");
                throw;
            }
        }
    }
}
