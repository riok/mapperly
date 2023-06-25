using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping : TypeMapping, IUserMapping
{
    private readonly MethodParameter _sourceParameter;
    private readonly MethodParameter? _referenceHandlerParameter;

    public UserImplementedMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        MethodParameter[] parameters
    )
        : base(method.Parameters[0].Type.UpgradeNullable(), method.ReturnType.UpgradeNullable(), parameters)
    {
        Method = method;
        _sourceParameter = sourceParameter;
        _referenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var sourceArgument = _sourceParameter.WithArgument(ctx.Source);
        var referenceArgument = _referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler);

        // TODO: order isn't valid
        var arguments = Parameters
            .Zip(ctx.Parameters, (a, b) => (a, b))
            .Select(x => x.a.WithArgument(x.b))
            .Cast<MethodArgument?>()
            .ToArray();

        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(Method.Name, sourceArgument, referenceArgument, arguments);

        var castedThis = CastExpression(FullyQualifiedIdentifier(Method.ReceiverType!), ThisExpression());
        var method = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        return Invocation(method, sourceArgument, referenceArgument, arguments);
    }
}
