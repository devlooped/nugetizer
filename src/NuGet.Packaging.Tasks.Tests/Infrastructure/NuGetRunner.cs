using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class NuGetRunner
    {
        public static Task RestorePackagesAsync(string path)
        {
            bool isMono = Type.GetType("Mono.Runtime") != null;
            string command = isMono ? "mono" : Assets.NuGetExePath;
            string arguments = isMono ? Assets.NuGetExePath + " restore" : "restore";

            return ToolRunner.Run(command, arguments, path);
        }
    }
}
