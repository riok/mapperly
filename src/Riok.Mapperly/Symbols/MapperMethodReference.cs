using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Describes a reference to a mapper method, including its symbol, declaring type can't be used directly,
/// and the member providing the instance for instance methods.
/// </summary>
/// <param name="Method">The symbol of the referenced method.</param>
/// <param name="TargetType">The declaring type if the method can't be used directly; otherwise, <c>null</c>.</param>
/// <param name="TargetMember">The instance member for instance methods; otherwise, <c>null</c>.</param>
public record MapperMethodReference(IMethodSymbol Method, INamedTypeSymbol? TargetType, ISymbol? TargetMember);
