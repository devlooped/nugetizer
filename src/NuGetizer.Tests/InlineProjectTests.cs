using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Execution;
using NuGet.Packaging;
using NuGetizer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class InlineProjectTests
    {
        readonly ITestOutputHelper output;

        public InlineProjectTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_is_packable_true_then_package_id_defaults_to_assembly_name()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata
            }));
        }

        [Fact]
        public void when_development_dependency_then_package_has_development_dependency_metadata()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
    <DevelopmentDependency>true</DevelopmentDependency>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                DevelopmentDependency = "true"
            }));
        }

        [Fact]
        public void when_is_packable_true_but_packageid_reset_to_empty_then_fails()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <Target Name='BeforeEverything' BeforeTargets='PrepareForBuild'>
    <!-- We need to try harder to reset it to empty, since it's defaulted 
         in this case to the AssemblyName automatically -->
    <PropertyGroup>
        <PackageId />
    </PropertyGroup>
  </Target>
</Project>", output: output);

            Assert.Equal(TargetResultCode.Failure, result.ResultCode);
            // Next best to checking the full string. No way I could find to 
            // get the actually build error code.
            Assert.Contains("PackageId", result.ToString());
            Assert.Contains("IsPackable", result.ToString());
        }

        [Fact]
        public void when_no_is_packable_and_no_package_id_then_defaults_to_non_packable()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_output_assembly()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));
        }

        [Fact]
        public void when_include_outputs_in_package_is_false_then_does_not_include_main_assembly()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_include_symbols_in_package_is_true_but_include_outputs_is_false_then_does_not_include_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <PackSymbols>true</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_include_symbols_in_package_is_false_then_does_not_include_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackSymbols>false</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_xmldoc()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackSymbols>false</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".xml",
                PackFolder = PackFolderKind.Lib,
            }));
        }

        [Fact]
        public void when_include_output_in_package_is_false_then_does_not_include_xmldoc()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);


            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".xml",
                PackFolder = PackFolderKind.Lib,
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_annotates_items_with_package_id()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.All(result.Items, item => Assert.Equal("Foo", item.GetMetadata("PackageId")));
        }

        [RuntimeFact("Windows")]
        public void when_getting_package_contents_then_includes_framework_reference()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='PresentationFramework' />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.NETFramework.ReferenceAssemblies' Version='1.0.2' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "PresentationFramework",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }

        [RuntimeFact("Windows")]
        public void when_include_framework_references_in_package_is_false_then_does_not_include_framework_reference()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <PackFrameworkReferences>false</PackFrameworkReferences>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='PresentationFramework' />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.NETFramework.ReferenceAssemblies' Version='1.0.2' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "PresentationFramework",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }

        [Fact]
        public void when_updating_package_item_metadata_then_updates_metadata()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <PackageId>Foo</PackageId>
                    <PackBuildOutput>false</PackBuildOutput>
                    <TargetFramework>netstandard2.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageMetadata Include="Foo">
                      <Description>ItemDescription</Description>
                    </PackageMetadata>
                  </ItemGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items, i => i.GetMetadata("PackFolder") == "Metadata");
            var metadata = result.Items.First(i => i.GetMetadata("PackFolder") == "Metadata");

            Assert.Equal("ItemDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_updating_package_metadata_via_target_then_updates_metadata()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <PackageId>Foo</PackageId>
                    <PackBuildOutput>false</PackBuildOutput>
                    <TargetFramework>netstandard2.0</TargetFramework>
                  </PropertyGroup>
                  <Target Name="BeforeGetMetadata" BeforeTargets="GetPackageMetadata">
                    <PropertyGroup>
                      <Description>PropertyDescription</Description>
                    </PropertyGroup>
                  </Target>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items, i => i.GetMetadata("PackFolder") == "Metadata");
            var metadata = result.Items.First(i => i.GetMetadata("PackFolder") == "Metadata");

            Assert.Equal("PropertyDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_setting_metadata_property_then_updates_metadata()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <PackageId>Foo</PackageId>
                    <PackBuildOutput>false</PackBuildOutput>
                    <Title>MyPackage</Title>
                    <Description>Description</Description>
                    <TargetFramework>netstandard2.0</TargetFramework>
                  </PropertyGroup>
                  <Target Name="BeforeGetMetadata" BeforeTargets="GetPackageMetadata">
                    <PropertyGroup>
                      <Description>PropertyDescription</Description>
                    </PropertyGroup>
                  </Target>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items, i => i.GetMetadata("PackFolder") == "Metadata");
            var metadata = result.Items.First(i => i.GetMetadata("PackFolder") == "Metadata");

            Assert.Equal("MyPackage", metadata.GetMetadata("Title"));
            Assert.Equal("PropertyDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_package_reference_has_metadata_then_inferred_package_references_has_same_metadata()
        {
            var project = @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CodeAnalysis' Version='3.8.0' MyMetadata='Foo' PrivateAssets='all' />
  </ItemGroup>
</Project>";

            var result = Builder.BuildProject(project, "_CollectPrimaryOutputDependencies", output: output);
            result.AssertSuccess(output);

            var metadata = result.Items.FirstOrDefault(i => i.ItemSpec == "Microsoft.CodeAnalysis.Common");

            Assert.NotNull(metadata);
            Assert.Equal("Foo", metadata.GetMetadata("MyMetadata"));
        }

        [Fact]
        public void when_referencing_package_reference_file_then_it_requires_generate_path_property()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='false' />
  </ItemGroup>
  <ItemGroup>
    <PackageFile Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            Assert.Equal(TargetResultCode.Failure, result.ResultCode);

            // Next best to checking the full string. No way I could find to 
            // get the actually build error code.
            Assert.Contains("GeneratePathProperty", result.ToString());
        }

        [Fact]
        public void when_referencing_package_reference_file_then_resolves_to_package_path()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='true' />
  </ItemGroup>
  <ItemGroup>
    <PackageFile Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            var icon = result.Items.FirstOrDefault(i => i.ItemSpec.EndsWith("icon-128.png"));

            Assert.NotNull(icon);
            Assert.True(File.Exists(icon.GetMetadata("FullPath")));
        }

        [Fact]
        public void when_referencing_package_reference_file_then_can_use_inference_items()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='true' />
  </ItemGroup>
  <ItemGroup>
    <Content Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            var icon = result.Items.FirstOrDefault(i => i.ItemSpec.EndsWith("icon-128.png"));

            Assert.NotNull(icon);
            Assert.True(File.Exists(icon.GetMetadata("FullPath")));
        }

        [RuntimeFact("Windows")]
        public void when_pack_on_build_multitargeting_then_contains_all_targets()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFrameworks>net472;netstandard2.0</TargetFrameworks>
    <PackOnBuild>true</PackOnBuild>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.NETFramework.ReferenceAssemblies' Version='1.0.2' />
  </ItemGroup>
