using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
[DebuggerDisplay("{GetType().Name}({SourceType} => {TargetType})")]
public abstract class MethodMapping : ITypeMapping
{
    protected const string DefaultReferenceHandlerParameterName = "refHandler";
    private const string DefaultSourceParameterName = "source";

    private const int SourceParameterIndex = 0;
    private const int ReferenceHandlerParameterIndex = 1;

    private static readonly IEnumerable<SyntaxToken> _privateSyntaxToken = new[] { TrailingSpacedToken(SyntaxKind.PrivateKeyword) };

    private static readonly IEnumerable<SyntaxToken> _privateStaticSyntaxToken = new[]
    {
        TrailingSpacedToken(SyntaxKind.PrivateKeyword),
        TrailingSpacedToken(SyntaxKind.StaticKeyword),
    };

    private readonly ITypeSymbol _returnType;
    private readonly MethodDeclarationSyntax? _methodDeclarationSyntax;

    private string? _methodName;

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        TargetType = targetType;
        SourceParameter = new MethodParameter(SourceParameterIndex, DefaultSourceParameterName, sourceType);
        _returnType = targetType;
    }

    protected MethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        ITypeSymbol targetType
    )
    {
        TargetType = targetType;
        SourceParameter = sourceParameter;
        Method = method;
        IsExtensionMethod = method.IsExtensionMethod;
        ReferenceHandlerParameter = referenceHandlerParameter;
        _methodDeclarationSyntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
        _methodName = method.Name;
        _returnType = method.ReturnsVoid ? method.ReturnType : targetType;
    }

    public IReadOnlyCollection<MethodParameter> AdditionalSourceParameters { get; init; } = [];

    protected IMethodSymbol? Method { get; }

    protected bool IsExtensionMethod { get; }

    protected string MethodName => _methodName ?? throw new InvalidOperationException();

    protected MethodParameter SourceParameter { get; }

    protected MethodParameter? ReferenceHandlerParameter { get; private set; }

    public ITypeSymbol SourceType => SourceParameter.Type;

    public ITypeSymbol TargetType { get; }

    public bool IsSynthetic => false;

    public virtual ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        Invocation(MethodName, SourceParameter.WithArgument(ctx.Source), ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));

    public virtual MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var typeMappingBuildContext = new TypeMappingBuildContext(
            SourceParameter.Name,
            ReferenceHandlerParameter?.Name,
            ctx.NameBuilder.NewScope(),
            ctx.SyntaxFactory.AddIndentation()
        );

        var parameters = BuildParameterList();
        ReserveParameterNames(typeMappingBuildContext.NameBuilder, parameters);

        var returnType = FullyQualifiedIdentifier(_returnType);
        return MethodDeclaration(returnType.AddTrailingSpace(), Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers(ctx.IsStatic)))
            .WithParameterList(parameters)
            .WithAttributeLists(ctx.SyntaxFactory.GeneratedCodeAttributeList())
            .WithBody(ctx.SyntaxFactory.Block(BuildBody(typeMappingBuildContext)));
    }

    public abstract IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx);

    internal void SetMethodNameIfNeeded(Func<MethodMapping, string> methodNameBuilder)
    {
        _methodName ??= methodNameBuilder(this);
    }

    internal virtual void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        ReferenceHandlerParameter ??= new MethodParameter(
            ReferenceHandlerParameterIndex,
            DefaultReferenceHandlerParameterName,
            iReferenceHandlerType
        );
    }

    protected virtual ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, [SourceParameter, ReferenceHandlerParameter, .. AdditionalSourceParameters]);

    private IEnumerable<SyntaxToken> BuildModifiers(bool isStatic)
    {
        // if a syntax is referenced the code written by the user copy all modifiers,
        // otherwise only set private and optionally static
        if (_methodDeclarationSyntax != null)
        {
            return _methodDeclarationSyntax.Modifiers.Select(x => TrailingSpacedToken(x.Kind()));
        }

        return isStatic ? _privateStaticSyntaxToken : _privateSyntaxToken;
    }

    private void ReserveParameterNames(UniqueNameBuilder nameBuilder, ParameterListSyntax parameters)
    {
        foreach (var param in parameters.Parameters)
        {
            nameBuilder.Reserve(param.Identifier.Text);
        }
    }
}
