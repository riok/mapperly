using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Riok.Mapperly.Tests;

public record TestHelperOptions(
    NullableContextOptions NullableOption = NullableContextOptions.Enable,
    LanguageVersion LanguageVersion = LanguageVersion.Default,
    IReadOnlySet<DiagnosticSeverity>? AllowedDiagnostics = null)
{
    public static readonly TestHelperOptions AllowDiagnostics = new();

    public static readonly TestHelperOptions DisabledNullable = AllowDiagnostics with { NullableOption = NullableContextOptions.Disable };

    public static readonly TestHelperOptions NoDiagnostics = AllowDiagnostics with
    {
        AllowedDiagnostics = new HashSet<DiagnosticSeverity>(),
    };

    public static readonly TestHelperOptions AllowAllDiagnostics = AllowDiagnostics with
    {
        AllowedDiagnostics = Enum.GetValues<DiagnosticSeverity>().ToHashSet(),
    };

    public static readonly TestHelperOptions AllowInfoDiagnostics = AllowDiagnostics with
    {
        AllowedDiagnostics = new HashSet<DiagnosticSeverity>
        {
            DiagnosticSeverity.Hidden,
            DiagnosticSeverity.Info
        }
    };
}
