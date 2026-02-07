using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class InlineExpressionMappingBuilder
{
    /// <summary>
    /// Builds an inline expression delegate mapping for the given source and target types.
    /// This is used by both queryable projection mappings and expression mappings.
    /// </summary>
    /// <param name="ctx">The mapping builder context.</param>
    /// <param name="sourceType">The source type to map from.</param>
    /// <param name="targetType">The target type to map to.</param>
    /// <returns>The built mapping, or <see langword="null"/> if no mapping could be created.</returns>
    public static INewInstanceMapping? TryBuildInlineMappingForExpression(
        MappingBuilderContext ctx,
        ITypeSymbol sourceType,
        ITypeSymbol targetType
    )
    {
        var mappingKey = BuildMappingKey(ctx, sourceType, targetType);
        var userMapping = ctx.FindMapping(sourceType, targetType) as IUserMapping;
        var inlineCtx = new InlineExpressionMappingBuilderContext(ctx, userMapping, mappingKey);

        if (userMapping is UserImplementedMethodMapping && inlineCtx.FindMapping(sourceType, targetType) is { } inlinedUserMapping)
        {
            return inlinedUserMapping;
        }

        var mapping = inlineCtx.BuildMapping(mappingKey, MappingBuildingOptions.KeepUserSymbol);
        if (mapping == null)
            return null;

        if (ctx.Configuration.Mapper.UseReferenceHandling)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling);
        }

        return mapping;
    }

    /// <summary>
    /// Tries to inline a given mapping.
    /// This works as long as the user implemented methods
    /// follow the expression tree limitations:
    /// https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="mapping">The mapping.</param>
    /// <returns>The inlined mapping or <c>null</c> if it could not be inlined.</returns>
    public static INewInstanceMapping? TryBuildMapping(InlineExpressionMappingBuilderContext ctx, UserImplementedMethodMapping mapping)
    {
        if (mapping.Method.DeclaringSyntaxReferences is not [var methodSyntaxRef])
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline, mapping.Method);
            return null;
        }

        var methodSyntax = methodSyntaxRef.GetSyntax();

        if (methodSyntax is not MethodDeclarationSyntax { ParameterList.Parameters: [var sourceParameter] } methodDeclaration)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline, mapping.Method);
            return null;
        }

        var bodyExpression = TryGetBodyExpression(methodDeclaration);
        if (bodyExpression == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline, mapping.Method);
            return null;
        }

        var semanticModel = ctx.GetSemanticModel(methodSyntax.SyntaxTree);
        if (semanticModel is null)
        {
            return null;
        }

        var inlineRewriter = new InlineExpressionRewriter(semanticModel, ctx.FindNewInstanceMapping);
        bodyExpression = (ExpressionSyntax?)bodyExpression.Accept(inlineRewriter);
        if (bodyExpression == null || !inlineRewriter.CanBeInlined)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline, mapping.Method);
            return null;
        }

        return new UserImplementedInlinedExpressionMapping(mapping, sourceParameter, inlineRewriter.MappingInvocations, bodyExpression);
    }

    private static ExpressionSyntax? TryGetBodyExpression(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration switch
        {
            // => expression
            { ExpressionBody: { } body } => body.Expression,

            // { return expression; }
            { Body.Statements: [ReturnStatementSyntax singleStatement] } => singleStatement.Expression,

            // { var dest = expression; return dest; }
            {
                Body.Statements: [
                    LocalDeclarationStatementSyntax
                    {
                        Declaration.Variables: [{ Initializer: { } variableInitializer } variableDeclarator]
                    },
                    ReturnStatementSyntax { Expression: IdentifierNameSyntax identifierName },
                ]
            } when identifierName.Identifier.Value == variableDeclarator.Identifier.Value => variableInitializer.Value,

            _ => null,
        };
    }

    private static TypeMappingKey BuildMappingKey(MappingBuilderContext ctx, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // if nullable reference types are disabled
        // and there was no explicit nullable annotation,
        // the non-nullable variant is used here.
        // Otherwise, this would lead to a select like source.Select(x => x == null ? throw ... : new ...)
        // which is not expected in this case.
        // see also https://github.com/riok/mapperly/issues/1196
        sourceType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(sourceType, ctx.UserMapping?.SourceType);
        targetType = ctx.SymbolAccessor.NonNullableIfNullableReferenceTypesDisabled(targetType, ctx.UserMapping?.TargetType);

        return new TypeMappingKey(sourceType, targetType);
    }
}
