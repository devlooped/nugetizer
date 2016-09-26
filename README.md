# NuGetizer3000

NuGetizer 3000 is here, and will be beyond awesome!

Check out the [spec](https://github.com/NuGet/Home/wiki/NuGetizer-3000) for more details.

## Installing

Add the following NuGet feed to Visual Studio:

    https://ci.appveyor.com/nuget/nugetizer

Install the following package:

    Install-Package NuGet.Build.Packaging

Add [NuGet properties to your MSBuild project](https://github.com/NuGet/Home/wiki/Adding-nuget-pack-as-a-msbuild-target) 
) and invoke the `/t:Pack` target to generated a `.nupkg` for the project.