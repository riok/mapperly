using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a cast mapping.
/// </summary>
public class CastMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping? delegateMapping = null)
    : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var objToCast = delegateMapping != null ? delegateMapping.Build(ctx) : ctx.Source;
        return CastExpression(FullyQualifiedIdentifier(TargetType), objToCast);
    }
}
