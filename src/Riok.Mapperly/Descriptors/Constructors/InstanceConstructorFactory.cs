using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Descriptors.UnsafeAccess;

namespace Riok.Mapperly.Descriptors.Constructors;

public class InstanceConstructorFactory(
    ObjectFactoryCollection objectFactories,
    SymbolAccessor symbolAccessor,
    UnsafeAccessorContext unsafeAccessorContext
)
{
    /// <summary>
    /// Tries to build an <see cref="IInstanceConstructor"/>.
    /// Creates an object factory ctor if possible,
    /// tries to use an accessible parameterless ctor otherwise.
    /// </summary>
    public bool TryBuild(ITypeSymbol source, ITypeSymbol target, [NotNullWhen(true)] out IInstanceConstructor? constructor)
    {
        return TryBuildObjectFactory(source, target, out constructor) || TryBuildParameterless(target, out constructor);
    }

    /// <summary>
    /// Tries to build an object factory constructor.
    /// </summary>
    public bool TryBuildObjectFactory(ITypeSymbol source, ITypeSymbol target, [NotNullWhen(true)] out IInstanceConstructor? constructor)
    {
        if (objectFactories.TryFindObjectFactory(source, target, out var factory))
        {
            constructor = new ObjectFactoryConstructorAdapter(factory, source, target);
            return true;
        }

        constructor = null;
        return false;
    }

    /// <summary>
    /// Tries to build a parameterless constructor.
    /// </summary>
    public bool TryBuildParameterless(ITypeSymbol type, [NotNullWhen(true)] out IInstanceConstructor? ctor)
    {
        if (type is not INamedTypeSymbol namedType || namedType.IsAbstract)
        {
            ctor = null;
            return false;
        }

        var ctorMethod = namedType.InstanceConstructors.FirstOrDefault(x =>
            x.Parameters.IsDefaultOrEmpty && symbolAccessor.IsConstructorAccessible(x)
        );
        if (ctorMethod == null)
        {
            ctor = null;
            return false;
        }

        ctor = BuildForConstructor(ctorMethod);
        return true;
    }

    /// <summary>
    /// Builds a parameterless ctor,
    /// throws if no accessible parameterless ctor is available.
    /// </summary>
    public IInstanceConstructor BuildParameterless(ITypeSymbol type) =>
        BuildForConstructor(((INamedTypeSymbol)type).InstanceConstructors.First(x => x.Parameters.IsDefaultOrEmpty));

    /// <summary>
    /// Builds an <see cref="IInstanceConstructor"/> for a given constructor method.
    /// </summary>
    public IInstanceConstructor BuildForConstructor(IMethodSymbol ctor)
    {
        Debug.Assert(ctor.MethodKind == MethodKind.Constructor);
        if (symbolAccessor.IsDirectlyAccessible(ctor))
            return new InstanceConstructor(ctor.ContainingType);

        Debug.Assert(symbolAccessor.IsConstructorAccessible(ctor));
        return unsafeAccessorContext.GetOrBuildConstructor(ctor);
    }
}
