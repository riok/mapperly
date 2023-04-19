using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

internal static class MappableMember
{
    public static IMappableMember? Create(ISymbol symbol)
    {
        return symbol switch
        {
            IPropertySymbol property => new PropertyMember(property),
            IFieldSymbol field => new FieldMember(field),
            _ => null,
        };
    }

    public static bool CanOnlySetViaInitializer(this IMappableMember member) => member.IsInitOnly || member.IsRequired;
}
