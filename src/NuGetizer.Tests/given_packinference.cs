using System.Linq;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_packinference
    {
        ITestOutputHelper output;

        public given_packinference(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_none_has_PackagePath_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <None Include='Library.props' PackagePath='build\netstandard2.0\%(Filename)%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build\netstandard2.0\Library.props",
            }));
        }

        [Fact]
        public void when_none_has_Kind_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <None Include='Library.props' Kind='build' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "Library",
                Extension = ".props",
            }));
        }

        [Fact]
        public void when_none_has_Kind_FrameworkSpecific_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <None Include='Library.props' Kind='build' FrameworkSpecific='true' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build\netstandard2.0\Library.props",
            }));
        }

        [Fact]
        public void when_PackContent_false_but_content_has_PackagePath_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackContent>false</PackContent>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' PackagePath='build\netstandard2.0\%(Filename)%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build\netstandard2.0\Library.props",
            }));
        }

        [Fact]
        public void when_PackContent_false_but_content_has_Pack_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackContent>false</PackContent>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' Pack='true' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "Library",
                Extension = ".props",
            }));
        }

        [Fact]
        public void when_PackContent_false_but_content_has_Kind_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackContent>false</PackContent>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' Kind='build' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "Library",
                Extension = ".props",
            }));
        }

        [Fact]
        public void when_content_has_buildaction_then_spec_has_attribute()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <EmitNuSpec>true</EmitNuSpec>
    <EmitPackage>false</EmitPackage>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' BuildAction='None' />
  </ItemGroup>
</Project>",
                "Pack", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            Assert.NotEmpty(result.Items[0].GetMetadata("Nuspec"));

            var nuspec = XDocument.Load(result.Items[0].GetMetadata("Nuspec"));

            // Nuspec should contain:
            // <files include="any\netstandard2.0\Library.props" buildAction="None" />
            Assert.Equal("None", nuspec
                .Descendants("{http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd}files")
                .Select(x => x.Attribute("buildAction")?.Value)
                .FirstOrDefault());
        }

        [Fact]
        public void when_content_has_copytooutput_then_spec_has_attribute()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <EmitNuSpec>true</EmitNuSpec>
    <EmitPackage>false</EmitPackage>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' CopyToOutput='true' />
  </ItemGroup>
</Project>",
                "Pack", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            Assert.NotEmpty(result.Items[0].GetMetadata("Nuspec"));

            var nuspec = XDocument.Load(result.Items[0].GetMetadata("Nuspec"));

            // Nuspec should contain:
            // <files include="any\netstandard2.0\Library.props" copyToOutput="true" />
            Assert.Equal("true", nuspec
                .Descendants("{http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd}files")
                .Select(x => x.Attribute("copyToOutput")?.Value)
                .FirstOrDefault());
        }

        [Fact]
        public void when_content_has_copytooutput_flatten_then_spec_has_attributes()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <EmitNuSpec>true</EmitNuSpec>
    <EmitPackage>false</EmitPackage>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' CopyToOutput='true' Flatten='true' />
  </ItemGroup>
</Project>",
                "Pack", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            Assert.NotEmpty(result.Items[0].GetMetadata("Nuspec"));

            var nuspec = XDocument.Load(result.Items[0].GetMetadata("Nuspec"));

            // Nuspec should contain:
            // <files include="any\netstandard2.0\Library.props" copyToOutput="true" flatten="true" />
            Assert.Equal("true", nuspec
                .Descendants("{http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd}files")
                .Select(x => x.Attribute("copyToOutput")?.Value)
                .FirstOrDefault());

            Assert.Equal("true", nuspec
                .Descendants("{http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd}files")
                .Select(x => x.Attribute("flatten")?.Value)
                .FirstOrDefault());
        }

        [Fact]
        public void when_content_has_CodeLanguage_then_sets_subpath()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Foo.cs' CodeLanguage='%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\netstandard2.0\\Foo.cs"
            }));
        }

        [Fact]
        public void when_content_has_CodeLanguage_and_TargetFramework_then_sets_subpath()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Readme.txt' TargetFramework='any' CodeLanguage='any' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\any\\any\\Readme.txt"
            }));
        }

        [Fact]
        public void when_compile_has_Pack_and_CodeLanguage_then_sets_subpath()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include='Foo.cs' Pack='true' Kind='content' CodeLanguage='%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\netstandard2.0\\Foo.cs"
            }));
        }

        [Fact]
        public void when_PackCompile_true_then_includes_compile_with_CodeLanguage_matching_project_language()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackCompile>true</PackCompile>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include='Foo.cs;Bar.cs' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\netstandard2.0\\Foo.cs"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\netstandard2.0\\Bar.cs"
            }));
        }

        [Fact]
        public void when_PackCompile_true_and_TargetFramework_then_can_change_framework_folder()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackCompile>true</PackCompile>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include='Foo.cs;Bar.cs' TargetFramework='net472' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\net472\\Foo.cs"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "contentFiles\\cs\\net472\\Bar.cs"
            }));
        }
    }
}
