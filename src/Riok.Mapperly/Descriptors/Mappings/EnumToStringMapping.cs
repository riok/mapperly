using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from an enum to a string.
/// Uses a switch expression for performance reasons (in comparison to <see cref="Enum.ToString()"/>).
/// Only supports defined enum values and no flags.
/// </summary>
public class EnumToStringMapping : MethodMapping
{
    private const string ToStringMethodName = nameof(Enum.ToString);

    private readonly IEnumerable<IFieldSymbol> _enumMembers;

    public EnumToStringMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        IEnumerable<IFieldSymbol> enumMembers)
        : base(sourceType, targetType)
    {
        _enumMembers = enumMembers;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // fallback switch arm: _ => source.ToString()
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            Invocation(MemberAccess(ctx.Source, ToStringMethodName)));

        // switch for each name to the enum value
        // eg: Enum1.Value1 => "Value1"
        var arms = _enumMembers
            .Select(BuildArm)
            .Append(fallbackArm);

        var switchExpr = SwitchExpression(ctx.Source)
            .WithArms(CommaSeparatedList(arms, true));

        yield return ReturnStatement(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol field)
    {
        var typeMemberAccess = MemberAccess(
            IdentifierName(field.ContainingType.NonNullable().GetFullyQualifiedIdentifierName()),
            field.Name);
        var pattern = ConstantPattern(typeMemberAccess);
        var nameOf = NameOf(typeMemberAccess);
        return SwitchExpressionArm(pattern, nameOf);
    }
}
