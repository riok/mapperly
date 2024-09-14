using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

public sealed record EnumMemberMapping(ExpressionSyntax SourceSyntax, ExpressionSyntax TargetSyntax)
{
    public SwitchExpressionArmSyntax BuildSwitchArm() => SwitchArm(ConstantPattern(SourceSyntax), TargetSyntax);
}
