using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A generic object factory which receives mapped source members as parameters and has generic source and target type parameters.
/// Example signature: <c>TTarget Create&lt;TSource, TTarget&gt;(SourceMemberType sourceMember);</c>
/// </summary>
public class GenericSourceTargetParameterObjectFactory(
    GenericTypeChecker typeChecker,
    SymbolAccessor symbolAccessor,
    IMethodSymbol method,
    int sourceTypeParameterIndex
) : ParameterObjectFactory(symbolAccessor, method)
{
    private readonly int _targetTypeParameterIndex = (sourceTypeParameterIndex + 1) % 2;

    public override bool CanCreateInstanceOfType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate) =>
        typeChecker.CheckTypes(
            (Method.TypeParameters[sourceTypeParameterIndex], sourceType),
            (Method.TypeParameters[_targetTypeParameterIndex], targetTypeToCreate)
        );

    protected override ExpressionSyntax BuildCreateType(
        ITypeSymbol sourceType,
        ITypeSymbol targetTypeToCreate,
        ExpressionSyntax source,
        IEnumerable<ArgumentSyntax> arguments
    )
    {
        var typeParams = new TypeSyntax[2];
        typeParams[sourceTypeParameterIndex] = NonNullableIdentifier(sourceType);
        typeParams[_targetTypeParameterIndex] = NonNullableIdentifier(targetTypeToCreate);
        var methodSyntax = GenericName(Method.Name).WithTypeArgumentList(SyntaxFactoryHelper.TypeArgumentList(typeParams));
        return Invocation(methodSyntax, arguments);
    }
}
