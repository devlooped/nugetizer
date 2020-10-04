using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;

// Disable test parallelization for VS test explorer as it doesn't work well together with msbuild.
// Note that the command line switch for the xunit console runner in build.proj is also needed.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

/// <summary>
/// General-purpose MSBuild builder with support for 
/// automatic solution-configuration generation for 
/// P2P references.
/// </summary>
static partial class Builder
{
    const string ToolsVersion = "Current";

    public static BuildResult Build(ProjectInstance project, string targets, Dictionary<string, string> properties = null, ILogger[] loggers = null)
    {
        properties = properties ?? new Dictionary<string, string>();

        var manager = BuildManager.DefaultBuildManager;
        var request = new BuildRequestData(project, targets.Split(','));
        var parameters = new BuildParameters
        {
            GlobalProperties = properties,
            LogInitialPropertiesAndItems = true,
            LogTaskInputs = true,
        };

        if (loggers != null)
            parameters.Loggers = loggers;

        return manager.Build(parameters, request);
    }

    public static BuildResult Build(string projectOrSolution, string targets, Dictionary<string, string> properties = null, ILogger[] loggers = null)
    {
        if (!Path.IsPathRooted(projectOrSolution))
            projectOrSolution = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, projectOrSolution);

        if (!File.Exists(projectOrSolution))
            throw new FileNotFoundException($"Project or solution to build {projectOrSolution} was not found.", projectOrSolution);

        properties = properties ?? new Dictionary<string, string>();

        // If file is not a solution, build up a fake solution configuration so P2P references just work
        if (Path.GetExtension(projectOrSolution) != ".sln")
            AddSolutionConfiguration(projectOrSolution, properties);

        var projectInstance = new ProjectInstance(projectOrSolution, properties, ToolsVersion);

        return Build(projectInstance, targets, properties, loggers);
    }

    public static string TestOutputPath([CallerMemberName] string testName = null) => "bin\\" + testName + "\\";

    static void AddSolutionConfiguration(string projectFile, Dictionary<string, string> properties)
    {
        var collection = new ProjectCollection(properties);
        var toolsVersion = default(string);
        var project = new Project(projectFile, properties, toolsVersion, collection);
        var config = project.GetPropertyValue("Configuration");
        var platform = project.GetPropertyValue("Platform");

        var xml = new XElement("SolutionConfiguration");
        xml.Add(new XElement("ProjectConfiguration",
            new XAttribute("Project", project.GetPropertyValue("ProjectGuid")),
            new XAttribute("AbsolutePath", project.FullPath),
            new XAttribute("BuildProjectInSolution", "true"))
        {
            Value = $"{config}|{platform}"
        });

        foreach (var reference in GetProjectReferences(project))
        {
            xml.Add(new XElement("ProjectConfiguration",
                new XAttribute("Project", reference.GetPropertyValue("ProjectGuid")),
                new XAttribute("AbsolutePath", reference.FullPath),
                new XAttribute("BuildProjectInSolution", "true"))
            {
                Value = $"{config}|{platform}"
            });
        }

        properties["CurrentSolutionConfigurationContents"] = xml.ToString(SaveOptions.None);
    }

    static IEnumerable<Project> GetProjectReferences(Project project)
    {
        foreach (var reference in project.GetItems("ProjectReference"))
        {
            var referenced = new Project(
                    Path.Combine(Path.GetDirectoryName(project.FullPath), reference.EvaluatedInclude),
                    project.GlobalProperties,
                    project.ToolsVersion,
                    new ProjectCollection(project.GlobalProperties));

            yield return referenced;
            foreach (var transitive in GetProjectReferences(referenced))
            {
                yield return transitive;
            }
        }
    }
}