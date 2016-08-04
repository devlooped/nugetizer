using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class MSBuildRunner
    {
        public static Task RebuildAsync(string solutionPath, bool buildNuGet)
        {
            return BuildAsync(solutionPath, buildNuGet, "Rebuild");
        }

        public static Task BuildAsync(string solutionPath, bool buildNuGet)
        {
            return BuildAsync(solutionPath, buildNuGet, "Build");
        }

        public static Task BuildAsync(string solutionPath, bool buildNuGet, string target)
        {
            var properties = new Dictionary<string, string>();
            if (buildNuGet)
            {
                properties.Add("BuildNuGet", "True");
            }

            return BuildAsync(solutionPath, target, properties);
        }

        public static Task BuildAsync(string solutionPath, string target, Dictionary<string, string> properties)
        {
            var arguments = new MSBuildArgumentBuilder(solutionPath);
            arguments.Append("/t:" + target);

            foreach (var property in properties) {
                arguments.AppendProperty(property.Key, property.Value);
            }

            arguments.Build();

            return ToolRunner.Run("msbuild", arguments.ToString(), Path.GetDirectoryName(solutionPath));
        }
    }
}

