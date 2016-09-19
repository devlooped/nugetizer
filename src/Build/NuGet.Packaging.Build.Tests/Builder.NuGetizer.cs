using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

/// <summary>
/// NuGetizer-specific overloads
/// </summary>
static partial class Builder
{
	public static ITargetResult BuildScenario(string scenarioName, object properties = null, string target = "GetPackageContents", ILogger logger = null)
	{
		var projectName = scenarioName;
		if (scenarioName.StartsWith("given", StringComparison.OrdinalIgnoreCase))
			projectName = string.Join("_", scenarioName.Split('_').Skip(2));

		var scenarioDir = Path.Combine(ModuleInitializer.BaseDirectory, "Scenarios", scenarioName);
		string projectOrSolution;

		if (File.Exists(Path.Combine(scenarioDir, $"{projectName}.csproj")))
			projectOrSolution = Path.Combine(scenarioDir, $"{projectName}.csproj");
		else
			projectOrSolution = Directory.EnumerateFiles(scenarioDir, "*.csproj").First();

		if (properties != null)
			return Build(projectOrSolution, target,
				properties: properties.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(properties).ToString()),
				logger: logger)[target];
		else
			return Build(projectOrSolution, target, logger: logger)[target];
	}
}
