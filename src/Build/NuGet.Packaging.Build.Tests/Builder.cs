using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Xunit.Abstractions;

/// <summary>
/// General-purpose MSBuild builder with support for 
/// automatic solution-configuration generation for 
/// P2P references.
/// </summary>
public static partial class Builder
{
	public static BuildResult Build(string projectOrSolution, string targets, Dictionary<string, string> properties = null, ILogger logger = null)
	{
		if (!Path.IsPathRooted(projectOrSolution))
			projectOrSolution = Path.Combine(ModuleInitializer.BaseDirectory, projectOrSolution);

		if (!File.Exists(projectOrSolution))
			throw new FileNotFoundException($"Project or solution to build {projectOrSolution} was not found.", projectOrSolution);

		try
		{
			properties = properties ?? new Dictionary<string, string>();

			// If file is not a solution, build up a fake solution configuration so P2P references just work
			if (Path.GetExtension(projectOrSolution) != ".sln")
				AddSolutionConfiguration(projectOrSolution, properties);

			var request = new BuildRequestData(projectOrSolution, properties, "14.0", targets.Split(','), null);
			var parameters = new BuildParameters
			{
				GlobalProperties = properties,
			};

			if (logger != null)
				parameters.Loggers = new[] { logger };

			return BuildManager.DefaultBuildManager.Build(parameters, request);
		}
		finally
		{
			BuildManager.DefaultBuildManager.Dispose();
		}
	}

	static void AddSolutionConfiguration(string projectFile, Dictionary<string, string> properties)
	{
		var collection = new ProjectCollection(properties);
		var toolsVersion = default(string);
		var project = new Project(projectFile, properties, toolsVersion, collection);
		var config = project.GetPropertyValue("Configuration");
		var platform = project.GetPropertyValue("Platform");

		var xml = new XElement("SolutionConfiguration");

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