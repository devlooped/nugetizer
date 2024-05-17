using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[Generator(LanguageNames.CSharp)]
public class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider,
            (c, t) => CheckAndReport(t.GlobalOptions, c.ReportDiagnostic));

    void CheckAndReport(AnalyzerConfigOptions globalOptions, Action<Diagnostic> reportDiagnostic)
    {
        reportDiagnostic(Diagnostic.Create("LIB001", "Compiler", "Hi", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1));
    }
}
