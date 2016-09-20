using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging.Build.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using static NuGet.Packaging.Build.Tasks.Properties.Strings;

namespace NuGet.Packaging
{
	public class CreatePackageTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;

		public CreatePackageTests(ITestOutputHelper output)
		{
			this.output = output;
			engine = new MockBuildEngine(output);
		}
	}
}
