using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

public abstract class NewInstanceMethodMapping : MethodMapping, INewInstanceMapping
{
    protected NewInstanceMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType) { }

    protected NewInstanceMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        ITypeSymbol targetType
    )
        : base(method, sourceParameter, referenceHandlerParameter, targetType) { }
}
