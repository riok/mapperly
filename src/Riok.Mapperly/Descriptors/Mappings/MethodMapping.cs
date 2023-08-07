using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
public abstract class MethodMapping : NewInstanceMapping
{
    protected const string DefaultReferenceHandlerParameterName = "refHandler";
    private const string DefaultSourceParameterName = "source";

    private const int SourceParameterIndex = 0;
    private const int ReferenceHandlerParameterIndex = 1;

    private readonly Accessibility _accessibility = Accessibility.Private;
    private readonly ITypeSymbol _returnType;

    private string? _methodName;

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
        SourceParameter = new MethodParameter(SourceParameterIndex, DefaultSourceParameterName, sourceType);
        _returnType = targetType;
    }

    protected MethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        ITypeSymbol targetType
    )
        : base(sourceParameter.Type, targetType)
    {
        SourceParameter = sourceParameter;
        IsExtensionMethod = method.IsExtensionMethod;
        IsPartial = method.IsPartialDefinition;
        ReferenceHandlerParameter = referenceHandlerParameter;
        _accessibility = method.DeclaredAccessibility;
        _methodName = method.Name;
        _returnType = method.ReturnType.UpgradeNullable();
    }

    private bool IsPartial { get; }

    protected bool IsExtensionMethod { get; }

    private string MethodName => _methodName ?? throw new InvalidOperationException();

    protected MethodParameter SourceParameter { get; }

    protected MethodParameter? ReferenceHandlerParameter { get; private set; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        Invocation(MethodName, SourceParameter.WithArgument(ctx.Source), ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler));

    public virtual MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var returnType = FullyQualifiedIdentifier(_returnType);

        var typeMappingBuildContext = new TypeMappingBuildContext(
            SourceParameter.Name,
            ReferenceHandlerParameter?.Name,
            ctx.NameBuilder.NewScope()
        );

        var parameters = BuildParameterList();
        ReserveParameterNames(typeMappingBuildContext.NameBuilder, parameters);

        return MethodDeclaration(returnType, Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers(ctx.IsStatic)))
            .WithParameterList(parameters)
            .WithBody(Block(BuildBody(typeMappingBuildContext)));
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
        ParameterList(IsExtensionMethod, SourceParameter, ReferenceHandlerParameter);

    private IEnumerable<SyntaxToken> BuildModifiers(bool isStatic)
    {
        yield return Accessibility(_accessibility);

        if (isStatic)
            yield return Token(SyntaxKind.StaticKeyword);

        if (IsPartial)
            yield return Token(SyntaxKind.PartialKeyword);
    }

    private void ReserveParameterNames(UniqueNameBuilder nameBuilder, ParameterListSyntax parameters)
    {
        foreach (var param in parameters.Parameters)
        {
            nameBuilder.Reserve(param.Identifier.Text);
        }
    }
}
