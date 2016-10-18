using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;
using NuGet.Build.Packaging.Tasks;
using NuGet.Frameworks;
using NuGet.ProjectManagement;
using Xunit;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace NuGet.Build.Packaging
{
	public class UtilitiesTests
	{
		[Fact]
		public void when_getting_target_framework_then_fallback_to_tfm()
		{
			var item = new TaskItem("Foo", new Metadata
			{
				{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" }
			});

			var framework = item.GetTargetFramework();

			Assert.Equal(new FrameworkName(".NETFramework,Version=v4.5"), framework);
		}

		[Fact]
		public void when_getting_target_framework_then_does_not_use_tfm()
		{
			var item = new TaskItem("Foo", new Metadata
			{
				{ "TargetFramework", "net46" },
				{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" }
			});

			var framework = item.GetTargetFramework();

			Assert.Equal(new FrameworkName(".NETFramework,Version=v4.6"), framework);
		}

		[Fact]
		public void when_getting_any_target_framework_then_suceeds()
		{
			var item = new TaskItem("Foo", new Metadata
			{
				{ "TargetFramework", "any" },
			});

			var framework = item.GetTargetFramework();

			Assert.Equal(new FrameworkName("Any,Version=v0.0"), framework);
			Assert.Equal("any", framework.GetShortFrameworkName());
		}
	}
}
