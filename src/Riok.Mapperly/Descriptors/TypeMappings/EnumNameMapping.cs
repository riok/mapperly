using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping from an enum to another enum by using their names.
/// Uses a switch expression. Only supports defined enum values and no flags.
/// The name has to match exactly.
/// </summary>
public class EnumNameMapping : MethodMapping
{
    private readonly IReadOnlyCollection<string> _enumMemberNames;

    public EnumNameMapping(
        ITypeSymbol source,
        ITypeSymbol target,
        IReadOnlyCollection<string> enumMemberNames)
        : base(source, target)
    {
        _enumMemberNames = enumMemberNames;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // fallback switch arm: _ => throw new ArgumentOutOfRangeException("source");
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            ThrowArgumentOutOfRangeException(source));

        // switch for each name to the enum value
        // eg: nameof(Enum1.Value1) => Enum2.Value1,
        var arms = _enumMemberNames
            .Select(BuildArm)
            .Append(fallbackArm);

        var switchExpr = SwitchExpression(source)
            .WithArms(CommaSeparatedList(arms, true));

        yield return ReturnStatement(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(string memberName)
    {
        var sourceMember = MemberAccess(SourceType.ToDisplayString(), memberName);
        var targetMember = MemberAccess(TargetType.ToDisplayString(), memberName);
        var pattern = ConstantPattern(sourceMember);
        return SwitchExpressionArm(pattern, targetMember);
    }
}
