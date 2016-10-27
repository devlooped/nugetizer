using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Build.Packaging.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class GetApiIntersectTargetPathsTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;
		GetApiIntersectTargetPaths task;

		public GetApiIntersectTargetPathsTests(ITestOutputHelper output)
		{
			this.output = output;
			engine = new MockBuildEngine(output);
			task = new GetApiIntersectTargetPaths
			{
				BuildEngine = engine
			};
		}

		[Fact]
		public void when_framework_is_profile_78_then_target_path_returned()
		{
			task.RootOutputDirectory = new TaskItem(Path.Combine("project", "obj", "Debug"));
			task.Frameworks = new[]
			{
				new TaskItem(".NETPortable,Version=v4.5,Profile=Profile78")
			};
			task.Assemblies = new[]
			{
				new TaskItem(Path.Combine("Android", "test.dll")),
				new TaskItem(Path.Combine("iOS", "test.dll"))
			};

			bool result = task.Execute();

			string expectedTargetPath = Path.Combine("project", "obj", "Debug", "Profile78", "bin", "test.dll");
			Assert.True(result);
			Assert.Equal(1, task.TargetPaths.Length);
			Assert.Equal(expectedTargetPath, task.TargetPaths[0].ItemSpec);
		}

		[Fact]
		public void when_assembly_filenames_do_not_match_then_warning_logged()
		{
			task.RootOutputDirectory = new TaskItem(Path.Combine("project", "obj", "Debug"));
			task.Frameworks = new[]
			{
				new TaskItem(".NETPortable,Version=v4.5,Profile=Profile78")
			};
			task.Assemblies = new[]
			{
				new TaskItem(Path.Combine("Android", "Test.Android.dll")),
				new TaskItem(Path.Combine("iOS", "Test.iOS.dll"))
			};

			task.Execute();

			string expectedMessage = string.Format("Assembly names should be the same for a bait and switch NuGet package. Names: 'Test.Android.dll, Test.iOS.dll' Assemblies: {0}, {1}",
				task.Assemblies[0].ItemSpec,
				task.Assemblies[1].ItemSpec);
			var warningEvent = engine.LoggedWarningEvents[0];
			Assert.Equal(1, engine.LoggedWarningEvents.Count);
			Assert.Equal("NG1003", warningEvent.Code);
			Assert.Equal(expectedMessage, warningEvent.Message);
		}
	}
}
