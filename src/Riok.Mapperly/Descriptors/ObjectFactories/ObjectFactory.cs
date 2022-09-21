using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An object factory represents a method to instantiate objects of a certain type.
/// </summary>
public abstract class ObjectFactory
{
    protected ObjectFactory(IMethodSymbol method)
    {
        Method = method;
    }

    protected IMethodSymbol Method { get; }

    public ExpressionSyntax CreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source)
        => HandleNull(BuildCreateType(sourceType, targetTypeToCreate, source), targetTypeToCreate);

    public abstract bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate);

    protected abstract ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source);

    /// <summary>
    /// Wraps the <see cref="expression"/> in null handling.
    /// If the <see cref="expression"/> returns a nullable type, but the <see cref="typeToCreate"/> is not nullable,
    /// a new instance is created (if a parameterless ctor is accessible). Otherwise a <see cref="NullReferenceException"/> is thrown.
    /// If the <see cref="typeToCreate"/> is nullable, the <see cref="expression"/> is returned without additional handling.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="typeToCreate">The type to create.</param>
    /// <returns></returns>
    private ExpressionSyntax HandleNull(ExpressionSyntax expression, ITypeSymbol typeToCreate)
    {
        if (!Method.ReturnType.UpgradeNullable().IsNullable())
            return expression;

        ExpressionSyntax nullFallback = typeToCreate.HasAccessibleParameterlessConstructor()
            ? CreateInstance(typeToCreate)
            : ThrowNullReferenceException($"The object factory {Method.Name} returned null");

        return Coalesce(expression, nullFallback);
    }
}
