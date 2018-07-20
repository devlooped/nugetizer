using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.Frameworks;

namespace NuGet.Build.Packaging.Tasks
{
	public class ApiIntersect : ToolTask
	{
		[Required]
		public ITaskItem Framework { get; set; }

		[Required]
		public ITaskItem[] IntersectionAssembly { get; set; }

		public ITaskItem[] ReferencePath { get; set; }

		public ITaskItem[] ExcludeType { get; set; }

		public ITaskItem[] RemoveAbstractTypeMembers { get; set; }

		public ITaskItem[] ExcludeAssembly { get; set; }

		public string KeepInternalConstructors { get; set; }

		public string KeepMarshalling { get; set; }

		public string IgnoreUnresolvedAssemblies { get; set; }

		[Required]
		public ITaskItem RootOutputDirectory { get; set; }

		[Output]
		public ITaskItem OutputDirectory { get; set; }

		protected override string ToolName
		{
			get { return "ApiIntersect"; }
		}

		protected override string GenerateFullPathToTool()
		{
			return Path.Combine(ToolPath, ToolExe);
		}

		protected override MessageImportance StandardErrorLoggingImportance
		{
			get { return MessageImportance.High; }
		}

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();

			var nugetFramework = NuGetFramework.Parse(Framework.ItemSpec);

			builder.AppendSwitch("-o");
			CreateOutputDirectoryItem(nugetFramework);
			builder.AppendFileNameIfNotNull(OutputDirectory.ItemSpec);

			string frameworkPath = GetFrameworkPath(nugetFramework);
			builder.AppendSwitch("-f");
			builder.AppendFileNameIfNotNull(frameworkPath);

			foreach (var assembly in IntersectionAssembly.NullAsEmpty())
			{
				builder.AppendSwitch("-i");
				builder.AppendFileNameIfNotNull(assembly.ItemSpec);
			}

			foreach (var referencePath in ReferencePath.NullAsEmpty())
			{
				builder.AppendSwitch("-r");
				builder.AppendFileNameIfNotNull(referencePath.ItemSpec);
			}

			foreach (var excludeType in ExcludeType.NullAsEmpty())
			{
				builder.AppendSwitch("-b");
				builder.AppendTextUnquoted(" \"");
				builder.AppendTextUnquoted(excludeType.ItemSpec);
				builder.AppendTextUnquoted("\"");
			}

			foreach (var removeAbstractType in RemoveAbstractTypeMembers.NullAsEmpty())
			{
				builder.AppendSwitch("-a");
				builder.AppendTextUnquoted(" \"");
				builder.AppendTextUnquoted(removeAbstractType.ItemSpec);
				builder.AppendTextUnquoted("\"");
			}

			foreach (var assembly in ExcludeAssembly.NullAsEmpty())
			{
				builder.AppendSwitch("-e");
				builder.AppendFileNameIfNotNull(assembly.ItemSpec);
			}

			AppendSwitchIfTrue(builder, "-k", KeepInternalConstructors);

			AppendSwitchIfTrue(builder, "-m", KeepMarshalling);

			AppendSwitchIfTrue(builder, "-iua", IgnoreUnresolvedAssemblies);

			return builder.ToString();
		}

		void CreateOutputDirectoryItem(NuGetFramework framework)
		{
			string fullOutputPath = GetApiIntersectTargetPaths.GetOutputDirectory(RootOutputDirectory.ItemSpec, framework);
			OutputDirectory = new TaskItem(fullOutputPath + Path.DirectorySeparatorChar);
			OutputDirectory.SetMetadata("TargetFrameworkVersion", "v" + framework.Version.ToString(2));
			OutputDirectory.SetMetadata("TargetFrameworkProfile", framework.Profile);
			OutputDirectory.SetMetadata(MetadataName.TargetFrameworkMoniker, Framework.ItemSpec);
		}

		string GetFrameworkPath(NuGetFramework framework)
		{
			if (framework.IsPCL)
			{
				return GetPortableLibraryPath(framework);
			}

			throw new NotImplementedException("Only PCL profiles supported.");
		}

		string GetPortableLibraryPath(NuGetFramework framework)
		{
			return Path.Combine(
				GetPortableRootDirectory(),
				"v" + framework.Version.ToString(2),
				"Profile",
				framework.Profile);
		}

		static string GetPortableRootDirectory()
		{
			if (RuntimeEnvironmentHelper.IsMono)
			{
				string macPath = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild-frameworks/.NETPortable/";
				if (Directory.Exists(macPath))
					return macPath;

				return Path.Combine(Path.GetDirectoryName(typeof(Object).Assembly.Location), "../xbuild-frameworks/.NETPortable/");
			}

			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify),
				@"Reference Assemblies\Microsoft\Framework\.NETPortable");
		}

		static void AppendSwitchIfTrue(CommandLineBuilder builder, string switchName, string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			bool result;
			if (bool.TryParse(value, out result))
				builder.AppendSwitch(switchName);
		}
	}
}