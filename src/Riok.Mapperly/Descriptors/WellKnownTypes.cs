using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class WellKnownTypes(Compilation compilation)
{
    private readonly Dictionary<string, INamedTypeSymbol?> _cachedTypes = new();

    // use string type name as they are not available in netstandard2.0
    public INamedTypeSymbol? DateOnly => TryGet("System.DateOnly");

    public INamedTypeSymbol? TimeOnly => TryGet("System.TimeOnly");

    public ITypeSymbol GetArrayType(ITypeSymbol type) =>
        compilation.CreateArrayTypeSymbol(type, elementNullableAnnotation: type.NullableAnnotation).NonNullable();

    public ITypeSymbol GetArrayType(ITypeSymbol elementType, int rank, NullableAnnotation elementNullableAnnotation) =>
        compilation.CreateArrayTypeSymbol(elementType, rank, elementNullableAnnotation);

    public INamedTypeSymbol Get<T>() => Get(typeof(T));

    public INamedTypeSymbol Get(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }

        return Get(type.FullName ?? throw new InvalidOperationException("Could not get name of type " + type));
    }

    public INamedTypeSymbol? TryGet(string typeFullName)
    {
        if (_cachedTypes.TryGetValue(typeFullName, out var typeSymbol))
        {
            return typeSymbol;
        }

        typeSymbol = compilation.GetBestTypeByMetadataName(typeFullName);
        _cachedTypes.Add(typeFullName, typeSymbol);

        return typeSymbol;
    }

    private INamedTypeSymbol Get(string typeFullName) =>
        TryGet(typeFullName) ?? throw new InvalidOperationException("Could not get type " + typeFullName);
}
