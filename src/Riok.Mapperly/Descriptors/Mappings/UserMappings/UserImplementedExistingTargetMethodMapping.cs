using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents an existing target type mapper which is implemented by the user.
/// </summary>
public class UserImplementedExistingTargetMethodMapping : ExistingTargetMapping, IUserMapping
{
    private readonly MethodParameter _sourceParameter;
    private readonly MethodParameter _targetParameter;
    private readonly MethodParameter? _referenceHandlerParameter;

    public UserImplementedExistingTargetMethodMapping(
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
    }

    public IMethodSymbol Method { get; }

    public ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        throw new InvalidOperationException(
            $"{nameof(UserImplementedExistingTargetMethodMapping)} {ctx.Source}, {ctx.ReferenceHandler} does not support {nameof(Build)}"
        );

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
        {
            yield return SyntaxFactory.ExpressionStatement(
                SyntaxFactoryHelper.Invocation(
                    Method.Name,
                    _sourceParameter.WithArgument(ctx.Source),
                    _targetParameter.WithArgument(target),
                    _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
                )
            );
            yield break;
        }

        var castedThis = SyntaxFactory.CastExpression(
            SyntaxFactoryHelper.FullyQualifiedIdentifier(Method.ReceiverType!),
            SyntaxFactory.ThisExpression()
        );
        var method = SyntaxFactoryHelper.MemberAccess(SyntaxFactory.ParenthesizedExpression(castedThis), Method.Name);
        yield return SyntaxFactory.ExpressionStatement(
            SyntaxFactoryHelper.Invocation(
                method,
                _sourceParameter.WithArgument(ctx.Source),
                _targetParameter.WithArgument(target),
                _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            )
        );
    }
}
