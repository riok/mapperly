using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents a mapping from an enum to a string.
/// Uses a switch expression for performance reasons (in comparison to <see cref="Enum.ToString()"/>).
/// Only supports defined enum values and no flags.
/// </summary>
public class EnumToStringMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    IEnumerable<EnumMemberMapping> enumMemberMappings,
    EnumFallbackValueMapping fallbackMapping
) : NewInstanceMethodMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // fallback switch arm: _ => source.ToString()
        // or: _ => "fallback_value" (if fallback mapping exists)
        var fallbackArm = fallbackMapping.BuildDiscardArm(ctx);

        // switch for each name to the enum value
        // eg: Enum1.Value1 => "Value1"
        // or: Enum1.Value1 => "value_1" (if explicit mapping exists)
        var arms = enumMemberMappings.Select(m => m.BuildSwitchArm()).Append(fallbackArm);
        var switchExpr = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpr);
    }
}
