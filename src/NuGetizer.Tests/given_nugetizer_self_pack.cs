using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using NuGetizer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    /// <summary>
    /// Regression tests for NuGetizer.Tasks self-packaging (dogfooding).
    /// </summary>
    public class given_nugetizer_self_pack
    {
        readonly ITestOutputHelper output;

        public given_nugetizer_self_pack(ITestOutputHelper output) => this.output = output;

        [RuntimeFact("Windows")]
        public void when_packing_without_nugetize_property_then_build_folder_contains_msbuild_assets()
        {
            var project = Path.GetFullPath(Path.Combine(
                ThisAssembly.Project.MSBuildProjectDirectory,
                "..",
                "NuGetizer.Tasks",
                "NuGetizer.Tasks.csproj"));

            var properties = new Dictionary<string, string>
            {
                ["Configuration"] = "Release",
            };

            var logger = new TestOutputLogger(output);
            ILogger[] loggers = { logger };

            AssertTargetSuccess(Builder.Build(project, "Restore", properties, loggers), "Restore");
            AssertTargetSuccess(Builder.Build(project, "Build", properties, loggers), "Build");

            var packResult = Builder.Build(project, "Pack,GetPackageTargetPath", properties, loggers);

            AssertTargetSuccess(packResult, "GetPackageTargetPath");

            var packagePath = packResult["GetPackageTargetPath"].Items
                .Select(i => i.GetMetadata("FullPath"))
                .FirstOrDefault(path => path.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase));

            Assert.False(string.IsNullOrEmpty(packagePath));
            Assert.True(File.Exists(packagePath), $"Package not found at {packagePath}");

            using var package = ZipFile.OpenRead(packagePath);
            var entries = package.Entries.Select(e => e.FullName.Replace('\\', '/')).ToArray();

            Assert.Contains("build/NuGetizer.props", entries);
            Assert.Contains("build/NuGetizer.Shared.targets", entries);
            Assert.Contains("buildMultiTargeting/NuGetizer.props", entries);
            Assert.DoesNotContain("content/NuGetizer.props", entries);
        }

        void AssertTargetSuccess(BuildResult result, string target)
        {
            if (!result.ResultsByTarget.TryGetValue(target, out var targetResult))
            {
                output.WriteLine(string.Join("\n", result.ResultsByTarget.Keys));
                Assert.Fail($"Build results do not contain output for target {target}.");
            }

            if (targetResult.ResultCode != TargetResultCode.Success)
                Assert.Fail($"Target {target} failed: {targetResult.Exception?.Message}");

            Assert.Equal(TargetResultCode.Success, targetResult.ResultCode);
        }
    }
}
