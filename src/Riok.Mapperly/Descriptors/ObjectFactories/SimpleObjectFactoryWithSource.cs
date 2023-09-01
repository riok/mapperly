using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An <see cref="SimpleObjectFactoryWithSource"/> is an <see cref="ObjectFactory"/>
/// with a single parameter (which is the source object and is a named type) and a named return type (not generic).
/// Example signature: <c>TypeToCreate Create(SourceType source);</c>
/// </summary>
public class SimpleObjectFactoryWithSource : SimpleObjectFactory
{
    public SimpleObjectFactoryWithSource(SymbolAccessor symbolAccessor, IMethodSymbol method)
        : base(symbolAccessor, method) { }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        base.CanCreateType(sourceType, targetTypeToCreate) && SymbolEqualityComparer.Default.Equals(sourceType, Method.Parameters[0].Type);

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        Invocation(Method.Name, source);
}
