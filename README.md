![Nugetizer-3000 Logo](https://raw.githubusercontent.com/NuGet/NuGet.Build.Packaging/master/Nugetizer-3000.png)

# NuGetizer3000

NuGetizer 3000 is here, and will be beyond awesome!

Check out the [spec](https://github.com/NuGet/Home/wiki/NuGetizer-3000) for more details.

[![Build status](https://ci.appveyor.com/api/projects/status/7wfadgtkhcwt0wrm?svg=true)](https://ci.appveyor.com/project/MobileEssentials/nuget-build-packaging)

## Installing

Add the following NuGet feed to Visual Studio:

    https://ci.appveyor.com/nuget/nugetizer3000

Install the following package:

    Install-Package NuGet.Build.Packaging

Add [NuGet properties to your MSBuild project](https://github.com/NuGet/Home/wiki/Adding-nuget-pack-as-a-msbuild-target) and invoke the `/t:Pack` target to generated a `.nupkg` for the project.

## Issues

Please report issues on the [NuGet/Home](https://github.com/NuGet/Home/issues?q=is%3Aopen+is%3Aissue+label%3AArea%3ANuGetizer) repo, and tag with `Area:NuGetizer`.

### IDE Integration Early Preview

Install the following VSIX from the evergreen CI URL [NuGetizer 3000 for VS2017](http://bit.ly/nugetizer-2017). Alternatively, you can download the specific build artifact for specifc builds from our [AppVeyor CI](https://ci.appveyor.com/project/MobileEssentials/nuget-build-packaging)

