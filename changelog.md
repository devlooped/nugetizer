

## [v0.7.4](https://github.com/devlooped/nugetizer/tree/v0.7.4) (2021-07-20)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.3...v0.7.4)

:sparkles: Implemented enhancements:

- Skip non-nugetized projects when nugetizer runs for solution [\#130](https://github.com/devlooped/nugetizer/pull/130) (@kzu)

## [v0.7.3](https://github.com/devlooped/nugetizer/tree/v0.7.3) (2021-07-19)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.2...v0.7.3)

:sparkles: Implemented enhancements:

- Include readme in package [\#126](https://github.com/devlooped/nugetizer/issues/126)

:bug: Fixed bugs:

- When inferring content, PackInference.PackExclude isn't properly evaluated [\#128](https://github.com/devlooped/nugetizer/issues/128)

:twisted_rightwards_arrows: Merged:

- Properly evaluate PackInference.PackExclude [\#129](https://github.com/devlooped/nugetizer/pull/129) (@kzu)

## [v0.7.2](https://github.com/devlooped/nugetizer/tree/v0.7.2) (2021-07-16)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.1...v0.7.2)

:sparkles: Implemented enhancements:

- Allow specifying PackFolder in ProjectReference [\#124](https://github.com/devlooped/nugetizer/issues/124)

:twisted_rightwards_arrows: Merged:

- Allow specifying PackFolder in ProjectReference [\#125](https://github.com/devlooped/nugetizer/pull/125) (@kzu)

## [v0.7.1](https://github.com/devlooped/nugetizer/tree/v0.7.1) (2021-06-17)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.0...v0.7.1)

:sparkles: Implemented enhancements:

- Hide --debug option from nugetize tool [\#108](https://github.com/devlooped/nugetizer/issues/108)
- Parity: add support for PackageReadmeFile property [\#85](https://github.com/devlooped/nugetizer/issues/85)

:twisted_rightwards_arrows: Merged:

- Add support for PackageReadmeFile [\#111](https://github.com/devlooped/nugetizer/pull/111) (@kzu)

## [v0.7.0](https://github.com/devlooped/nugetizer/tree/v0.7.0) (2021-05-10)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.6.2...v0.7.0)

:sparkles: Implemented enhancements:

- If short sha is available, use it for package metadata [\#94](https://github.com/devlooped/nugetizer/issues/94)
- Automatically default PackageProjectUrl to RepositoryUrl from source control info [\#91](https://github.com/devlooped/nugetizer/issues/91)
- Rename PackProjectReference to PackProjectReferences for consistency [\#90](https://github.com/devlooped/nugetizer/issues/90)
- Populatate RepositoryBranch, ensure we pack it as expected [\#57](https://github.com/devlooped/nugetizer/issues/57)
- Populate RepositoryUrl/RepositoryCommit early [\#56](https://github.com/devlooped/nugetizer/issues/56)

:twisted_rightwards_arrows: Merged:

- If short SHA is available, use it for package metadata [\#95](https://github.com/devlooped/nugetizer/pull/95) (@kzu)
- Populate RepositoryBranch automatically [\#93](https://github.com/devlooped/nugetizer/pull/93) (@kzu)
- Improve integration with source link [\#92](https://github.com/devlooped/nugetizer/pull/92) (@kzu)
- Bump files with dotnet-file sync [\#86](https://github.com/devlooped/nugetizer/pull/86) (@kzu)

## [v0.6.2](https://github.com/devlooped/nugetizer/tree/v0.6.2) (2021-03-30)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.6.0...v0.6.2)

:sparkles: Implemented enhancements:

- Add PackDependencies property to opt-out of dependencies packing easily [\#73](https://github.com/devlooped/nugetizer/issues/73)
- Add support for SuppressDependenciesWhenPacking for compatibility with SDK pack [\#67](https://github.com/devlooped/nugetizer/issues/67)
- Include readme in package [\#127](https://github.com/devlooped/nugetizer/pull/127) (@kzu)
- Add support for opting out of dependencies packing [\#74](https://github.com/devlooped/nugetizer/pull/74) (@kzu)

:twisted_rightwards_arrows: Merged:

- :package: Fix packing on build and release [\#72](https://github.com/devlooped/nugetizer/pull/72) (@kzu)
- 🖆 Apply kzu/oss template via dotnet-file [\#71](https://github.com/devlooped/nugetizer/pull/71) (@kzu)
- Minor CI and dependencies bumps [\#70](https://github.com/devlooped/nugetizer/pull/70) (@kzu)
- 🖆 Apply kzu/oss template via dotnet-file [\#69](https://github.com/devlooped/nugetizer/pull/69) (@kzu)
- :wastebasket: Add support for compatibility SuppressDependenciesWhenPacking property [\#68](https://github.com/devlooped/nugetizer/pull/68) (@kzu)

## [v0.6.0](https://github.com/devlooped/nugetizer/tree/v0.6.0) (2020-12-10)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.5.0...v0.6.0)

:sparkles: Implemented enhancements:

- Allow opting out of transitive private assets package reference packing [\#37](https://github.com/devlooped/nugetizer/issues/37)
- When packing transitive dependencies for PrivateAssets=all, pack lib, not ref [\#36](https://github.com/devlooped/nugetizer/issues/36)
- Add more common GetTargetPath to multi-targeting [\#43](https://github.com/devlooped/nugetizer/pull/43) (@kzu)

:bug: Fixed bugs:

- Multitargeting =\> No auto nuget package creation [\#34](https://github.com/devlooped/nugetizer/issues/34)
- Multitarget Issues [\#27](https://github.com/devlooped/nugetizer/issues/27)

:hammer: Other:

- When using PackageReference item metadata, automatically infer Pack=true [\#44](https://github.com/devlooped/nugetizer/issues/44)
- Provide a default value for package description for compatibility with SDK pack [\#40](https://github.com/devlooped/nugetizer/issues/40)
- Enable GeneratePathProperty by default [\#35](https://github.com/devlooped/nugetizer/issues/35)
- Pack issues with multi targeting [\#31](https://github.com/devlooped/nugetizer/issues/31)

:twisted_rightwards_arrows: Merged:

- 🖆 Apply kzu/oss template via dotnet-file [\#47](https://github.com/devlooped/nugetizer/pull/47) (@kzu)
- When using PackageReference metadata infer Pack=true [\#45](https://github.com/devlooped/nugetizer/pull/45) (@kzu)
- When resolving P2P references for packaging, set target framework [\#42](https://github.com/devlooped/nugetizer/pull/42) (@kzu)
- Provide a default value for package description for compatibility with SDK pack [\#41](https://github.com/devlooped/nugetizer/pull/41) (@kzu)
- Import compatibility props for multi-targeting too [\#39](https://github.com/devlooped/nugetizer/pull/39) (@kzu)
- Dev \> Main [\#38](https://github.com/devlooped/nugetizer/pull/38) (@kzu)

## [v0.5.0](https://github.com/devlooped/nugetizer/tree/v0.5.0) (2020-11-25)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.12...v0.5.0)

:sparkles: Implemented enhancements:

- Allow packing content from referenced packages [\#30](https://github.com/devlooped/nugetizer/issues/30)

:hammer: Other:

- When using PackOnBuild in multitargeting project, only one TFM is packed [\#32](https://github.com/devlooped/nugetizer/issues/32)
- When package reference is direct but also indirect, pack fails to include it [\#28](https://github.com/devlooped/nugetizer/issues/28)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#33](https://github.com/devlooped/nugetizer/pull/33) (@kzu)
- Dev \> Main [\#29](https://github.com/devlooped/nugetizer/pull/29) (@kzu)
- Make sure the output package cleanup happens before Pack [\#25](https://github.com/devlooped/nugetizer/pull/25) (@kzu)

## [v0.4.12](https://github.com/devlooped/nugetizer/tree/v0.4.12) (2020-11-20)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.11...v0.4.12)

:hammer: Other:

- IsPackable default [\#26](https://github.com/devlooped/nugetizer/issues/26)

## [v0.4.11](https://github.com/devlooped/nugetizer/tree/v0.4.11) (2020-10-29)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.10...v0.4.11)

## [v0.4.10](https://github.com/devlooped/nugetizer/tree/v0.4.10) (2020-10-29)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.9...v0.4.10)

:bug: Fixed bugs:

- Not Respecting "Generate NuGet package on build" [\#22](https://github.com/devlooped/nugetizer/issues/22)

:twisted_rightwards_arrows: Merged:

- Improvements on transitive package content with relative paths [\#24](https://github.com/devlooped/nugetizer/pull/24) (@kzu)
- Minor improvements to dotnet-nugetize [\#23](https://github.com/devlooped/nugetizer/pull/23) (@kzu)
- TargetPath and Compile pack improvements [\#21](https://github.com/devlooped/nugetizer/pull/21) (@kzu)
- Inner devloop improvements [\#18](https://github.com/devlooped/nugetizer/pull/18) (@kzu)

## [v0.4.9](https://github.com/devlooped/nugetizer/tree/v0.4.9) (2020-10-26)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.8...v0.4.9)

## [v0.4.8](https://github.com/devlooped/nugetizer/tree/v0.4.8) (2020-10-25)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.7...v0.4.8)

## [v0.4.7](https://github.com/devlooped/nugetizer/tree/v0.4.7) (2020-10-21)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.6...v0.4.7)

:twisted_rightwards_arrows: Merged:

- If an item provides TargetPath, preserve it as relative to the PackFolder [\#20](https://github.com/devlooped/nugetizer/pull/20) (@kzu)
- By default, packing Compile should not include generated files [\#19](https://github.com/devlooped/nugetizer/pull/19) (@kzu)

## [v0.4.6](https://github.com/devlooped/nugetizer/tree/v0.4.6) (2020-10-20)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.5...v0.4.6)

:twisted_rightwards_arrows: Merged:

- Add automatic cleanups to improve the inner devloop [\#17](https://github.com/devlooped/nugetizer/pull/17) (@kzu)

## [v0.4.5](https://github.com/devlooped/nugetizer/tree/v0.4.5) (2020-10-08)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.4...v0.4.5)

:twisted_rightwards_arrows: Merged:

- Add support for Microsoft.Build.NoTargets SDK [\#16](https://github.com/devlooped/nugetizer/pull/16) (@kzu)

## [v0.4.4](https://github.com/devlooped/nugetizer/tree/v0.4.4) (2020-10-08)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.3...v0.4.4)

:twisted_rightwards_arrows: Merged:

- Ensure we have at least a dependency when testing package license [\#15](https://github.com/devlooped/nugetizer/pull/15) (@kzu)
- Make sure Release configuration is used in all steps [\#14](https://github.com/devlooped/nugetizer/pull/14) (@kzu)
- Dev \> Main [\#13](https://github.com/devlooped/nugetizer/pull/13) (@kzu)
- Fix wrong curly brace in helper from \#region [\#12](https://github.com/devlooped/nugetizer/pull/12) (@kzu)
- Dev \> Main [\#11](https://github.com/devlooped/nugetizer/pull/11) (@kzu)
- Centrally managed package versions support [\#10](https://github.com/devlooped/nugetizer/pull/10) (@kzu)

## [v0.4.3](https://github.com/devlooped/nugetizer/tree/v0.4.3) (2020-10-04)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.2...v0.4.3)

## [v0.4.2](https://github.com/devlooped/nugetizer/tree/v0.4.2) (2020-10-04)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.1...v0.4.2)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#9](https://github.com/devlooped/nugetizer/pull/9) (@kzu)

## [v0.4.1](https://github.com/devlooped/nugetizer/tree/v0.4.1) (2020-10-02)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.0...v0.4.1)

:twisted_rightwards_arrows: Merged:

- Make sure we never get duplicate NuspecFile items [\#8](https://github.com/devlooped/nugetizer/pull/8) (@kzu)

## [v0.4.0](https://github.com/devlooped/nugetizer/tree/v0.4.0) (2020-10-01)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.3.0...v0.4.0)

:twisted_rightwards_arrows: Merged:

- Rename Kind to PackFolder [\#7](https://github.com/devlooped/nugetizer/pull/7) (@kzu)

## [v0.3.0](https://github.com/devlooped/nugetizer/tree/v0.3.0) (2020-10-01)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.3.0-rc...v0.3.0)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#6](https://github.com/devlooped/nugetizer/pull/6) (@kzu)

## [v0.3.0-rc](https://github.com/devlooped/nugetizer/tree/v0.3.0-rc) (2020-09-30)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.3.0-beta...v0.3.0-rc)

:hammer: Other:

- Add PackageFolder support as project property [\#1](https://github.com/devlooped/nugetizer/issues/1)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#5](https://github.com/devlooped/nugetizer/pull/5) (@kzu)

## [v0.3.0-beta](https://github.com/devlooped/nugetizer/tree/v0.3.0-beta) (2020-09-30)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.3.0-alpha...v0.3.0-beta)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#4](https://github.com/devlooped/nugetizer/pull/4) (@kzu)

## [v0.3.0-alpha](https://github.com/devlooped/nugetizer/tree/v0.3.0-alpha) (2020-09-29)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/a12adfed1f05c215b10cac40ee5f2465771c785d...v0.3.0-alpha)

:hammer: Other:

- Add RepositoryBranch/RepositoryCommit support [\#2](https://github.com/devlooped/nugetizer/issues/2)

:twisted_rightwards_arrows: Merged:

- Dev \> Main [\#3](https://github.com/devlooped/nugetizer/pull/3) (@kzu)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
