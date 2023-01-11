using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
public abstract class MethodMapping : TypeMapping
{
    protected const string DefaultReferenceHandlerParameterName = "refHandler";
    private const string DefaultSourceParameterName = "source";

    private const int SourceParameterIndex = 0;
    private const int ReferenceHandlerParameterIndex = 1;

    private string? _methodName;

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : this(new MethodParameter(SourceParameterIndex, DefaultSourceParameterName, sourceType), targetType)
    {
    }

    protected MethodMapping(MethodParameter sourceParameter, ITypeSymbol targetType)
        : base(sourceParameter.Type, targetType)
    {
        SourceParameter = sourceParameter;
    }

    protected Accessibility Accessibility { get; set; } = Accessibility.Private;

    protected bool IsPartial { get; set; }

    protected bool IsExtensionMethod { get; set; }

    protected string MethodName
    {
        get => _methodName ?? throw new InvalidOperationException();
        set => _methodName = value;
    }

    protected MethodParameter SourceParameter { get; }

    protected MethodParameter? ReferenceHandlerParameter { get; set; }

    protected virtual ITypeSymbol? ReturnType => TargetType;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
        => Invocation(MethodName, SourceParameter.WithArgument(ctx.Source), ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));

    public MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        TypeSyntax returnType = ReturnType == null
            ? PredefinedType(Token(SyntaxKind.VoidKeyword))
            : IdentifierName(TargetType.ToDisplayString());

        var typeMappingBuildContext = new TypeMappingBuildContext(SourceParameter.Name, ReferenceHandlerParameter?.Name);

        return MethodDeclaration(returnType, Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers(ctx.IsStatic)))
            .WithParameterList(BuildParameterList())
            .WithBody(Block(BuildBody(typeMappingBuildContext)));
    }

    public abstract IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx);

    internal void SetMethodNameIfNeeded(Func<MethodMapping, string> methodNameBuilder)
    {
        _methodName ??= methodNameBuilder(this);
    }

    internal virtual void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        ReferenceHandlerParameter ??= new MethodParameter(ReferenceHandlerParameterIndex, DefaultReferenceHandlerParameterName, iReferenceHandlerType);
    }

    protected virtual ParameterListSyntax BuildParameterList()
        => ParameterList(IsExtensionMethod, SourceParameter, ReferenceHandlerParameter);

    private IEnumerable<SyntaxToken> BuildModifiers(bool isStatic)
    {
        yield return Accessibility(Accessibility);

        if (isStatic)
            yield return Token(SyntaxKind.StaticKeyword);

        if (IsPartial)
            yield return Token(SyntaxKind.PartialKeyword);
    }
}
