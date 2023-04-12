using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from a string to an enum.
/// Uses a switch expression for performance reasons (in comparison to <see cref="Enum.Parse(System.Type,string)"/>).
/// Optimized version of <see cref="EnumFromStringParseMapping"/>.
/// </summary>
public class EnumFromStringSwitchMapping : MethodMapping
{
    private const string IgnoreCaseSwitchDesignatedVariableName = "s";
    private const string StringEqualsMethodName = nameof(string.Equals);
    private const string StringComparisonFullName = "System.StringComparison.OrdinalIgnoreCase";

    private readonly IEnumerable<IFieldSymbol> _enumMembers;
    private readonly bool _ignoreCase;
    private readonly EnumFromStringParseMapping _fallbackMapping;

    public EnumFromStringSwitchMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        IEnumerable<IFieldSymbol> enumMembers,
        bool genericParseMethodSupported,
        bool ignoreCase)
        : base(sourceType, targetType)
    {
        _enumMembers = enumMembers;
        _ignoreCase = ignoreCase;
        _fallbackMapping = new EnumFromStringParseMapping(sourceType, targetType, genericParseMethodSupported, ignoreCase);
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // fallback switch arm: _ => System.Enum.Parse<TargetType>(source, ignoreCase)
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            _fallbackMapping.Build(ctx));

        // switch for each name to the enum value
        var arms = _ignoreCase
            ? BuildArmsIgnoreCase(ctx)
            : _enumMembers.Select(BuildArm);
        arms = arms.Append(fallbackArm);

        var switchExpr = SwitchExpression(ctx.Source)
            .WithArms(CommaSeparatedList(arms, true));

        yield return ReturnStatement(switchExpr);
    }

    private IEnumerable<SwitchExpressionArmSyntax> BuildArmsIgnoreCase(TypeMappingBuildContext ctx)
    {
        var ignoreCaseSwitchDesignatedVariableName = ctx.NameBuilder.New(IgnoreCaseSwitchDesignatedVariableName);
        return _enumMembers.Select(f => BuildArmIgnoreCase(ignoreCaseSwitchDesignatedVariableName, f));
    }

    private SwitchExpressionArmSyntax BuildArmIgnoreCase(string ignoreCaseSwitchDesignatedVariableName, IFieldSymbol field)
    {
        // { } s
        var pattern = RecursivePattern()
            .WithPropertyPatternClause(PropertyPatternClause())
            .WithDesignation(SingleVariableDesignation(Identifier(ignoreCaseSwitchDesignatedVariableName)));

        // source.Value1
        var typeMemberAccess = MemberAccess(
            FullyQualifiedNonNullableIdentifier(field.ContainingType),
            field.Name);

        // when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase)
        var whenClause = WhenClause(
            Invocation(
                MemberAccess(ignoreCaseSwitchDesignatedVariableName, StringEqualsMethodName),
                NameOf(typeMemberAccess),
                IdentifierName(StringComparisonFullName)));

        // { } s when s.Equals(nameof(source.Value1), StringComparison.OrdinalIgnoreCase) => source.Value1;
        return SwitchExpressionArm(pattern, typeMemberAccess)
            .WithWhenClause(whenClause);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol field)
    {
        // nameof(source.Value1) => source.Value1;
        var typeMemberAccess = MemberAccess(
            FullyQualifiedIdentifier(field.ContainingType),
            field.Name);
        var pattern = ConstantPattern(NameOf(typeMemberAccess));
        return SwitchExpressionArm(pattern, typeMemberAccess);
    }
}
