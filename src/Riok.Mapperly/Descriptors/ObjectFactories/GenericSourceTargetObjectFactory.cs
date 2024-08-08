using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class GenericSourceTargetObjectFactory(
    GenericTypeChecker typeChecker,
    SymbolAccessor symbolAccessor,
    IMethodSymbol method,
    int sourceTypeParameterIndex
) : ObjectFactory(symbolAccessor, method)
{
    private readonly int _targetTypeParameterIndex = (sourceTypeParameterIndex + 1) % 2;

    public override bool CanCreateInstanceOfType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        typeChecker.CheckTypes(
            (Method.TypeParameters[sourceTypeParameterIndex], sourceType),
            (Method.TypeParameters[_targetTypeParameterIndex], targetTypeToCreate)
        );

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source)
    {
        var typeParams = new TypeSyntax[2];
        typeParams[sourceTypeParameterIndex] = NonNullableIdentifier(sourceType);
        typeParams[_targetTypeParameterIndex] = NonNullableIdentifier(targetTypeToCreate);
        return GenericInvocation(Method.Name, typeParams, source);
    }
}
