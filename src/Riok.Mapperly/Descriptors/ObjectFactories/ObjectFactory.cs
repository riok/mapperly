using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An object factory represents a method to instantiate objects of a certain type.
/// </summary>
public class ObjectFactory
{
    public ObjectFactory(IMethodSymbol method)
    {
        Method = method;
    }

    protected IMethodSymbol Method { get; }

    public virtual ExpressionSyntax CreateType(ITypeSymbol typeToCreate)
        => HandleNull(Invocation(Method.Name), typeToCreate);

    public virtual bool CanCreateType(ITypeSymbol typeToCreate)
        => SymbolEqualityComparer.Default.Equals(Method.ReturnType, typeToCreate);

    protected ExpressionSyntax HandleNull(ExpressionSyntax expression, ITypeSymbol typeToCreate)
    {
        if (!Method.ReturnType.UpgradeNullable().IsNullable())
            return expression;

        ExpressionSyntax nullFallback = typeToCreate.HasAccessibleParameterlessConstructor()
            ? CreateInstance(typeToCreate)
            : ThrowNullReferenceException($"The object factory {Method.Name} returned null");

        return Coalesce(expression, nullFallback);
    }
}
