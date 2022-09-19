using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class ObjectFactoryCollection
{
    public static readonly ObjectFactoryCollection Empty = new(Array.Empty<ObjectFactory>());

    private readonly IReadOnlyCollection<ObjectFactory> _objectFactories;
    private readonly Dictionary<ITypeSymbol, ObjectFactory> _concreteObjectFactories = new(SymbolEqualityComparer.IncludeNullability);

    public ObjectFactoryCollection(IReadOnlyCollection<ObjectFactory> objectFactories)
    {
        _objectFactories = objectFactories;
    }

    public bool TryFindObjectFactory(ITypeSymbol typeToCreate, [NotNullWhen(true)] out ObjectFactory? objectFactory)
    {
        if (_concreteObjectFactories.TryGetValue(typeToCreate, out objectFactory))
            return true;

        objectFactory = _objectFactories.FirstOrDefault(f => f.CanCreateType(typeToCreate));
        if (objectFactory == null)
            return false;

        _concreteObjectFactories[typeToCreate] = objectFactory;
        return true;
    }
}
