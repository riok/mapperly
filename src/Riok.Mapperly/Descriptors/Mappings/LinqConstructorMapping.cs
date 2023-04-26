using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping where the target type accepts IEnumerable as a single argument.
/// </summary>
public class LinqConstructorMapping : TypeMapping
{
    private const string LambdaParamName = "x";

    private readonly ITypeMapping _elementMapping;
    private readonly IMethodSymbol? _selectMethod;

    public LinqConstructorMapping(ITypeSymbol sourceType, ITypeSymbol targetType, ITypeMapping elementMapping, IMethodSymbol? selectMethod)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _selectMethod = selectMethod;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var scopedNameBuilder = ctx.NameBuilder.NewScope();
        var lambdaParamName = scopedNameBuilder.New(LambdaParamName);

        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (_selectMethod != null)
        {
            var sourceMapExpression = _elementMapping.Build(ctx.WithSource(lambdaParamName));
            var convertLambda = SimpleLambdaExpression(Parameter(Identifier(lambdaParamName))).WithExpressionBody(sourceMapExpression);
            mappedSource = StaticInvocation(_selectMethod, ctx.Source, convertLambda);
        }
        else
        {
            mappedSource = _elementMapping.Build(ctx);
        }

        return CreateInstance(TargetType, mappedSource);
    }
}
