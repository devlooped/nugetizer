using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NuGetizer.Tests
{
    static class ModuleInitializer
    {
        static readonly string logFile = Environment.ExpandEnvironmentVariables(@"%TEMP%\NuGetizer.txt");

        [ModuleInitializer]
        internal static void Run()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            File.AppendAllText(logFile, $"Initializing MSBuild to {ThisAssembly.Project.MSBuildBinPath}\r\n");

            var binPath = ThisAssembly.Project.MSBuildBinPath;
            Microsoft.Build.Locator.MSBuildLocator.RegisterMSBuildPath(binPath);
            // Set environment variables so SDKs can be resolved. 
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(binPath, "MSBuild.exe"), EnvironmentVariableTarget.Process);
        }

        static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            var file = Path.Combine(ThisAssembly.Project.MSBuildBinPath, name + ".dll");

            File.AppendAllText(logFile, $"Resolving {name}\r\n");

            if (name.StartsWith("Microsoft.Build") && File.Exists(file))
            {
                File.AppendAllText(logFile, $"Found {file}\r\n");
                return Assembly.LoadFrom(file);
            }

            return null;
        }
    }
}
