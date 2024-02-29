using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping(
    string? receiver,
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    ITypeSymbol targetType,
    MethodParameter? referenceHandlerParameter
) : NewInstanceMapping(sourceParameter.Type, targetType), INewInstanceUserMapping
{
    public IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(
                receiver == null ? IdentifierName(Method.Name) : MemberAccess(receiver, Method.Name),
                sourceParameter.WithArgument(ctx.Source),
                referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            );

        var castedReceiver = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            receiver == null ? ThisExpression() : IdentifierName(receiver)
        );
        var methodExpr = MemberAccess(ParenthesizedExpression(castedReceiver), Method.Name);
        return Invocation(
            methodExpr,
            sourceParameter.WithArgument(ctx.Source),
            referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
        );
    }
}
