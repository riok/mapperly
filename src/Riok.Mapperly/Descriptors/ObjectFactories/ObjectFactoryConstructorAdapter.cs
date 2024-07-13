using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An adapter to adapt a general <see cref="ObjectFactory"/> as a <see cref="IInstanceConstructor"/>
/// for a concrete type pair.
/// </summary>
public class ObjectFactoryConstructorAdapter(ObjectFactory objectFactory, ITypeSymbol sourceType, ITypeSymbol targetType)
    : IInstanceConstructor
{
    public bool SupportsObjectInitializer => false;

    public ExpressionSyntax CreateInstance(
        TypeMappingBuildContext ctx,
        IEnumerable<ArgumentSyntax> args,
        InitializerExpressionSyntax? initializer = null
    )
    {
        return objectFactory.CreateType(sourceType, targetType, ctx.Source);
    }
}
