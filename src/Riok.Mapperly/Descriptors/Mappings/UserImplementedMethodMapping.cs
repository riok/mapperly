using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping : TypeMapping, IUserMapping
{

    private readonly MethodParameter _sourceParameter;
    private readonly MethodParameter? _referenceHandlerParameter;

    public UserImplementedMethodMapping(IMethodSymbol method, MethodParameter sourceParameter, MethodParameter? referenceHandlerParameter)
        : base(method.Parameters[0].Type.UpgradeNullable(), method.ReturnType.UpgradeNullable())
    {
        Method = method;
        _sourceParameter = sourceParameter;
        _referenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(Method.Name, _sourceParameter.WithArgument(ctx.Source), _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));

        var castedThis = CastExpression(FullyQualifiedIdentifier(Method.ReceiverType!), ThisExpression());
        var method = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        return Invocation(method, _sourceParameter.WithArgument(ctx.Source), _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));
    }
}
