using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

internal class MethodNameBuilder : UniqueNameBuilder
{
    private const string MethodNamePrefix = "MapTo";
    private const string ArrayTypeNameSuffix = "Array";

    internal string Build(MethodMapping mapping)
        => Build(MethodNamePrefix + BuildTypeMethodName(mapping.TargetType.NonNullable()));

    private string BuildTypeMethodName(ITypeSymbol t)
    {
        return t is IArrayTypeSymbol arrType
            ? BuildTypeMethodName(arrType.ElementType) + ArrayTypeNameSuffix
            : t.Name;
    }
}
