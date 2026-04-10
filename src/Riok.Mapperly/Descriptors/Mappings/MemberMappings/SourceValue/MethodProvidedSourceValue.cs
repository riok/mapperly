using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A source value which is provided by a method.
/// </summary>
public class MethodProvidedSourceValue(string methodName, string? targetType, IReadOnlyList<string> additionalParameterNames) : ISourceValue
{
    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ExpressionSyntax memberAccess = targetType == null ? IdentifierName(methodName) : MemberAccess(targetType, methodName);

        if (additionalParameterNames.Count == 0 || ctx.AdditionalParameters is null)
            return ctx.SyntaxFactory.Invocation(memberAccess);

        var arguments = additionalParameterNames
            .Select(name => Argument(ctx.AdditionalParameters.TryGetValue(name, out var expr) ? expr : IdentifierName(name)))
            .ToArray();

        return ctx.SyntaxFactory.Invocation(memberAccess, arguments);
    }
}
