using System;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class MSBuildRunner
    {
        public static Task RebuildAsync (string solutionPath, bool buildNuGet)
        {
            bool isMono = Type.GetType("Mono.Runtime") != null;
            string command = isMono ? "xbuild" : "msbuild";

            var arguments = new MSBuildArgumentBuilder(solutionPath);
            arguments.Append("/t:Rebuild");

            if (buildNuGet)
            {
                arguments.AppendProperty("BuildNuGet", "True");
            }

            arguments.Build();

            return ToolRunner.Run(command, arguments.ToString(), Path.GetDirectoryName(solutionPath));
        }
    }
}

