using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping where the target type has the source as single ctor argument.
/// </summary>
public class CtorMapping : NewInstanceMapping
{
    public CtorMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType) { }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => CreateInstance(TargetType, ctx.Source);
}
