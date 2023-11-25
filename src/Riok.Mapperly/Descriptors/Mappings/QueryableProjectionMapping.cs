using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A projections queryable mapping
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class QueryableProjectionMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : MethodMapping(sourceType, targetType)
{
    private const string QueryableReceiverName = "System.Linq.Queryable";
    private const string SelectMethodName = nameof(Queryable.Select);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var innerCtx = ctx.WithNoSource();
        var delegateMappingSyntax = delegateMapping.Build(innerCtx);
        var select = StaticInvocation(QueryableReceiverName, SelectMethodName, ctx.Source, delegateMappingSyntax);
        var returnStatement = ctx.SyntaxFactory.Return(select);
        return new[] { returnStatement };
    }
}
