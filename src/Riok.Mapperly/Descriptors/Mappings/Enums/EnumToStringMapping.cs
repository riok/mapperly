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
    IEnumerable<IFieldSymbol> enumMembers,
    IReadOnlyDictionary<IFieldSymbol, ExpressionSyntax> explicitMappings
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string ToStringMethodName = nameof(Enum.ToString);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // fallback switch arm: _ => source.ToString()
        var fallbackArm = SwitchArm(DiscardPattern(), ctx.SyntaxFactory.Invocation(MemberAccess(ctx.Source, ToStringMethodName)));

        // switch for each name to the enum value
        // eg: Enum1.Value1 => "Value1"
        // or: Enum1.Value1 => "value_1" (if explicit mapping exists)
        var arms = enumMembers.Select(BuildArm).Append(fallbackArm);
        var switchExpr = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol field)
    {
        // source.Value1
        var typeMemberAccess = MemberAccess(FullyQualifiedIdentifier(field.ContainingType.NonNullable()), field.Name);
        var pattern = ConstantPattern(typeMemberAccess);
        if (explicitMappings.TryGetValue(field, out var expression))
        {
            // source.Value1 => "VALUE-1"
            return SwitchArm(pattern, expression);
        }

        // nameof(source.Value1)
        var nameOf = NameOf(typeMemberAccess);
        // source.Value1 => nameof(source.Value1)
        return SwitchArm(pattern, nameOf);
    }
}
