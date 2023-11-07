using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by accessing an instance member on the source object
/// and then mapping the result with an optional mapper.
/// <code>
/// target = source.Span;
/// target = Map(source.Span);
/// </code>
/// </summary>
public class SourceObjectMemberMapping : NewInstanceMapping
{
    private readonly string _memberName;
    private readonly INewInstanceMapping? _delegateMapping;

    public SourceObjectMemberMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        string memberName,
        INewInstanceMapping? delegateMapping = null
    )
        : base(sourceType, targetType)
    {
        _memberName = memberName;
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var sourceExpression = MemberAccess(ctx.Source, _memberName);
        return _delegateMapping == null ? sourceExpression : _delegateMapping.Build(ctx.WithSource(sourceExpression));
    }
}
