namespace Riok.Mapperly.Configuration.MethodReferences;

/// <summary>
/// Represents an invalid method reference configuration.
/// </summary>
/// <param name="Name">The method name.</param>
/// <param name="Target"> The target type name (if any).</param>
public record InvalidMethodReferenceConfiguration(string Name, string? Target) : IMethodReferenceConfiguration
{
    public string FullName => Target is not null ? $"{Target}.{Name}" : Name;

    public bool IsExternal => true;

    public override string ToString() => FullName;
}
