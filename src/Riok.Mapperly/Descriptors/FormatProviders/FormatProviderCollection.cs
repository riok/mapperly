namespace Riok.Mapperly.Descriptors.FormatProviders;

public class FormatProviderCollection(
    IReadOnlyDictionary<string, FormatProvider> formatProvidersByName,
    FormatProvider? defaultFormatProvider
)
{
    public FormatProvider? Get(string? reference)
    {
        return reference == null ? defaultFormatProvider : formatProvidersByName.GetValueOrDefault(reference);
    }
}
