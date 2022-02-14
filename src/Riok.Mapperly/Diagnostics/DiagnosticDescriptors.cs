using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedMappingMethodSignature = new(
        "RMG001",
        "Has an unsupported mapping method signature",
        "{0} has an unsupported mapping method signature",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor NoParameterlessConstructorFound = new(
        "RMG002",
        "No accessible parameterless constructor found",
        "{0} has no accessible parameterless constructor",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor EnumNameMappingNoOverlappingValuesFound = new(
        "RMG003",
        "No overlapping enum members found",
        "{0} and {1} don't have overlapping enum member names, mapping will therefore always result in an exception",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor IgnoredPropertyNotFound = new(
        "RMG004",
        "Ignored property not found",
        "Ignored property {0} on {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor ConfiguredMappingTargetPropertyNotFound = new(
        "RMG005",
        "Mapping target property not found",
        "Specified property {0} on mapping target type {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ConfiguredMappingSourcePropertyNotFound = new(
        "RMG006",
        "Mapping source property not found",
        "Specified property {0} on source type {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor CouldNotMapProperty = new(
        "RMG007",
        "Could not map property",
        "Could not map property {0}.{1} of type {2} to {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor CouldNotCreateMapping = new(
        "RMG008",
        "Could not create mapping",
        "Could not create mapping from {0} to {1}. Consider implementing the mapping manually.",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);
}
