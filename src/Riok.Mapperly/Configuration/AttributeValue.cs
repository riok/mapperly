using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Represents an attribute value provided by the user.
/// Allows access to the intepreted value as well as the source syntax.
/// </summary>
/// <param name="ConstantValue">The interpreted compile-time constant value.</param>
/// <param name="Expression">The syntax as written by the user.</param>
public readonly record struct AttributeValue(TypedConstant ConstantValue, ExpressionSyntax Expression);
