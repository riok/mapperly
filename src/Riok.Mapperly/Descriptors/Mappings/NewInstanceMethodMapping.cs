using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

public abstract class NewInstanceMethodMapping : MethodMapping, INewInstanceMapping
{
    protected NewInstanceMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType, ITypeSymbol? returnType = null)
        : base(sourceType, targetType, returnType) { }

    protected NewInstanceMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        ITypeSymbol targetType,
        ITypeSymbol? returnType = null
    )
        : base(method, sourceParameter, referenceHandlerParameter, targetType, returnType) { }
}
