using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via new ValueTuple&lt;int, string&gt;(source.A, source.B),
/// mapping properties via ctor, but not by assigning.
/// <seealso cref="NewInstanceObjectMemberMethodMapping"/>
/// </summary>
public class NewValueTupleConstructorMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    : NewInstanceMapping(sourceType, targetType),
        INewValueTupleMapping
{
    private const string ValueTupleName = "global::System.ValueTuple";
    private readonly HashSet<ValueTupleConstructorParameterMapping> _constructorPropertyMappings = [];

    public void AddConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping) => _constructorPropertyMappings.Add(mapping);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // new ValueTuple<T..>(ctorArgs)
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(ctx, emitFieldName: false));
        var typeArguments = TypeArgumentList(((INamedTypeSymbol)TargetType).TypeArguments.Select(NonNullableIdentifier));
        return ctx.SyntaxFactory.CreateGenericInstance(ValueTupleName, typeArguments, ctorArgs);
    }
}
