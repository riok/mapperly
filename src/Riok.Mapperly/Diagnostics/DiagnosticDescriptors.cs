using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedMappingMethodSignature =
        new(
            "RMG001",
            "Has an unsupported mapping method signature",
            "{0} has an unsupported mapping method signature",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor NoParameterlessConstructorFound =
        new(
            "RMG002",
            "No accessible parameterless constructor found",
            "{0} has no accessible parameterless constructor",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor EnumNameMappingNoOverlappingValuesFound =
        new(
            "RMG003",
            "No overlapping enum members found",
            "{0} and {1} don't have overlapping enum member names, mapping will therefore always result in an exception",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredTargetMemberNotFound =
        new(
            "RMG004",
            "Ignored target member not found",
            "Ignored target member {0} on {1} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor ConfiguredMappingTargetMemberNotFound =
        new(
            "RMG005",
            "Mapping target member not found",
            "Specified member {0} on mapping target type {1} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ConfiguredMappingSourceMemberNotFound =
        new(
            "RMG006",
            "Mapping source member not found",
            "Specified member {0} on source type {1} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CouldNotMapMember =
        new(
            "RMG007",
            "Could not map member",
            "Could not map member {0} to {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CouldNotCreateMapping =
        new(
            "RMG008",
            "Could not create mapping",
            "Could not create mapping from {0} to {1}. Consider implementing the mapping manually.",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapToReadOnlyMember =
        new(
            "RMG009",
            "Cannot map to read only member",
            "Cannot map {0} to read only member {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapFromWriteOnlyMember =
        new(
            "RMG010",
            "Cannot map from write only member",
            "Cannot map from write only member {0} to member {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapToWriteOnlyMemberPath =
        new(
            "RMG011",
            "Cannot map to write only member path",
            "Cannot map from {0} to write only member path {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor SourceMemberNotFound =
        new(
            "RMG012",
            "Source member was not found for target member",
            "The member {0} on the mapping target type {1} was not found on the mapping source type {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor NoConstructorFound =
        new(
            "RMG013",
            "No accessible constructor with mappable arguments found",
            "{0} has no accessible constructor with mappable arguments",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapToConfiguredConstructor =
        new(
            "RMG014",
            "Cannot map to the configured constructor to be used by Mapperly",
            "Cannot map from {0} to the configured constructor {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapToInitOnlyMemberPath =
        new(
            "RMG015",
            "Cannot map to init only member path",
            "Cannot map from {0} to init only member path {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor InitOnlyMemberDoesNotSupportPaths =
        new(
            "RMG016",
            "Init only member cannot handle target paths",
            "Cannot map to init only member path {0}.{1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor SourceMemberNotMapped =
        new(
            "RMG020",
            "Source member is not mapped to any target member",
            "The member {0} on the mapping source type {1} is not mapped to any member on the mapping target type {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredSourceMemberNotFound =
        new(
            "RMG021",
            "Ignored source member not found",
            "Ignored source member {0} on {1} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor InvalidObjectFactorySignature =
        new(
            "RMG022",
            "Invalid object factory signature",
            "The object factory {0} has an invalid signature",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor RequiredMemberNotMapped =
        new(
            "RMG023",
            "Source member was not found for required target member",
            "Required member {0} on mapping target type {1} was not found on the mapping source type {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ReferenceHandlerParameterWrongType =
        new(
            "RMG024",
            "The reference handler parameter is not of the correct type",
            "The reference handler parameter of {0}.{1} needs to be of type {2} but is {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ReferenceHandlingNotEnabled =
        new(
            "RMG025",
            "To use reference handling it needs to be enabled on the mapper attribute",
            $"{{0}}.{{1}} uses reference handling, but it is not enabled on the mapper attribute, to enable reference handling set {nameof(MapperAttribute.UseReferenceHandling)} to true",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor QueryableProjectionMappingsDoNotSupportReferenceHandling =
        new(
            "RMG029",
            "Queryable projection mappings do not support reference handling",
            "Queryable projection mappings do not support reference handling",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ReferenceLoopInInitOnlyMapping =
        new(
            "RMG030",
            "Reference loop detected while mapping to an init only member",
            "Reference loop detected while mapping from {0} to the init only member {1}, consider ignoring this member",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ReferenceLoopInCtorMapping =
        new(
            "RMG031",
            "Reference loop detected while mapping to a constructor parameter",
            "Reference loop detected while mapping from {0} to the constructor parameter {2} of {1}, consider ignoring this member or mark another constructor as mapping constructor",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor EnumMappingNotSupportedInProjectionMappings =
        new(
            "RMG032",
            "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings",
            "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings to map from {0} to {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor MappedObjectToObjectWithoutDeepClone =
        new(
            "RMG033",
            "Object mapped to another object without deep clone",
            "Object mapped to another object without deep clone, consider implementing the mapping manually",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor DerivedSourceTypeDuplicated =
        new(
            "RMG034",
            "Derived source type is specified multiple times, a source type may only be specified once",
            "Derived source type {0} is specified multiple times, a source type may only be specified once",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor DerivedSourceTypeIsNotAssignableToParameterType =
        new(
            "RMG035",
            "Derived source type is not assignable to parameter type",
            "Derived source type {0} is not assignable to parameter type {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor DerivedTargetTypeIsNotAssignableToReturnType =
        new(
            "RMG036",
            "Derived target type is not assignable to return type",
            "Derived target type {0} is not assignable to return type {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor TargetEnumValueNotMapped =
        new(
            "RMG037",
            "An enum member could not be found on the source enum",
            "Enum member {0} ({1}) on {2} not found on source enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor SourceEnumValueNotMapped =
        new(
            "RMG038",
            "An enum member could not be found on the target enum",
            "Enum member {0} ({1}) on {2} not found on target enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor EnumSourceValueDuplicated =
        new(
            "RMG039",
            "Enum source value is specified multiple times, a source enum value may only be specified once",
            "Enum source value {0} is specified multiple times, a source enum value may only be specified once",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor TargetEnumValueDoesNotMatchTargetEnumType =
        new(
            "RMG040",
            "A target enum member value does not match the target enum type",
            "Enum member {0} ({1}) on {2} does not match type of target enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor SourceEnumValueDoesNotMatchSourceEnumType =
        new(
            "RMG041",
            "A source enum member value does not match the source enum type",
            "Enum member {0} ({1}) on {2} does not match type of source enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor EnumFallbackValueTypeDoesNotMatchTargetEnumType =
        new(
            "RMG042",
            "The type of the enum fallback value does not match the target enum type",
            "Enum mapping fallback value {0} ({1}) on {2} does not match target enum type {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor EnumFallbackValueRequiresByValueCheckDefinedStrategy =
        new(
            "RMG043",
            "Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy",
            "Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredEnumSourceMemberNotFound =
        new(
            "RMG044",
            "An ignored enum member can not be found on the source enum",
            "Ignored enum member {0} ({1}) on {2} not found on source enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredEnumTargetMemberNotFound =
        new(
            "RMG045",
            "An ignored enum member can not be found on the target enum",
            "Ignored enum member {0} ({1}) not found on target enum {3}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor LanguageVersionNotSupported =
        new(
            "RMG046",
            "The used C# language version is not supported by Mapperly, Mapperly requires at least C# 9.0",
            "Mapperly does not support the C# language version {0} but requires at C# least version {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapToTemporarySourceMember =
        new(
            "RMG047",
            "Cannot map to member path due to modifying a temporary value, see CS1612",
            "Cannot map from member {0} to member path {1} because {2}.{3} is a value type, returning a temporary value, see CS1612",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ExternalMapperMemberCannotBeNullable =
        new(
            "RMG048",
            "Used mapper members cannot be nullable",
            "The used mapper member {0} cannot be nullable",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredSourceMemberExplicitlyMapped =
        new(
            "RMG049",
            "Source member is ignored and also explicitly mapped",
            "The source member {0} on {1} is ignored, but is also mapped explicitly",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor IgnoredTargetMemberExplicitlyMapped =
        new(
            "RMG050",
            "Target member is ignored and also explicitly mapped",
            "The target member {0} on {1} is ignored, but is also mapped explicitly",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor NestedIgnoredSourceMember =
        new(
            "RMG051",
            "Invalid ignore source member found, nested ignores are not supported",
            "Invalid ignore source member {0} found for type {1}, nested ignores are not supported",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor NestedIgnoredTargetMember =
        new(
            "RMG052",
            "Invalid ignore target member found, nested ignores are not supported",
            "Invalid ignore target member {0} found for type {1}, nested ignores are not supported",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor UnsafeAccessorNotAvailable =
        new(
            "RMG053",
            $"The flag {nameof(MemberVisibility)}.{nameof(MemberVisibility.Accessible)} cannot be disabled, this feature requires .NET 8.0 or greater",
            $"The flag {nameof(MemberVisibility)}.{nameof(MemberVisibility.Accessible)} cannot be disabled, this feature requires .NET 8.0 or greater",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MixingStaticPartialWithInstanceMethod =
        new(
            "RMG054",
            "Mapper class containing 'static partial' method must not have any instance methods",
            "Mapper class {0} contains 'static partial' methods. Use either only instance methods or only static methods.",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor SourceDoesNotImplementToStringWithFormatParameters =
        new(
            "RMG055",
            $"The source type does not implement {nameof(ToString)} with the provided formatting parameters, string format and format provider cannot be applied",
            $"The source type {{0}} does not implement {nameof(ToString)} with the provided formatting parameters, string format and format provider cannot be applied",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor InvalidFormatProviderSignature =
        new(
            "RMG056",
            "Invalid format provider signature",
            "The format provider {0} has an invalid signature",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor FormatProviderNotFound =
        new(
            "RMG057",
            "Format provider not found",
            $"The format provider {{0}} could not be found, make sure it is annotated with {nameof(FormatProviderAttribute)}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MultipleDefaultFormatProviders =
        new(
            "RMG058",
            "Multiple default format providers found, only one is allowed",
            "Multiple default format providers found, only one is allowed",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MultipleDefaultUserMappings =
        new(
            "RMG059",
            "Multiple default user mappings found, only one is allowed",
            "Multiple default user mappings for the mapping from {0} to {1} found, only one is allowed",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MultipleUserMappingsWithoutDefault =
        new(
            "RMG060",
            "Multiple user mappings discovered without specifying an explicit default",
            "Multiple user mappings discovered for the mapping from {0} to {1} without specifying an explicit default",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: BuildHelpUri("RMG060")
        );

    public static readonly DiagnosticDescriptor ReferencedMappingNotFound =
        new(
            "RMG061",
            "The referenced mapping was not found",
            "The referenced mapping named {0} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor ReferencedMappingAmbiguous =
        new(
            "RMG062",
            "The referenced mapping name is ambiguous",
            "The referenced mapping name {0} is ambiguous, use a unique name",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor EnumConfigurationOnNonEnumMapping =
        new(
            "RMG063",
            "Cannot configure an enum mapping on a non-enum mapping",
            "Cannot configure an enum mapping on a non-enum mapping",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MemberConfigurationOnNonMemberMapping =
        new(
            "RMG064",
            "Cannot configure an object mapping on a non-object mapping",
            "Cannot configure an object mapping on a non-object mapping",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MemberConfigurationOnQueryableProjectionMapping =
        new(
            "RMG065",
            "Cannot configure an object mapping on a queryable projection mapping, apply the configurations to an object mapping method instead",
            "Cannot configure an object mapping on a queryable projection mapping, apply the configurations to an object mapping method instead",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor NoMemberMappings =
        new(
            "RMG066",
            "No members are mapped in an object mapping",
            "No members are mapped in the object mapping from {0} to {1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: BuildHelpUri("RMG066")
        );

    public static readonly DiagnosticDescriptor InvalidMapPropertyAttributeUsage =
        new(
            "RMG067",
            "Invalid usage of the " + nameof(MapPropertyAttribute),
            "Invalid usage of the " + nameof(MapPropertyAttribute),
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor QueryableProjectionMappingCannotInline =
        new(
            "RMG068",
            "Cannot inline user implemented queryable expression mapping",
            "Cannot inline user implemented queryable expression mapping",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Info,
            true
        );

    public static readonly DiagnosticDescriptor RuntimeTargetTypeMappingNoContentMappings =
        new(
            "RMG069",
            "Runtime target type or generic type mapping does not match any mappings",
            "Runtime target type or generic type mapping does not match any mappings",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor ConfiguredMappingNestedMemberNotFound =
        new(
            "RMG070",
            "Mapping nested member not found",
            "Specified nested member {0} on source type {1} was not found",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor NestedMemberNotUsed =
        new(
            "RMG071",
            "Nested properties mapping is not used",
            "Configured nested member {0} on source type {1} is not used",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor ReferencedMappingSourceTypeMismatch =
        new(
            "RMG072",
            "The source type of the referenced mapping does not match",
            "The source type {1} of the referenced mapping {0} does not match the expected type {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor ReferencedMappingTargetTypeMismatch =
        new(
            "RMG073",
            "The target type of the referenced mapping does not match",
            "The target type {1} of the referenced mapping {0} does not match the expected type {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor MultipleConfigurationsForTargetMember =
        new(
            "RMG074",
            "Multiple mappings are configured for the same target member",
            "Multiple mappings are configured for the same target member {0}.{1}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor InvalidMapValueAttributeUsage =
        new(
            "RMG075",
            "Invalid usage of the " + nameof(MapValueAttribute),
            "Invalid usage of the " + nameof(MapValueAttribute),
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor CannotMapValueNullToNonNullable =
        new(
            "RMG076",
            "Cannot assign null to non-nullable member",
            "Cannot assign null to non-nullable member {0}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Warning,
            true
        );

    public static readonly DiagnosticDescriptor MapValueTypeMismatch =
        new(
            "RMG077",
            "Cannot assign constant value because the type of the value does not match the type of the target",
            "Cannot assign constant value {0} of type {1} to {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MapValueMethodTypeMismatch =
        new(
            "RMG078",
            "Cannot assign method return type because the type of the value does not match the type of the target",
            "Cannot assign method return type {1} of {0}() to {2}",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MapValueReferencedMethodNotFound =
        new(
            "RMG079",
            "The referenced method could not be found or has an unsupported signature",
            "The referenced method {0} could not be found or has an unsupported signature",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    public static readonly DiagnosticDescriptor MapValueUnsupportedType =
        new(
            "RMG080",
            $"The {nameof(MapValueAttribute)} does not support types and arrays",
            $"The {nameof(MapValueAttribute)} does not support types and arrays",
            DiagnosticCategories.Mapper,
            DiagnosticSeverity.Error,
            true
        );

    private static string BuildHelpUri(string id)
    {
#if ENV_NEXT
        var host = "next.mapperly.riok.app";
#elif ENV_LOCAL
        var host = "localhost:3000";
#else
        var host = "mapperly.riok.app";
#endif

        return $"https://{host}/docs/configuration/analyzer-diagnostics/{id}";
    }
}