</Project>", "Build,GetPackageTargetPath", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var packagePath = result.Items[0].GetMetadata("FullPath");

            Assert.True(File.Exists(packagePath));

            using var package = ZipFile.OpenRead(packagePath);
            var files = package.GetFiles().ToArray();

            Assert.Contains(files, file => file.StartsWith("lib/net472"));
            Assert.Contains(files, file => file.StartsWith("lib/netstandard2.0"));
        }

        [RuntimeFact("Windows")]
        public void when_generate_package_on_build_multitargeting_then_contains_all_targets()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFrameworks>net472;netstandard2.0</TargetFrameworks>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.NETFramework.ReferenceAssemblies' Version='1.0.0' />
  </ItemGroup>
</Project>", "Build,GetPackageTargetPath", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var packagePath = result.Items[0].GetMetadata("FullPath");

            Assert.True(File.Exists(packagePath));

            using var package = ZipFile.OpenRead(packagePath);
            var files = package.GetFiles().ToArray();

            Assert.Contains(files, file => file.StartsWith("lib/net472"));
            Assert.Contains(files, file => file.StartsWith("lib/netstandard2.0"));
        }

        [Fact]
        public void when_packing_private_transive_reference_then_packs_lib()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Scriban' Version='3.0.4' PrivateAssets='all' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            // Ref files are not there
            Assert.DoesNotContain(result.Items, item
                => item.GetMetadata("FullPath").EndsWith(Path.Combine("ref", "netstandard2.0", "System.Runtime.CompilerServices.Unsafe.dll")));
            Assert.DoesNotContain(result.Items, item
                => item.GetMetadata("FullPath").EndsWith(Path.Combine("ref", "netstandard2.0", "System.Threading.Tasks.Extensions.dll")));

            // Replaced by lib
            Assert.Contains(result.Items, item
                => item.GetMetadata("FullPath").EndsWith(Path.Combine("lib", "netstandard2.0", "System.Runtime.CompilerServices.Unsafe.dll")));
            Assert.Contains(result.Items, item
                => item.GetMetadata("FullPath").EndsWith(Path.Combine("lib", "netstandard2.0", "System.Threading.Tasks.Extensions.dll")));
        }

        [Fact]
        public void when_packing_private_dependency_then_can_opt_out_of_transitive()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Scriban' Version='3.0.4' PrivateAssets='all' PackTransitive='false' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Filename = "System.Runtime.CompilerServices.Unsafe",
                Extension = ".dll"
            }));

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Filename = "System.Threading.Tasks.Extensions",
                Extension = ".dll"
            }));
        }

        [Fact]
        public void when_packing_none_with_packagereference_then_includes_it()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='12.0.3' />
    <None Include='lib\netstandard2.0\Newtonsoft.Json.dll' PackageReference='Newtonsoft.Json' /> 
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "lib/netstandard2.0/Newtonsoft.Json.dll"
            }));
        }

        [Fact]
        public void when_packing_none_with_packagereference_then_can_change_package_path()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='12.0.3' />
    <None Include='lib\netstandard2.0\Newtonsoft.Json.dll' PackageReference='Newtonsoft.Json' PackagePath='build\%(Filename)%(Extension)' /> 
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "build/Newtonsoft.Json.dll"
            }));
        }

        [RuntimeFact("Windows")]
        public void when_packing_transitive_dependency_then_retargets_to_main_project()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' />
  </ItemGroup>
