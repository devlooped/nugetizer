# Showcases smart library packing

Packs and consumes a library containing:

1. Library.csproj: main API for the package packed to `lib/netstandard2.0`
1. Analyzer.csproj: sample analyzer/generator lib packed to `analyzers/dotnet/roslyn4.0` to showcase Roslyn 4.0+ API requirement
1. Tasks.csproj: containing sample targets and tasks packed to `buildTransitive`

