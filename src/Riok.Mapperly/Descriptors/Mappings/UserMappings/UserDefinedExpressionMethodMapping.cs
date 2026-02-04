using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which returns an Expression{Func{TSource, TTarget}}.
/// This allows the source type to be extracted from the return type rather than from parameters.
/// </summary>
public class UserDefinedExpressionMethodMapping : NewInstanceMethodMapping, INewInstanceUserMapping
{
    private INewInstanceMapping? _delegateMapping;
    private readonly IMethodSymbol _method;

    public UserDefinedExpressionMethodMapping(
        IMethodSymbol method,
        ITypeSymbol expressionSourceType,
        ITypeSymbol expressionTargetType,
        ITypeSymbol returnType
    )
        : base(expressionSourceType, returnType)
    {
        _method = method;
        ExpressionSourceType = expressionSourceType;
        ExpressionTargetType = expressionTargetType;
        SetMethodNameIfNeeded(_ => method.Name);
    }

    public new IMethodSymbol Method => _method;

    /// <summary>
    /// The source type of the expression (TSource in Expression{Func{TSource, TTarget}}).
    /// </summary>
    public ITypeSymbol ExpressionSourceType { get; }

    /// <summary>
    /// The target type of the expression (TTarget in Expression{Func{TSource, TTarget}}).
    /// </summary>
    public ITypeSymbol ExpressionTargetType { get; }

    public bool? Default => false;

    public bool IsExternal => false;

    public void SetDelegateMapping(INewInstanceMapping mapping) => _delegateMapping = mapping;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        throw new InvalidOperationException("Expression mappings should not be built as expressions.");

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping == null)
            return [ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException())];

        return _delegateMapping is MethodMapping mm ? mm.BuildBody(ctx) : [ctx.SyntaxFactory.Return(_delegateMapping.Build(ctx))];
    }

    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var typeMappingBuildContext = new TypeMappingBuildContext("source", null, ctx.NameBuilder.NewScope(), ctx.SyntaxFactory);

        return MethodDeclaration(SyntaxFactoryHelper.FullyQualifiedIdentifier(TargetType).AddTrailingSpace(), Identifier(_method.Name))
            .WithModifiers(BuildModifiers())
            .WithParameterList(ParameterList())
            .WithAttributeLists(BuildAttributes(typeMappingBuildContext))
            .WithBody(ctx.SyntaxFactory.Block(BuildBody(typeMappingBuildContext.AddIndentation())));
    }

    protected override ParameterListSyntax BuildParameterList() => ParameterList();

    private SyntaxTokenList BuildModifiers()
    {
        if (_method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax methodSyntax)
            return methodSyntax.Modifiers;

        // Fallback - shouldn't happen for partial methods
        return TokenList(
            SyntaxFactoryHelper.TrailingSpacedToken(SyntaxKind.PrivateKeyword),
            SyntaxFactoryHelper.TrailingSpacedToken(SyntaxKind.StaticKeyword),
            SyntaxFactoryHelper.TrailingSpacedToken(SyntaxKind.PartialKeyword)
        );
    }
}
