using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="GenericTargetObjectFactory"/> is an <see cref="ObjectFactory"/>
/// without any parameters but a single type parameter which is also the return type.
/// Example signature: <c>T Create&lt;T&gt;();</c>
/// </summary>
public class GenericTargetObjectFactory : ObjectFactory
{
    public GenericTargetObjectFactory(SymbolAccessor symbolAccessor, IMethodSymbol method)
        : base(symbolAccessor, method) { }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        SymbolAccessor.DoesTypeSatisfyTypeParameterConstraints(
            Method.TypeParameters[0],
            targetTypeToCreate,
            Method.ReturnType.NullableAnnotation
        );

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        GenericInvocation(Method.Name, new[] { NonNullableIdentifier(targetTypeToCreate) });
}
