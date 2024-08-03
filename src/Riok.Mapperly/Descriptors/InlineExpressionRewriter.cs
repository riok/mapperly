using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// Prepares an expression body to be used in an inlined expression mapping.
/// * annotates <see cref="InvocationExpressionSyntax"/> which target another mapping with a <see cref="SyntaxAnnotation"/> of kind <see cref="SyntaxAnnotationKindMapperInvocation"/>. <seealso cref="MappingInvocations"/>
/// * expands type names to their fully qualified names
/// * expands extension method invocations
/// * expands static method invocation receiver type names
/// Note: does not support method groups.
/// </summary>
/// <param name="semanticModel">The semantic model</param>
/// <param name="mappingResolver">To resolve mappings for a given method invocation</param>
public class InlineExpressionRewriter(SemanticModel semanticModel, Func<IMethodSymbol, INewInstanceMapping?> mappingResolver)
    : CSharpSyntaxRewriter
{
    public const string SyntaxAnnotationKindMapperInvocation = "mapperInvocation";

    private readonly Dictionary<SyntaxAnnotation, INewInstanceMapping> _mappingInvocations = new();

    /// <summary>
    /// A dictionary which maps annotations to the matching mappings.
    /// Each annotation is attached to a <see cref="InvocationExpressionSyntax"/>.
    /// </summary>
    public IReadOnlyDictionary<SyntaxAnnotation, INewInstanceMapping> MappingInvocations => _mappingInvocations;

    /// <summary>
    /// Whether the processed expression can successfully be inlined.
    /// The expression needs to follow all the expression tree limitations
    /// listed here: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/expression-tree-restrictions#expression-tree-restrictions.
    /// </summary>
    public bool CanBeInlined { get; private set; } = true;

    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var fullyQualified = FullyQualifiedParentType(node.Type);
        node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node)!;
        if (fullyQualified != null)
        {
            node = node.WithType(fullyQualified);
        }

        return node;
    }

    public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
    {
        var args = node.Arguments.Select(a => FullyQualifiedParentType(a) ?? a);
        return node.WithArguments(SeparatedList(args));
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        if (semanticModel.GetSymbolInfo(node.Expression).Symbol is ITypeSymbol namedTypeSymbol)
        {
            var expression = FullyQualifiedIdentifier(namedTypeSymbol).WithTriviaFrom(node.Expression);
            return node.WithExpression(expression);
        }

        return base.VisitMemberAccessExpression(node);
    }

    public override SyntaxNode? VisitCastExpression(CastExpressionSyntax node)
    {
        var result = base.VisitCastExpression(node);

        if (result is CastExpressionSyntax typedResult && semanticModel.GetSymbolInfo(node.Type).Symbol is ITypeSymbol namedTypeSymbol)
        {
            var fullyQualifiedType = FullyQualifiedIdentifier(namedTypeSymbol);

            return typedResult.WithType(fullyQualifiedType.WithTriviaFrom(typedResult.Type));
        }

        return result;
    }

    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var result = base.VisitBinaryExpression(node);

        if (
            result is BinaryExpressionSyntax typedResult
            && typedResult.IsKind(SyntaxKind.AsExpression)
            && semanticModel.GetSymbolInfo(node.Right).Symbol is ITypeSymbol namedTypeSymbol
        )
        {
            var fullyQualifiedType = FullyQualifiedIdentifier(namedTypeSymbol);

            return typedResult.WithRight(fullyQualifiedType.WithTriviaFrom(typedResult.Right));
        }

        return result;
    }

    public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        if (
            semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol
            {
                MethodKind: MethodKind.Constructor,
                ReceiverType: not null
            } ctorSymbol
        )
        {
            return ObjectCreationExpression(
                TrailingSpacedToken(SyntaxKind.NewKeyword),
                FullyQualifiedIdentifier(ctorSymbol.ReceiverType),
                (ArgumentListSyntax?)base.VisitArgumentList(node.ArgumentList),
                node.Initializer == null ? null : (InitializerExpressionSyntax?)base.VisitInitializerExpression(node.Initializer)
            );
        }

        return node;
    }

    public override SyntaxNode VisitArrayType(ArrayTypeSyntax node)
    {
        var fullyQualifiedElementType = FullyQualifiedType(node.ElementType);
        if (fullyQualifiedElementType != null)
        {
            node = node.WithElementType(fullyQualifiedElementType);
        }

        return node;
    }

    public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        var fullyQualifiedElementType = FullyQualifiedParentType(node.Type);
        if (fullyQualifiedElementType != null)
        {
            node = node.WithType(fullyQualifiedElementType);
        }

        return node;
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // expand static method invocation type names
        // and extension method invocations
        if (semanticModel.GetSymbolInfo(node).Symbol is not IMethodSymbol methodSymbol)
            return base.VisitInvocationExpression(node);

        if (node.ArgumentList.Arguments.Count == 1 && mappingResolver.Invoke(methodSymbol) is { } mapping)
        {
            var annotation = new SyntaxAnnotation(SyntaxAnnotationKindMapperInvocation);
            _mappingInvocations.Add(annotation, mapping);
            node = node.WithAdditionalAnnotations(annotation);
        }

        return methodSymbol switch
        {
            { IsExtensionMethod: true } => VisitExtensionMethodInvocation(node, methodSymbol),
            { IsStatic: true } => VisitStaticMethodInvocation(node, methodSymbol),
            _ => base.VisitInvocationExpression(node),
        };
    }

    public override SyntaxNode? VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        // CS8072
        CanBeInlined = false;
        return base.VisitConditionalAccessExpression(node);
    }

    public override SyntaxNode? VisitWithExpression(WithExpressionSyntax node)
    {
        // CS8849
        CanBeInlined = false;
        return base.VisitWithExpression(node);
    }

    public override SyntaxNode? VisitBaseExpression(BaseExpressionSyntax node)
    {
        // CS0831
        CanBeInlined = false;
        return base.VisitBaseExpression(node);
    }

