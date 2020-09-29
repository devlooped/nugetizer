![icon](img/nugetizer-32.png) nugetizer
===

Simple, flexible, intuitive and powerful NuGet packaging.

## Why

The .NET SDK has built-in support for packing. The design of its targets, property 
and item names it not very consistent, however. When packing non-trivial solutions 
with multiple projects, it's quite hard to actually get it to pack exactly the 
way you want it to.

An [alternative clean and clear design](https://github.com/NuGet/Home/wiki/NuGetizer-3000) 
was proposed and I got to implement the initial spec, but it never got traction 
with the NuGet team.

## What

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
in your package.

### Package Inference

Package inference provides some built-in heuristics for common scenarios so you 
don't have to customize the packing much. It works by transforming various built-in 
items into corresponding `PackageFile` items, much as if you had added them by 
hand.

Inference can be turned off for specific items by just adding `Pack="false"` 
item metadata.

The default transformations are:

| Source                                                          | Inferred                                                          | Notes                                                                                            |
| --------------------------------------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| Project build output                                            | `<PackageFile Kind="$(BuildOutputKind)"/>`                        | Kind defaults to `Lib`. Includes .xml and .pdb if they are generated. (1)                        |
| `<PackageReference ... />`                                      | `<PackageFile ... Kind="Dependency" />`                           | `PrivateAssets=all` is present, it's not added as dependency. (2)                                |
| `<Content .../>`                                                | `<PackageFile ... Kind="content" />`                              | Regular content that's not part of the build output                                              |
| `<Content ... CopyToOutputDirectory="PreserveNewest\|Always"/>` | `<PackageFile Include="%(TargetPath)" Kind="$(BuildOutputKind)" ` | Content that's copied to the build output is part of the primary output so it uses its kind. (3) |
| `<None ... Pack="true" />`                                      | `<PackageFile ... Kind="none" />`                                 |                                                                                                  |
| `<Reference ... />`                                             | `<PackageFile ... Kind="frameworkReference"`                      | Only those resolved from the target framework directory are included. (4)                        |
|                                                                 |                                                                   |                                                                                                  |

1. Back in the day, PDBs were Windows-only and fat files. Nowadays, portable PDBs 
   (the new default) are lightweight and can even be embedded. Combined with [SourceLink](https://github.com/dotnet/sourcelink), including them in the package by default (either standalone or embeded) provides the best experience for your users, so it's the default.
2. `PrivateAssets=all` in a `PackageReference` causes all the contributed files to the compilation to be packed together with the built output, unless `Pack=false` is also specified. Build-only dependencies that don't contribute assemblies to the output (i.e. analyzers or things like [GitInfo](https://github.com/kzu/GitInfo) or [ThisAssembly](https://github.com/kzu/ThisAssembly) won't cause any extra items).
3. This is the most intuitive default: things that you typically copy to the output in order to "run" your project, is stuff that will typically also be needed at run-time when users reference your package. This can be MSBuild props/targets for a build tasks package, for example, or additional files used by an analyzer or a tool.
4. `Reference` items are resolved by the `ResolveAssemblyReference` standard targets as `ReferencePath` items. Only those that have a `ResolvedFrom` metadata of `{TargetFrameworkDirectory}` are considered.

Inference control properties and metadata:

| Property                              | Description                                                                                                                    |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `PackContent=true\|false`             | Turns on/off the default inference for `@(Content)`.  Items with `Pack=true` metadata are always included. Defaults to `true`. |
| `PackNone=true\|false`                | Turns on/off the default inference for `@(None)`.  Items with `Pack=true` metadata are always included. Defaults to `false`.   |
| `PackSymbols=true\|false`             | Turns on/off inclusion of symbols (if generated). Defaults to `true`.                                                          |
| `PackFrameworkReferences=true\|false` | Turns on/off the default inference of `<Reference...` items                                                                    |

All of these [inference rules are laid out in a single .targets](src/NuGetizer.Tasks/NuGetizer.Inference.targets] file that's easy to inspect to learn more.

### Project References

Unlike SDK Pack that [considers project references as package references by default](https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#project-to-project-references), NuGetizer has an explicit contract between projects: `GetPackageContents`. This target is invoked when packing project references, and it returns whatever the referenced project exposes as package contents (including the inference rules above). If the project is *packable* (that is, it produces a package, denoted by the presence of a `PackageId` property), it will be packed as a dependency/package reference instead.

This means that by default, things Just Work: if you reference a library with no `PackageId`, it becomes part of whatever output your main project produces (analyzer, tools, plain lib). The moment you decide you want to make it a package on its own, you add the required metadata properties to that project, and automatically it becomes a dependency instead.

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

### dotnet-nugetize

Carefully tweaking your packages until they look exactly the way you want them should not be a tedious and slow process. Even requiring your project to be built between changes can be costly and reduce the speed at which you can iterate on the packaging aspects of the project. Also, generating the final `.nupkg`, opening it in a tool and inspecting its content, is also not ideal for rapid iteration.

For this reason, NuGetizer provides a dotnet global tool to make this process straightforward and quick. Installation is just like for any other dotnet tool:

```
> dotnet tool install -g dotnet-nugetize
```

After installation, you can just run `nugetize` from the project directory to quickly get a report of the package that would be generated. This is done in the fastest possible way without compromising your customizations to the build process. They way this is achieved is by a combination of a simulated [design-time build](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md) that skips the compiler invocation and avoids the output file copying entirely, and built-in support in NuGetizer to emit the entire contents of the package as MSBuild items with full metadata, that the tool can use to render an accurate report that contains exactly the same information that would be used to actually emit the final `.nupkg` without actually emitting it.

Here's a sample output screenshot:

![nugetize screenshot](img/dotnet-nugetize.png)
