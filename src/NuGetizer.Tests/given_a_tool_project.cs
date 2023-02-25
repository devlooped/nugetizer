using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_tool_project
    {
        ITestOutputHelper output;

        public given_a_tool_project(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_pack_as_tool_then_packs_no_dependencies()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <PackageId>MyTool</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <PackFolder>tools</PackFolder>
    <PackAsTool>true</PackAsTool>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Extensions.DependencyModel' Version='6.0.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Extensions.DependencyModel"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackageFile = "Microsoft.Extensions.DependencyModel.dll"
            }));
        }

        [Fact]
        public void when_pack_as_tool_then_packs_dotnet_tool_runtime_assets()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <AssemblyName>MyTool</AssemblyName>
    <PackageId>MyTool</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <PackFolder>tools</PackFolder>
    <PackAsTool>true</PackAsTool>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackageFile = "DotnetToolSettings.xml"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackageFile = "MyTool.deps.json"
            }));
        }

        [Fact]
        public void when_pack_folder_tool_but_no_pack_as_tool_then_packs_dependencies_normally()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <PackageId>MyTool</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <PackFolder>tools</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Extensions.DependencyModel' Version='6.0.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Extensions.DependencyModel"
            }));
        }

        [Fact]
        public void when_pack_folder_tool_no_pack_as_tool_and_executable_then_packs_as_publish_with_no_dependencies()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net6.0</TargetFramework>
                    <PackageId>MyTool</PackageId>
                    <PackFolder>tools</PackFolder>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageReference Include='Microsoft.Extensions.DependencyModel' Version='6.0.0' />
                  </ItemGroup>
                </Project>
                """,
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Extensions.DependencyModel"
            }));

            Assert.All(result.Items, item => item.Matches(new
            {
                OutputGroup = "PublishItemsOutputGroup",
            }));
        }

        [Fact]
        public void when_both_PackAsTool_and_PackAsPublish_true_then_fails()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net6.0</TargetFramework>
                    <PackageId>MyTool</PackageId>
                    <PackFolder>tools</PackFolder>
                    <PackAsTool>true</PackAsTool>
                    <PackAsPublish>true</PackAsPublish>
                  </PropertyGroup>
                </Project>
                """,
                "GetPackageContents", output);

            Assert.Equal(Microsoft.Build.Execution.BuildResultCode.Failure, result.BuildResult.OverallResult);
        }

    }
}
