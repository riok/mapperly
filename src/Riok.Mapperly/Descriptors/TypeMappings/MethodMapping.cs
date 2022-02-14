using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
public abstract class MethodMapping : TypeMapping
{
    private const string SourceParamName = "source";
    private const string MappingMethodNamePrefix = "MapTo";

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    protected Accessibility Accessibility { get; set; } = Accessibility.Private;

    protected bool Override { get; set; }

    protected virtual string MethodName => MappingMethodNamePrefix + TargetType.NonNullable().Name;

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => Invocation(MethodName, source);

    public MethodDeclarationSyntax BuildMethod()
    {
        TypeSyntax returnType = ReturnType == null
            ? PredefinedType(Token(SyntaxKind.VoidKeyword))
            : IdentifierName(TargetType.ToDisplayString());

        return MethodDeclaration(returnType, Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers()))
            .WithParameterList(BuildParameterList())
            .WithBody(Block(BuildBody(IdentifierName(SourceParamName))))
            .WithAttributeLists(List(BuildAttributes(SourceParamName)));
    }

    public abstract IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source);

    protected virtual ITypeSymbol? ReturnType => TargetType;

    protected virtual IEnumerable<ParameterSyntax> BuildParameters()
    {
        return new[]
        {
            Parameter(Identifier(SourceParamName)).WithType(IdentifierName(SourceType.ToDisplayString())),
        };
    }

    private IEnumerable<SyntaxToken> BuildModifiers()
    {
        yield return Accessibility(Accessibility);

        if (Override)
            yield return Token(SyntaxKind.OverrideKeyword);
    }

    private ParameterListSyntax BuildParameterList()
    {
        return ParameterList(CommaSeparatedList(BuildParameters()));
    }

    private IEnumerable<AttributeListSyntax> BuildAttributes(string sourceParamName)
    {
        // if target and source types are nullable we add a [return: NotNullIfNotNull("source")] annotation
        if (TargetType.NullableAnnotation == NullableAnnotation.Annotated
            && SourceType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            yield return ReturnNotNullIfNotNullAttribute(sourceParamName);
        }
    }
}
