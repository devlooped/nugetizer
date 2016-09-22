using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using NuGet.Packaging;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// NuGetizer-specific overloads
/// </summary>
static partial class Builder
{
	public static ITargetResult BuildScenario(string scenarioName, object properties = null, string projectName = null, string target = "GetPackageContents", ITestOutputHelper output = null, LoggerVerbosity? verbosity = null)
	{
		var scenarioDir = Path.Combine(ModuleInitializer.BaseDirectory, "Scenarios", scenarioName);

		if (projectName == null)
		{
			projectName = scenarioName;
			if (scenarioName.StartsWith("given", StringComparison.OrdinalIgnoreCase))
				projectName = string.Join("_", scenarioName.Split('_').Skip(2));
		}
		else if (!File.Exists(Path.Combine(scenarioDir, $"{projectName}.csproj")))
		{
			throw new FileNotFoundException($"Project '{projectName}' was not found under scenario path '{scenarioDir}'.", projectName);
		}

		string projectOrSolution;

		if (File.Exists(Path.Combine(scenarioDir, $"{projectName}.csproj")))
			projectOrSolution = Path.Combine(scenarioDir, $"{projectName}.csproj");
		else
			projectOrSolution = Directory.EnumerateFiles(scenarioDir, "*.csproj").First();

		var logger = default(ILogger);
		if (output != null)
		{
			if (verbosity == null)
			{
				var data = Thread.GetNamedDataSlot(nameof(LoggerVerbosity));
				if (data != null)
					verbosity = (LoggerVerbosity?)Thread.GetData(data);
			}

			logger = new TestOutputLogger(output, verbosity);
		}

		if (properties != null)
			return Build(projectOrSolution, target,
				properties: properties.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(properties).ToString()),
				logger: logger)[target];
		else
			return Build(projectOrSolution, target, logger: logger)[target];
	}
}

/// <summary>
/// Allows declaratively increasing the MSBuild logger verbosity. In 
/// order to output anything at all, the BuildScenario must be called 
/// passing an <see cref="ITestOutputHelper"/> to write to.
/// </summary>
public class VerbosityAttribute : BeforeAfterTestAttribute
{
	public VerbosityAttribute(LoggerVerbosity verbosity)
	{
		Verbosity = verbosity;
	}

	public LoggerVerbosity? Verbosity { get; }

	public override void Before(MethodInfo methodUnderTest)
	{
		var data = Thread.GetNamedDataSlot(nameof(LoggerVerbosity));
		if (data == null)
			data = Thread.AllocateNamedDataSlot(nameof(LoggerVerbosity));

		Thread.SetData(data, Verbosity);

		base.Before(methodUnderTest);
	}

	public override void After(MethodInfo methodUnderTest)
	{
		var data = Thread.GetNamedDataSlot(nameof(LoggerVerbosity));
		if (data != null)
			Thread.SetData(data, null);

		base.After(methodUnderTest);
	}
}
