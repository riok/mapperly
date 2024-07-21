using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Symbols.Members;

internal static class MappableMember
{
    public static IMappableMember? Create(SymbolAccessor accessor, ISymbol symbol)
    {
        if (!accessor.IsAccessible(symbol) || !symbol.CanBeReferencedByName)
            return null;

        return symbol switch
        {
            IPropertySymbol property => new PropertyMember(property, accessor),
            IFieldSymbol field => new FieldMember(field, accessor),
            _ => null,
        };
    }

    public static bool CanOnlySetViaInitializer(this IMappableMember member) => member.IsInitOnly || member.IsRequired;
}
