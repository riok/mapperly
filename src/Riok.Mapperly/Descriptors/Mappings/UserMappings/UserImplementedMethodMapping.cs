using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping : NewInstanceMapping, IUserMapping
{
    private readonly string? _receiver;
    private readonly MethodParameter _sourceParameter;
    private readonly MethodParameter? _referenceHandlerParameter;

    public UserImplementedMethodMapping(
        string? receiver,
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter
    )
        : base(method.Parameters[0].Type.UpgradeNullable(), method.ReturnType.UpgradeNullable())
    {
        Method = method;
        _receiver = receiver;
        _sourceParameter = sourceParameter;
        _referenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(
                _receiver == null ? IdentifierName(Method.Name) : MemberAccess(_receiver, Method.Name),
                _sourceParameter.WithArgument(ctx.Source),
                _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            );

        var castedReceiver = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            _receiver == null ? ThisExpression() : IdentifierName(_receiver)
        );
        var method = MemberAccess(ParenthesizedExpression(castedReceiver), Method.Name);
        return Invocation(
            method,
            _sourceParameter.WithArgument(ctx.Source),
            _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
        );
    }
}
