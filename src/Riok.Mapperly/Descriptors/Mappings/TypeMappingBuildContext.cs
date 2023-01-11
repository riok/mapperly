using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

public readonly struct TypeMappingBuildContext
{
    public TypeMappingBuildContext(string source, string? referenceHandler)
        : this(IdentifierName(source), referenceHandler == null ? null : IdentifierName(referenceHandler))
    {
    }

    private TypeMappingBuildContext(ExpressionSyntax source, ExpressionSyntax? referenceHandler)
    {
        Source = source;
        ReferenceHandler = referenceHandler;
    }

    public ExpressionSyntax Source { get; }

    public ExpressionSyntax? ReferenceHandler { get; }

    public TypeMappingBuildContext WithSource(ExpressionSyntax source)
        => new(source, ReferenceHandler);

    public TypeMappingBuildContext WithSource(string source)
        => WithSource(IdentifierName(source));

    public TypeMappingBuildContext WithRefHandler(string refHandler)
        => WithRefHandler(IdentifierName(refHandler));

    public TypeMappingBuildContext WithRefHandler(ExpressionSyntax refHandler)
        => new(Source, refHandler);
}
