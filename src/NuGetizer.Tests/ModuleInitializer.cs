using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Build.Locator;

namespace NuGetizer.Tests
{
    static class ModuleInitializer
    {
        static readonly string logFile = Path.Combine(Path.GetTempPath(), "NuGetizer.txt");

        [ModuleInitializer]
        internal static void Run()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // Use MSBuildLocator.RegisterDefaults() to find the appropriate MSBuild instance
            // This works better across different environments (VS MSBuild, .NET SDK, etc.)
            var instance = MSBuildLocator.RegisterDefaults();
            File.AppendAllText(logFile, $"Registered MSBuild from {instance.MSBuildPath}{Environment.NewLine}");

            // Set environment variables so SDKs can be resolved. 
            var msbuildExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "MSBuild.exe" : "MSBuild.dll";
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(instance.MSBuildPath, msbuildExeName), EnvironmentVariableTarget.Process);
        }

        static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;

            File.AppendAllText(logFile, $"Resolving {name}{Environment.NewLine}");

            // Try to resolve from the registered MSBuild instance
            var instances = MSBuildLocator.QueryVisualStudioInstances();
            foreach (var instance in instances)
            {
                var file = Path.Combine(instance.MSBuildPath, name + ".dll");
                if (name.StartsWith("Microsoft.Build") && File.Exists(file))
                {
                    File.AppendAllText(logFile, $"Found {file}{Environment.NewLine}");
                    return Assembly.LoadFrom(file);
                }
            }

            // Fallback to output directory
            var outputFile = Path.Combine(ThisAssembly.Project.MSBuildProjectDirectory, ThisAssembly.Project.OutputPath, name + ".dll");
            if (File.Exists(outputFile))
            {
                File.AppendAllText(logFile, $"Found {outputFile}{Environment.NewLine}");
                return Assembly.LoadFrom(outputFile);
            }

            return null;
        }
    }
}
