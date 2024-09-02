using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Configuration;
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
    EnumFallbackValueMapping fallbackMapping,
    IReadOnlyDictionary<IFieldSymbol, HashSet<ExpressionSyntax>> explicitMappings
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
        foreach (var field in enumMembers)
        {
            // source.Value1
            var typeMemberAccess = MemberAccess(field.ContainingType.NonNullable().FullyQualifiedIdentifierName(), field.Name);
            if (explicitMappings.TryGetValue(field, out var sourceExpressions))
            {
                // add switch arm for each source string
                foreach (var sourceExpression in sourceExpressions)
                {
                    yield return BuildArmIgnoreCase(ignoreCaseSwitchDesignatedVariableName, typeMemberAccess, sourceExpression);
                }
            }
            else
            {
                yield return BuildArmIgnoreCase(ignoreCaseSwitchDesignatedVariableName, typeMemberAccess, NameOf(typeMemberAccess));
            }
        }
    }

    private static SwitchExpressionArmSyntax BuildArmIgnoreCase(
        string ignoreCaseSwitchDesignatedVariableName,
        MemberAccessExpressionSyntax typeMemberAccess,
        ExpressionSyntax stringExpression
    )
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
                stringExpression,
                IdentifierName(StringComparisonFullName)
            )
        );

        // { } s when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase) => source.Value1;
        return SwitchArm(pattern, typeMemberAccess).WithWhenClause(whenClause);
    }

    private IEnumerable<SwitchExpressionArmSyntax> BuildArms()
    {
        foreach (var field in enumMembers)
        {
            // source.Value1
            var typeMemberAccess = MemberAccess(field.ContainingType.NonNullable().FullyQualifiedIdentifierName(), field.Name);
            if (explicitMappings.TryGetValue(field, out var sourceExpressions))
            {
                // add switch arm for each source string
                foreach (var sourceExpression in sourceExpressions)
                {
                    yield return BuildArm(typeMemberAccess, sourceExpression);
                }
            }
            else
            {
                yield return BuildArm(typeMemberAccess, NameOf(typeMemberAccess));
            }
        }
    }

    private static SwitchExpressionArmSyntax BuildArm(MemberAccessExpressionSyntax typeMemberAccess, ExpressionSyntax patternSyntax) =>
        SwitchArm(ConstantPattern(patternSyntax), typeMemberAccess);
}
