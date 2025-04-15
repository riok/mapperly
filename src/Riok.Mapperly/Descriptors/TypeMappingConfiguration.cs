namespace Riok.Mapperly.Descriptors;

/// <summary>
/// Configuration for a type mapping.
/// Eg. the format to apply to `ToString` calls.
/// </summary>
/// <param name="StringFormat">The format to apply to <see cref="IFormattable"/>.</param>
/// <param name="FormatProviderName">The name of the format provider to apply to <see cref="IFormattable"/>.</param>
/// <param name="UseNamedMapping">The name of the mapping to use to convert the source type to the target type.</param>
public record TypeMappingConfiguration(
    string? StringFormat = null,
    string? FormatProviderName = null,
    string? UseNamedMapping = null,
    bool SuppressNullMismatchDiagnostic = false
)
{
    public static readonly TypeMappingConfiguration Default = new();
}
