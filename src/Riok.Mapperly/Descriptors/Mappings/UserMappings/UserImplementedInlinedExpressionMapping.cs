using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

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
) : NewInstanceMapping(userMapping.SourceType, userMapping.TargetType), INewInstanceUserMapping
{
    public IMethodSymbol Method => userMapping.Method;
    public bool? Default => userMapping.Default;
    public bool IsExternal => userMapping.IsExternal;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var body = InlineUserMappings(ctx, mappingBody);
        body = RenameLambdaParameters(ctx, body);
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

    private ExpressionSyntax RenameLambdaParameters(TypeMappingBuildContext ctx, ExpressionSyntax body)
    {
        var lambdas = body.DescendantNodesAndSelf().OfType<LambdaExpressionSyntax>().ToList();
        if (lambdas.Count == 0)
            return body;

        body = body.ReplaceNodes(
            lambdas,
            (originalLambda, _) =>
                originalLambda switch
                {
                    SimpleLambdaExpressionSyntax simpleLambda => RenameSimpleLambdaParameters(ctx.NameBuilder, simpleLambda),
                    ParenthesizedLambdaExpressionSyntax parenthesizedLambda => RenameParenthesizedLambdaParameters(
                        ctx.NameBuilder,
                        parenthesizedLambda
                    ),
                    _ => originalLambda,
                }
        );

        return body;
    }

    // TODO handle nested shadowing
    private SimpleLambdaExpressionSyntax RenameSimpleLambdaParameters(
        UniqueNameBuilder nameBuilder,
        SimpleLambdaExpressionSyntax originalLambda
    )
    {
        nameBuilder = nameBuilder.NewScope();
        var oldName = originalLambda.Parameter.Identifier.Text;
        var newName = nameBuilder.New(oldName, out var renamed);
        if (!renamed)
            return originalLambda;

        var newParameter = originalLambda.Parameter.WithIdentifier(SyntaxFactory.Identifier(newName));
        var newBody = RenameIdentifiersInLambdaBody(
            originalLambda.Body,
            new Dictionary<string, string>(StringComparer.Ordinal) { [oldName] = newName }
        );
        return originalLambda.WithParameter(newParameter.WithTriviaFrom(originalLambda.Parameter)).WithBody(newBody);
    }

    private ParenthesizedLambdaExpressionSyntax RenameParenthesizedLambdaParameters(
        UniqueNameBuilder nameBuilder,
        ParenthesizedLambdaExpressionSyntax originalLambda
    )
    {
        nameBuilder = nameBuilder.NewScope();
        var nameMappings = BuildNameMappings(nameBuilder, originalLambda.ParameterList.Parameters);
        if (nameMappings.Count == 0)
            return originalLambda;

        var newParameters = originalLambda
            .ParameterList.Parameters.Select(p =>
                p.WithIdentifier(
                    SyntaxFactory
                        .Identifier(nameMappings.GetValueOrDefault(p.Identifier.Text, p.Identifier.Text))
                        .WithTriviaFrom(p.Identifier)
                )
            )
            .ToArray();
        var newParameterList = originalLambda.ParameterList.WithParameters(SyntaxFactory.SeparatedList(newParameters));
        var newBody = RenameIdentifiersInLambdaBody(originalLambda.Body, nameMappings);
        return originalLambda.WithParameterList(newParameterList.WithTriviaFrom(originalLambda.ParameterList)).WithBody(newBody);
    }

    private Dictionary<string, string> BuildNameMappings(UniqueNameBuilder nameBuilder, SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var nameMappings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var parameter in parameters)
        {
            var oldName = parameter.Identifier.Text;
            var newName = nameBuilder.New(oldName, out var renamed);
            if (renamed)
            {
                nameMappings[oldName] = newName;
            }
        }

        return nameMappings;
    }

    private static CSharpSyntaxNode RenameIdentifiersInLambdaBody(CSharpSyntaxNode body, IReadOnlyDictionary<string, string> nameMappings)
    {
        var identifiers = body.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Where(id => nameMappings.ContainsKey(id.Identifier.Text))
            .ToList();

        if (identifiers.Count == 0)
            return body;

        return body.ReplaceNodes(
            identifiers,
            (original, _) =>
                original.WithIdentifier(
                    SyntaxFactory.Identifier(nameMappings[original.Identifier.Text]).WithTriviaFrom(original.Identifier)
                )
        );
    }
}
