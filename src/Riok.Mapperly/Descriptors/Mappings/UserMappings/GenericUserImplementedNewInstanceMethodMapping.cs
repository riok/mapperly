using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A template for a generic user-implemented new instance mapping method.
/// This is not a concrete mapping but stores the information needed to create
/// concrete instantiations when a matching type pair is found.
/// The <see cref="Default"/> is always <c>false</c> so it is not added to the default mappings dictionary.
/// </summary>
internal sealed class GenericUserImplementedNewInstanceMethodMapping(
    string? receiver,
    IMethodSymbol method,
    MethodParameter sourceParameter,
    ITypeSymbol genericTargetType,
    MethodParameter? referenceHandlerParameter,
    MethodParameter? targetOriginalValueParameter,
    bool isExternal,
    UserImplementedMethodMapping.TargetNullability targetNullability,
    bool noExpressionInlining
) : INewInstanceUserMapping
{
    public ITypeSymbol SourceType => sourceParameter.Type;

    public ITypeSymbol TargetType => genericTargetType;

    public IMethodSymbol Method => method;

    public bool? Default => false;

    public bool IsExternal => isExternal;

    public bool NoExpressionInlining => noExpressionInlining;

    public bool IsSynthetic => false;

    public IEnumerable<TypeMappingKey> BuildAdditionalMappingKeys(TypeMappingConfiguration config) => [];

    public ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        throw new InvalidOperationException("Generic mapping template should not be built directly");

    /// <summary>
    /// Tries to create a concrete mapping for the given source and target types
    /// by inferring the type arguments from the generic method signature.
    /// </summary>
    public UserImplementedGenericMethodMapping? TryCreateConcreteMapping(
        GenericTypeChecker checker,
        ITypeSymbol concreteSourceType,
        ITypeSymbol concreteTargetType
    )
    {
        var result = checker.InferAndCheckTypes(
            method.TypeParameters,
            (sourceParameter.Type, concreteSourceType),
            (genericTargetType, concreteTargetType)
        );

        if (!result.Success)
            return null;

        var typeArguments = method.TypeParameters.Select(tp => result.InferredTypes[tp]).ToArray();

        var concreteTargetOriginalValueParameter = targetOriginalValueParameter;
        if (concreteTargetOriginalValueParameter is { } p)
        {
            concreteTargetOriginalValueParameter = p with { Type = method.Construct(typeArguments).Parameters[p.Ordinal].Type };
        }

        return new UserImplementedGenericMethodMapping(
            receiver,
            method,
            null,
            sourceParameter,
            concreteSourceType,
            concreteTargetType,
            typeArguments,
            referenceHandlerParameter,
            concreteTargetOriginalValueParameter,
            isExternal,
            targetNullability,
            noExpressionInlining
        );
    }
}
