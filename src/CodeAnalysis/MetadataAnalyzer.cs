using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static ThisAssembly;
using static ThisAssembly.Strings;

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
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon");
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.DefaultDescription,
        Descriptors.LongDescription, 
        Descriptors.MissingIcon);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationAction(ctx =>
        {
            var options = ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions;

            // If the project isn't packable, don't issue any warnings.
            if (!options.TryGetValue("build_property.IsPackable", out var packable) ||
                !bool.TryParse(packable, out var isPackable) ||
                !isPackable)
                return;

            if (ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.Description", out var description))
            {
                System.Diagnostics.Debugger.Launch();

                if (description == DefaultDescription.DefaultValue)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.DefaultDescription, Location.None));
                // There is really no way of getting such a long text in the diagnostic. We actually get an empty string.
                else if (description.Length > 4000)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.LongDescription, Location.None));
            }

            string? packageIcon = default;
            string? packageIconUrl = default;

            if (!ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageIcon", out packageIcon) &&
                !ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageIconUrl", out packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, Location.None));
            else if (string.IsNullOrEmpty(packageIcon) && string.IsNullOrEmpty(packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, Location.None));
        });
    }
}