#if ROSLYN4_7_OR_GREATER
    public override SyntaxNode VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        // CS9175
        CanBeInlined = false;
        return node;
    }
#endif

    public override SyntaxNode VisitRangeExpression(RangeExpressionSyntax node)
    {
        // CS8792
        CanBeInlined = false;
        return node;
    }

    public override SyntaxNode VisitTupleExpression(TupleExpressionSyntax node)
    {
        // CS8143
        CanBeInlined = false;
        return node;
    }

    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        // CS8514
        CanBeInlined = false;
        return node;
    }

    public override SyntaxNode VisitThrowExpression(ThrowExpressionSyntax node)
    {
        // CS8188
        CanBeInlined = false;
        return node;
    }

    private static InvocationExpressionSyntax VisitStaticMethodInvocation(InvocationExpressionSyntax node, IMethodSymbol methodSymbol)
    {
        var receiverType = FullyQualifiedIdentifier(methodSymbol.ReceiverType!);
        var expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiverType, IdentifierName(methodSymbol.Name));
        return node.WithExpression(expression);
    }

    private InvocationExpressionSyntax VisitExtensionMethodInvocation(InvocationExpressionSyntax node, IMethodSymbol methodSymbol)
    {
        if (node.Expression is not MemberAccessExpressionSyntax memberAccess)
            return node;

        var receiverArgument = (ExpressionSyntax)Visit(memberAccess.Expression);
        var arguments = (ArgumentListSyntax)Visit(node.ArgumentList);

        var args = new List<ArgumentSyntax>(arguments.Arguments.Count + 1);
        args.Add(Argument(receiverArgument.WithoutTrivia()));
        args.AddRange(arguments.Arguments);

        var extensionMethodContainingType = FullyQualifiedIdentifier(methodSymbol.ReducedFrom!.ReceiverType!);
        var expression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            extensionMethodContainingType,
            IdentifierName(methodSymbol.Name)
        );
        return node.Update(expression, ArgumentList(CommaSeparatedList(args)));
    }

    private IdentifierNameSyntax? FullyQualifiedParentType(TypeSyntax typeSyntax)
    {
        var parent = typeSyntax.Parent;
        if (parent == null)
            return null;

        var type = semanticModel.GetTypeInfo(parent).Type;
        return type != null ? FullyQualifiedIdentifier(type).WithTriviaFrom(typeSyntax) : null;
    }

    private IdentifierNameSyntax? FullyQualifiedType(TypeSyntax typeSyntax)
    {
        var type = semanticModel.GetTypeInfo(typeSyntax).Type;
        return type != null ? FullyQualifiedIdentifier(type).WithTriviaFrom(typeSyntax) : null;
    }
}
