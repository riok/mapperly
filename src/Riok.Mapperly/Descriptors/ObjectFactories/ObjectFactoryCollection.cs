using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class ObjectFactoryCollection(IReadOnlyCollection<ObjectFactory> objectFactories)
{
    private readonly Dictionary<ITypeSymbol, ObjectFactory> _concreteObjectFactories = new(SymbolEqualityComparer.IncludeNullability);

    public bool TryFindObjectFactory(ITypeSymbol sourceType, ITypeSymbol targetType, [NotNullWhen(true)] out ObjectFactory? objectFactory)
    {
        if (_concreteObjectFactories.TryGetValue(targetType, out objectFactory))
            return true;

        objectFactory = objectFactories.FirstOrDefault(f => f.CanCreateType(sourceType, targetType));
        if (objectFactory == null)
            return false;

        _concreteObjectFactories[targetType] = objectFactory;
        return true;
    }
}
