namespace Riok.Mapperly.Descriptors;

/// <summary>
/// Configuration for a type mapping.
/// Eg. the format to apply to `ToString` calls.
/// </summary>
/// <param name="StringFormat">The format to apply to `ToString` calls.</param>
public record TypeMappingConfiguration(string? StringFormat = null)
{
    public static readonly TypeMappingConfiguration Default = new();
}
