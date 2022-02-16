using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping : TypeMapping, IUserMapping
{
    public UserImplementedMethodMapping(IMethodSymbol method)
        : base(method.Parameters.Single().Type, method.ReturnType)
    {
        Method = method;
    }

    public IMethodSymbol Method { get; }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
            return Invocation(Method.Name, source);

        var castedThis = CastExpression(IdentifierName(Method.ReceiverType!.ToDisplayString()), ThisExpression());
        var method = MemberAccess(ParenthesizedExpression(castedThis), Method.Name);
        return Invocation(
            method,
            source);
    }
}
