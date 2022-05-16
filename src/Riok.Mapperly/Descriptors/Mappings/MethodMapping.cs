using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
public abstract class MethodMapping : TypeMapping
{
    private const string DefaultSourceParamName = "source";
    private string? _methodName;

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    protected Accessibility Accessibility { get; set; } = Accessibility.Private;

    protected bool IsPartial { get; set; }

    protected bool IsExtensionMethod { get; set; }

    protected string MethodName
    {
        get => _methodName ?? throw new InvalidOperationException();
        set => _methodName = value;
    }

    protected string MappingSourceParameterName
    {
        get;
        set;
    } = DefaultSourceParamName;

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => Invocation(MethodName, source);

    public MethodDeclarationSyntax BuildMethod(SourceEmitterContext context)
    {
        TypeSyntax returnType = ReturnType == null
            ? PredefinedType(Token(SyntaxKind.VoidKeyword))
            : IdentifierName(TargetType.ToDisplayString());

        return MethodDeclaration(returnType, Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers(context.IsStatic)))
            .WithParameterList(BuildParameterList())
            .WithBody(Block(BuildBody(IdentifierName(MappingSourceParameterName))));
    }

    public abstract IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source);

    internal void SetMethodNameIfNeeded(Func<MethodMapping, string> methodNameBuilder)
    {
        _methodName ??= methodNameBuilder(this);
    }

    protected virtual ITypeSymbol? ReturnType => TargetType;

    protected virtual IEnumerable<ParameterSyntax> BuildParameters()
    {
        return new[]
        {
            Parameter(Identifier(MappingSourceParameterName))
                .WithType(IdentifierName(SourceType.ToDisplayString()))
                .WithModifiers(TokenList(BuildParameterModifiers())),
        };
    }

    private IEnumerable<SyntaxToken> BuildParameterModifiers()
    {
        if (IsExtensionMethod)
            yield return Token(SyntaxKind.ThisKeyword);
    }

    private IEnumerable<SyntaxToken> BuildModifiers(bool isStatic)
    {
        yield return Accessibility(Accessibility);

        if (isStatic)
            yield return Token(SyntaxKind.StaticKeyword);

        if (IsPartial)
            yield return Token(SyntaxKind.PartialKeyword);
    }

    private ParameterListSyntax BuildParameterList()
    {
        return ParameterList(CommaSeparatedList(BuildParameters()));
    }
}
