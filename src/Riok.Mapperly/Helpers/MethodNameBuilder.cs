using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Helpers;

public class MethodNameBuilder : UniqueNameBuilder
{
    private const string MethodNamePrefix = "MapTo";
    private const string ArrayTypeNameSuffix = "Array";
    private const string GenericTypeNameSeparator = "Of";
    private const string TypeArgumentSeparator = "And";
    private const int MaxTypeArguments = 2;
    private const int MaxNameLength = 62;

    public string Build(MethodMapping mapping)
    {
        var name = New(MethodNamePrefix + BuildTypeMethodName(mapping.TargetType.NonNullable()));
        return name.Length > MaxNameLength ? name[..MaxNameLength] : name;
    }

    private string BuildTypeMethodName(ITypeSymbol t)
    {
        return t switch
        {
            IArrayTypeSymbol arrType => BuildTypeMethodName(arrType.ElementType) + ArrayTypeNameSuffix,
            INamedTypeSymbol { TypeArguments.Length: 1 } genericT => genericT.Name
                + GenericTypeNameSeparator
                + genericT.TypeArguments[0].Name,
            INamedTypeSymbol { TypeArguments.Length: > 1 } genericT => genericT.Name
                + GenericTypeNameSeparator
                + string.Join(TypeArgumentSeparator, genericT.TypeArguments.Take(MaxTypeArguments).Select(static x => x.Name)),
            _ => t.Name,
        };
    }
}
