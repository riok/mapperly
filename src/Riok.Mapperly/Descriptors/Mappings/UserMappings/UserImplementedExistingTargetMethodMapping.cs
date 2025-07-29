using Microsoft.CodeAnalysis;
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
    MethodParameter? referenceHandlerParameter,
    bool isExternal
) : ExistingTargetMapping(method.Parameters[0].Type, targetParameter.Type), IExistingTargetUserMapping
{
    public IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public bool IsExternal { get; } = isExternal;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
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
                        receiver == null ? IdentifierName(Method.Name) : MemberAccess(receiver, Method.Name),
                        sourceParameter.WithArgument(ctx.Source),
                        targetParameter.WithArgument(IdentifierName(targetRefVarName)),
                        referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
                    )
                );
                yield return ctx.SyntaxFactory.ExpressionStatement(Assignment(target, IdentifierName(targetRefVarName), false));

                yield break;
            }

            yield return ctx.SyntaxFactory.ExpressionStatement(
                ctx.SyntaxFactory.Invocation(
                    receiver == null ? IdentifierName(Method.Name) : MemberAccess(receiver, Method.Name),
                    sourceParameter.WithArgument(ctx.Source),
                    targetParameter.WithArgument(target),
                    referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
                )
            );
            yield break;
        }

        var castedThis = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            receiver != null ? IdentifierName(receiver) : ThisExpression()
        );
        var methodExpr = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        yield return ctx.SyntaxFactory.ExpressionStatement(
            ctx.SyntaxFactory.Invocation(
                methodExpr,
                sourceParameter.WithArgument(ctx.Source),
                targetParameter.WithArgument(target),
                referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            )
        );
    }
}
