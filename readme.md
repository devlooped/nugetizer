![Icon](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/nugetizer-32.png) nugetizer
============

Simple, flexible, intuitive and powerful NuGet packaging.

[![Version](https://img.shields.io/nuget/vpre/NuGetizer.svg?color=royalblue)](https://www.nuget.org/packages/NuGetizer) [![Downloads](https://img.shields.io/nuget/dt/NuGetizer?color=darkmagenta)](https://www.nuget.org/packages/NuGetizer) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/devlooped/nugetizer/blob/main/license.txt) [![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/nugetizer)

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/nugetizer/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json) [![CI Status](https://github.com/devlooped/nugetizer/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/nugetizer/actions?query=branch%3Amain+workflow%3Abuild+)


# Why

The .NET SDK has built-in support for packing. The design of its targets, property 
and item names it not very consistent, however. When packing non-trivial solutions 
with multiple projects, it's quite hard to actually get it to pack exactly the 
way you want it to.

An [alternative clean and clear design](https://github.com/NuGet/Home/wiki/NuGetizer-3000) 
was proposed and I got to implement the initial spec, but it never got traction 
with the NuGet team.

# How

You *must* install the [NuGetizer](https://nuget.org/packages/nugetizer) package on all 
projects that are directly or indirectly being packed, since NuGetizer relies heavily on 
MSBuild to provide discovery of contributed package content from projects and their 
project references.

Package Manager:
```
Install-Package NuGetizer
```

CLI:
```
dotnet add package NuGetizer
```

MSBuild:
```
<PackageReference Include="NuGetizer" Version="..." />
```

You don't need to set `PrivateAssets=all` for NuGetizer: it will automatically 
exclude itself from your packed dependencies. 

# What

With the learnings from years of building and shipping packages of different 
levels of complexity, as well as significant use of the SDK Pack functionality 
and its various extension points, NuGetizer takes a fresh look and exposes a 
clean set of primitives so that you never have to create `.nuspec` files again.

All the [built-in properties](https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#pack-target) 
are supported.

A key difference is that adding arbitrary content to the package is supported 
with the first-class `PackageFile` item for absolute control of the package 
contents.

```xml
<ItemGroup>
    <PackageFile Include=".." PackagePath="..." />
</ItemGroup>
```

Another key design choice is that any package content inference should be trivial 
to turn off wholesale in case the heuristics don't do exactly what you need. Just set 
`EnablePackInference=false` and you will only get explicit `PackageFile` items 
in your package. This gives you ultimate control without having to understand any of the inference rules explained below. 

All [inference rules are laid out in a single .targets](src/NuGetizer.Tasks/NuGetizer.Inference.targets) file that's easy to inspect them to learn more, and the file is not imported at all when `EnablePackInference=false`.

## Package Readme

Since the introduction of [package readme on nuget.org](https://docs.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org), 
more and more packages are leveraging this feature to make a package more discoverable and user friendly. One common 
need that arises is reusing existing documentation content that exists elsewhere in the project repository, such as 
on the root readme for the project (which typically contains additional information beyond user facing documentation, 
such as how to clone, build and contribute to the repository). In order to maximize reuse for these documentation files, 
NuGetizer supports includes in the package readme, such as:

```
This is the package readme.
<!-- include ../../../readme.md#usage -->

<!-- include ../../../footer.md -->
```

This readme includes a specific section of the repository root readme (via `#usage`), which is defined as follows:

```
# Project Foo
This is a general section on cloning, contributing, CI badges, etc.

<!-- #usage -->
# Usage
Here we explain our awesome API...
<!-- #usage -->
...
```

By defining both starting and closing `#usage` markup, the package readme can include a specific section. 
The footer, by contrast, is included wholesale. 

When the `.nupkg` is created, these includes are resolved automatically so you keep content duplication to a 
minimum. Nested includes are also supported (i.e. `footer.md` might in turn include a `sponsors.md` file or 
a fragment of it).

## dotnet-nugetize

Carefully tweaking your packages until they look exactly the way you want them should not be a tedious and slow process. Even requiring your project to be built between changes can be costly and reduce the speed at which you can iterate on the packaging aspects of the project. Also, generating the final `.nupkg`, opening it in a tool and inspecting its content, is also not ideal for rapid iteration.

For this reason, NuGetizer provides a dotnet global tool to make this process straightforward and quick. Installation is just like for any other dotnet tool:

```
> dotnet tool install -g dotnet-nugetize
```

After installation, you can just run `nugetize` from the project directory to quickly get a report of the package that would be generated. This is done in the fastest possible way without compromising your customizations to the build process. They way this is achieved is by a combination of a simulated [design-time build](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md) that skips the compiler invocation and avoids the output file copying entirely, and built-in support in NuGetizer to emit the entire contents of the package as MSBuild items with full metadata, that the tool can use to render an accurate report that contains exactly the same information that would be used to emit the final `.nupkg` without actually generating it.

Here's a sample output screenshot:

![nugetize screenshot](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/dotnet-nugetize.png)

## Inner Devloop

Authoring, testing and iterating on your nuget packages should be easy and straightforward. NuGetizer makes it trivial to consume your locally-built packages from a sample test project to exercise its features, by automatically performing the following cleanups whenever you build a new version of a package:

   a. Clean previous versions of the same package in the package output path
   b. Clean NuGet cache folder for the package id (i.e. *%userprofile%\.nuget\packages\mypackage*)
   c. Clean the NuGet HTTP cache: this avoids a subsequent restore from a consuming project from getting a cached older version, in case you build locally the same version number that was previously restored.

This means that to iterate quickly, these are the only needed steps:

  1. Build/Pack a new version
  2. Run Restore/Build on the sample project


To make the process smoother, consider the following tweaks:

  * Use single `PackageOutputPath`: if you create multiple packages, it's helpful to place them all in a single output directory. This can be achieved easily by adding the property to a `Directory.Build.props` file and place it at your repository root (or your `src` folder).:

    ```xml
    <PackageOutputPath Condition="'$(PackageOutputPath)' == ''">$(MSBuildThisFileDirectory)..\bin</PackageOutputPath>
    ```

  * Use `<RestoreSources>` in your consuming/test projects: this allows you to point to that common folder and even do it selectively only if the folder exists (i.e. use local packages if you just built them, use regular feed otherwise). You can place this too in a `Directory.Build.props` for all your consuming sample/test projects to use:

    ```xml
    <RestoreSources>https://api.nuget.org/v3/index.json;$(RestoreSources)</RestoreSources>
    <RestoreSources Condition="Exists('$(MSBuildThisFileDirectory)..\..\bin\')">
      $([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)..\..\bin'));$(RestoreSources)
    </RestoreSources>
    ```

## Package Contents Inference

Package content inference provides some built-in heuristics for common scenarios so you 
don't have to customize the project much and can instead let the rules build up the contents 
of your package by interpreting your existing project elements. It works by transforming various built-in 
items into corresponding `PackageFile` items, much as if you had added them by hand.

For example, if you create a `readme.md` file alongside the project, it will (by default) be 
automatically included in the package and set as the `Readme` metadata. Likewise, if you provide 
the `$(PackageReadmeFile)` property pointing to a different filename (say, `readme.txt`), it will 
also be automatically added to the package, without you having to add an explicit `PackageFile` or 
update the item with `<None Update='readme.txt' Pack='true' />` so it packs properly. 

> NOTE: package readme inference can be turned off with the `PackReadme=false` project property.

Inference can be turned off for specific items by just adding `Pack="false"` 
item metadata. It can also be turned off by default for all items of a given type with an item definition group:

```xml
<ItemDefinitionGroup>
  <PackageReference>
    <Pack>false</Pack>
  </PackageReference>
</ItemDefinitionGroup>
```

The basic item metadata that drive pack inference are:

1. **Pack**: *true*/*false*, determines whether inference applies to the item at all.
2. **PackagePath**: final path within the package. Can be a directory path ending in `\` and in that case the item's *RelativeDir*, *Filename* and *Extension* will be appended automatically. Linked files are also supported automatically.

If the item does **not** provide a *PackagePath*, and *Pack* is not *false*, the inference targets wil try to determine the right value, based on the following additional metadata:

  * **PackFolder**: typically one of the [built-in package folders](https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Packaging/PackagingConstants.cs#L19), such as *build*, *lib*, etc.
  * **FrameworkSpecific**: *true*/*false*, determines whether the project's target framework is used when building the final *PackagePath*.
  * **TargetPath**: optional PackFolder-relative path for the item. If not provided, the relative path of the item in the project (or its *Link* metadata) is used.


When an item specifies *FrameworkSpecific=true*, the project's target framework is added to the final package path, such as `lib\netstandard2.0\My.dll`. Since the package folder itself typically determines whether it contains framework-specific files or not, the *FrameworkSpecific* value has sensible defaults so you don't have to specify it unless you want to override it. The [default values from NuGetizer.props](src/NuGetizer.Tasks/NuGetizer.props) are:

| PackFolder       | FrameworkSpecific |
|------------------|-------------------|
| content (*)      | true              |
| lib              | true              |
| dependency (**)  | true              |
| frameworkReference (**) | true       |
| build            | false             |
| all others (***) | false             |

\* Since the plain *content* folder [is deprecated as of NuGet v3+](https://docs.nuget.org/ndocs/schema/nuspec#using-the-contentfiles-element-for-content-files), we use *content* to mean *contentFiles* throughout the docs, targets and implementation. They are interchangeable in NuGetizer and always mean the latter.

\** *dependency* and *frameworkReference* are pseudo folders containing the package references and framework (`<Reference ...`) references.

\** tool(s), native, runtime(s), ref, analyzer(s), source/src, any custom folder.

The `PackFolder` property (at the project level) determines the *PackFolder* metadata value for the build outputs of the project (and its xml docs, pdb and other related files like satellite assemblies). It defaults to `lib`.

For files that end up mapping to *content*, you can also specify *BuildAction*, *CopyToOutput* and *Flatten* item metadata, [as supported by NuGet v4+](https://docs.nuget.org/ndocs/schema/nuspec#using-the-contentfiles-element-for-content-files). In addition to those, NuGetizer also supports *CodeLanguage* and *TargetFramework* to [control the subfolders](https://docs.microsoft.com/en-us/nuget/reference/nuspec#package-folder-structure) too.

Since it wouldn't be much fun having to annotate everything with either *PackFolder* or *PackagePath* (and also the additional *content* file metadata as needed), most common item types have sensible defaults too, defined in [NuGetizer.Inference.targets](src/NuGetizer.Tasks/NuGetizer.Inference.targets). 

| ItemType        | Default Metadata  |
|------------------|-------------------|
| Content<br/>EmbeddedResource<br/>ApplicationDefinition<br/>Page<br/>Resource<br/>SplashScreen<br/>DesignData<br/>DesignDataWithDesignTimeCreatableTypes<br/>CodeAnalysisDictionary<br/>AndroidAsset<br/>AndroidResource<br/>BundleResource | PackFolder="content" <br/>BuildAction="[*ItemType*]" |
| None             | PackFolder="" <br/>BuildAction="None"         |
| Compile          | PackFolder="content" <br/>BuildAction="Compile"<br/>CodeLanguage="$(DefaultLanguageSourceExtension)"                |

`None` is sort of special in that the package folder is root of the package by default.

Whether items are packed by default or not is controlled by properties named after the item type (such as `PackEmbeddedResource`, `PackNone` and so on). Except for the ones below, they all default to *false* (or more precisely, empty, so, not *true*).

| Property        | Default Value |
|-----------------|---------------|
| PackBuildOutput | true |
| PackReadme      | true |
| PackSymbols     | true if PackBuildOutput=true (*) |
| PackSatelliteDlls | true if PackBuildOutput=true (**) |
| PackDependencies| empty (***) |
| PackFrameworkReferences | true if PackFolder=lib, false if PackDependencies=false |
| PackProjectReferences | true |



\* Back in the day, PDBs were Windows-only and fat files. Nowadays, portable PDBs 
   (the new default) are lightweight and can even be embedded. Combined with [SourceLink](https://github.com/dotnet/sourcelink), including them in the package (either standalone or embeded) provides the best experience for your users, so it's the default.

\** Satellite resources can come from the main project or from dependencies, if those PackageReferences 
    have `PrivateAssets=all`.

\*** In some scenarios, you might want to turn off packing behavior for all PackageReference and FrameworkReferences alike. Setting PackDependencies=false achieves that.


The various supported item inference are surfaced as `<PackInference Include="Compile;Content;None;..." />` items, which are ultimately evaluated together with the metadata for the individual items. These make the package inference candidates. You can also provide an exclude expression for that evaluation so that certain items are excluded by default, even if every other item of the same type is included. For example, to pack all `Content` items, except those in the `docs` folder, you can simply update the inference item like so:

```xml
<ItemGroup>
  <PackInference Update="Content" PackExclude="docs/**/*.*" />
</ItemGroup>
```

Of course you could have achieved a similar effect by updating the Content items themselves too instead:

```xml
<ItemGroup>
  <Content Update="docs/**/*.*" Pack="false" />
</ItemGroup>
```

By default (see [NuGetizer.Inference.props](src/NuGetizer.Tasks/NuGetizer.Inference.props)), `Compile` has the following exclude expression, so generated intermediate compile files aren't packed:

```xml
<ItemGroup>
  <PackInference Include="Compile"
                 PackExclude="$(IntermediateOutputPath)/**/*$(DefaultLanguageSourceExtension)" />
</ItemGroup>
```


### CopyToOutputDirectory

There is a common metadata item that's used quite frequently: *CopyToOutputDirectory*, which is typically set to *PreserveNewest* to change it from its default behavior (when empty or set to *Never*).

> NOTE: if you're using *Always*, you're likely ruining your build performance for no reason.

When copying items to the output directory, you're implicitly saying that those items are needed in order to run/execute the built output. For example, if you have build targets/props in a build-only project (i.e. the one that builds the tasks), then those files are needed alongside the built output when packaging.

Given this common scenario, NuGetizer changes the default `PackFolder` metadata for packable items (i.e. those with explicit `Pack=true` metadata or defaulted to *true*, such as `Content` items) to match the `PackFolder` property defined for the project's built output, whenever `CopyToOutputDirectory` is not empty or *Never*.

Like other default inference behaviors, you can always opt out of it by specifying an explicit `PackFolder` item metadata.

In addition, the resulting `PackageFile` items for these items point to the location in the project's output folder, rather than the source location. This makes it easier to have custom behavior that might modify the item after copying to the output directory.

### PackageReference

Package references are turned into package dependencies by default (essentially converting `<PackageReference>` to `<PackageFile ... PackFolder="Dependency">`), unless `PackDependencies` property is `false`. If the package reference specifies `PrivateAssets="all"`, however, it's not added as a dependency. Instead, in that case, all the files contributed to the compilation (more precisely: all copy-local runtime dependencies) are placed in the same `PackFolder` as the project's build output (if packable, depending on `PackBuildOutput` property).

Build-only dependencies that don't contribute assemblies to the output (i.e. analyzers or things like [GitInfo](https://github.com/devlooped/GitInfo) or [ThisAssembly](https://github.com/devlooped/ThisAssembly) won't cause any extra items.

This even works transitively, so if you use *PrivateAssets=all* on package reference *A*, which in turn has a package dependency on *B* and *B* in turn depends on *C*, all of *A*, *B* and *C* assets will be packed. You can opt out of the transitive packing with `PackTransitive=false` metadata on the `PackageReference`.

As usual, you can change this default behavior by using `Pack=false` metadata.

You can also control precisely what assets are surfaced from your dependencies, by 
using `PackInclude` and `PackExclude` metadata on the `PackageReference`. This will 
result in the corresponding `include`/`exclude` attributes as documented in the 
[nuspec reference](https://learn.microsoft.com/en-us/nuget/reference/nuspec#dependencies-element). If not defined, both are defaulted to the package 
reference `IncludeAssets` and `ExcludeAssets` metadata.

### ProjectReference

Unlike SDK Pack that [considers project references as package references by default](https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#project-to-project-references), NuGetizer has an explicit contract between projects: the `GetPackageContents` target. This target is invoked when packing project references, and it returns whatever the referenced project exposes as package contents (including the inference rules above). If the project is *packable* (that is, it produces a package, denoted by the presence of a `PackageId` property or `IsPackable=true`, for compatibility with SDK Pack), it will be packed as a dependency/package reference instead.

This means that by default, things Just Work: if you reference a library with no `PackageId`, it becomes part of whatever output your main project produces (analyzer, tools, plain lib). The moment you decide you want to make it a package on its own, you add the required metadata properties to that project and it automatically becomes a dependency instead.

This works flawlessly even when multi-targeting: if the main (packable) project multitargets `net472;netcoreapp3.1`, say, and it references a `netstandard2.0` (non-packable) library, the package contents will be:

```
  /lib/
    net472/
      library.dll
      library.pdb
      sample.dll
      sample.pdb
    netcoreapp3.1/
      library.dll
      library.pdb
      sample.dll
      sample.pdb
```

If the packaging metadata is added to the library, it automatically turns to:

```
Package: Sample.1.0.0.nupkg
         ...\Sample.nuspec
    Authors                 : sample
    Description             : Sample
    Version                 : 1.0.0
  Dependencies:
    net472
      Library, 1.0.0
    netcoreapp3.1
      Library, 1.0.0
  Contents:
    /lib/
      net472/
        sample.dll
        sample.pdb
      netcoreapp3.1/
        sample.dll
        sample.pdb
```

If you need to tweak target folder of a referenced project, you can also do so 
via the `PackFolder` attribute on the `ProjectReference` itself:

```xml
   <ProjectReference Include="..\MyDesktopLibrary\MyDesktopLibrary.csproj" 
                     PackFolder="lib\net6.0\SpecificFolder" />
```

> NOTE: this is a convenience shortcut since you can already pass additional project 
> properties for project references using the built-in 
> [`AdditionalProperties` attribute](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-task?view=vs-2019#pass-properties-to-projects).

Finally, you can focedly turn a project reference build output into a private asset even if it defines a `PackageId` by adding `PrivateAssets=all`. This is very useful for build and analyzer packages, which typically reference the main library project too, but need its output as private, since neither can use dependencies at run-time.

## Packaging Projects

Typically, when creating a package involves more than one project (i.e. main library, some 
build tasks + targets, some other runtime tools), you will want to create a separate packaging 
project that is *not* a typical class library. For that purpose, you can create an `.msbuildproj` 
which has built-in support in Visual Studio. It can use the `Microsoft.Build.NoTargets` SDK as follows:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.Build.NoTargets/3.7.0">
  <PropertyGroup>
    <PackageId>MyPackage</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>
```

> NOTE: the requirement of a `TargetFramework` comes from the underlying SDK and the .NET SDK 
targets themselves, but this kind of project will not build any output. Running the `nugetize` 
on this project (after a `dotnet restore`) would render:

![nugetize authoring screenshot](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/nugetize-authoring-1.png)

If you add a project reference to a build tasks project like the following:

```xml
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>buildTransitive</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' />
  </ItemGroup>
</Project>
```

> NOTE: this project would contain MSBuild tasks, and likely a [PackageId].targets alongside so that 
it's automatically imported in consuming projects. 

The packaging project would now look as follows:

```xml
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackageId>MyPackage</PackageId>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='..\Tasks\Tasks.csproj' />
  </ItemGroup>
</Project>
```

And `nugetize` would show the following package structure:

![nugetize authoring screenshot](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/nugetize-authoring-2.png)

Note that the targets file was automatically added to the package as expected. Packaging projects 
can reference other packaging projects in turn for complex packing scenarios too.

If the packaging project references both a build-targeting project (such as the one above) and 
also a regular library project, the package contents becomes the aggregation of the contents 
contributed by each referenced project automatically. For example, if you add a project reference 
from the packaging project to the following class library project:

```xml
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='System.Text.Json' Version='6.0.0' />
  </ItemGroup>
</Project>
```

The content would now be:

![nugetize authoring screenshot](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/nugetize-authoring-3.png)

You can also add a reference to a CLI *tools* program like the following:

```xml

<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <PackFolder>tools</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='System.CommandLine' Version='2.0.0-beta3.22114.1' />
  </ItemGroup>
</Project>
```

![nugetize authoring screenshot](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/nugetize-authoring-4.png)

As you can see, it's quite trivial to build fairly complex packages using very intuitive defaults and 
content inference. 

## Advanced Features

This section contains miscellaneous useful features that are typically used in advanced scenarios and 
are not necessarily mainstream.

### PackAsPublish for CLI tools

When a project's output type is `Exe` and it's not set to `PackAsTool=true` (used specifically for .NET tools), 
it will default to be use the `Publish` output for packing. This is typically what you want for a CLI 
project, since dependencies are included in the publish directory automatically without having to annotate 
any references with `PrivateAssets=all`.

This can be turned off by setting `PackAsPublish=false` on the project, which will cause the project 
to be packed as a regular class library, with the dependencies inference rules applied (such as 
`PrivateAssets=all` for package reference and `CopyLocal=true` for references).

When packing as publish, the output won't be framework-specific by default, and will just contribute 
the published contents to the specified `PackFolder`.

### Dynamically Extending Package Contents

If you need to calculate additional items to inject into the package dynamically, you can run a target 
before `GetPackageContents`, which is the target NuGetizer uses before packing to determine what needs 
to be included. At this point you can add arbitrary `<PackageFile ... PackagePath=... />` items laying 
out precisely what it is you want to inject into the .nupkg. For example:
    
```xml
<Target Name="AddPackageContents" BeforeTargets="GetPackageContents">
    <ItemGroup>
        <PackageFile Include="$(MSBuildProjectDirectory)\..\docs\**\*.md" PackagePath="docs\%(RelativeDir)%(Filename)%(Extension)" />
    </ItemGroup>
</Target>
```
    
This example will add all markdown files in a `docs` folder one level above the current project, and 
place them all under the `docs` folder in the `.nupkg`, preserving their original folder structure.
    
### Packing arbitrary files from referenced packages

If you want to pack files from referenced packages, you can simply add `PackageReference` attribute 
to `PackageFile`. Say we want to reuse the awesome icon from the 
[ThisAssembly](https://nuget.org/packages/ThisAssembly) package, we can just bring it in with:

```xml
<ItemGroup>
  <PackageFile Include="icon-128.png" PackagePath="icon.png" PackageReference="ThisAssembly" />
</ItemGroup>
```

The project will need to reference that package too, of course:

```xml
<ItemGroup>
  <PackageReference Include="ThisAssembly" Version="1.0.0" GeneratePathProperty="true" Pack="false" />
</ItemGroup>
```

Note that we had to add the `GeneratePathProperty` to the reference, so that the package-relative 
path `icon-128.png` can be properly resolved to the package install location. You can also set that 
metadata for all your `PackageReference`s automatically by adding the following to your `Directory.Build.props` 
(or .targets): 

```xml
  <ItemDefinitionGroup>
    <PackageReference>
      <!-- This enables referencing arbitrary files from any package by adding PackageReference="" to any packable item -->
      <GeneratePathProperty>true</GeneratePathProperty>
    </PackageReference>
```

Also note that in the scenario shown before, we don't want to pack the reference as a dependency (it's a build-only or development 
dependency package). That is, this feature does not require a package *dependency* for the referenced package content 
we're bringing in.

It even works for inferred content item types, such as `None`:

```xml
<ItemGroup>
  <None Include="icon-128.png" PackageReference="ThisAssembly" />
</ItemGroup>
```

### Skip Build during Pack

If you are building explicitly prior to running `Pack` (and you're not using 
`PackOnBuild=true`), you might want to optimize the process by skipping the 
automatic `Build` run that happens by default when you run `Pack` by setting 
`BuildOnPack=false`. Not building before `Pack` with `BuildOnPack=false` 
can cause the target run to fail since output files expected by the packaging 
might be missing (i.e. the primary output, content files, etc.).

This option is useful in combination with `BuildProjectReferences=false` when 
packing on CI, since at that point all that's run are the P2P protocol involving 
`GetPackageContents`.

### Package Validation

[Package validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview) 
is a new feature in .NET 6 that allows you to validate that your multi-targeting 
library packages offer consistent APIs across all targets. Since it's quite 
important to validate that your packages are consistent across all targets, 
NuGetizer turns this feature on by default for `Release` builds in multi-targeting 
projects (unlike the default which is strictly opt-in).

You can turn this off by setting the following property at the project level:

```xml
<PropertyGroup>
  <EnablePackageValidation>false</EnablePackageValidation>
</PropertyGroup>
```

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![C. Augusto Proiete](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/augustoproiete.png "C. Augusto Proiete")](https://github.com/augustoproiete)
[![Kirill Osenkov](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KirillOsenkov.png "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Stephen Shaw](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/decriptor.png "Stephen Shaw")](https://github.com/decriptor)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![David Kean](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davkean.png "David Kean")](https://github.com/davkean)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/chiluap.png "")](https://github.com/chiluap)
[![Daniel Gnägi](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dgnaegi.png "Daniel Gnägi")](https://github.com/dgnaegi)
[![Ashley Medway](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/AshleyMedway.png "Ashley Medway")](https://github.com/AshleyMedway)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![bitbonk](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/bitbonk.png "bitbonk")](https://github.com/bitbonk)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Yurii Rashkovskii](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/yrashk.png "Yurii Rashkovskii")](https://github.com/yrashk)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Zdenek Havlin](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/wdolek.png "Zdenek Havlin")](https://github.com/wdolek)
[![Sean Killeen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SeanKilleen.png "Sean Killeen")](https://github.com/SeanKilleen)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Giorgi Dalakishvili](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Giorgi.png "Giorgi Dalakishvili")](https://github.com/Giorgi)
[![Kelly White](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mckhendry.png "Kelly White")](https://github.com/mckhendry)
[![Allan Ritchie](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/aritchie.png "Allan Ritchie")](https://github.com/aritchie)
[![Mike James](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MikeCodesDotNET.png "Mike James")](https://github.com/MikeCodesDotNET)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Dan Siegel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dansiegel.png "Dan Siegel")](https://github.com/dansiegel)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jeremy Simmons](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jeremysimmons.png "Jeremy Simmons")](https://github.com/jeremysimmons)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![Norman Mackay](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mackayn.png "Norman Mackay")](https://github.com/mackayn)
[![Certify The Web](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/certifytheweb.png "Certify The Web")](https://github.com/certifytheweb)
[![Taylor Mansfield](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/lavahot.png "Taylor Mansfield")](https://github.com/lavahot)
[![Mårten Rånge](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mrange.png "Mårten Rånge")](https://github.com/mrange)
[![David Petric](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidpetric.png "David Petric")](https://github.com/davidpetric)
[![Rich Lee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/richlee.png "Rich Lee")](https://github.com/richlee)
[![Danilo Dantas](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dannevesdantas.png "Danilo Dantas")](https://github.com/dannevesdantas)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/nietras.png "")](https://github.com/nietras)
[![Gary Woodfine](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/garywoodfine.png "Gary Woodfine")](https://github.com/garywoodfine)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kristinnstefansson.png "")](https://github.com/kristinnstefansson)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DarrenAtConexus.png "")](https://github.com/DarrenAtConexus)
[![Steve Bilogan](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kazo0.png "Steve Bilogan")](https://github.com/kazo0)
[![Ix Technologies B.V.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/IxTechnologies.png "Ix Technologies B.V.")](https://github.com/IxTechnologies)
[![New Relic](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/newrelic.png "New Relic")](https://github.com/newrelic)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
