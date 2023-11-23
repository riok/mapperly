using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration;

public record PropertyMappingConfiguration(StringMemberPath Source, StringMemberPath Target) : HasSyntaxReference
{
    public string? StringFormat { get; set; }

    public string? FormatProvider { get; set; }

    public TypeMappingConfiguration ToTypeMappingConfiguration() => new(StringFormat, FormatProvider);
}
