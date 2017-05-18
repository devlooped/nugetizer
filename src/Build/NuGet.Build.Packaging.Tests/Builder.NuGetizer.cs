using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// NuGetizer-specific overloads
/// </summary>
static partial class Builder
{
	public static TargetResult BuildScenario(string scenarioName, object properties = null, string projectName = null, string target = "GetPackageContents", ITestOutputHelper output = null, LoggerVerbosity? verbosity = null)
	{
		var scenarioDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenarios", scenarioName);

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
			projectOrSolution = Directory.EnumerateFiles(scenarioDir, "*.*proj").FirstOrDefault();

		var logger = default(TestOutputLogger);
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
		else
		{
			logger = new TestOutputLogger(null);
		}

		if (properties != null)
			return new TargetResult(Build(projectOrSolution, target,
				properties: properties.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(properties).ToString()),
				logger: logger), target, logger);
		else
			return new TargetResult(Build(projectOrSolution, target, logger: logger), target, logger);
	}

	public class TargetResult : ITargetResult
	{
		public TargetResult(BuildResult result, string target, TestOutputLogger logger)
		{
			BuildResult = result;
			Target = target;
			Logger = logger;
		}

		public BuildResult BuildResult { get; private set; }

		public TestOutputLogger Logger { get; private set; }

		public string Target { get; private set; }

		public Exception Exception => BuildResult[Target].Exception;

		public ITaskItem[] Items => BuildResult[Target].Items;

		public TargetResultCode ResultCode => BuildResult[Target].ResultCode;

		public void AssertSuccess(ITestOutputHelper output)
		{
			if (!BuildResult.ResultsByTarget.ContainsKey(Target))
			{
				output.WriteLine(this.ToString());
				Assert.False(true, "Build results do not contain output for target " + Target);
			}

			if (ResultCode != TargetResultCode.Success)
				output.WriteLine(this.ToString());

			Assert.Equal(TargetResultCode.Success, ResultCode);
		}

		public override string ToString()
        {
            return string.Join(Environment.NewLine, Logger.Warnings
                .Select(e => e.Message)
                .Concat(Logger.Errors.Select(e => e.Message)));
        }
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
		// Don't ever set the verbosity on release builds just in case 
		// we forget the attribute in a commit ;)
#if DEBUG
		var data = Thread.GetNamedDataSlot(nameof(LoggerVerbosity));
		if (data == null)
			data = Thread.AllocateNamedDataSlot(nameof(LoggerVerbosity));

		Thread.SetData(data, Verbosity);
#endif

		base.Before(methodUnderTest);
	}

	public override void After(MethodInfo methodUnderTest)
	{
#if DEBUG
		var data = Thread.GetNamedDataSlot(nameof(LoggerVerbosity));
		if (data != null)
			Thread.SetData(data, null);
#endif

		base.After(methodUnderTest);
	}
}
