namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    bool UseDeepCloning = false,
    bool ThrowOnMappingNullMismatch = true,
    bool ThrowOnPropertyMappingNullMismatch = false)
{
    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
}
