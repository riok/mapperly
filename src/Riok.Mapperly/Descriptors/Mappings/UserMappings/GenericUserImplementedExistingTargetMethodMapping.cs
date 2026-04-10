using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A template for a generic user-implemented existing target mapping method.
/// This is not a concrete mapping but stores the information needed to create
/// concrete instantiations when a matching type pair is found.
/// The <see cref="Default"/> is always <c>false</c> so it is not added to the default mappings dictionary.
/// </summary>
internal sealed class GenericUserImplementedExistingTargetMethodMapping(
    string? receiver,
    IMethodSymbol method,
    MethodParameter sourceParameter,
    MethodParameter targetParameter,
    MethodParameter? referenceHandlerParameter,
    bool isExternal
) : IExistingTargetUserMapping
{
    public ITypeSymbol SourceType => sourceParameter.Type;

    public ITypeSymbol TargetType => targetParameter.Type;

    public IMethodSymbol Method => method;

    public bool? Default => false;

    public bool IsExternal => isExternal;

    public bool IsSynthetic => false;

    public IEnumerable<TypeMappingKey> BuildAdditionalMappingKeys(TypeMappingConfiguration config) => [];

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) =>
        throw new InvalidOperationException("Generic mapping template should not be built directly");

    /// <summary>
    /// Tries to create a concrete mapping for the given source and target types
    /// by inferring the type arguments from the generic method signature.
    /// </summary>
    public UserImplementedGenericExistingTargetMethodMapping? TryCreateConcreteMapping(
        GenericTypeChecker checker,
        ITypeSymbol concreteSourceType,
        ITypeSymbol concreteTargetType
    )
    {
        var result = checker.InferAndCheckTypes(
            method.TypeParameters,
            (sourceParameter.Type, concreteSourceType),
            (targetParameter.Type, concreteTargetType)
        );

        if (!result.Success)
            return null;

        var typeArguments = method.TypeParameters.Select(tp => result.InferredTypes[tp]).ToList();

        return new UserImplementedGenericExistingTargetMethodMapping(
            receiver,
            method,
            null,
            sourceParameter,
            targetParameter,
            concreteSourceType,
            concreteTargetType,
            typeArguments,
            referenceHandlerParameter,
            isExternal
        );
    }
}
