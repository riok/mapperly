using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols;

public sealed record MapperDeclaration(INamedTypeSymbol Symbol, ClassDeclarationSyntax Syntax);
