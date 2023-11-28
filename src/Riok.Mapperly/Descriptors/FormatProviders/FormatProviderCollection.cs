namespace Riok.Mapperly.Descriptors.FormatProviders;

public class FormatProviderCollection(
    IReadOnlyDictionary<string, FormatProvider> formatProvidersByName,
    FormatProvider? defaultFormatProvider
)
{
    public (FormatProvider? formatProvider, bool isDefault) Get(string? reference)
    {
        return reference == null ? (defaultFormatProvider, true) : (formatProvidersByName.GetValueOrDefault(reference), false);
    }
}
