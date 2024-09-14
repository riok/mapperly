using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

public class EnumFallbackToStringMapping(ITypeSymbol source, ITypeSymbol target) : NewInstanceMapping(source, target)
{
    private const string ToStringMethodName = nameof(Enum.ToString);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        ctx.SyntaxFactory.Invocation(MemberAccess(ctx.Source, ToStringMethodName));
}
