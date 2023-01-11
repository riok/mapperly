using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Abstractions.ReferenceHandling.Internal;

namespace Riok.Mapperly.Descriptors;

public class WellKnownTypes
{
    private readonly Compilation _compilation;

    private INamedTypeSymbol? _referenceHandlerAttribute;
    private INamedTypeSymbol? _objectFactoryAttribute;
    private INamedTypeSymbol? _mapperConstructorAttribute;

    private INamedTypeSymbol? _obsoleteAttribute;

    private INamedTypeSymbol? _iReferenceHandler;
    private INamedTypeSymbol? _preserveReferenceHandler;

    private INamedTypeSymbol? _iDictionary;
    private INamedTypeSymbol? _iReadOnlyDictionary;
    private INamedTypeSymbol? _iEnumerable;
    private INamedTypeSymbol? _enumerable;
    private INamedTypeSymbol? _iCollection;
    private INamedTypeSymbol? _iReadOnlyCollection;
    private INamedTypeSymbol? _keyValuePair;
    private INamedTypeSymbol? _dictionary;

    internal WellKnownTypes(Compilation compilation)
    {
        _compilation = compilation;
    }

    public INamedTypeSymbol ReferenceHandlerAttribute => _referenceHandlerAttribute ??= GetTypeSymbol(typeof(ReferenceHandlerAttribute));
    public INamedTypeSymbol ObjectFactoryAttribute => _objectFactoryAttribute ??= GetTypeSymbol(typeof(ObjectFactoryAttribute));
    public INamedTypeSymbol MapperConstructorAttribute => _mapperConstructorAttribute ??= GetTypeSymbol(typeof(MapperConstructorAttribute));
    public INamedTypeSymbol ObsoleteAttribute => _obsoleteAttribute ??= GetTypeSymbol(typeof(ObsoleteAttribute));
    public INamedTypeSymbol IReferenceHandler => _iReferenceHandler ??= GetTypeSymbol(typeof(IReferenceHandler));
    public INamedTypeSymbol PreserveReferenceHandler => _preserveReferenceHandler ??= GetTypeSymbol(typeof(PreserveReferenceHandler));
    public INamedTypeSymbol IDictionary => _iDictionary ??= GetTypeSymbol(typeof(IDictionary<,>));
    public INamedTypeSymbol IReadOnlyDictionary => _iReadOnlyDictionary ??= GetTypeSymbol(typeof(IReadOnlyDictionary<,>));
    public INamedTypeSymbol IEnumerable => _iEnumerable ??= GetTypeSymbol(typeof(IEnumerable<>));
    public INamedTypeSymbol Enumerable => _enumerable ??= GetTypeSymbol(typeof(Enumerable));
    public INamedTypeSymbol ICollection => _iCollection ??= GetTypeSymbol(typeof(ICollection<>));
    public INamedTypeSymbol IReadOnlyCollection => _iReadOnlyCollection ??= GetTypeSymbol(typeof(IReadOnlyCollection<>));
    public INamedTypeSymbol KeyValuePair => _keyValuePair ??= GetTypeSymbol(typeof(KeyValuePair<,>));
    public INamedTypeSymbol Dictionary => _dictionary ??= GetTypeSymbol(typeof(Dictionary<,>));

    private INamedTypeSymbol GetTypeSymbol(Type type)
        => _compilation.GetTypeByMetadataName(type.FullName ?? throw new InvalidOperationException("Could not get name of type " + type))
            ?? throw new InvalidOperationException("Could not get type " + type.FullName);
}
