using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static ThisAssembly;
using static ThisAssembly.Strings;

namespace NuGetizer;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class MetadataAnalyzer : DiagnosticAnalyzer
{
    const string Category = "NuGet";

    class Descriptors
    {
        public static readonly DiagnosticDescriptor DefaultDescription = new(
            Strings.DefaultDescription.ID,
            Strings.DefaultDescription.Title,
            Strings.DefaultDescription.Message,
            Category,
            DiagnosticSeverity.Warning,
            true,
            description: Strings.DefaultDescription.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#description");

        public static readonly DiagnosticDescriptor LongDescription = new(
            Strings.LongDescription.ID,
            Strings.LongDescription.Title,
            Strings.LongDescription.Message,
            Category,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#description");

        public static readonly DiagnosticDescriptor MissingIcon = new(
            Strings.MissingIcon.ID,
            Strings.MissingIcon.Title,
            Strings.MissingIcon.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: Strings.MissingIcon.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon");

        public static readonly DiagnosticDescriptor MissingReadme = new(
            Strings.MissingReadme.ID,
            Strings.MissingReadme.Title,
            Strings.MissingReadme.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: Strings.MissingReadme.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/NuGet/nuget-org/package-readme-on-nuget-org");

        public static readonly DiagnosticDescriptor MissingLicense = new(
            Strings.MissingLicense.ID,
            Strings.MissingLicense.Title,
            Strings.MissingLicense.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: Strings.MissingLicense.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#license");

        public static readonly DiagnosticDescriptor DuplicateLicense = new(
            Strings.DuplicateLicense.ID,
            Strings.DuplicateLicense.Title,
            Strings.DuplicateLicense.Message,
            Category,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#license");

        public static readonly DiagnosticDescriptor MissingRepositoryCommit = new(
            RepositoryCommit.ID,
            RepositoryCommit.Title,
            RepositoryCommit.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: RepositoryCommit.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink");

        public static readonly DiagnosticDescriptor MissingRepositoryUrl = new(
            RepositoryUrl.ID,
            RepositoryUrl.Title,
            RepositoryUrl.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: RepositoryUrl.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/reference/nuspec#repository");

        public static readonly DiagnosticDescriptor MissingProjectUrl = new(
            ProjectUrl.ID,
            ProjectUrl.Title,
            ProjectUrl.MessageString,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: ProjectUrl.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices#package-metadata");

        public static readonly DiagnosticDescriptor MissingSourceLink = new(
            SourceLink.ID,
            SourceLink.Title,
            SourceLink.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: SourceLink.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink");

        public static readonly DiagnosticDescriptor MissingSourceEmbed = new(
            SourceLinkEmbed.ID,
            SourceLinkEmbed.Title,
            SourceLinkEmbed.Message,
            Category,
            DiagnosticSeverity.Info,
            true,
            description: SourceLinkEmbed.Description,
            helpLinkUri: "https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink");
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.DefaultDescription,
        Descriptors.LongDescription,
        Descriptors.MissingIcon,
        Descriptors.MissingReadme,
        Descriptors.MissingLicense,
        Descriptors.DuplicateLicense,
        Descriptors.MissingRepositoryCommit,
        Descriptors.MissingRepositoryUrl,
        Descriptors.MissingProjectUrl,
        Descriptors.MissingSourceLink,
        Descriptors.MissingSourceEmbed);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationAction(ctx =>
        {
            var options = ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions;

            // If the project isn't packable, don't issue any warnings.
            if (!options.TryGetValue("build_property.PackageId", out var packageId) ||
                string.IsNullOrEmpty(packageId))
                return;

            var isPacking = options.TryGetValue("build_property.IsPacking", out var packingProp) &&
                "true".Equals(packingProp, StringComparison.OrdinalIgnoreCase);

            var sourceLinkEnabled = options.TryGetValue("build_property.EnableSourceLink", out var enableSLProp) &&
                "true".Equals(enableSLProp, StringComparison.OrdinalIgnoreCase);

            if (options.TryGetValue("build_property.Description", out var description))
            {
                if (description == DefaultDescription.DefaultValue)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.DefaultDescription, null));
                // There is really no way of getting such a long text in the diagnostic. We actually get an empty string.
                else if (description.Length > 4000)
                    ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.LongDescription, null));
            }

            string? packageIcon = default;
            string? packageIconUrl = default;

            if (!options.TryGetValue("build_property.PackageIcon", out packageIcon) &&
                !options.TryGetValue("build_property.PackageIconUrl", out packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, null));
            else if (string.IsNullOrWhiteSpace(packageIcon) && string.IsNullOrWhiteSpace(packageIconUrl))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingIcon, null));

            if (!options.TryGetValue("build_property.PackageReadmeFile", out var readme) ||
                string.IsNullOrWhiteSpace(readme))
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingReadme, null));

            string? licenseExpr = default;
            string? licenseFile = default;
            string? licenseUrl = default;

            options.TryGetValue("build_property.PackageLicenseExpression", out licenseExpr);
            options.TryGetValue("build_property.PackageLicenseFile", out licenseFile);
            options.TryGetValue("build_property.PackageLicenseUrl", out licenseUrl);

            var specified =
                (!string.IsNullOrWhiteSpace(licenseExpr) ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(licenseFile) ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(licenseUrl) ? 1 : 0);

            if (specified == 0)
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingLicense, null));

            // if two or more of the license types are specified, report
            if (specified > 1)
                ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateLicense, null));

            // Source control properties
            if (options.TryGetValue("build_property.SourceControlInformationFeatureSupported", out var sccSupported) &&
                "true".Equals(sccSupported, StringComparison.OrdinalIgnoreCase))
            {

                if (isPacking)
                {
                    string? repoCommit = default;

                    if (!options.TryGetValue("build_property.RepositoryCommit", out repoCommit) ||
                        string.IsNullOrWhiteSpace(repoCommit))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingRepositoryCommit, null));
                        repoCommit = default;
                    }

                    if (!sourceLinkEnabled)
                        ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingSourceLink, null));
                    // When packing, suggest reproducible builds by embedding untrack sources
                    else if (!options.TryGetValue("build_property.EmbedUntrackedSources", out var embedUntracked) ||
                             !"true".Equals(embedUntracked, StringComparison.OrdinalIgnoreCase))
                        ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingSourceEmbed, null));

                    if (!options.TryGetValue("build_property.RepositoryUrl", out var repoUrl) ||
                        string.IsNullOrWhiteSpace(repoUrl))
                        ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingRepositoryUrl, null));

                    if (!options.TryGetValue("build_property.PackageProjectUrl", out var projectUrl) ||
                        string.IsNullOrWhiteSpace(projectUrl) ||
                        projectUrl == repoUrl)
                        ctx.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingProjectUrl, null, repoUrl));
                }
            }
        });
    }
}
