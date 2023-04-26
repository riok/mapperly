using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

public class TypeMappingBuildContext
{
    public TypeMappingBuildContext(string source, string? referenceHandler, UniqueNameBuilder nameBuilder)
        : this(IdentifierName(source), referenceHandler == null ? null : IdentifierName(referenceHandler), nameBuilder) { }

    private TypeMappingBuildContext(ExpressionSyntax source, ExpressionSyntax? referenceHandler, UniqueNameBuilder nameBuilder)
    {
        Source = source;
        ReferenceHandler = referenceHandler;
        NameBuilder = nameBuilder;
    }

    public UniqueNameBuilder NameBuilder { get; }

    public ExpressionSyntax Source { get; }

    public ExpressionSyntax? ReferenceHandler { get; }

    public TypeMappingBuildContext WithSource(ExpressionSyntax source) => new(source, ReferenceHandler, NameBuilder);

    public TypeMappingBuildContext WithSource(string source) => WithSource(IdentifierName(source));

    public TypeMappingBuildContext WithRefHandler(string refHandler) => WithRefHandler(IdentifierName(refHandler));

    public TypeMappingBuildContext WithRefHandler(ExpressionSyntax refHandler) => new(Source, refHandler, NameBuilder);
}
