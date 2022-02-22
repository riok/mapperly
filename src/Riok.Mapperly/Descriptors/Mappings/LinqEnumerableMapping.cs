using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping which works by using linq (select + collect).
/// </summary>
public class LinqEnumerableMapping : TypeMapping
{
    private const string LambdaParamName = "x";

    private readonly TypeMapping _elementMapping;
    private readonly IMethodSymbol? _selectMethod;
    private readonly IMethodSymbol? _collectMethod;

    public LinqEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        TypeMapping elementMapping,
        IMethodSymbol? selectMethod,
        IMethodSymbol? collectMethod)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _selectMethod = selectMethod;
        _collectMethod = collectMethod;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (_selectMethod != null)
        {
            var sourceMapExpression = _elementMapping.Build(IdentifierName(LambdaParamName));
            var convertLambda = SimpleLambdaExpression(Parameter(Identifier(LambdaParamName)))
                .WithExpressionBody(sourceMapExpression);
            mappedSource = StaticInvocation(_selectMethod, source, convertLambda);
        }
        else
        {
            mappedSource = _elementMapping.Build(source);
        }

        return _collectMethod == null
            ? mappedSource
            : StaticInvocation(_collectMethod, mappedSource);
    }
}
