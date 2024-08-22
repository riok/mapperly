using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

public abstract class NewInstanceMethodMapping : MethodMapping, INewInstanceMapping
{
    protected NewInstanceMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType, bool enableAggressiveInlining)
        : base(sourceType, targetType, enableAggressiveInlining) { }

    protected NewInstanceMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        ITypeSymbol targetType,
        bool enableAggressiveInlining
    )
        : base(method, sourceParameter, referenceHandlerParameter, targetType, enableAggressiveInlining) { }
}
