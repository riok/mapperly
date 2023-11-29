using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

public readonly record struct TypeMappingBuildContext
{
    private const string DefaultSourceName = "x";

    public TypeMappingBuildContext(
        string source,
        string? referenceHandler,
        UniqueNameBuilder nameBuilder,
        SyntaxFactoryHelper syntaxFactory
    )
        : this(IdentifierName(source), referenceHandler == null ? null : IdentifierName(referenceHandler), nameBuilder, syntaxFactory) { }

    private TypeMappingBuildContext(
        ExpressionSyntax source,
        ExpressionSyntax? referenceHandler,
        UniqueNameBuilder nameBuilder,
        SyntaxFactoryHelper syntaxFactory
    )
    {
        Source = source;
        ReferenceHandler = referenceHandler;
        NameBuilder = nameBuilder;
        SyntaxFactory = syntaxFactory;
    }

    public UniqueNameBuilder NameBuilder { get; }

    public ExpressionSyntax Source { get; }

    public ExpressionSyntax? ReferenceHandler { get; }

    public SyntaxFactoryHelper SyntaxFactory { get; }

    public TypeMappingBuildContext AddIndentation() => new(Source, ReferenceHandler, NameBuilder, SyntaxFactory.AddIndentation());

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
        var ctx = new TypeMappingBuildContext(sourceBuilder(scopedSourceName), ReferenceHandler, scopedNameBuilder, SyntaxFactory);
        return (ctx, scopedSourceName);
    }

    public (TypeMappingBuildContext Context, string SourceName) WithNewSource(string sourceName = DefaultSourceName)
    {
        var scopedSourceName = NameBuilder.New(sourceName);
        return (WithSource(IdentifierName(scopedSourceName)), scopedSourceName);
    }

    public TypeMappingBuildContext WithSource(ExpressionSyntax source) => new(source, ReferenceHandler, NameBuilder, SyntaxFactory);

    public TypeMappingBuildContext WithRefHandler(string refHandler) => WithRefHandler(IdentifierName(refHandler));

    public TypeMappingBuildContext WithRefHandler(ExpressionSyntax refHandler) => new(Source, refHandler, NameBuilder, SyntaxFactory);
}
