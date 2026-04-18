using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents an existing target type mapper which is implemented by the user.
/// </summary>
public class UserImplementedExistingTargetMethodMapping(
    string? receiver,
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    MethodParameter targetParameter,
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    MethodParameter? referenceHandlerParameter,
    bool isExternal
) : ExistingTargetMapping(sourceType, targetType), IExistingTargetUserMapping, IParameterizedMapping
{
    public IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public bool IsExternal { get; } = isExternal;

    public bool IsRefTarget => targetParameter.RefKind == RefKind.Ref;

    public IReadOnlyCollection<MethodParameter> AdditionalSourceParameters { get; } =
        method
            .Parameters.Where(p =>
                p.Ordinal != sourceParameter.Ordinal
                && p.Ordinal != targetParameter.Ordinal
                && (referenceHandlerParameter is null || p.Ordinal != referenceHandlerParameter.Value.Ordinal)
            )
            .Select(p => new MethodParameter(p, p.Type))
            .ToList();

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var methodName = BuildMethodName();

        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
        {
            // if target parameter expects the ref keyword we need to add some extra lines so the reference is correctly stored back into the property.
            if (targetParameter.RefKind == RefKind.Ref)
            {
                var targetRefVarName = ctx.NameBuilder.New("targetRef");
                yield return ctx.SyntaxFactory.DeclareLocalVariable(targetRefVarName, target);
                yield return ctx.SyntaxFactory.ExpressionStatement(
                    ctx.SyntaxFactory.Invocation(
                        GetMethodExpr(),
                        ctx.BuildArguments(
                            Method,
                            sourceParameter,
                            referenceHandlerParameter,
                            targetParameter.WithArgument(IdentifierName(targetRefVarName))
                        )
                    )
                );
                yield return ctx.SyntaxFactory.ExpressionStatement(Assignment(target, IdentifierName(targetRefVarName), false));

                yield break;
            }

            yield return ctx.SyntaxFactory.ExpressionStatement(
                ctx.SyntaxFactory.Invocation(
                    GetMethodExpr(),
                    ctx.BuildArguments(Method, sourceParameter, referenceHandlerParameter, targetParameter.WithArgument(target))
                )
            );
            yield break;

            ExpressionSyntax GetMethodExpr() =>
                receiver == null
                    ? methodName
                    : MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(receiver), methodName);
        }

        var castedThis = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            receiver != null ? IdentifierName(receiver) : ThisExpression()
        );
        var castedMethodExpr = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            ParenthesizedExpression(castedThis),
            methodName
        );
        yield return ctx.SyntaxFactory.ExpressionStatement(
            ctx.SyntaxFactory.Invocation(
                castedMethodExpr,
                ctx.BuildArguments(Method, sourceParameter, referenceHandlerParameter, targetParameter.WithArgument(target))
            )
        );
    }

    protected virtual SimpleNameSyntax BuildMethodName() => IdentifierName(Method.Name);
}
