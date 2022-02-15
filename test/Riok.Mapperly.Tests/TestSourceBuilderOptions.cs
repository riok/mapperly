namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    bool AsInterface = true,
    string? Namespace = null,
    bool UseDeepCloning = false)
{
    public static readonly TestSourceBuilderOptions Default = new();
}
