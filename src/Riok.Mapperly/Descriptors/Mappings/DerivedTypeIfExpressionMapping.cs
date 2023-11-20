using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A derived type mapping maps one base type or interface to another
/// by implementing a if with instance checks over known types and performs the provided mapping for each type.
/// </summary>
public class DerivedTypeIfExpressionMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    IReadOnlyCollection<INewInstanceMapping> typeMappings
) : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // source is A x ? MapToA(x) : <other cases>
        var typeExpressions = typeMappings
            .Reverse()
            .Aggregate<INewInstanceMapping, ExpressionSyntax>(
                DefaultLiteral(),
                (aggregate, current) => BuildConditional(ctx, aggregate, current)
            );

        // cast to target type, to ensure the compiler picks the correct type
        // (B)(<ifs...>
        return CastExpression(FullyQualifiedIdentifier(TargetType), ParenthesizedExpression(typeExpressions));
    }

    private ConditionalExpressionSyntax BuildConditional(
        TypeMappingBuildContext ctx,
        ExpressionSyntax notMatched,
        INewInstanceMapping mapping
    )
    {
        // cannot use is pattern matching is operator due to expression limitations
        // use is with a cast instead
        // source is A ? MapToB((A)x) : <other cases>
        var castedSourceContext = ctx.WithSource(
            ParenthesizedExpression(CastExpression(FullyQualifiedIdentifier(mapping.SourceType), ctx.Source))
        );
        var condition = Is(ctx.Source, FullyQualifiedIdentifier(mapping.SourceType));
        return Conditional(condition, mapping.Build(castedSourceContext), notMatched);
    }
}
