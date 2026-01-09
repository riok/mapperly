using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
        var rewriter = new InliningRewriter(ctx, sourceParameter, mappingInvocations, ctx.NameBuilder);
        return (ExpressionSyntax)rewriter.Visit(mappingBody);
    }

    private class InliningRewriter(
        TypeMappingBuildContext ctx,
        ParameterSyntax sourceParameter,
        IReadOnlyDictionary<SyntaxAnnotation, INewInstanceMapping> mappingInvocations,
        UniqueNameBuilder nameBuilder,
        Dictionary<string, string>? replacements = null
    ) : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _replacements =
            replacements != null ? new Dictionary<string, string>(replacements) : new();

        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var scopedNameBuilder = nameBuilder.NewScope();
            var rewriter = new InliningRewriter(ctx, sourceParameter, mappingInvocations, scopedNameBuilder, _replacements);

            var oldName = node.Parameter.Identifier.Text;
            var isRenamed = scopedNameBuilder.NewIfNeeded(oldName, out var newName);
            rewriter._replacements[oldName] = newName;

            var newBody = (CSharpSyntaxNode)rewriter.Visit(node.Body);
            var newNode = node.WithBody(newBody);
            if (isRenamed)
            {
                newNode = newNode.WithParameter(
                    node.Parameter.WithIdentifier(Identifier(newName).WithTriviaFrom(node.Parameter.Identifier))
                );
            }

            return newNode;
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var scopedNameBuilder = nameBuilder.NewScope();
            var rewriter = new InliningRewriter(ctx, sourceParameter, mappingInvocations, scopedNameBuilder, _replacements);

            var updatedParameters = new List<ParameterSyntax>(node.ParameterList.Parameters.Count);
            var anyRenamed = false;

            foreach (var parameter in node.ParameterList.Parameters)
            {
                var oldName = parameter.Identifier.Text;
                var isRenamed = scopedNameBuilder.NewIfNeeded(oldName, out var newName);
                rewriter._replacements[oldName] = newName;

                if (isRenamed)
                {
                    updatedParameters.Add(parameter.WithIdentifier(Identifier(newName).WithTriviaFrom(parameter.Identifier)));
                    anyRenamed = true;
                }
                else
                {
                    updatedParameters.Add(parameter);
                }
            }

            var newBody = (CSharpSyntaxNode)rewriter.Visit(node.Body);

            var newNode = node.WithBody(newBody);
            if (anyRenamed)
            {
                newNode = newNode.WithParameterList(
                    node.ParameterList.WithParameters(SeparatedList(updatedParameters, node.ParameterList.Parameters.GetSeparators()))
                );
            }

            return newNode;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            // replace parameter names
            if (_replacements.TryGetValue(node.Identifier.Text, out var newName))
            {
                if (string.Equals(newName, node.Identifier.Text, StringComparison.Ordinal))
                    return node;

                return node.WithIdentifier(Identifier(newName).WithTriviaFrom(node.Identifier));
            }

            // replace source parameter
            if (node.Identifier.Text.Equals(sourceParameter.Identifier.Text, StringComparison.Ordinal))
            {
                return ctx.Source.WithTriviaFrom(node);
            }

            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var annotation = node.GetAnnotations(InlineExpressionRewriter.SyntaxAnnotationKindMapperInvocation).FirstOrDefault();
            if (annotation == null || !mappingInvocations.TryGetValue(annotation, out var mapping))
            {
                return base.VisitInvocationExpression(node);
            }

            var argument = node.ArgumentList.Arguments[0];
            var visitedArgument = (ArgumentSyntax)Visit(argument);
            return mapping.Build(ctx.WithSource(visitedArgument.Expression));
        }
    }
}
