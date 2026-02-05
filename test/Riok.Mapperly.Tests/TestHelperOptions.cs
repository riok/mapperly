using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests;

/// <param name="AllowedDiagnosticSeverities">
/// The severities of diagnostics which are allowed without an exception being thrown.
/// If <c>null</c>, all severities are allowed.
/// </param>
/// <param name="IgnoredDiagnostics">Diagnostics which are completely ignored.</param>
public record TestHelperOptions(
    NullableContextOptions NullableOption = NullableContextOptions.Enable,
    LanguageVersion LanguageVersion = LanguageVersion.Default,
    IReadOnlySet<DiagnosticSeverity>? AllowedDiagnosticSeverities = null,
    IReadOnlySet<DiagnosticDescriptor>? IgnoredDiagnostics = null,
    string AssemblyName = "Tests",
    string GeneratedTreeFileName = $"{TestSourceBuilderOptions.DefaultMapperClassName}.g.cs",
    IReadOnlyDictionary<string, string>? AnalyzerConfigOptions = null
)
{
    public static readonly TestHelperOptions Default = new(
        AllowedDiagnosticSeverities: new HashSet<DiagnosticSeverity>(),
        IgnoredDiagnostics: new HashSet<DiagnosticDescriptor>
        {
            // ignore NoMemberMappings as a lot of tests use this for simplicity
            DiagnosticDescriptors.NoMemberMappings,
        }
    );

    public static readonly TestHelperOptions DisabledNullable = Default with { NullableOption = NullableContextOptions.Disable };

    public static readonly TestHelperOptions AllowDiagnostics = Default with { AllowedDiagnosticSeverities = null };

    /// <summary>
    /// Includes all ignored diagnostics.
    /// </summary>
    public static readonly TestHelperOptions AllowAndIncludeAllDiagnostics = AllowDiagnostics with { IgnoredDiagnostics = null };

    public static readonly TestHelperOptions AllowInfoDiagnostics = Default with
    {
        AllowedDiagnosticSeverities = new HashSet<DiagnosticSeverity> { DiagnosticSeverity.Hidden, DiagnosticSeverity.Info },
    };
}
