using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking a static method with the source as only argument.
/// </summary>
public class StaticMethodMapping : TypeMapping
{
    private readonly IMethodSymbol _method;

    public StaticMethodMapping(IMethodSymbol method)
        : base(method.Parameters.Single().Type, method.ReturnType)
    {
        _method = method;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => StaticInvocation(_method, ctx.Source);
}
