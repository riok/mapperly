using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A source value which is provided by a method.
/// </summary>
public class MethodProvidedSourceValue(IMethodSymbol method) : ISourceValue
{
    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        if (method.Parameters.Length <= 1)
            return ctx.SyntaxFactory.Invocation(method.Name);

        // Skip the first parameter (source) and convert them.
        var parameters = method
            .Parameters.Skip(1)
            .Select((param, i) => new MethodParameter(i, param.Name, param.Type).WithArgument(IdentifierName(param.Name)))
            .Cast<MethodArgument?>()
            .ToArray();
        return ctx.SyntaxFactory.Invocation(method.Name, parameters);
    }
}
