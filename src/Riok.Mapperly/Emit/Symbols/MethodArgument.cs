using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Emit.Symbols;

/// <summary>
/// A method argument (a parameter and an argument value).
/// </summary>
public readonly struct MethodArgument
{
    public MethodArgument(MethodParameter parameter, ExpressionSyntax argument)
    {
        Parameter = parameter;
        Argument = argument;
    }

    public MethodParameter Parameter { get; }

    public ExpressionSyntax Argument { get; }
}
