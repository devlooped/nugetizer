using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static ThisAssembly;
using static ThisAssembly.Strings;
using System.Diagnostics;

namespace NuGetizer;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class MetadataAnalyzer : DiagnosticAnalyzer
{
    class Descriptors
    {
        public static readonly DiagnosticDescriptor DefaultDescription = new(
            Strings.DefaultDescription.ID,
            Strings.DefaultDescription.Title,
            Strings.DefaultDescription.Message,
            "Design",
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#description");

        public static readonly DiagnosticDescriptor LongDescription = new(
            Strings.LongDescription.ID,
            Strings.LongDescription.Title,
            Strings.LongDescription.Message,
            "Design",
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#description");


        public static readonly DiagnosticDescriptor MissingIcon = new(
            Strings.MissingIcon.ID,
            Strings.MissingIcon.Title,
            Strings.MissingIcon.Message,
            "Design",
            DiagnosticSeverity.Info,
            true,
            description: Strings.MissingIcon.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon");

        public static readonly DiagnosticDescriptor MissingReadme = new(
            Strings.MissingReadme.ID,
            Strings.MissingReadme.Title,
            Strings.MissingReadme.Message,
            "Design",
            DiagnosticSeverity.Info,
            true,
            description: Strings.MissingReadme.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/NuGet/nuget-org/package-readme-on-nuget-org");
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.DefaultDescription,
        Descriptors.LongDescription, 
        Descriptors.MissingIcon, 
        Descriptors.MissingReadme);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationAction(ctx =>
        {
            var options = ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions;

            Debugger.Launch();

            // If the project isn't packable, don't issue any warnings.
            if (!options.TryGetValue("build_property.PackageId", out var packageId) ||
                string.IsNullOrEmpty(packageId))
                return;

            //var location = Location.Create(projectPath, new TextSpan(), new LinePositionSpan());

            if (ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.Description", out var description))
            {
                if (description == DefaultDescription.DefaultValue)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.DefaultDescription, null));
                // There is really no way of getting such a long text in the diagnostic. We actually get an empty string.
                else if (description.Length > 4000)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.LongDescription, null));
            }

            string? packageIcon = default;
            string? packageIconUrl = default;

            if (!ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageIcon", out packageIcon) &&
                !ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageIconUrl", out packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, null));
            else if (string.IsNullOrWhiteSpace(packageIcon) && string.IsNullOrWhiteSpace(packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, null));

            if (!ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageReadmeFile", out var readme) || 
                string.IsNullOrWhiteSpace(readme))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingReadme, null));
        });
    }
}
