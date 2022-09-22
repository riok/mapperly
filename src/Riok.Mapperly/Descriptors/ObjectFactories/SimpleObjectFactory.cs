using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="SimpleObjectFactory"/> is an <see cref="ObjectFactory"/> without any parameters and a named return type (not generic).
/// Example signature: <c>TypeToCreate Create();</c>
/// </summary>
public class SimpleObjectFactory : ObjectFactory
{
    public SimpleObjectFactory(IMethodSymbol method) : base(method)
    {
    }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate)
        => SymbolEqualityComparer.Default.Equals(Method.ReturnType, targetTypeToCreate);

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source)
        => Invocation(Method.Name);
}
