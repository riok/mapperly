using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Emit.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

public abstract class EnsureCapacityInfo
{
    private const string EnsureCapacityName = "EnsureCapacity";

    public abstract StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target);

    protected static ExpressionStatementSyntax EnsureCapacityStatement(
        SyntaxFactoryHelper syntaxFactory,
        ExpressionSyntax target,
        ExpressionSyntax sourceCount,
        ExpressionSyntax targetCount
    )
    {
        var sum = Add(sourceCount, targetCount);
        return syntaxFactory.ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sum));
    }
}
