using System;
using Devlooped;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NuGetizer;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class SponsorLinker : SponsorLink
{
    public SponsorLinker() : base(SponsorLinkSettings.Create(
        "devlooped", "NuGetizer",
        version: new Version(ThisAssembly.Info.Version).ToString(3),
        // Add an extra digit so the SL diagnostics have the same length as ours
        diagnosticsIdPrefix: "NG1"
#if DEBUG
        , quietDays: 0
#endif
        ))
    { }
}