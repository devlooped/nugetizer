# Setup

Just open the available `.sln` files under `src` and build :) (see Building in Visual Studio below).

Build, test and run code coverage:

Windows:

    build.cmd

Mac:

    make

> Mac support comming as soon as MSBuild ships by default with XS/Mono on Mac


# Building in Visual Studio

There are two parts to the project:
1. The build targets and tasks
2. The NuGet package project system

## Targets and tasks

Install [Visual Studio 2015](https://www.visualstudio.com/) (preferrable also Update 3 or later).

These can be built by just opening the [NuGet.Packaging.Build.sln](src\Build\NuGet.Packaging.Build.sln) solution.

Alternatively, you can also build with the default configuration and platform from the command line with `msbuild` 
from a developer command prompt.

## NuGet Project System

The project system is built on top of the new [Visual Studio Project System](https://github.com/Microsoft/VSProjectSystem/) 
also known as "CPS" (Common Project System). 

There were API changes between CPS for Visual Studio 2015 and the upcoming `Visual Studio 15` (code name), so 
at the moment, we require installing a [Preview 4](https://www.visualstudio.com/en-us/news/releasenotes/vs15-relnotes) 
or later version. 

> NOTE: The project system is in flux at the moment, as many artifacts will move to the authoring package 
> under the `src\Build` directory.

# Contributing

1. [Open an issue here](https://github.com/NuGet/Home/issues) and get some feedback from the NuGet team.
1. Create a branch. Base it on the `dev` branch.
1. Add unit tests (to the test project in the relevant solution).
1. Make sure all tests pass (via `Test -> Run -> All Tests`, or by running `build.cmd` on Windows).
1. Create a [pull request](https://github.com/NuGet/NuGet.Build.Packaging/pulls).
1. _One-time_: Sign the contributor license agreement, if you haven't signed it before. 
   The [.NET Foundation Bot](https://github.com/dnfclas) will comment on the pull request you just 
   created and guide you on how to sign the CLA.
1. Consider submitting a doc pull request to the [nugetdocs](https://github.com/NuGet/NuGetDocs/tree/master/NuGet.Docs) 
   repo, if this is a new feature or behavior change.