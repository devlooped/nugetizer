# Overview

> [!TIP]
> Run this example directly in your browser by navigating to the 
> `docs/scenarios/overview` folder and running `nugetize` in a 
> [![GitHub Codespaces](https://img.shields.io/badge/-GitHub%20codespace-black?logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=docs&repo=297430130&machine=basicLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json)

This example showcases the most basic usage of NuGetizer. 
It creates a NuGet package from a single project and uses some of the 
most common packaging metadata properties (which are fully compatible 
with the [SDK pack ones](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#pack-target))

Run `dotnet restore` from this directory, and then run the `nugetize` 
[tool](https://nuget.org/packages/dotnet-nugetizer) to see the package 
layout that will be produced.

For the following project file:

[!code-xml[](Quickstart.csproj)]

The following package contents will be produced:

![nugetize output](~/img/scenarios/overview/quickstart.png)


Highlights from the project file:

1. The standard [SDK pack](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#pack-target) 
   MSBuild properties are used to specify the package metadata:

[!code-xml[](Quickstart.csproj#L8-L9)]




