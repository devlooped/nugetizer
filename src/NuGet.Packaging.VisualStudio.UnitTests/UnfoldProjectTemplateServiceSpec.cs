using Clide;
using EnvDTE;
using EnvDTE80;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Packaging.VisualStudio.UnitTests.Wizards
{
	public class UnfoldProjectTemplateServiceSpec
	{
		Mock<Solution2> solution = new Mock<Solution2>();
		Mock<ISolutionExplorer> solutionExplorer = new Mock<ISolutionExplorer>();

		[Fact]
		public void when_checking_installed_templates_then_returns_true_if_template_is_installed()
		{
			var service = new UnfoldProjectTemplateService(solutionExplorer.Object, solution.Object);

			solution
				.Setup(x => x.GetProjectTemplate("Foo", "CSharp"))
				.Returns(@"c:\foo.vstemplate");

			Assert.True(service.IsTemplateInstalled("Foo"));
			Assert.False(service.IsTemplateInstalled("Bar"));
		}

		[Fact]
		public void when_unfolding_template_then_template_is_unfolded()
		{
			var service = new UnfoldProjectTemplateService(solutionExplorer.Object, solution.Object);

			solution
				.Setup(x => x.GetProjectTemplate("Foo", "CSharp"))
				.Returns(@"c:\foo.vstemplate");

			solutionExplorer.
				Setup(x => x.Solution)
				.Returns(Mock.Of<ISolutionNode>(x =>
					x.Nodes == new[]
					{
						Mock.Of<IProjectNode> (project =>
							project.Name == "MyLibrary" &&
							project.PhysicalPath == @"c:\src\MySolution\MyLibrary\MyLibrary.csproj")
					}));

			var unfoldedProject = service.UnfoldTemplate("Foo", @"c:\src\MySolution\MyLibrary");

			Assert.NotNull(unfoldedProject);
			solution.Verify(x =>
			   x.AddFromTemplate(
				   @"c:\foo.vstemplate",
				   @"c:\src\MySolution\MyLibrary",
				   "MyLibrary",
				   false));
		}
	}
}
