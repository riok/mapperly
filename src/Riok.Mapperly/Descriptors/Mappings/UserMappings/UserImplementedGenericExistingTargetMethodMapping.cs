using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a generic user-implemented existing target mapping method.
/// Generates invocations with explicit type arguments.
/// </summary>
public class UserImplementedGenericExistingTargetMethodMapping(
    string? receiver,
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    MethodParameter targetParameter,
    ITypeSymbol concreteSourceType,
    ITypeSymbol concreteTargetType,
    IReadOnlyList<ITypeSymbol> typeArguments,
    MethodParameter? referenceHandlerParameter,
    bool isExternal
)
    : UserImplementedExistingTargetMethodMapping(
        receiver,
        method,
        isDefault,
        sourceParameter,
        targetParameter,
        concreteSourceType,
        concreteTargetType,
        referenceHandlerParameter,
        isExternal
    )
{
    protected override SimpleNameSyntax BuildMethodName()
    {
        var typeArgs = typeArguments.Select(TypeSyntax (t) => NonNullableIdentifier(t)).ToArray();
        return GenericName(Method.Name).WithTypeArgumentList(TypeArgumentList(typeArgs));
    }
}
