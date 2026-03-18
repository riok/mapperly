using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

public readonly record struct TypeMappingBuildContext
{
    private const string DefaultSourceName = "x";

    public TypeMappingBuildContext(
        string source,
        string? referenceHandler,
        UniqueNameBuilder nameBuilder,
        SyntaxFactoryHelper syntaxFactory,
        IReadOnlyDictionary<string, ExpressionSyntax>? additionalParameters = null
    )
        : this(
            IdentifierName(source),
            referenceHandler == null ? null : IdentifierName(referenceHandler),
            nameBuilder,
            syntaxFactory,
            additionalParameters
        ) { }

    private TypeMappingBuildContext(
        ExpressionSyntax source,
        ExpressionSyntax? referenceHandler,
        UniqueNameBuilder nameBuilder,
        SyntaxFactoryHelper syntaxFactory,
        IReadOnlyDictionary<string, ExpressionSyntax>? additionalParameters = null
    )
    {
        Source = source;
        ReferenceHandler = referenceHandler;
        NameBuilder = nameBuilder;
        SyntaxFactory = syntaxFactory;
        AdditionalParameters = additionalParameters;
    }

    public UniqueNameBuilder NameBuilder { get; }

    public ExpressionSyntax Source { get; }

    public ExpressionSyntax? ReferenceHandler { get; }

    public SyntaxFactoryHelper SyntaxFactory { get; }

    public IReadOnlyDictionary<string, ExpressionSyntax>? AdditionalParameters { get; }

    public TypeMappingBuildContext AddIndentation() =>
        new(Source, ReferenceHandler, NameBuilder, SyntaxFactory.AddIndentation(), AdditionalParameters);

    /// <summary>
    /// Creates a new scoped name builder,
    /// builds the name of the source in this new scope
    /// and creates a new context with the new source.
    /// </summary>
    /// <param name="sourceName">The name for the new scoped source.</param>
    /// <returns>The new context and the scoped name of the source.</returns>
    public (TypeMappingBuildContext Context, string SourceName) WithNewScopedSource(string sourceName = DefaultSourceName) =>
        WithNewScopedSource(IdentifierName, sourceName);

    /// <summary>
    /// Creates a new scoped name builder,
    /// builds the name of the source in this new scope
    /// and creates a new context with the new source.
    /// </summary>
    /// <param name="sourceBuilder">A function to build the source access for the new context.</param>
    /// <param name="sourceName">The name for the new scoped source.</param>
    /// <returns>The new context and the scoped name of the source.</returns>
    public (TypeMappingBuildContext Context, string SourceName) WithNewScopedSource(
        Func<string, ExpressionSyntax> sourceBuilder,
        string sourceName = DefaultSourceName
    )
    {
        var scopedNameBuilder = NameBuilder.NewScope();
        var scopedSourceName = scopedNameBuilder.New(sourceName);
        var ctx = new TypeMappingBuildContext(
            sourceBuilder(scopedSourceName),
            ReferenceHandler,
            scopedNameBuilder,
            SyntaxFactory,
            AdditionalParameters
        );
        return (ctx, scopedSourceName);
    }

    public (TypeMappingBuildContext Context, string SourceName) WithNewSource(string sourceName = DefaultSourceName)
    {
        var scopedSourceName = NameBuilder.New(sourceName);
        return (WithSource(IdentifierName(scopedSourceName)), scopedSourceName);
    }

    public TypeMappingBuildContext WithSource(ExpressionSyntax source) =>
        new(source, ReferenceHandler, NameBuilder, SyntaxFactory, AdditionalParameters);

    public TypeMappingBuildContext WithRefHandler(string refHandler) => WithRefHandler(IdentifierName(refHandler));

    public TypeMappingBuildContext WithRefHandler(ExpressionSyntax refHandler) =>
        new(Source, refHandler, NameBuilder, SyntaxFactory, AdditionalParameters);

    /// <summary>
    /// Builds arguments for a user-implemented method call by matching each parameter
    /// by ordinal to source, target, referenceHandler, or additional parameters.
    /// </summary>
    public MethodArgument?[] BuildArguments(
        IMethodSymbol? method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        MethodArgument? targetArgument = null
    )
    {
        if (method is null)
            return [sourceParameter.WithArgument(Source), targetArgument, referenceHandlerParameter?.WithArgument(ReferenceHandler)];

        return Arguments(Source, ReferenceHandler, AdditionalParameters).ToArray();

        IEnumerable<MethodArgument?> Arguments(
            ExpressionSyntax? source,
            ExpressionSyntax? refHandler,
            IReadOnlyDictionary<string, ExpressionSyntax>? additionalParams
        )
        {
            foreach (var param in method.Parameters)
            {
                if (param.Ordinal == sourceParameter.Ordinal)
                    yield return sourceParameter.WithArgument(source);
                else if (targetArgument is not null && param.Ordinal == targetArgument.Value.Parameter.Ordinal)
                    yield return targetArgument.Value;
                else if (referenceHandlerParameter is not null && param.Ordinal == referenceHandlerParameter.Value.Ordinal)
                    yield return referenceHandlerParameter.Value.WithArgument(refHandler);
                else if (additionalParams?.TryGetValue(param.Name.TrimStart('@'), out var expr) == true)
                    yield return new MethodParameter(param, param.Type).WithArgument(expr);
            }
        }
    }
}
