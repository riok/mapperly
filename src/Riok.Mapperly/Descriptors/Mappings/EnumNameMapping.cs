using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from an enum to another enum by using their names.
/// Uses a switch expression. Only supports defined enum values and no flags.
/// The name has to match exactly.
/// </summary>
public class EnumNameMapping : MethodMapping
{
    private readonly IReadOnlyDictionary<string, string> _enumMemberMappings;
    private readonly ITypeMapping? _fallbackMapping;

    public EnumNameMapping(
        ITypeSymbol source,
        ITypeSymbol target,
        IReadOnlyDictionary<string, string> enumMemberMappings,
        ITypeMapping? fallbackMapping = null
    )
        : base(source, target)
    {
        _enumMemberMappings = enumMemberMappings;
        _fallbackMapping = fallbackMapping;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // fallback switch arm with _fallbackMapping: _ => Map(src);
        // fallback switch arm without _fallbackMapping: _ => throw new ArgumentOutOfRangeException(nameof(source), source, message);
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            _fallbackMapping?.Build(ctx)
                ?? ThrowArgumentOutOfRangeException(ctx.Source, $"The value of enum {SourceType.Name} is not supported")
        );

        // switch for each name to the enum value
        // eg: Enum1.Value1 => Enum2.Value1,
        var arms = _enumMemberMappings.Select(BuildArm).Append(fallbackArm);

        var switchExpr = SwitchExpression(ctx.Source).WithArms(CommaSeparatedList(arms, true));
        yield return ReturnStatement(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(KeyValuePair<string, string> sourceTargetField)
    {
        var sourceMember = MemberAccess(FullyQualifiedIdentifier(SourceType), sourceTargetField.Key);
        var targetMember = MemberAccess(FullyQualifiedIdentifier(TargetType), sourceTargetField.Value);
        var pattern = ConstantPattern(sourceMember);
        return SwitchExpressionArm(pattern, targetMember);
    }
}
