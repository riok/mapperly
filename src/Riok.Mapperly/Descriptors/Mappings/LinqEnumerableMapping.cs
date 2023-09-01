using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping which works by using linq (select + collect).
/// </summary>
public class LinqEnumerableMapping : NewInstanceMapping
{
    private readonly INewInstanceMapping _elementMapping;
    private readonly string? _selectMethod;
    private readonly string? _collectMethod;

    public LinqEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        string? selectMethod,
        string? collectMethod
    )
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _selectMethod = selectMethod;
        _collectMethod = collectMethod;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (_selectMethod != null)
        {
            var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();
            var sourceMapExpression = _elementMapping.Build(lambdaCtx);
            var convertLambda = Lambda(lambdaSourceName, sourceMapExpression);
            mappedSource = Invocation(_selectMethod, ctx.Source, convertLambda);
        }
        else
        {
            mappedSource = _elementMapping.Build(ctx);
        }

        return _collectMethod == null ? mappedSource : Invocation(_collectMethod, mappedSource);
    }
}
