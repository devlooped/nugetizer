﻿using System;
using Devlooped;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NuGetizer;

[Generator]
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class SponsorLinker : SponsorLink
{
    public SponsorLinker() : base(SponsorLinkSettings.Create(
        "devlooped", "NuGetizer",
        version: new Version(ThisAssembly.Info.Version).ToString(2)
#if DEBUG
        , quietDays: 0
#endif
        ))
    { }
}