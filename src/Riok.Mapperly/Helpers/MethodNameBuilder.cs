using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Helpers;

internal class MethodNameBuilder : UniqueNameBuilder
{
    private const string MethodNamePrefix = "MapTo";
    private const string ArrayTypeNameSuffix = "Array";

    public string Build(MethodMapping mapping)
        => New(MethodNamePrefix + BuildTypeMethodName(mapping.TargetType.NonNullable()));

    private string BuildTypeMethodName(ITypeSymbol t)
    {
        return t is IArrayTypeSymbol arrType
            ? BuildTypeMethodName(arrType.ElementType) + ArrayTypeNameSuffix
            : t.Name;
    }
}
