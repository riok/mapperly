using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Symbols;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping : TypeMapping, IUserMapping
{
    public UserImplementedMethodMapping(IMethodSymbol method, MethodParameter sourceParameter, MethodParameter? referenceHandlerParameter)
        : base(method.Parameters[0].Type.UpgradeNullable(), method.ReturnType.UpgradeNullable())
    {
        Method = method;
        SourceParameter = sourceParameter;
        ReferenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    private MethodParameter SourceParameter { get; }

    public MethodParameter? ReferenceHandlerParameter { get; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(Method.Name, SourceParameter.WithArgument(ctx.Source), ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));

        var castedThis = CastExpression(IdentifierName(Method.ReceiverType!.ToDisplayString()), ThisExpression());
        var method = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        return Invocation(method, SourceParameter.WithArgument(ctx.Source), ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));
    }
}
