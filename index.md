# Quickstart

NuGetizer is a drop-in replacement for the .NET SDK built-in Pack (a.k.a. "SDK Pack") 
which instantly supercharges your ability to customize and extend the packing process 
in a consistent and easy to understand process designed and centered around best 
practices in MSBuild design and extensibility.

Yes, this means you'll never need to write a `.nuspec` by hand ever again, no matter 
how complicated or advanced your packing scenarios are.

Since packing is an exercise in getting the resulting .nupkg to contain exactly what 
you expect it to contain, *NuGetizer* provides a .NET global tool that allows you 
to quickly and iteratively test your packing scenarios, without incurring in lengthy 
builds, zipping and subsequent unzipping of the .nupkg for each attempt and so on. 
Install the tool like any other .NET global tool with:

```
dotnet tool install -g dotnet-nugetizer
```

Afterwards, from a directory containing your solution or project, just run `nugetize`. This 
will provide a quick render of the package metadata, dependencies and contents, 
such as:

![nugetize output](img/scenarios/overview/quickstart.png)

> [!TIP]
> For added convenience in exploring **NuGetizer**'s capabilities, you can run all 
the examples in this documentation directly in your browser in a new 
[![GitHub Codespace](https://img.shields.io/badge/-GitHub%20Codespace-black?logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=docs&repo=297430130&machine=basicLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json)

## It's all just PackageFile

At its core, NuGetizer just packs any `<PackageFile>` in your project. These items can declare the following metadata to affect the resulting location of the file in the package:

// markdown table with metadata name and description, for PackagePath. PackFolder and FrameworkSpecific:

| Metadata | Description |
|----------|-------------|
| `PackagePath` | The relative path of the file in the package. |
| `PackFolder` | The known folder in the package where the file will be placed. |
| `FrameworkSpecific` | Whether the file is framework-specific. |

When `PackagePath` is provided, it determines the precise location of the file in the package and the other attributes are ignored. 

When `PackFolder` is provided (i.e. `lib` or `build`), the `FrameworkSpecific` metadata complements it by optionally appending the project target framework automatically.

For example, given the following project file:



