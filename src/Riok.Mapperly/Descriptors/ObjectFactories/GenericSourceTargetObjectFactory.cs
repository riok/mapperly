using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class GenericSourceTargetObjectFactory : ObjectFactory
{
    private readonly Compilation _compilation;
    private readonly int _sourceTypeParameterIndex;
    private readonly int _targetTypeParameterIndex;

    public GenericSourceTargetObjectFactory(IMethodSymbol method, Compilation compilation, int sourceTypeParameterIndex) : base(method)
    {
        _compilation = compilation;
        _sourceTypeParameterIndex = sourceTypeParameterIndex;
        _targetTypeParameterIndex = (sourceTypeParameterIndex + 1) % 2;
    }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate)
        => Method.TypeParameters[_sourceTypeParameterIndex].CanConsumeType(_compilation, sourceType)
        && Method.TypeParameters[_targetTypeParameterIndex].CanConsumeType(_compilation, targetTypeToCreate);

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source)
    {
        var typeParams = new TypeSyntax[2];
        typeParams[_sourceTypeParameterIndex] = NonNullableIdentifier(sourceType);
        typeParams[_targetTypeParameterIndex] = NonNullableIdentifier(targetTypeToCreate);
        return GenericInvocation(Method.Name, typeParams, source);
    }
}
