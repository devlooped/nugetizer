using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// NuGetizer-specific overloads
/// </summary>
static partial class Builder
{
    public static TargetResult BuildProject(
        string projectContent = null,
        string target = "GetPackageContents",
        ITestOutputHelper output = null,
        LoggerVerbosity? verbosity = null)
    {
        using var sha = new SHA1Managed();
        var hash = Base62.Encode(BitConverter.ToInt64(
            sha.ComputeHash(Encoding.UTF8.GetBytes(projectContent)), 0));

        var scenarioName = hash;
        var scenarioDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenarios", scenarioName);
        Directory.CreateDirectory(scenarioDir);
        var doc = XDocument.Parse(projectContent);
        doc.Root.AddFirst(XElement
            .Parse("<Import Project='$([MSBuild]::GetPathOfFileAbove(Scenario.props, $(MSBuildThisFileDirectory)))' />"));
        
        doc.Save(Path.Combine(scenarioDir, "library.csproj"));

        var openLog = OpenBuildLogAttribute.IsActive;
        try
        {
            OpenBuildLogAttribute.IsActive = false;
            BuildScenario(scenarioName, target: "Restore", output: output, verbosity: verbosity)
                .AssertSuccess(output);
        }
        finally
        {
            OpenBuildLogAttribute.IsActive = openLog;
        }

        return BuildScenario(scenarioName, target: target, output: output, verbosity: verbosity);
    }

    public static TargetResult BuildScenario(
    string scenarioName,
    object properties = null,
    string projectName = null,
    string target = "GetPackageContents",
    ITestOutputHelper output = null,
    LoggerVerbosity? verbosity = null)
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

        var logFile = scenarioName + ".binlog";

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY")))
        {
            var logDir = Path.Combine(Environment.GetEnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY"), "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            logFile = Path.Combine(logDir, logFile);
        }

        var loggers = OpenBuildLogAttribute.IsActive || Environment.GetEnvironmentVariable("SYSTEM_DEBUG") == "true" ?
            new ILogger[] { logger, new StructuredLogger
            {
                Verbosity = LoggerVerbosity.Diagnostic,
                Parameters = logFile
            } } :
            new ILogger[] { logger };

        var buildProps = properties?.GetType().GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(properties).ToString())
            ?? new Dictionary<string, string>();

        buildProps[nameof(ThisAssembly.Project.NuGetRestoreTargets)] = ThisAssembly.Project.NuGetRestoreTargets;
        buildProps[nameof(ThisAssembly.Project.NuGetTargets)] = ThisAssembly.Project.NuGetTargets;

        var result = Build(projectOrSolution, target, buildProps, loggers);
        if (OpenBuildLogAttribute.IsActive)
            Process.Start(scenarioName + ".binlog");

        return new TargetResult(projectOrSolution, result, target, logger);
    }

    public class TargetResult : ITargetResult
    {
        public TargetResult(string projectOrSolutionFile, BuildResult result, string target, TestOutputLogger logger)
        {
            ProjectOrSolutionFile = projectOrSolutionFile;
            BuildResult = result;
            Target = target;
            Logger = logger;
        }

        public string ProjectOrSolutionFile { get; private set; }

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
/// Declaratively specifies that the .binlog for the build 
/// should be opened automatically after building a project.
/// </summary>
internal class OpenBuildLogAttribute : BeforeAfterTestAttribute
{
    /// <summary>
    /// Whether the attribute is active for the current test.
    /// </summary>
    public static bool IsActive
    {
        get
        {
#if DEBUG
            var data = Thread.GetNamedDataSlot(nameof(OpenBuildLogAttribute));
            if (data == null)
                return false;

            return Thread.GetData(data) != null;
#else
			return false;
#endif
        }
        set
        {
            // Don't ever set this flag on release builds just in case 
            // we forget the attribute in a commit ;)
#if DEBUG
            if (value)
            {
                var data = Thread.GetNamedDataSlot(nameof(OpenBuildLogAttribute));
                if (data == null)
                    data = Thread.AllocateNamedDataSlot(nameof(OpenBuildLogAttribute));

                Thread.SetData(data, new object());
            }
            else
            {
                var data = Thread.GetNamedDataSlot(nameof(OpenBuildLogAttribute));
                if (data != null)
                    Thread.SetData(data, null);
#endif
            }
        }
    }

    public override void Before(MethodInfo methodUnderTest)
    {
        IsActive = true;
        base.Before(methodUnderTest);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        IsActive = false;
        base.After(methodUnderTest);
    }
}

/// <summary>
/// Allows declaratively increasing the MSBuild logger verbosity. In 
/// order to output anything at all, the BuildScenario must be called 
/// passing an <see cref="ITestOutputHelper"/> to write to.
/// </summary>
internal class VerbosityAttribute : BeforeAfterTestAttribute
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
