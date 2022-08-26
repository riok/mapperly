using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An object factory with a generic underlying method.
/// Eg. <c>T Create&lt;T&gt; where T : new() => new T();</c>
/// </summary>
public class GenericObjectFactory : ObjectFactory
{
    private readonly Compilation _compilation;

    public GenericObjectFactory(IMethodSymbol method, Compilation compilation) : base(method)
    {
        _compilation = compilation;
    }

    public override ExpressionSyntax CreateType(ITypeSymbol typeToCreate)
        => HandleNull(GenericInvocation(Method.Name, new[] { NonNullableIdentifier(typeToCreate) }), typeToCreate);

    public override bool CanCreateType(ITypeSymbol typeToCreate)
    {
        var typeParameter = Method.TypeParameters[0];
        if (typeParameter.HasConstructorConstraint && !typeToCreate.HasAccessibleParameterlessConstructor())
            return false;

        if (typeParameter.HasNotNullConstraint && typeToCreate.IsNullable())
            return false;

        if (typeParameter.HasValueTypeConstraint && !typeToCreate.IsValueType)
            return false;

        if (typeParameter.HasReferenceTypeConstraint && !typeToCreate.IsReferenceType)
            return false;

        foreach (var constraintType in typeParameter.ConstraintTypes)
        {
            if (!_compilation.ClassifyConversion(typeToCreate, constraintType.UpgradeNullable()).IsImplicit)
                return false;
        }

        return true;
    }
}
