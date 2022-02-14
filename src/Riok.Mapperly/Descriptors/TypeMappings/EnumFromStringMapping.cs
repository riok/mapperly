using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping from a string to an enum.
/// Uses a switch expression for performance reasons (in comparison to <see cref="Enum.Parse(System.Type,string)"/>).
/// Only supports defined enum names (must match exactly) and no flags.
/// </summary>
public class EnumFromStringMapping : MethodMapping
{
    private const string EnumClassName = "Enum";
    private const string ParseMethodName = "Parse";

    private readonly IEnumerable<IFieldSymbol> _enumMembers;

    public EnumFromStringMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        IEnumerable<IFieldSymbol> enumMembers)
        : base(sourceType, targetType)
    {
        _enumMembers = enumMembers;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // fallback switch arm: _ => (TargetType)Enum.Parse(typeof(TargetType), source)
        var enumParseInvocation = Invocation(
            MemberAccess(EnumClassName, ParseMethodName),
            TypeOfExpression(IdentifierName(TargetType.ToDisplayString())), source);
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            CastExpression(IdentifierName(TargetType.ToDisplayString()), enumParseInvocation));

        // switch for each name to the enum value
        // eg: nameof(Enum1.Value1) => Enum1.Value1,
        var arms = _enumMembers.Select(BuildArm)
            .Append(fallbackArm);

        var switchExpr = SwitchExpression(source)
            .WithArms(CommaSeparatedList(arms, true));

        yield return ReturnStatement(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol field)
    {
        var typeMemberAccess = MemberAccess(
            IdentifierName(field.ContainingType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()),
            field.Name);
        var pattern = ConstantPattern(NameOf(typeMemberAccess));
        return SwitchExpressionArm(pattern, typeMemberAccess);
    }
}
