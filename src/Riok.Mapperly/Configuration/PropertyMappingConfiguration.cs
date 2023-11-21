using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration;

public record PropertyMappingConfiguration(StringMemberPath Source, StringMemberPath Target)
{
    public string? StringFormat { get; set; }

    public TypeMappingConfiguration ToTypeMappingConfiguration() => new(StringFormat);
}
