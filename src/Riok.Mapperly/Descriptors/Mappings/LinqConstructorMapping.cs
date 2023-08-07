using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping where the target type accepts IEnumerable as a single argument.
/// </summary>
public class LinqConstructorMapping : NewInstanceMapping
{
    private readonly INamedTypeSymbol _targetTypeToConstruct;
    private readonly INewInstanceMapping _elementMapping;
    private readonly string? _selectMethod;

    public LinqConstructorMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INamedTypeSymbol targetTypeToConstruct,
        INewInstanceMapping elementMapping,
        string? selectMethod
    )
        : base(sourceType, targetType)
    {
        _targetTypeToConstruct = targetTypeToConstruct;
        _elementMapping = elementMapping;
        _selectMethod = selectMethod;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (_selectMethod != null)
        {
            var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();
            var sourceMapExpression = _elementMapping.Build(lambdaCtx);
            var convertLambda = SimpleLambdaExpression(Parameter(Identifier(lambdaSourceName))).WithExpressionBody(sourceMapExpression);
            mappedSource = Invocation(_selectMethod, ctx.Source, convertLambda);
        }
        else
        {
            mappedSource = _elementMapping.Build(ctx);
        }

        return CreateInstance(_targetTypeToConstruct, mappedSource);
    }
}
