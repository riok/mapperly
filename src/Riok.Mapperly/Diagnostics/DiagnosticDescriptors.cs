using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Diagnostics;

// cannot use target-typed new: https://github.com/dotnet/roslyn-analyzers/issues/5828
public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedMappingMethodSignature = new DiagnosticDescriptor(
        "RMG001",
        "Has an unsupported mapping method signature",
        "{0} has an unsupported mapping method signature",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor NoParameterlessConstructorFound = new DiagnosticDescriptor(
        "RMG002",
        "No accessible parameterless constructor found",
        "{0} has no accessible parameterless constructor",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor EnumNameMappingNoOverlappingValuesFound = new DiagnosticDescriptor(
        "RMG003",
        "No overlapping enum members found",
        "{0} and {1} don't have overlapping enum member names, mapping will therefore always result in an exception",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor IgnoredTargetMemberNotFound = new DiagnosticDescriptor(
        "RMG004",
        "Ignored target member not found",
        "Ignored target member {0} on {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor ConfiguredMappingTargetMemberNotFound = new DiagnosticDescriptor(
        "RMG005",
        "Mapping target member not found",
        "Specified member {0} on mapping target type {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ConfiguredMappingSourceMemberNotFound = new DiagnosticDescriptor(
        "RMG006",
        "Mapping source member not found",
        "Specified member {0} on source type {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CouldNotMapMember = new DiagnosticDescriptor(
        "RMG007",
        "Could not map member",
        "Could not map member {0}.{1} of type {2} to {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CouldNotCreateMapping = new DiagnosticDescriptor(
        "RMG008",
        "Could not create mapping",
        "Could not create mapping from {0} to {1}. Consider implementing the mapping manually.",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapToReadOnlyMember = new DiagnosticDescriptor(
        "RMG009",
        "Cannot map to read only member",
        "Cannot map member {0}.{1} of type {2} to read only member {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapFromWriteOnlyMember = new DiagnosticDescriptor(
        "RMG010",
        "Cannot map from write only member",
        "Cannot map from write only member {0}.{1} of type {2} to member {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapToWriteOnlyMemberPath = new DiagnosticDescriptor(
        "RMG011",
        "Cannot map to write only member path",
        "Cannot map from member {0}.{1} of type {2} to write only member path {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor SourceMemberNotFound = new DiagnosticDescriptor(
        "RMG012",
        "Source member was not found for target member",
        "The member {0} on the mapping target type {1} was not found on the mapping source type {2}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor NoConstructorFound = new DiagnosticDescriptor(
        "RMG013",
        "No accessible constructor with mappable arguments found",
        "{0} has no accessible constructor with mappable arguments",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapToConfiguredConstructor = new DiagnosticDescriptor(
        "RMG014",
        "Cannot map to the configured constructor to be used by Mapperly",
        "Cannot map from {0} to the configured constructor {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapToInitOnlyMemberPath = new DiagnosticDescriptor(
        "RMG015",
        "Cannot map to init only member path",
        "Cannot map from member {0}.{1} of type {2} to init only member path {3}.{4} of type {5}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor InitOnlyMemberDoesNotSupportPaths = new DiagnosticDescriptor(
        "RMG016",
        "Init only member cannot handle target paths",
        "Cannot map to init only member path {0}.{1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor MultipleConfigurationsForInitOnlyMember = new DiagnosticDescriptor(
        "RMG017",
        "An init only member can have one configuration at max",
        "The init only member {0}.{1} can have one configuration at max",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor PartialStaticMethodInInstanceMapper = new DiagnosticDescriptor(
        "RMG018",
        "Partial static mapping method in an instance mapper",
        "{0} is a partial static mapping method in an instance mapper. Static mapping methods are only supported in static mappers.",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor SourceMemberNotMapped = new DiagnosticDescriptor(
        "RMG020",
        "Source member is not mapped to any target member",
        "The member {0} on the mapping source type {1} is not mapped to any member on the mapping target type {2}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor IgnoredSourceMemberNotFound = new DiagnosticDescriptor(
        "RMG021",
        "Ignored source member not found",
        "Ignored source member {0} on {1} was not found",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor InvalidObjectFactorySignature = new DiagnosticDescriptor(
        "RMG022",
        "Invalid object factory signature",
        "The object factory {0} has an invalid signature",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor RequiredMemberNotMapped = new DiagnosticDescriptor(
        "RMG023",
        "Source member was not found for required target member",
        "Required member {0} on mapping target type {1} was not found on the mapping source type {2}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ReferenceHandlerParameterWrongType = new DiagnosticDescriptor(
        "RMG024",
        "The reference handler parameter is not of the correct type",
        "The reference handler parameter of {0}.{1} needs to be of type {2} but is {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ReferenceHandlingNotEnabled = new DiagnosticDescriptor(
        "RMG025",
        "To use reference handling it needs to be enabled on the mapper attribute",
        $"{{0}}.{{1}} uses reference handling, but it is not enabled on the mapper attribute, to enable reference handling set {nameof(MapperAttribute.UseReferenceHandling)} to true",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapFromIndexedMember = new DiagnosticDescriptor(
        "RMG026",
        "Cannot map from indexed member",
        "Cannot map from indexed member {0}.{1} to member {2}.{3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor MultipleConfigurationsForConstructorParameter = new DiagnosticDescriptor(
        "RMG027",
        "A constructor parameter can have one configuration at max",
        "The constructor parameter at {0}.{1} can have one configuration at max",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor ConstructorParameterDoesNotSupportPaths = new DiagnosticDescriptor(
        "RMG028",
        "Constructor parameter cannot handle target paths",
        "Cannot map to constructor parameter target path {0}.{1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor QueryableProjectionMappingsDoNotSupportReferenceHandling = new DiagnosticDescriptor(
        "RMG029",
        "Queryable projection mappings do not support reference handling",
        "Queryable projection mappings do not support reference handling",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ReferenceLoopInInitOnlyMapping = new DiagnosticDescriptor(
        "RMG030",
        "Reference loop detected while mapping to an init only member",
        "Reference loop detected while mapping from {0}.{1} to the init only member {2}.{3}, consider ignoring this member",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ReferenceLoopInCtorMapping = new DiagnosticDescriptor(
        "RMG031",
        "Reference loop detected while mapping to a constructor parameter",
        "Reference loop detected while mapping from {0}.{1} to the constructor parameter {3} of {2}, consider ignoring this member or mark another constructor as mapping constructor",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor EnumMappingNotSupportedInProjectionMappings = new DiagnosticDescriptor(
        "RMG032",
        "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings",
        "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings to map from {0} to {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor MappedObjectToObjectWithoutDeepClone = new DiagnosticDescriptor(
        "RMG033",
        "Object mapped to another object without deep clone",
        "Object mapped to another object without deep clone, consider implementing the mapping manually",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor DerivedSourceTypeDuplicated = new DiagnosticDescriptor(
        "RMG034",
        "Derived source type is specified multiple times, a source type may only be specified once",
        "Derived source type {0} is specified multiple times, a source type may only be specified once",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor DerivedSourceTypeIsNotAssignableToParameterType = new DiagnosticDescriptor(
        "RMG035",
        "Derived source type is not assignable to parameter type",
        "Derived source type {0} is not assignable to parameter type {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor DerivedTargetTypeIsNotAssignableToReturnType = new DiagnosticDescriptor(
        "RMG036",
        "Derived target type is not assignable to return type",
        "Derived target type {0} is not assignable to return type {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor TargetEnumValueNotMapped = new DiagnosticDescriptor(
        "RMG037",
        "An enum member could not be found on the source enum",
        "Enum member {0} ({1}) on {2} not found on source enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor SourceEnumValueNotMapped = new DiagnosticDescriptor(
        "RMG038",
        "An enum member could not be found on the target enum",
        "Enum member {0} ({1}) on {2} not found on target enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Info,
        true
    );

    public static readonly DiagnosticDescriptor EnumSourceValueDuplicated = new DiagnosticDescriptor(
        "RMG039",
        "Enum source value is specified multiple times, a source enum value may only be specified once",
        "Enum source value {0} is specified multiple times, a source enum value may only be specified once",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor TargetEnumValueDoesNotMatchTargetEnumType = new DiagnosticDescriptor(
        "RMG040",
        "A target enum member value does not match the target enum type",
        "Enum member {0} ({1}) on {2} does not match type of target enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor SourceEnumValueDoesNotMatchSourceEnumType = new DiagnosticDescriptor(
        "RMG041",
        "A source enum member value does not match the source enum type",
        "Enum member {0} ({1}) on {2} does not match type of source enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor EnumFallbackValueTypeDoesNotMatchTargetEnumType = new DiagnosticDescriptor(
        "RMG042",
        "The type of the enum fallback value does not match the target enum type",
        "Enum mapping fallback value {0} ({1}) on {2} does not match target enum type {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor EnumFallbackValueRequiresByValueCheckDefinedStrategy = new DiagnosticDescriptor(
        "RMG043",
        "Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy",
        "Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor IgnoredEnumSourceMemberNotFound = new DiagnosticDescriptor(
        "RMG044",
        "An ignored enum member can not be found on the source enum",
        "Ignored enum member {0} ({1}) on {2} not found on source enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor IgnoredEnumTargetMemberNotFound = new DiagnosticDescriptor(
        "RMG045",
        "An ignored enum member can not be found on the target enum",
        "Ignored enum member {0} ({1}) not found on target enum {3}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor LanguageVersionNotSupported = new DiagnosticDescriptor(
        "RMG046",
        "The used C# language version is not supported by Mapperly, Mapperly requires at least C# 9.0",
        "Mapperly does not support the C# language version {0} but requires at C# least version {1}",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor CannotMapToTemporarySourceMember = new DiagnosticDescriptor(
        "RMG047",
        "Cannot map to member path due to modifying a temporary value, see CS1612",
        "Cannot map from member {0}.{1} of type {2} to member path {3}.{4} of type {5} because {6}.{7} is a value type, returning a temporary value, see CS1612",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );
}
