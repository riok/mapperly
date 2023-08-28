using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents an existing target type mapper which is implemented by the user.
/// </summary>
public class UserImplementedExistingTargetMethodMapping : ExistingTargetMapping, IUserMapping
{
    private readonly MethodParameter _sourceParameter;
    private readonly MethodParameter _targetParameter;
    private readonly MethodParameter? _referenceHandlerParameter;
    private readonly string? _receiver;

    public UserImplementedExistingTargetMethodMapping(
        string? receiver,
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter targetParameter,
        MethodParameter? referenceHandlerParameter
    )
        : base(method.Parameters[0].Type.UpgradeNullable(), targetParameter.Type.UpgradeNullable())
    {
        Method = method;
        _sourceParameter = sourceParameter;
        _targetParameter = targetParameter;
        _referenceHandlerParameter = referenceHandlerParameter;
        _receiver = receiver;
    }

    public IMethodSymbol Method { get; }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
        {
            yield return ExpressionStatement(
                Invocation(
                    _receiver == null ? IdentifierName(Method.Name) : MemberAccess(_receiver, Method.Name),
                    _sourceParameter.WithArgument(ctx.Source),
                    _targetParameter.WithArgument(target),
                    _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
                )
            );
            yield break;
        }

        var castedThis = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            _receiver != null ? IdentifierName(_receiver) : ThisExpression()
        );
        var method = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        yield return ExpressionStatement(
            Invocation(
                method,
                _sourceParameter.WithArgument(ctx.Source),
                _targetParameter.WithArgument(target),
                _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            )
        );
    }
}
