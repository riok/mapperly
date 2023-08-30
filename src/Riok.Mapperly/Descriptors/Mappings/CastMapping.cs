using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a cast mapping.
/// </summary>
public class CastMapping : NewInstanceMapping
{
    private readonly INewInstanceMapping? _delegateMapping;

    public CastMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping? delegateMapping = null)
        : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var objToCast = _delegateMapping != null ? _delegateMapping.Build(ctx) : ctx.Source;
        return CastExpression(FullyQualifiedIdentifier(TargetType), objToCast);
    }
}
