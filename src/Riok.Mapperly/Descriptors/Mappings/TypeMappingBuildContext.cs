using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

public class TypeMappingBuildContext
{
    private const string DefaultSourceName = "x";

    public TypeMappingBuildContext(string source, string? referenceHandler, IEnumerable<string> parameters, UniqueNameBuilder nameBuilder)
        : this(
            IdentifierName(source),
            referenceHandler == null ? null : IdentifierName(referenceHandler),
            parameters.Select(IdentifierName).ToArray(),
            nameBuilder
        ) { }

    private TypeMappingBuildContext(
        ExpressionSyntax source,
        ExpressionSyntax? referenceHandler,
        ExpressionSyntax[] parameters,
        UniqueNameBuilder nameBuilder
    )
    {
        Source = source;
        ReferenceHandler = referenceHandler;
        Parameters = parameters;
        NameBuilder = nameBuilder;
    }

    public UniqueNameBuilder NameBuilder { get; }

    public ExpressionSyntax Source { get; }

    public ExpressionSyntax? ReferenceHandler { get; }

    public ExpressionSyntax[] Parameters { get; }

    /// <summary>
    /// Creates a new scoped name builder,
    /// builds the name of the source in this new scope
    /// and creates a new context with the new source.
    /// </summary>
    /// <returns>The new context and the scoped name of the source.</returns>
    public (TypeMappingBuildContext Context, string SourceName) WithNewScopedSource() => WithNewScopedSource(IdentifierName);

    /// <summary>
    /// Creates a new scoped name builder,
    /// builds the name of the source in this new scope
    /// and creates a new context with the new source.
    /// </summary>
    /// <param name="sourceBuilder">A function to build the source access for the new context.</param>
    /// <returns>The new context and the scoped name of the source.</returns>
    public (TypeMappingBuildContext Context, string SourceName) WithNewScopedSource(Func<string, ExpressionSyntax> sourceBuilder)
    {
        var scopedNameBuilder = NameBuilder.NewScope();
        var scopedSourceName = scopedNameBuilder.New(DefaultSourceName);
        var ctx = new TypeMappingBuildContext(sourceBuilder(scopedSourceName), ReferenceHandler, Parameters, scopedNameBuilder);
        return (ctx, scopedSourceName);
    }

    public (TypeMappingBuildContext Context, string SourceName) WithNewSource(string sourceName = DefaultSourceName)
    {
        var scopedSourceName = NameBuilder.New(sourceName);
        return (WithSource(IdentifierName(scopedSourceName)), scopedSourceName);
    }

    public TypeMappingBuildContext WithSource(ExpressionSyntax source) => new(source, ReferenceHandler, Parameters, NameBuilder);

    public TypeMappingBuildContext WithRefHandler(string refHandler) => WithRefHandler(IdentifierName(refHandler));

    public TypeMappingBuildContext WithRefHandler(ExpressionSyntax refHandler) => new(Source, refHandler, Parameters, NameBuilder);
}
