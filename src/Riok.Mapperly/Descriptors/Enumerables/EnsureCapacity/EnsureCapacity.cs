using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

public abstract class EnsureCapacity
{
    private const string EnsureCapacityName = "EnsureCapacity";

    public abstract StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target);

    protected static ExpressionStatementSyntax EnsureCapacityStatement(ExpressionSyntax target, ExpressionSyntax sourceCount, ExpressionSyntax targetCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }
}
