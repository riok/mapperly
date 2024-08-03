using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking a static method with the source as only argument.
/// </summary>
public class StaticMethodMapping(IMethodSymbol method) : NewInstanceMapping(method.Parameters.Single().Type, method.ReturnType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => ctx.SyntaxFactory.StaticInvocation(method, ctx.Source);
}
