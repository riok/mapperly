using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// An object factory represents a method to instantiate objects of a certain type.
/// </summary>
public abstract class ObjectFactory(SymbolAccessor symbolAccessor, IMethodSymbol method)
{
    protected IMethodSymbol Method { get; } = method;

    public IEnumerable<StatementSyntax> CreateInstance(
        TypeMappingBuildContext ctx,
        INewInstanceMapping mapping,
        bool enableReferenceHandling,
        string targetVariableName
    )
    {
        if (enableReferenceHandling)
        {
            // TryGetReference
            yield return ReferenceHandlingSyntaxFactoryHelper.TryGetReference(ctx, mapping);
        }

        // var target = CreateMyObject<T>();
        yield return ctx.SyntaxFactory.DeclareLocalVariable(
            targetVariableName,
            CreateType(mapping.SourceType, mapping.TargetType, ctx.Source)
        );

        // set the reference as soon as it is created,
        // as property mappings could refer to the same instance.
        if (enableReferenceHandling)
        {
            // SetReference
            yield return ctx.SyntaxFactory.ExpressionStatement(
                ReferenceHandlingSyntaxFactoryHelper.SetReference(mapping, ctx, IdentifierName(targetVariableName))
            );
        }
    }

    public abstract bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate);

    protected abstract ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source);

    private ExpressionSyntax CreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source) =>
        HandleNull(BuildCreateType(sourceType, targetTypeToCreate, source), targetTypeToCreate);

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
        if (!Method.ReturnType.IsNullable())
            return expression;

        ExpressionSyntax nullFallback = symbolAccessor.HasDirectlyAccessibleParameterlessConstructor(typeToCreate)
            ? SyntaxFactoryHelper.CreateInstance(typeToCreate)
            : ThrowNullReferenceException($"The object factory {Method.Name} returned null");

        return Coalesce(expression, nullFallback);
    }
}
