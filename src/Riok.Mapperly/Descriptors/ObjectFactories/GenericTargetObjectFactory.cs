using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="GenericTargetObjectFactory"/> is an <see cref="ObjectFactory"/>
/// without any parameters but a single type parameter which is also the return type.
/// Example signature: <c>T Create&lt;T&gt;();</c>
/// </summary>
public class GenericTargetObjectFactory(GenericTypeChecker typeChecker, SymbolAccessor symbolAccessor, IMethodSymbol method)
    : ObjectFactory(symbolAccessor, method)
{
    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        typeChecker.CheckTypes((Method.TypeParameters[0], targetTypeToCreate));

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        GenericInvocation(Method.Name, new[] { NonNullableIdentifier(targetTypeToCreate) });
}
