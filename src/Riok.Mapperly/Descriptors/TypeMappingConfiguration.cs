namespace Riok.Mapperly.Descriptors;

/// <summary>
/// Configuration for a type mapping.
/// Eg. the format to apply to `ToString` calls.
/// </summary>
/// <param name="StringFormat">The format to apply to <see cref="IFormattable"/>.</param>
/// <param name="FormatProviderName">The name of the format provider to apply to <see cref="IFormattable"/>.</param>
public record TypeMappingConfiguration(string? StringFormat = null, string? FormatProviderName = null)
{
    public static readonly TypeMappingConfiguration Default = new();
}
