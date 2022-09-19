using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Riok.Mapperly.Tests;

public record TestHelperOptions(
    NullableContextOptions NullableOption = NullableContextOptions.Enable,
    LanguageVersion LanguageVersion = LanguageVersion.Default,
    IReadOnlySet<DiagnosticSeverity>? AllowedDiagnostics = null)
{
    public static readonly TestHelperOptions Default = new();

    public static readonly TestHelperOptions DisabledNullable = Default with { NullableOption = NullableContextOptions.Disable };

    public static readonly TestHelperOptions NoDiagnostics = Default with
    {
        AllowedDiagnostics = new HashSet<DiagnosticSeverity>(),
    };

    public static readonly TestHelperOptions AllowAllDiagnostics = Default with
    {
        AllowedDiagnostics = Enum.GetValues<DiagnosticSeverity>().ToHashSet(),
    };

    public static readonly TestHelperOptions AllowInfoDiagnostics = Default with
    {
        AllowedDiagnostics = new HashSet<DiagnosticSeverity>
        {
            DiagnosticSeverity.Hidden,
            DiagnosticSeverity.Info
        }
    };
}
