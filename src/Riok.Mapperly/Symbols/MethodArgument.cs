using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// A method argument (a parameter and an argument value).
/// </summary>
public readonly record struct MethodArgument(MethodParameter Parameter, ExpressionSyntax Argument);
