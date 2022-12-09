using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    bool UseDeepCloning = false,
    bool ThrowOnMappingNullMismatch = true,
    bool ThrowOnPropertyMappingNullMismatch = false,
    PropertyNameMappingStrategy PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseSensitive)
{
    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
}
