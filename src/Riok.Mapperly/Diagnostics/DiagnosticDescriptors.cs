using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Diagnostics;

// if adjusted, these files may need adjustments as well:
// analyzer-diagnostics.md, AnalyzerReleases.Shipped.md and AnalyzerReleases.Unshipped.md
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

    public static readonly DiagnosticDescriptor IgnoredTargetPropertyNotFound = new(
        "RMG004",
        "Ignored target property not found",
        "Ignored target property {0} on {1} was not found",
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

    public static readonly DiagnosticDescriptor CannotMapToReadOnlyProperty = new(
        "RMG009",
        "Cannot map to read only property",
        "Cannot map property {0}.{1} of type {2} to read only property {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor CannotMapFromWriteOnlyProperty = new(
        "RMG010",
        "Cannot map from write only property",
        "Cannot map from write only property {0}.{1} of type {2} to property {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor CannotMapToWriteOnlyPropertyPath = new(
        "RMG011",
        "Cannot map to write only property path",
        "Cannot map from property {0}.{1} of type {2} to write only property path {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor MappingSourcePropertyNotFound = new(
        "RMG012",
        "Mapping source property not found",
        "Property {0} on source type {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor NoConstructorFound = new(
        "RMG013",
        "No accessible constructor with mappable arguments found",
        "{0} has no accessible constructor with mappable arguments",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor CannotMapToConfiguredConstructor = new(
        "RMG014",
        "Cannot map to the configured constructor to be used by Mapperly",
        "Cannot map from {0} to the configured constructor {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor CannotMapToInitOnlyPropertyPath = new(
        "RMG015",
        "Cannot map to init only property path",
        "Cannot map from property {0}.{1} of type {2} to init only property path {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor InitOnlyPropertyDoesNotSupportPaths = new(
        "RMG016",
        "Init only property cannot handle target paths",
        "Cannot map to init only property path {0}.{1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MultipleConfigurationsForInitOnlyProperty = new(
        "RMG017",
        "An init only property can have one configuration at max",
        "The init only property {0}.{1} can have one configuration at max",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor PartialStaticMethodInInstanceMapper = new(
        "RMG018",
        "Partial static mapping method in an instance mapper",
        "{0} is a partial static mapping method in an instance mapper. Static mapping methods are only supported in static mappers.",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor PartialInstanceMethodInStaticMapper = new(
        "RMG019",
        "Partial instance mapping method in a static mapper",
        "{0} is a partial instance mapping method in a static mapper. Instance mapping methods are only supported in instance (non-static) mappers.",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor SourcePropertyNotMapped = new(
        "RMG020",
        "Source property is not mapped to any target property",
        "The property {0} on the mapping source type {1} is not mapped to any property on the mapping target type {2}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor IgnoredSourcePropertyNotFound = new(
        "RMG021",
        "Ignored source property not found",
        "Ignored source property {0} on {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor InvalidObjectFactorySignature = new(
        "RMG022",
        "Invalid object factory signature",
        "The object factory {0} has an invalid signature",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true);
}
