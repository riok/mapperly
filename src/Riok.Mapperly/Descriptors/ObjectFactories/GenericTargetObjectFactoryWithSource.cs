using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="GenericTargetObjectFactoryWithSource"/> is an <see cref="ObjectFactory"/>
/// with a single parameter (which is the source object and is a named type) and a single type parameter which is also the return type.
/// Example signature: <c>T Create&lt;T&gt;(SourceType source);</c>
/// </summary>
public class GenericTargetObjectFactoryWithSource : GenericTargetObjectFactory
{
    public GenericTargetObjectFactoryWithSource(SymbolAccessor symbolAccessor, IMethodSymbol method)
        : base(symbolAccessor, method) { }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        base.CanCreateType(sourceType, targetTypeToCreate) && SymbolEqualityComparer.Default.Equals(Method.Parameters[0].Type, sourceType);

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        GenericInvocation(Method.Name, new[] { NonNullableIdentifier(targetTypeToCreate) }, source);
}
