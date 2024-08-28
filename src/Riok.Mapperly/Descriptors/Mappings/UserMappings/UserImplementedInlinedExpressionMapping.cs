using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// An inlined version of a <see cref="UserImplementedMethodMapping"/>.
/// Does not support reference handling and has several other limitations,
/// <see cref="InlineExpressionRewriter"/>.
/// </summary>
/// <param name="userMapping">The original user mapping.</param>
/// <param name="sourceParameter">The source parameter of the user mapping. This will probably be rewritten when inlining.</param>
/// <param name="mappingInvocations">Mapping invocations to be inlined.</param>
/// <param name="mappingBody">The prepared user written mapping body code (rewritten by <see cref="InlineExpressionRewriter"/>.</param>
public class UserImplementedInlinedExpressionMapping(
    UserImplementedMethodMapping userMapping,
    ParameterSyntax sourceParameter,
    IReadOnlyDictionary<SyntaxAnnotation, INewInstanceMapping> mappingInvocations,
    ExpressionSyntax mappingBody
) : NewInstanceMapping(userMapping.SourceType, userMapping.TargetType), INewInstanceMapping
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var body = InlineUserMappings(ctx, mappingBody);
        return ReplaceSource(ctx, body);
    }

    private ExpressionSyntax InlineUserMappings(TypeMappingBuildContext ctx, ExpressionSyntax body)
    {
        var invocations = body.GetAnnotatedNodes(InlineExpressionRewriter.SyntaxAnnotationKindMapperInvocation)
            .OfType<InvocationExpressionSyntax>();
        return body.ReplaceNodes(invocations, (invocation, _) => InlineMapping(ctx, invocation));
    }

    private ExpressionSyntax InlineMapping(TypeMappingBuildContext ctx, InvocationExpressionSyntax invocation)
    {
        var annotation = invocation.GetAnnotations(InlineExpressionRewriter.SyntaxAnnotationKindMapperInvocation).FirstOrDefault();
        if (!mappingInvocations.TryGetValue(annotation, out var mapping))
            return invocation;

        return mapping.Build(ctx.WithSource(invocation.ArgumentList.Arguments[0].Expression));
    }

    private ExpressionSyntax ReplaceSource(TypeMappingBuildContext ctx, ExpressionSyntax body)
    {
        // include self since the method could just be TTarget MyMapping(TSource source) => source;
        // do not further descend if the source parameter is hidden
        var identifierNodes = body.DescendantNodesAndSelf(n => !IsSourceParameterHidden(n))
            .OfType<IdentifierNameSyntax>()
            .Where(x => x.Identifier.Text.Equals(sourceParameter.Identifier.Text, StringComparison.Ordinal));
        return body.ReplaceNodes(identifierNodes, (n, _) => ctx.Source.WithTriviaFrom(n));
    }

    private bool IsSourceParameterHidden(SyntaxNode node)
    {
        return ExtractOverwrittenIdentifiers(node).Any(x => x.Text.Equals(sourceParameter.Identifier.Text, StringComparison.Ordinal));
    }

    private IEnumerable<SyntaxToken> ExtractOverwrittenIdentifiers(SyntaxNode node)
    {
        return node switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => [simpleLambda.Parameter.Identifier],
            ParenthesizedLambdaExpressionSyntax lambda => lambda.ParameterList.Parameters.Select(p => p.Identifier),
            _ => [],
        };
    }
}
