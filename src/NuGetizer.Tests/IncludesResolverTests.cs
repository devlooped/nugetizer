using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer;

/// <summary>
/// Asserts NuGetizer + Readme package combination: includes, tokens, and GitHub relative URL
/// expansion come from the Readme dependency; NuGetizer packs the processed result.
/// </summary>
public class IncludesResolverTests(ITestOutputHelper output)
{
    const string ReadmeVersion = "1.1.3";

    [Fact]
    public void when_packing_with_readme_package_then_resolves_includes()
    {
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>ReadmeIncludesSample</PackageId>
                <EmitPackage>true</EmitPackage>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
              </ItemGroup>
            </Project>
            """,
            "Pack",
            output,
            files:
            [
                ("readme.md", """
                    <!-- include Common/header.md -->
                    body
                    <!-- include sections.md#first -->
                    <!-- include sections.md#third -->
                    <!-- include Common/footer.md -->
                    """),
                ("Common/header.md", "the-header"),
                ("Common/footer.md", """
                    the-footer
                    <!-- include ../sections.md#copyright -->
                    """),
                ("sections.md", """
                    <!-- #first -->
                    section#1
                    <!-- #first -->

                    <!-- #second -->
                    section#2
                    <!-- #second -->

                    <!-- #third -->
                    section#3
                    <!-- #third -->

                    <!-- #copyright -->
                    @kzu
                    <!-- #copyright -->
                    """)
            ]);

        result.AssertSuccess(output);

        var nupkg = result.Items.Select(i => i.GetMetadata("FullPath"))
            .FirstOrDefault(p => p != null && p.EndsWith(".nupkg"));
        Assert.True(nupkg != null && File.Exists(nupkg), "Expected packed nupkg output");

        var content = ReadPackageEntry(nupkg!, "readme.md");

        Assert.Contains("the-header", content);
        Assert.Contains("the-footer", content);
        Assert.Contains("section#1", content);
        Assert.DoesNotContain("section#2", content);
        Assert.Contains("section#3", content);
        Assert.Contains("@kzu", content);
    }

    [Fact]
    public void when_packing_with_readme_package_then_resolves_url_include()
    {
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>ReadmeUrlIncludeSample</PackageId>
                <EmitPackage>true</EmitPackage>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
              </ItemGroup>
            </Project>
            """,
            "Pack",
            output,
            files:
            [
                ("readme.md", """
                    <!-- include https://github.com/devlooped/nugetizer/raw/main/license.txt -->
                    <!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
                    """)
            ]);

        result.AssertSuccess(output);

        var nupkg = result.Items.Select(i => i.GetMetadata("FullPath"))
            .FirstOrDefault(p => p != null && p.EndsWith(".nupkg"));
        Assert.True(nupkg != null && File.Exists(nupkg), "Expected packed nupkg output");

        var content = ReadPackageEntry(nupkg!, "readme.md");

