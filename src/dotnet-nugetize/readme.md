The `nugetize` dotnet global tool (available after installation) allows quick iteration while creating nuget packages. This is done in the fastest possible way without compromising your customizations to the build process, all without incurring a full build/pack!

Carefully tweaking your packages until they look exactly the way you want them should not be a tedious and slow process. Even requiring your project to be built between changes can be costly and reduce the speed at which you can iterate on the packaging aspects of the project. Also, generating the final `.nupkg`, opening it in a tool and inspecting its content, is also not ideal for rapid iteration.

This tool works together with [NuGetizer](https://nuget.org/packages/NuGetizer) to provide the most productive solution for your packing needs. This is achieved is by a combination of a simulated [design-time build](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md) that skips the compiler invocation and avoids the output file copying entirely, and built-in support in NuGetizer to emit the entire contents of the package as MSBuild items with full metadata, that the tool can use to render an accurate report that contains exactly the same information that would be used to emit the final `.nupkg` without actually generating it.

The following is the output of a comprehensive [smart multi-targeted library](https://www.cazzulino.com/smart-libraries.html#packaging) example [from the documentation](https://github.com/devlooped/nugetizer/tree/docs/docs/scenarios/quickstart/packaging-complete):

![nugetize smart library](https://raw.githubusercontent.com/devlooped/nugetizer/main/img/complete.png)

The tool will emit the `.nuspec` file so you can inspect precisely what's being packed and from where, should you need to dig deeper. Moreover, it's rendered as a helpful link (in most terminals) so you can click on it directly to open (i.e. from within the VSCode terminal).

NuGetizer supports running `nugetize` on all the samples in the [documentation](https://www.clarius.org/nugetizer/) directly in your browser in a [![GitHub Codespace](https://img.shields.io/badge/-GitHub%20Codespace-black?logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=docs&repo=297430130&machine=basicLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json)

As soon as the codespace is running, just use the terminal to go to the desired sample directory and just run `nugetize`! You can even open a `pwsh` terminal if that's more your thing.

> NOTE: `dotnet-nugetize` does not support projects that don't reference the [NuGetizer](https://nuget.org/packages/NuGetizer) package.

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
<!-- exclude -->