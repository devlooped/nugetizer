# Changelog

## [v1.0.2](https://github.com/devlooped/nugetizer/tree/v1.0.2) (2023-04-19)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.1...v1.0.2)

:sparkles: Implemented enhancements:

- Add more clear descriptions to SL usage by nugetizer [\#380](https://github.com/devlooped/nugetizer/pull/380) (@kzu)
- NuGetizer should always be Pack=false [\#377](https://github.com/devlooped/nugetizer/pull/377) (@kzu)

:bug: Fixed bugs:

- Cleanup happens on every build [\#369](https://github.com/devlooped/nugetizer/issues/369)
- Bump SponsorLink to get fixes [\#381](https://github.com/devlooped/nugetizer/pull/381) (@kzu)
- Clean package cache only after packing [\#379](https://github.com/devlooped/nugetizer/pull/379) (@kzu)

:twisted_rightwards_arrows: Merged:

- ⛙ ⬆️ Bump dependencies [\#378](https://github.com/devlooped/nugetizer/pull/378) (@github-actions[bot])

## [v1.0.1](https://github.com/devlooped/nugetizer/tree/v1.0.1) (2023-03-22)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.0...v1.0.1)

:bug: Fixed bugs:

- Revert changes to make build targets transitive [\#360](https://github.com/devlooped/nugetizer/pull/360) (@kzu)

## [v1.0.0](https://github.com/devlooped/nugetizer/tree/v1.0.0) (2023-03-21)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.0-rc.1...v1.0.0)

## [v1.0.0-rc.1](https://github.com/devlooped/nugetizer/tree/v1.0.0-rc.1) (2023-03-21)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.0-rc...v1.0.0-rc.1)

:sparkles: Implemented enhancements:

- Floating version problem [\#64](https://github.com/devlooped/nugetizer/issues/64)
- Ensure private assets always for NuGetizer [\#355](https://github.com/devlooped/nugetizer/pull/355) (@kzu)
- Never pack NuGetizer transitively [\#354](https://github.com/devlooped/nugetizer/pull/354) (@kzu)
- Make NuGetizer targets buildTransitive to fix SponsorLink [\#353](https://github.com/devlooped/nugetizer/pull/353) (@kzu)
- Resolve wildcard dependency just like central package versions [\#351](https://github.com/devlooped/nugetizer/pull/351) (@kzu)
- Automatically pack dependencies satellite resources for private packages [\#342](https://github.com/devlooped/nugetizer/pull/342) (@kzu)
- Improve support for recursive dir inclusion [\#341](https://github.com/devlooped/nugetizer/pull/341) (@kzu)
- In nugetize tool, skip Authors metadata if it equals AssemblyName [\#339](https://github.com/devlooped/nugetizer/pull/339) (@kzu)
- Add warning when Authors == AssemblyName [\#338](https://github.com/devlooped/nugetizer/pull/338) (@kzu)
- Set diagnostics category to NuGet [\#337](https://github.com/devlooped/nugetizer/pull/337) (@kzu)
- For nugetize tool, consider default description as empty [\#336](https://github.com/devlooped/nugetizer/pull/336) (@kzu)
- Add best-practice analyzers for nuget packaging [\#334](https://github.com/devlooped/nugetizer/pull/334) (@kzu)
- Implement PackageIcon automatic packing [\#332](https://github.com/devlooped/nugetizer/pull/332) (@kzu)
- Add compatibility with SDK package validation [\#331](https://github.com/devlooped/nugetizer/pull/331) (@kzu)

:bug: Fixed bugs:

- Never infer implicit reference for existing direct reference [\#352](https://github.com/devlooped/nugetizer/pull/352) (@kzu)
- Don't include Description as compiler metadata unconditionally [\#344](https://github.com/devlooped/nugetizer/pull/344) (@kzu)

:twisted_rightwards_arrows: Merged:

- Address minor style issues, bump SponsorLink [\#347](https://github.com/devlooped/nugetizer/pull/347) (@kzu)

## [v1.0.0-rc](https://github.com/devlooped/nugetizer/tree/v1.0.0-rc) (2023-02-26)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.0-beta...v1.0.0-rc)

:sparkles: Implemented enhancements:

- Allow running nugetizer on any major .NET version [\#330](https://github.com/devlooped/nugetizer/pull/330) (@kzu)
- Fix nugetize tool for solutions that use project dependencies [\#329](https://github.com/devlooped/nugetizer/pull/329) (@kzu)
- Render include/exclude attributes in dependencies [\#328](https://github.com/devlooped/nugetizer/pull/328) (@kzu)
- Fix link for nuspec by using full path in actual link [\#327](https://github.com/devlooped/nugetizer/pull/327) (@kzu)

## [v1.0.0-beta](https://github.com/devlooped/nugetizer/tree/v1.0.0-beta) (2023-02-25)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v1.0.0-alpha...v1.0.0-beta)

## [v1.0.0-alpha](https://github.com/devlooped/nugetizer/tree/v1.0.0-alpha) (2023-02-25)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.5...v1.0.0-alpha)

## [v0.9.5](https://github.com/devlooped/nugetizer/tree/v0.9.5) (2023-02-25)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.4...v0.9.5)

:sparkles: Implemented enhancements:

- Pack console apps using publish output by default [\#324](https://github.com/devlooped/nugetizer/pull/324) (@kzu)
- Allow extending the build with local-only targets [\#323](https://github.com/devlooped/nugetizer/pull/323) (@kzu)
- Ensure transitive dependencies are properly resolved for inference [\#322](https://github.com/devlooped/nugetizer/pull/322) (@kzu)
- Render nuspec as a relative path to the nugetize output [\#321](https://github.com/devlooped/nugetizer/pull/321) (@kzu)

## [v0.9.4](https://github.com/devlooped/nugetizer/tree/v0.9.4) (2023-02-22)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.3...v0.9.4)

:sparkles: Implemented enhancements:

- Change nugetize default to non-verbose [\#313](https://github.com/devlooped/nugetizer/pull/313) (@kzu)
- Add SponsorLink to the nugetize CLI [\#309](https://github.com/devlooped/nugetizer/pull/309) (@kzu)
- Improve package rendering in dotnet-nugetize [\#307](https://github.com/devlooped/nugetizer/pull/307) (@kzu)
- Improve the handling of platform-suffixed target frameworks [\#306](https://github.com/devlooped/nugetizer/pull/306) (@kzu)

:twisted_rightwards_arrows: Merged:

- Devcontainer and docs experimentation [\#314](https://github.com/devlooped/nugetizer/pull/314) (@kzu)
- ⛙ ⬆️ Bump dependencies [\#311](https://github.com/devlooped/nugetizer/pull/311) (@github-actions[bot])

## [v0.9.3](https://github.com/devlooped/nugetizer/tree/v0.9.3) (2023-02-18)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.2...v0.9.3)

:sparkles: Implemented enhancements:

- 💜 Bump SponsorLink for better privacy [\#303](https://github.com/devlooped/nugetizer/pull/303) (@kzu)

:bug: Fixed bugs:

- Fix support for platform-suffixed build output [\#288](https://github.com/devlooped/nugetizer/pull/288) (@kzu)

:twisted_rightwards_arrows: Merged:

- Initial docfx boilerplate for a docs site [\#289](https://github.com/devlooped/nugetizer/pull/289) (@kzu)

## [v0.9.2](https://github.com/devlooped/nugetizer/tree/v0.9.2) (2023-02-11)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.1...v0.9.2)

:sparkles: Implemented enhancements:

- F\# projects are not supported [\#191](https://github.com/devlooped/nugetizer/issues/191)
- Document support for packaging projects [\#55](https://github.com/devlooped/nugetizer/issues/55)
- Only surface build and analyzer from SponsorLink [\#286](https://github.com/devlooped/nugetizer/pull/286) (@kzu)
- Support Include/Exclude attributes from Dependency [\#285](https://github.com/devlooped/nugetizer/pull/285) (@kzu)
- Add SponsorLink to ensure ongoing development and maintenance [\#284](https://github.com/devlooped/nugetizer/pull/284) (@kzu)
- Pack runtime lib assets when inferring dependencies [\#282](https://github.com/devlooped/nugetizer/pull/282) (@kzu)
- PackageFile should be hidden from solution explorer [\#281](https://github.com/devlooped/nugetizer/pull/281) (@kzu)
- Ensure package metadata is populated before GetAssemblyVersion [\#280](https://github.com/devlooped/nugetizer/pull/280) (@kzu)
- Add NuGetPackageId metadata to target path item [\#273](https://github.com/devlooped/nugetizer/pull/273) (@kzu)

:bug: Fixed bugs:

- NuGetizer v0.9.1 packing reference assembly instead of real assembly [\#263](https://github.com/devlooped/nugetizer/issues/263)

:twisted_rightwards_arrows: Merged:

- Add missing ItemGroup to MSBuild snipped [\#269](https://github.com/devlooped/nugetizer/pull/269) (@ap0llo)

## [v0.9.1](https://github.com/devlooped/nugetizer/tree/v0.9.1) (2022-11-16)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.9.0...v0.9.1)

:sparkles: Implemented enhancements:

- When include in readme fails, log a warning [\#232](https://github.com/devlooped/nugetizer/issues/232)

:bug: Fixed bugs:

- NuGetizer is incompatible with SDK Pack DevelopmentDependency property [\#253](https://github.com/devlooped/nugetizer/issues/253)

:hammer: Other:

- Allow nugetize \(non-build\) tool for dotnet-tool projects [\#247](https://github.com/devlooped/nugetizer/issues/247)

:twisted_rightwards_arrows: Merged:

- ⬆️ Bump files with dotnet-file sync [\#258](https://github.com/devlooped/nugetizer/pull/258) (@kzu)
- Bump ThisAssembly [\#257](https://github.com/devlooped/nugetizer/pull/257) (@kzu)
- Remove redundant files from upstream [\#256](https://github.com/devlooped/nugetizer/pull/256) (@kzu)
- Make IsDevelopmentDependency legacy [\#254](https://github.com/devlooped/nugetizer/pull/254) (@kzu)
- Don't fail nugetize for tools [\#248](https://github.com/devlooped/nugetizer/pull/248) (@kzu)
- When include in readme fails, log a warning [\#233](https://github.com/devlooped/nugetizer/pull/233) (@kzu)

## [v0.9.0](https://github.com/devlooped/nugetizer/tree/v0.9.0) (2022-09-03)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.8.0...v0.9.0)

:sparkles: Implemented enhancements:

- Allow including content in the package readme from a URL [\#216](https://github.com/devlooped/nugetizer/issues/216)
- Support markdown includes in package readme files [\#210](https://github.com/devlooped/nugetizer/issues/210)
- Support transitive package dependency from P2P [\#202](https://github.com/devlooped/nugetizer/pull/202) (@kzu)

:bug: Fixed bugs:

- If readme isn't already a @\(None\) item, it's not automatically added to package [\#213](https://github.com/devlooped/nugetizer/issues/213)
- Support transitive package dependency of referenced \(non-packing\) project [\#199](https://github.com/devlooped/nugetizer/issues/199)
- Packaging when using Nerdbank.Gitversioning fails [\#198](https://github.com/devlooped/nugetizer/issues/198)
- Yellow triangle on Nugetizer nuget package reference  under project  dependencies -\> packages  [\#196](https://github.com/devlooped/nugetizer/issues/196)
- running nugetize fails - unable to find framework [\#189](https://github.com/devlooped/nugetizer/issues/189)

:twisted_rightwards_arrows: Merged:

- Generate and upload binlogs, don't version test build/run [\#230](https://github.com/devlooped/nugetizer/pull/230) (@kzu)
- Latest agents with .net48 fail to run tests due to missing method [\#227](https://github.com/devlooped/nugetizer/pull/227) (@kzu)
- Package readme improvements [\#214](https://github.com/devlooped/nugetizer/pull/214) (@kzu)
- +M▼ includes [\#204](https://github.com/devlooped/nugetizer/pull/204) (@github-actions[bot])

## [v0.8.0](https://github.com/devlooped/nugetizer/tree/v0.8.0) (2022-06-01)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.8.0-beta...v0.8.0)

## [v0.8.0-beta](https://github.com/devlooped/nugetizer/tree/v0.8.0-beta) (2022-06-01)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.6...v0.8.0-beta)

:sparkles: Implemented enhancements:

- Improve readme.md content inclusion inference [\#185](https://github.com/devlooped/nugetizer/issues/185)
- Make it easier to create build packages by improving inference heuristics [\#184](https://github.com/devlooped/nugetizer/issues/184)
- Allow packaging projects to be framework-specific [\#182](https://github.com/devlooped/nugetizer/issues/182)
- Add support for PackFolder=buildTransitive [\#179](https://github.com/devlooped/nugetizer/issues/179)

:bug: Fixed bugs:

- Packaging project generates package improperly [\#139](https://github.com/devlooped/nugetizer/issues/139)
- Improve compatibility with SDK Pack for tools [\#134](https://github.com/devlooped/nugetizer/issues/134)
- SDK Pack always packs build as framework-specific [\#133](https://github.com/devlooped/nugetizer/issues/133)

:twisted_rightwards_arrows: Merged:

- Improve compatibility with SDK Pack for tools [\#188](https://github.com/devlooped/nugetizer/pull/188) (@kzu)
- Improve readme content inclusion metadata inference [\#187](https://github.com/devlooped/nugetizer/pull/187) (@kzu)
- Improve default heuristics for build projects [\#186](https://github.com/devlooped/nugetizer/pull/186) (@kzu)
- Allow packaging projects to be framework-specific [\#183](https://github.com/devlooped/nugetizer/pull/183) (@kzu)
- Be smarter about PackFolder on project references [\#181](https://github.com/devlooped/nugetizer/pull/181) (@kzu)
- Add support for buildTransitive pack folder kind [\#180](https://github.com/devlooped/nugetizer/pull/180) (@kzu)

## [v0.7.6](https://github.com/devlooped/nugetizer/tree/v0.7.6) (2022-05-26)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.5...v0.7.6)

:bug: Fixed bugs:

- Packing with Microsoft.Build.NoTargets doesn't put dependency under proper target framework group when reference other Microsoft.Build.NoTargets project [\#155](https://github.com/devlooped/nugetizer/issues/155)
- Cannot use PackageRequireLicenseAcceptance = true with Microsoft.Build.NoTargets sdk [\#149](https://github.com/devlooped/nugetizer/issues/149)
- Do BuildAction and CopyToOutput work? [\#143](https://github.com/devlooped/nugetizer/issues/143)

:twisted_rightwards_arrows: Merged:

- Bump test dependencies [\#177](https://github.com/devlooped/nugetizer/pull/177) (@kzu)
- Allow specifying that packaging projects are framework specific [\#176](https://github.com/devlooped/nugetizer/pull/176) (@kzu)
- Fix contentFiles relative path in manifest [\#175](https://github.com/devlooped/nugetizer/pull/175) (@kzu)
- A few fixes for cleanup targets [\#166](https://github.com/devlooped/nugetizer/pull/166) (@gpwen)
- Fixed a few typos in the readme [\#161](https://github.com/devlooped/nugetizer/pull/161) (@AntonC9018)

## [v0.7.5](https://github.com/devlooped/nugetizer/tree/v0.7.5) (2021-10-13)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.7.4...v0.7.5)

:sparkles: Implemented enhancements:

- Normalize all paths for consistency, use SDK Pack format [\#135](https://github.com/devlooped/nugetizer/issues/135)
- PackAsTool from SDK Pack should cause PackFolder=tool [\#131](https://github.com/devlooped/nugetizer/issues/131)

:bug: Fixed bugs:

- SDK Pack always packs tools as framework-specific [\#132](https://github.com/devlooped/nugetizer/issues/132)
- dotnet.exe nugetize My.sln errors if one project doesn't PackageReference Nugetizer [\#107](https://github.com/devlooped/nugetizer/issues/107)
- Fixes using of license file. [\#150](https://github.com/devlooped/nugetizer/pull/150) (@denjmpr)

:twisted_rightwards_arrows: Merged:

- Minor improvements and improved compatibility with SDK pack  [\#137](https://github.com/devlooped/nugetizer/pull/137) (@kzu)

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
- Include readme in package [\#127](https://github.com/devlooped/nugetizer/pull/127) (@kzu)

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

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.10...v0.4.12)

:hammer: Other:

- IsPackable default [\#26](https://github.com/devlooped/nugetizer/issues/26)

## [v0.4.10](https://github.com/devlooped/nugetizer/tree/v0.4.10) (2020-10-29)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.11...v0.4.10)

## [v0.4.11](https://github.com/devlooped/nugetizer/tree/v0.4.11) (2020-10-29)

[Full Changelog](https://github.com/devlooped/nugetizer/compare/v0.4.9...v0.4.11)

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
