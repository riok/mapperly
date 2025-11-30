using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking
/// the Parse static method on the source object.
/// <code>
/// target = TTarget.Parse(source);
/// </code>
/// If the Parse method takes an IFormatProvider that is passed across,
/// respecting the <see cref="Riok.Mapperly.Abstractions.FormatProviderAttribute"/> format provider logic
/// </summary>
public class ParseMethodMapping : NewInstanceMapping
{
    private readonly IMethodSymbol _method;
    private readonly string? _formatProviderName;
    private readonly bool _simpleInvocation;

    public ParseMethodMapping(IMethodSymbol method, string? formatProviderName = null, bool simpleInvocation = true)
        : base(method.Parameters.First().Type, method.ReturnType)
    {
        _method = method;
        _formatProviderName = formatProviderName;
        _simpleInvocation = simpleInvocation;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ExpressionSyntax? formatProviderArgument =
            _formatProviderName != null ? IdentifierName(_formatProviderName)
            : _simpleInvocation ? null
            : NullLiteral();
        var arguments = new ExpressionSyntax?[] { ctx.Source, formatProviderArgument }
            .WhereNotNull()
            .ToArray();
        return ctx.SyntaxFactory.StaticInvocation(_method, arguments);
    }
}
