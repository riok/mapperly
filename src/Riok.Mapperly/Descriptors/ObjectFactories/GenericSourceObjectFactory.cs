using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="GenericSourceObjectFactory"/> is an <see cref="ObjectFactory"/>
/// with a named return type and one type parameter which is also the only parameter of the method.
/// Example signature: <c>TypeToCreate Create&lt;S&gt;(S source);</c>
/// </summary>
public class GenericSourceObjectFactory : ObjectFactory
{
    public GenericSourceObjectFactory(SymbolAccessor symbolAccessor, IMethodSymbol method)
        : base(symbolAccessor, method) { }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        SymbolEqualityComparer.Default.Equals(Method.ReturnType, targetTypeToCreate)
        && SymbolAccessor.DoesTypeSatisfyTypeParameterConstraints(
            Method.TypeParameters[0],
            sourceType,
            Method.Parameters[0].Type.NullableAnnotation
        );

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        GenericInvocation(Method.Name, new[] { NonNullableIdentifier(sourceType) }, source);
}
