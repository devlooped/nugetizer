![Nugetizer-3000 Logo](https://raw.githubusercontent.com/NuGet/NuGet.Build.Packaging/master/Nugetizer-3000.png)

# NuGetizer3000

NuGetizer 3000 is here, and will be beyond awesome!

Check out the [spec](https://github.com/NuGet/Home/wiki/NuGetizer-3000) for more details.

## Installing

Add the following NuGet feed to Visual Studio:

    https://ci.appveyor.com/nuget/nugetizer3000

Install the following package:

    Install-Package NuGet.Build.Packaging

Add [NuGet properties to your MSBuild project](https://github.com/NuGet/Home/wiki/Adding-nuget-pack-as-a-msbuild-target) 
) and invoke the `/t:Pack` target to generated a `.nupkg` for the project.

### IDE Early Preview

Install the following VSIX from the evergreen CI URL: [http://bit.ly/nugetizer3000](http://bit.ly/nugetizer3000)