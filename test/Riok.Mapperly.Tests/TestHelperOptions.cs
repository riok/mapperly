using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Riok.Mapperly.Tests;

public record TestHelperOptions(
    NullableContextOptions NullableOption = NullableContextOptions.Enable,
    LanguageVersion LanguageVersion = LanguageVersion.Default,
    IReadOnlySet<DiagnosticSeverity>? AllowedDiagnostics = null)
{
    public static readonly TestHelperOptions Default = new();
}