</Project>", output: output, files: new[] {
    ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Moq' Version='4.18.1' />
    <ProjectReference Include='Helpers.csproj' />
  </ItemGroup>
</Project>"),
    ("Helpers.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Xunit' Version='2.4.1' />
  </ItemGroup>
</Project>")
            });

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Moq",
                TargetFramework = "net472",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Xunit",
                TargetFramework = "net472",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_multi_targeting_packing_transitive_dependency_then_retargets_to_main_project()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFrameworks>net472;netstandard2.0;net6.0</TargetFrameworks>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' />
  </ItemGroup>
</Project>", output: output, files:
    ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Moq' Version='4.18.1' />
  </ItemGroup>
</Project>"
    ));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Moq",
                TargetFramework = "net472",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Moq",
                TargetFramework = "net6.0",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Moq",
                TargetFramework = "netstandard2.0",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_packing_with_refs_then_includes_runtime_libs_for_private()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                	<PropertyGroup>
                        <IsPackable>true</IsPackable>
                        <TargetFramework>net472</TargetFramework>
                	</PropertyGroup>

                	<ItemGroup>
                		<PackageReference Include="System.Buffers" Version="4.5.1" PrivateAssets="all" />
                		<PackageReference Include="System.Memory" Version="4.5.5" PrivateAssets="all" />
                	</ItemGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PathInPackage = "lib/net461/System.Buffers.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PathInPackage = "lib/net461/System.Memory.dll",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_packing_dependencies_then_can_include_exclude_assets()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                	<PropertyGroup>
                		<OutputType>Exe</OutputType>
                		<TargetFramework>net472</TargetFramework>
                        <IsPackable>true</IsPackable>
                        <LangVersion>Latest</LangVersion>
                	</PropertyGroup>
                
                	<ItemGroup>
                		<PackageReference Include="PolySharp" Version="1.12.1" IncludeAssets="analyzers" ExcludeAssets="build" />
                	</ItemGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "PolySharp",
                PackFolder = "Dependency",
                IncludeAssets = "analyzers",
                ExcludeAssets = "build"
            }));
        }

        [Fact]
        public void when_packing_dependencies_then_defaults_to_no_packinclude_nor_packexclude()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                	<PropertyGroup>
                		<OutputType>Exe</OutputType>
                		<TargetFramework>netstandard2.0</TargetFramework>
                        <IsPackable>true</IsPackable>
                        <LangVersion>Latest</LangVersion>
                	</PropertyGroup>
                
                	<ItemGroup>
                		<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
                	</ItemGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.CSharp",
                PackFolder = "Dependency",
                PackInclude = "",
                PackExclude = ""
            }));
        }

        [Fact]
        public void when_private_assets_then_packs_transitively()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                	<PropertyGroup>
                		<TargetFramework>net8.0</TargetFramework>
                        <IsPackable>true</IsPackable>
                	</PropertyGroup>
                
                	<ItemGroup>
                		<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" PrivateAssets="all" />
                	</ItemGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                NuGetPackageId = "Microsoft.Extensions.Configuration.Abstractions",
                PackFolder = "Lib",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_validating_package_then_succeeds()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                	<PropertyGroup>
                		<TargetFrameworks>net6.0;net8.0</TargetFrameworks>
                        <IsPackable>true</IsPackable>
                        <EnablePackageValidation>true</EnablePackageValidation>
                	</PropertyGroup>                
                </Project>
                """, target: "Pack", output: output);

            result.AssertSuccess(output);
        }

        [Fact]
        public void when_package_icon_default_then_packs_icon()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                    <PackageIcon>icon.png</PackageIcon>
                    <EnableDefaultItems>true</EnableDefaultItems>
                  </PropertyGroup>
                </Project>  
                """
                , output: output,
                files: ("icon.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = "Metadata",
                Icon = "icon.png",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "icon.png",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_package_icon_relative_folder_default_then_packs_icon()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                    <PackageIcon>assets\icon.png</PackageIcon>
                    <EnableDefaultItems>true</EnableDefaultItems>
                  </PropertyGroup>
                </Project>  
                """
                , output: output,
                files: ("assets\\icon.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = "Metadata",
                Icon = "assets/icon.png",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/icon.png",
            }));
        }

        [Fact]
        public void when_package_icon_content_then_packs_icon_and_not_content()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                    <PackageIcon>icon.png</PackageIcon>
                  </PropertyGroup>
                  <ItemGroup>
                    <Content Include="icon.png" />
                  </ItemGroup>
                </Project>  
                """
                , output: output,
                files: ("icon.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = "Metadata",
                Icon = "icon.png",
            }));
            // The icon that would be in the content folder is not packed as a contentFile
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = "content"
            }));
            // And it's instead in the root of the package
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "icon.png",
            }));
        }

        [Fact]
        public void when_package_icon_linked_content_then_packs_link()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                    <PackageIcon>assets\icon.png</PackageIcon>
                  </PropertyGroup>
                  <ItemGroup>
                    <Content Include="..\icon.png" Link="assets\icon.png" />
                  </ItemGroup>
                </Project>  
                """
                , output: output,
                files: ("..\\icon.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = "Metadata",
                Icon = "assets/icon.png",
            }));
            // The icon that would be in the content folder is not packed as a contentFile
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = "content"
            }));
            // And it's instead in the root of the package
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/icon.png",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_packagepath_ends_in_path_then_packs_recursive_dir()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageFile Include="..\img\**\*.*" PackagePath="assets\" />
                  </ItemGroup>
                </Project>  
                """
                , "GetPackageContents", output, default,
                ("../img/brand/icon.png", ""),
                ("../img/docs/screen.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/brand/icon.png",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/docs/screen.png",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_packagepath_ends_in_path_then_packs_basedir_dir()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageFile Include="..\img\**\*.*" PackagePath="assets\" />
                  </ItemGroup>
                </Project>  
                """
                , "GetPackageContents", output, default,
                ("../img/icon.png", ""),
                ("../img/screen.png", ""));

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/icon.png",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "assets/screen.png",
            }));
        }

        [Fact]
        public void when_dependency_is_development_dependency_then_can_explicitly_pack_it()
        {
            var project =
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageReference Include="ThisAssembly.Constants" Version="1.2.14" Pack="true" TargetFramework="netstandard2.0" />
                  </ItemGroup>
                </Project>  
                """;
            var result = Builder.BuildProject(project, "GetPackageContents", output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "ThisAssembly.Constants",
                PackFolder = "Dependency",
            }));

            result = Builder.BuildProject(project, "Pack", output);

            result.AssertSuccess(output);

            var package = result.Items[0].ItemSpec;
            File.Exists(package);

            using (var archive = ZipFile.OpenRead(package))
            {
                Assert.Contains(archive.Entries, entry => entry.FullName == "scenario.nuspec");
                using var stream = archive.Entries.First(x => x.FullName == "scenario.nuspec").Open();
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.NotEmpty(manifest.Metadata.DependencyGroups);
                Assert.NotEmpty(manifest.Metadata.DependencyGroups.First().Packages);
                Assert.Equal("ThisAssembly.Constants", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            }
        }

        [Fact]
        public void when_produce_reference_assembly_then_includes_ref_assembly()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ProduceReferenceAssembly>true</ProduceReferenceAssembly>
                  </PropertyGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            // Should have the main assembly in lib
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));

            // Should also have the reference assembly in ref
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Ref
            }));
        }

        [Fact]
        public void when_produce_reference_assembly_false_then_does_not_include_ref_assembly()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ProduceReferenceAssembly>false</ProduceReferenceAssembly>
                  </PropertyGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            // Should have the main assembly in lib
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));

            // Should NOT have a reference assembly in ref
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Ref
            }));
        }

        [Fact]
        public void when_produce_reference_assembly_but_pack_build_output_false_then_does_not_include_ref_assembly()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ProduceReferenceAssembly>true</ProduceReferenceAssembly>
                    <PackBuildOutput>false</PackBuildOutput>
                  </PropertyGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            // Should NOT have the main assembly in lib
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));

            // Should NOT have a reference assembly in ref either
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Ref
            }));
        }

        [Fact]
        public void when_produce_reference_assembly_can_opt_out_with_pack_ref_assembly_false()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ProduceReferenceAssembly>true</ProduceReferenceAssembly>
                    <PackRefAssembly>false</PackRefAssembly>
                  </PropertyGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            // Should have the main assembly in lib
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));

            // Should NOT have a reference assembly in ref
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Ref
            }));
        }

        [Fact]
        public void when_produce_reference_assembly_then_ref_assembly_is_framework_specific()
        {
            var result = Builder.BuildProject(
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ProduceReferenceAssembly>true</ProduceReferenceAssembly>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                </Project>
                """, output: output);

            result.AssertSuccess(output);

            // The ref assembly should have a package path that includes the TFM
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Ref,
                PackagePath = "ref/net8.0/scenario.dll"
            }));
        }
    }
}
