using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Symbols;

internal static class MappableMember
{
    public static IMappableMember? Create(SymbolAccessor accessor, ISymbol symbol)
    {
        if (!accessor.IsAccessibleToMemberVisibility(symbol))
            return null;

        return symbol switch
        {
            IPropertySymbol property => new PropertyMember(property, accessor),
            IFieldSymbol field => new FieldMember(field),
            _ => null,
        };
    }

    public static bool CanOnlySetViaInitializer(this IMappableMember member) => member.IsInitOnly || member.IsRequired;
}
