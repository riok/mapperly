namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(bool AsInterface = true, string? Namespace = null)
{
    public static readonly TestSourceBuilderOptions Default = new();
}
