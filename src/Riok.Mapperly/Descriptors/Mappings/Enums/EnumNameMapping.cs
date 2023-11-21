using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents a mapping from an enum to another enum by using their names.
/// Uses a switch expression. Only supports defined enum values and no flags.
/// The name has to match exactly.
/// </summary>
public class EnumNameMapping(
    ITypeSymbol source,
    ITypeSymbol target,
    IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> enumMemberMappings,
    EnumFallbackValueMapping fallback
) : MethodMapping(source, target)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // switch for each name to the enum value
        // eg: Enum1.Value1 => Enum2.Value1,
        var arms = enumMemberMappings.Select(x => BuildArm(x.Key, x.Value)).Append(fallback.BuildDiscardArm(ctx));
        var switchExpr = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpr);
    }

    private SwitchExpressionArmSyntax BuildArm(IFieldSymbol sourceMemberField, IFieldSymbol targetMemberField)
    {
        var sourceMember = MemberAccess(FullyQualifiedIdentifier(SourceType), sourceMemberField.Name);
        var targetMember = MemberAccess(FullyQualifiedIdentifier(TargetType), targetMemberField.Name);
        var pattern = ConstantPattern(sourceMember);
        return SwitchArm(pattern, targetMember);
    }
}
