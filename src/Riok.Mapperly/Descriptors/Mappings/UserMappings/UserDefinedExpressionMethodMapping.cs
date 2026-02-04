using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which returns an Expression{Func{TSource, TTarget}}.
/// This allows the source type to be extracted from the return type rather than from parameters.
/// </summary>
public class UserDefinedExpressionMethodMapping : NewInstanceMethodMapping, INewInstanceUserMapping
{
    private INewInstanceMapping? _delegateMapping;
    private readonly IMethodSymbol _method;
    private readonly MethodDeclarationSyntax? _methodDeclarationSyntax;

    public UserDefinedExpressionMethodMapping(
        IMethodSymbol method,
        ITypeSymbol expressionSourceType,
        ITypeSymbol expressionTargetType,
        ITypeSymbol returnType
    )
        : base(expressionSourceType, returnType)
    {
        _method = method;
        _methodDeclarationSyntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
        ExpressionSourceType = expressionSourceType;
        ExpressionTargetType = expressionTargetType;
        // Force the method name to be preserved
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

    public bool? Default => false; // Expression mappings are never default mappings

    public bool IsExternal => false;

    public void SetDelegateMapping(INewInstanceMapping mapping) => _delegateMapping = mapping;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // Expression mappings generate code directly and don't call other methods
        throw new InvalidOperationException("Expression mappings should not be built as expressions.");
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping == null)
        {
            return [ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException())];
        }

        // The delegate mapping is an ExpressionMapping that builds the actual expression lambda
        return _delegateMapping is MethodMapping mm ? mm.BuildBody(ctx) : [ctx.SyntaxFactory.Return(_delegateMapping.Build(ctx))];
    }

    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var typeMappingBuildContext = new TypeMappingBuildContext(
            "source", // dummy source name, not used
            null,
            ctx.NameBuilder.NewScope(),
            ctx.SyntaxFactory
        );

        var parameters = BuildParameterList();
        var returnType = SyntaxFactoryHelper.FullyQualifiedIdentifier(TargetType);
        return Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(
                returnType.AddTrailingSpace(),
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(_method.Name)
            )
            .WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(BuildModifiers(ctx.IsStatic)))
            .WithParameterList(parameters)
            .WithAttributeLists(BuildAttributes(typeMappingBuildContext))
            .WithBody(ctx.SyntaxFactory.Block(BuildBody(typeMappingBuildContext.AddIndentation())));
    }

    protected override ParameterListSyntax BuildParameterList()
    {
        // No parameters for expression-returning methods
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList();
    }

    private IEnumerable<SyntaxToken> BuildModifiers(bool isStatic)
    {
        // Copy modifiers from the original method declaration
        if (_methodDeclarationSyntax != null)
        {
            return _methodDeclarationSyntax.Modifiers.Select(x => SyntaxFactoryHelper.TrailingSpacedToken(x.Kind()));
        }

        // Fallback to private (and optionally static)
        return isStatic
            ?
            [
                SyntaxFactoryHelper.TrailingSpacedToken(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword),
                SyntaxFactoryHelper.TrailingSpacedToken(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword),
            ]
            : [SyntaxFactoryHelper.TrailingSpacedToken(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)];
    }
}
