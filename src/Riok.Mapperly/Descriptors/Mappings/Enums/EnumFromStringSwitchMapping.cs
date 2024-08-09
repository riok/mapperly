using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
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
    IEnumerable<IFieldSymbol> enumMembers,
    bool ignoreCase,
    EnumFallbackValueMapping fallbackMapping
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string IgnoreCaseSwitchDesignatedVariableName = "s";
    private const string StringEqualsMethodName = nameof(string.Equals);
    private const string StringComparisonFullName = "System.StringComparison.OrdinalIgnoreCase";

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // switch for each name to the enum value
        var arms = ignoreCase ? BuildArmsIgnoreCase(ctx) : enumMembers.Select(BuildArm);
        arms = arms.Append(fallbackMapping.BuildDiscardArm(ctx));

        var switchExpr = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpr);
    }

    private IEnumerable<SwitchExpressionArmSyntax> BuildArmsIgnoreCase(TypeMappingBuildContext ctx)
    {
        var ignoreCaseSwitchDesignatedVariableName = ctx.NameBuilder.New(IgnoreCaseSwitchDesignatedVariableName);
        return enumMembers.Select(f => BuildArmIgnoreCase(ignoreCaseSwitchDesignatedVariableName, f));
    }

    private SwitchExpressionArmSyntax BuildArmIgnoreCase(string ignoreCaseSwitchDesignatedVariableName, IFieldSymbol field)
    {
        // { } s
        var pattern = RecursivePattern()
            .WithPropertyPatternClause(PropertyPatternClause().AddTrailingSpace())
            .WithDesignation(SingleVariableDesignation(Identifier(ignoreCaseSwitchDesignatedVariableName)));

        // source.Value1
        var typeMemberAccess = MemberAccess(field.ContainingType.NonNullable().FullyQualifiedIdentifierName(), field.Name);

        // when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase)
        var whenClause = SwitchWhen(
            InvocationWithoutIndention(
                MemberAccess(ignoreCaseSwitchDesignatedVariableName, StringEqualsMethodName),
                NameOf(typeMemberAccess),
                IdentifierName(StringComparisonFullName)
            )
        );

        // { } s when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase) => source.Value1;
        return SwitchArm(pattern, typeMemberAccess).WithWhenClause(whenClause);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol field)
    {
        // nameof(source.Value1) => source.Value1;
        var typeMemberAccess = MemberAccess(FullyQualifiedIdentifier(field.ContainingType), field.Name);
        var pattern = ConstantPattern(NameOf(typeMemberAccess));
        return SwitchArm(pattern, typeMemberAccess);
    }
}
