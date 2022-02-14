using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping which works by invoking an instance method on the source object.
/// </summary>
public class SourceObjectMethodMapping : TypeMapping
{
    private readonly string _methodName;

    public SourceObjectMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType, string methodName) : base(sourceType, targetType)
    {
        _methodName = methodName;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => InvocationExpression(MemberAccess(source, _methodName));
}
