using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    bool UseDeepCloning = false,
    bool UseReferenceHandling = false,
    bool ThrowOnMappingNullMismatch = true,
    bool ThrowOnPropertyMappingNullMismatch = false,
    PropertyNameMappingStrategy PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseSensitive,
    MappingConversionType EnabledConversions = MappingConversionType.All,
    EnumMappingStrategy EnumMappingStrategy = EnumMappingStrategy.ByValue,
	bool EnumMappingIgnoreCase = false
)
{
    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
    public static readonly TestSourceBuilderOptions WithReferenceHandling = new(UseReferenceHandling: true);

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
