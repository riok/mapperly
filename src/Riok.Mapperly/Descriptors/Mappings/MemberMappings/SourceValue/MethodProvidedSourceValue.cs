using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A source value which is provided by a method.
/// </summary>
public class MethodProvidedSourceValue(string methodName, string? targetType) : ISourceValue
{
    public ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        ctx.SyntaxFactory.Invocation(targetType == null ? IdentifierName(methodName) : MemberAccess(targetType, methodName));
}
