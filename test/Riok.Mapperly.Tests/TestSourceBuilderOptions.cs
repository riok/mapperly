using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    bool? UseDeepCloning = null,
    bool? UseReferenceHandling = null,
    bool? ThrowOnMappingNullMismatch = null,
    bool? ThrowOnPropertyMappingNullMismatch = null,
    bool? AllowNullPropertyAssignment = null,
    PropertyNameMappingStrategy? PropertyNameMappingStrategy = null,
    MappingConversionType? EnabledConversions = null,
    EnumMappingStrategy? EnumMappingStrategy = null,
    bool? EnumMappingIgnoreCase = null,
    IgnoreObsoleteMembersStrategy? IgnoreObsoleteMembersStrategy = null
)
{
    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
    public static readonly TestSourceBuilderOptions WithReferenceHandling = new(UseReferenceHandling: true);

    public static TestSourceBuilderOptions WithIgnoreObsolete(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy) =>
        new(IgnoreObsoleteMembersStrategy: ignoreObsoleteStrategy);

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
