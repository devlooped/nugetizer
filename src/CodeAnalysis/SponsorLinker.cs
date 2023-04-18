using System;
using System.Collections.Immutable;
using System.Linq;
using Devlooped;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NuGetizer;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class SponsorLinker : SponsorLink
{
    static readonly SponsorLinkSettings settings;

    static SponsorLinker()
    {
        settings = SponsorLinkSettings.Create(
                "devlooped", "NuGetizer",
                version: new Version(ThisAssembly.Info.Version).ToString(3),
                // Add an extra digit so the SL diagnostics have the same length as ours
                diagnosticsIdPrefix: "NG1"
#if DEBUG
                , quietDays: 0
#endif
                );

        settings.SupportedDiagnostics = settings.SupportedDiagnostics
            .Select(x => x.IsKind(DiagnosticKind.AppNotInstalled) ?
                x.With(description: "Your package users will NOT have a dependency on SponsorLink OR NuGetizer. This is about *you* helping out the NuGetizer project itself. Thanks!") :
                x)
            .Select(x => x.IsKind(DiagnosticKind.UserNotSponsoring) ?
                x.With(description: "Your sponsorship is used to further develop NuGetizer and make it great for the entire oss community!") :
                x)
            .ToImmutableArray();
    }

    public SponsorLinker() : base(settings) { }
}