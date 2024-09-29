using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents a mapping from a string to an enum.
/// Uses a switch expression for performance reasons (in comparison to <see cref="Enum.Parse(System.Type,string)"/>).
/// Optimized version of <see cref="EnumFromStringParseMapping"/>.
/// </summary>
public class EnumFromStringSwitchMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    bool ignoreCase,
    IEnumerable<EnumMemberMapping> enumMemberMappings,
    EnumFallbackValueMapping fallbackMapping
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string IgnoreCaseSwitchDesignatedVariableName = "s";
    private const string StringEqualsMethodName = nameof(string.Equals);
    private const string StringComparisonFullName = "System.StringComparison.OrdinalIgnoreCase";

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // switch for each name to the enum value
        var arms = ignoreCase ? BuildArmsIgnoreCase(ctx) : BuildArms();
        arms = arms.Append(fallbackMapping.BuildDiscardArm(ctx));

        var switchExpr = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpr);
    }

    private IEnumerable<SwitchExpressionArmSyntax> BuildArmsIgnoreCase(TypeMappingBuildContext ctx)
    {
        var ignoreCaseSwitchDesignatedVariableName = ctx.NameBuilder.New(IgnoreCaseSwitchDesignatedVariableName);
        return enumMemberMappings.Select(m => BuildArmIgnoreCase(ignoreCaseSwitchDesignatedVariableName, m));
    }

    private static SwitchExpressionArmSyntax BuildArmIgnoreCase(string ignoreCaseSwitchDesignatedVariableName, EnumMemberMapping mapping)
    {
        // { } s
        var pattern = RecursivePattern()
            .WithPropertyPatternClause(PropertyPatternClause().AddTrailingSpace())
            .WithDesignation(SingleVariableDesignation(Identifier(ignoreCaseSwitchDesignatedVariableName)));

        // when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase)
        // or (if explicit mapping exists)
        // when s.Equals("VALUE-1", StringComparison.OrdinalIgnoreCase)
        var whenClause = SwitchWhen(
            InvocationWithoutIndention(
                MemberAccess(ignoreCaseSwitchDesignatedVariableName, StringEqualsMethodName),
                mapping.SourceSyntax,
                IdentifierName(StringComparisonFullName)
            )
        );

        // { } s when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase) => source.Value1;
        return SwitchArm(pattern, mapping.TargetSyntax).WithWhenClause(whenClause);
    }

    private IEnumerable<SwitchExpressionArmSyntax> BuildArms() => enumMemberMappings.Select(m => m.BuildSwitchArm());
}
