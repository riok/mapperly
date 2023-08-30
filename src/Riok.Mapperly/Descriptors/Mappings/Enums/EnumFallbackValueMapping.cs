using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Enum fallback value which is used if no enum value matched.
/// Either throws or returns the fallback value.
/// </summary>
public class EnumFallbackValueMapping : NewInstanceMapping
{
    private readonly INewInstanceMapping? _fallbackMapping;

    public EnumFallbackValueMapping(
        ITypeSymbol source,
        ITypeSymbol target,
        INewInstanceMapping? fallbackMapping = null,
        IFieldSymbol? fallbackMember = null
    )
        : base(source, target)
    {
        _fallbackMapping = fallbackMapping;
        FallbackMember = fallbackMember;
    }

    public IFieldSymbol? FallbackMember { get; }

    public SwitchExpressionArmSyntax BuildDiscardArm(TypeMappingBuildContext ctx) => SwitchArm(DiscardPattern(), Build(ctx));

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        if (_fallbackMapping != null)
            return _fallbackMapping.Build(ctx);

        if (FallbackMember == null)
            return ThrowArgumentOutOfRangeException(ctx.Source, $"The value of enum {SourceType.Name} is not supported");

        return MemberAccess(FullyQualifiedIdentifier(TargetType), FallbackMember.Name);
    }
}
