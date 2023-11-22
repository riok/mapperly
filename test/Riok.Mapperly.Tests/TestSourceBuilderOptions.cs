using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    string MapperClassName = TestSourceBuilderOptions.DefaultMapperClassName,
    bool? UseDeepCloning = null,
    bool? UseReferenceHandling = null,
    bool? ThrowOnMappingNullMismatch = null,
    bool? ThrowOnPropertyMappingNullMismatch = null,
    bool? AllowNullPropertyAssignment = null,
    PropertyNameMappingStrategy? PropertyNameMappingStrategy = null,
    MappingConversionType? EnabledConversions = null,
    EnumMappingStrategy? EnumMappingStrategy = null,
    bool? EnumMappingIgnoreCase = null,
    IgnoreObsoleteMembersStrategy? IgnoreObsoleteMembersStrategy = null,
    RequiredMappingStrategy? RequiredMappingStrategy = null,
    MemberVisibility? IncludedMembers = null,
    bool Static = false
)
{
    public const string DefaultMapperClassName = "Mapper";

    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions AsStatic = new(Static: true);
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
    public static readonly TestSourceBuilderOptions WithReferenceHandling = new(UseReferenceHandling: true);

    public static TestSourceBuilderOptions WithIgnoreObsolete(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy) =>
        new(IgnoreObsoleteMembersStrategy: ignoreObsoleteStrategy);

    public static TestSourceBuilderOptions WithRequiredMappingStrategy(RequiredMappingStrategy requiredMappingStrategy) =>
        new(RequiredMappingStrategy: requiredMappingStrategy);

    public static TestSourceBuilderOptions WithMemberVisibility(MemberVisibility memberVisibility) =>
        new(IncludedMembers: memberVisibility);

    public static TestSourceBuilderOptions WithDisabledMappingConversion(params MappingConversionType[] conversionTypes)
    {
        var enabled = MappingConversionType.All;

        foreach (var disabledConversionType in conversionTypes)
        {
            enabled &= ~disabledConversionType;
        }

        return new(EnabledConversions: enabled);
    }
}