        Assert.Contains("Daniel Cazzulino", content);
        Assert.Contains("Sponsors", content);
    }

    [Fact]
    public void when_packing_with_readme_package_then_applies_tokens()
    {
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>ReadmeTokensSample</PackageId>
                <Version>1.2.3</Version>
                <Product>TokenProduct</Product>
                <EmitPackage>true</EmitPackage>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
              </ItemGroup>
            </Project>
            """,
            "Pack",
            output,
            files: [("readme.md", "Package $product$ ($id$) v$version$.")]);

        result.AssertSuccess(output);

        var nupkg = result.Items.Select(i => i.GetMetadata("FullPath"))
            .FirstOrDefault(p => p != null && p.EndsWith(".nupkg"));
        Assert.True(nupkg != null && File.Exists(nupkg), "Expected packed nupkg output");

        var content = ReadPackageEntry(nupkg!, "readme.md");

        Assert.Contains("TokenProduct", content);
        Assert.Contains("ReadmeTokensSample", content);
        Assert.Contains("1.2.3", content);
        Assert.DoesNotContain("$product$", content);
        Assert.DoesNotContain("$id$", content);
        Assert.DoesNotContain("$version$", content);
    }

    [Fact]
    public void when_packing_with_readme_package_then_expands_github_relative_urls()
    {
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>ReadmeGitHubUrlSample</PackageId>
                <RepositoryType>git</RepositoryType>
                <RepositoryUrl>https://github.com/devlooped/nugetizer</RepositoryUrl>
                <RepositoryCommit>9dc2cb5deabcdef0123456789abcdef012345678</RepositoryCommit>
                <EmitPackage>true</EmitPackage>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
              </ItemGroup>
            </Project>
            """,
            "Pack",
            output,
            files: [("readme.md", "See [license](license.txt) and ![logo](img/logo.png).")]);

        result.AssertSuccess(output);

        var nupkg = result.Items.Select(i => i.GetMetadata("FullPath"))
            .FirstOrDefault(p => p != null && p.EndsWith(".nupkg"));
        Assert.True(nupkg != null && File.Exists(nupkg), "Expected packed nupkg output");

        var content = ReadPackageEntry(nupkg!, "readme.md");
        var commit = "9dc2cb5deabcdef0123456789abcdef012345678";

        Assert.Contains($"https://raw.githubusercontent.com/devlooped/nugetizer/{commit}/license.txt", content);
        Assert.Contains($"https://raw.githubusercontent.com/devlooped/nugetizer/{commit}/img/logo.png", content);
        Assert.DoesNotContain("(license.txt)", content);
    }

    [Fact]
    public void when_packing_with_readme_package_then_missing_include_does_not_fail_pack()
    {
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>ReadmeMissingIncludeSample</PackageId>
                <EmitPackage>true</EmitPackage>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
              </ItemGroup>
            </Project>
            """,
            "Pack",
            output,
            files: [("readme.md", "<!-- include foo.md#bar -->")]);

        result.AssertSuccess(output);

        var nupkg = result.Items.Select(i => i.GetMetadata("FullPath"))
            .FirstOrDefault(p => p != null && p.EndsWith(".nupkg"));
        Assert.True(nupkg != null && File.Exists(nupkg), "Expected packed nupkg output");

        // Missing includes stay as markers; pack still succeeds with a warning from Readme.
        var content = ReadPackageEntry(nupkg!, "readme.md");
        Assert.Contains("foo.md#bar", content);
    }

    [Fact]
    public void when_packagefile_forces_readme_dependency_then_packed_contents_include_it()
    {
        // Mirrors NuGetizer.Tasks: Readme self-sets PrivateAssets=all Pack=false, so consumers
        // must re-add it via PackageFile PackFolder=Dependency (see readme commit 7d092d51).
        var result = Builder.BuildProject($$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <IsPackable>true</IsPackable>
                <TargetFramework>netstandard2.0</TargetFramework>
                <PackageId>NuGetizerReadmeDependencySample</PackageId>
                <DevelopmentDependency>true</DevelopmentDependency>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Readme" Version="{{ReadmeVersion}}" />
                <PackageFile Include="Readme" Version="{{ReadmeVersion}}" PackFolder="Dependency" />
              </ItemGroup>
            </Project>
            """,
            "GetPackageContents",
            output);

        result.AssertSuccess(output);

        Assert.Contains(result.Items, item => item.Matches(new
        {
            Identity = "Readme",
            PackFolder = PackFolderKind.Dependency,
            Version = ReadmeVersion,
        }));
    }

    static string ReadPackageEntry(string nupkgPath, string entryName)
    {
        using var zip = ZipFile.OpenRead(nupkgPath);
        var entry = zip.Entries.SingleOrDefault(e =>
            e.FullName.Equals(entryName, System.StringComparison.OrdinalIgnoreCase) ||
            e.Name.Equals(entryName, System.StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(entry);

        using var stream = entry!.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
