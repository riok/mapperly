using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping where the target type has the source as single ctor argument.
/// </summary>
public class CtorMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => ctx.SyntaxFactory.CreateInstance(TargetType, ctx.Source);
}
