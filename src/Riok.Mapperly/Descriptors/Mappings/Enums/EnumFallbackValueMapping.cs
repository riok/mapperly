using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Enum fallback value which is used if no enum value matched.
/// Either throws or returns the fallback value.
/// </summary>
public class EnumFallbackValueMapping(
    ITypeSymbol source,
    ITypeSymbol target,
    INewInstanceMapping? fallbackMapping = null,
    ExpressionSyntax? fallbackExpression = null
) : NewInstanceMapping(source, target)
{
    public ExpressionSyntax? FallbackExpression { get; } = fallbackExpression;

    public SwitchExpressionArmSyntax BuildDiscardArm(TypeMappingBuildContext ctx) => SwitchArm(DiscardPattern(), Build(ctx));

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        if (fallbackMapping is not null)
            return fallbackMapping.Build(ctx);

        if (FallbackExpression is not null)
            return FallbackExpression;

        return ThrowArgumentOutOfRangeException(ctx.Source, $"The value of enum {SourceType.Name} is not supported");
    }
}
