using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which returns an Expression{Func{TSource, TTarget}}.
/// Unlike IQueryable mappings which have a source parameter and can use <see cref="UserDefinedNewInstanceMethodMapping"/>,
/// Expression mappings are parameterless, so the source/target types must be extracted from the return type's type arguments.
/// </summary>
public class UserDefinedExpressionMethodMapping(
    IMethodSymbol method,
    ITypeSymbol expressionSourceType,
    ITypeSymbol expressionTargetType,
    ITypeSymbol returnType
) : NewInstanceMethodMapping(method, CreateSourceParameter(expressionSourceType), null, returnType), INewInstanceUserMapping
{
    private INewInstanceMapping? _delegateMapping;

    public new IMethodSymbol Method { get; } = method;

    /// <summary>
    /// The source type of the expression (TSource in Expression{Func{TSource, TTarget}}).
    /// </summary>
    public ITypeSymbol ExpressionSourceType { get; } = expressionSourceType;

    /// <summary>
    /// The target type of the expression (TTarget in Expression{Func{TSource, TTarget}}).
    /// </summary>
    public ITypeSymbol ExpressionTargetType { get; } = expressionTargetType;

    public bool? Default => false;

    public bool IsExternal => false;

    public void SetDelegateMapping(INewInstanceMapping mapping) => _delegateMapping = mapping;

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping is not MethodMapping mm)
            return [ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException())];

        return mm.BuildBody(ctx);
    }

    protected override ParameterListSyntax BuildParameterList() => ParameterList();

    /// <summary>
    /// Dummy source parameter required by the base <see cref="MethodMapping"/> constructor.
    /// We override <see cref="BuildParameterList"/> to emit no parameters, so this value
    /// is never used in the generated code.
    /// </summary>
    private static MethodParameter CreateSourceParameter(ITypeSymbol expressionSourceType) => new(0, "_", expressionSourceType);
}
