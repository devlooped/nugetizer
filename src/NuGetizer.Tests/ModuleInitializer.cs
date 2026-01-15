using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
#if !NET472
using System.Runtime.InteropServices;
#endif

namespace NuGetizer.Tests
{
    static class ModuleInitializer
    {
#if NET472
        static readonly string logFile = Environment.ExpandEnvironmentVariables(@"%TEMP%\NuGetizer.txt");
#else
        static readonly string logFile = Path.Combine(Path.GetTempPath(), "NuGetizer.txt");
#endif

        [ModuleInitializer]
        internal static void Run()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            File.AppendAllText(logFile, $"Initializing MSBuild to {ThisAssembly.Project.MSBuildBinPath}{Environment.NewLine}");

            var binPath = ThisAssembly.Project.MSBuildBinPath;
            Microsoft.Build.Locator.MSBuildLocator.RegisterMSBuildPath(binPath);

            // Set environment variables so SDKs can be resolved. 
#if NET472
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(binPath, "MSBuild.exe"), EnvironmentVariableTarget.Process);
#else
            var msbuildExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "MSBuild.exe" : "MSBuild.dll";
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(binPath, msbuildExeName), EnvironmentVariableTarget.Process);
#endif
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
