using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a generic user-implemented mapping method.
/// Generates invocations with explicit type arguments, e.g. <c>MapOptional&lt;UserDto, User&gt;(source.Value)</c>.
/// </summary>
public class UserImplementedGenericMethodMapping(
    string? receiver,
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    ITypeSymbol concreteSourceType,
    ITypeSymbol concreteTargetType,
    IReadOnlyList<ITypeSymbol> typeArguments,
    MethodParameter? referenceHandlerParameter,
    bool isExternal,
    UserImplementedMethodMapping.TargetNullability targetNullability
)
    : UserImplementedMethodMapping(
        receiver,
        method,
        isDefault,
        sourceParameter,
        concreteSourceType,
        concreteTargetType,
        referenceHandlerParameter,
        isExternal,
        targetNullability
    )
{
    protected override SimpleNameSyntax BuildMethodName()
    {
        var typeArgs = typeArguments.Select(TypeSyntax (t) => NonNullableIdentifier(t)).ToArray();
        return GenericName(Method.Name).WithTypeArgumentList(TypeArgumentList(typeArgs));
    }
}
