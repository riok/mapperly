using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class ObjectFactoryCollection(IReadOnlyCollection<ObjectFactory> objectFactories)
{
    private readonly Dictionary<TypeMappingKey, ObjectFactory> _concreteObjectFactories = new();

    public bool TryFindObjectFactory(ITypeSymbol sourceType, ITypeSymbol targetType, [NotNullWhen(true)] out ObjectFactory? objectFactory)
    {
        var key = new TypeMappingKey(sourceType, targetType);
        if (_concreteObjectFactories.TryGetValue(key, out objectFactory))
            return true;

        objectFactory = objectFactories.FirstOrDefault(f => f.CanCreateInstanceOfType(sourceType, targetType));
        if (objectFactory == null)
            return false;

        _concreteObjectFactories[key] = objectFactory;
        return true;
    }
}
