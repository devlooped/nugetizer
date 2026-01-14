using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuGetizer.Tests
{
    static class ModuleInitializer
    {
        static readonly string logFile = Path.Combine(Path.GetTempPath(), "NuGetizer.txt");

        [ModuleInitializer]
        internal static void Run()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            File.AppendAllText(logFile, $"Initializing MSBuild to {ThisAssembly.Project.MSBuildBinPath}{Environment.NewLine}");

            var binPath = ThisAssembly.Project.MSBuildBinPath;
            Microsoft.Build.Locator.MSBuildLocator.RegisterMSBuildPath(binPath);

            // Set environment variables so SDKs can be resolved. 
            var msbuildExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "MSBuild.exe" : "MSBuild.dll";
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(binPath, msbuildExeName), EnvironmentVariableTarget.Process);
        }

        static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            var file = Path.Combine(ThisAssembly.Project.MSBuildBinPath, name + ".dll");

            File.AppendAllText(logFile, $"Resolving {name}{Environment.NewLine}");

            if (name.StartsWith("Microsoft.Build") && File.Exists(file))
            {
                File.AppendAllText(logFile, $"Found {file}{Environment.NewLine}");
                return Assembly.LoadFrom(file);
            }
            else
            {
                file = Path.Combine(ThisAssembly.Project.MSBuildProjectDirectory, ThisAssembly.Project.OutputPath, name + ".dll");
                if (File.Exists(file))
                {
                    File.AppendAllText(logFile, $"Found {file}{Environment.NewLine}");
                    return Assembly.LoadFrom(file);
                }
            }

            return null;
        }
    }
}
