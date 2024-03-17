using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A user defined generic mapping method,
/// which can have a generic source parameter,
/// a generic target parameter or both.
/// Similar to <see cref="UserDefinedNewInstanceRuntimeTargetTypeParameterMapping"/>.
/// </summary>
public class UserDefinedNewInstanceGenericTypeProjection(
    IMethodSymbol method,
    GenericProjectionTypeParameters typeParameters,
    MappingMethodParameters parameters,
    ITypeSymbol targetType,
    bool enableReferenceHandling,
    NullFallbackValue nullArm,
    ITypeSymbol objectType
)
    : UserDefinedNewInstanceRuntimeTargetTypeMapping(
        method,
        parameters.Source,
        parameters.ReferenceHandler,
        targetType,
        enableReferenceHandling,
        nullArm,
        objectType
    )
{
    public GenericProjectionTypeParameters TypeParameters { get; } = typeParameters;

    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx) =>
        base.BuildMethod(ctx)
            .WithTypeParameterList(TypeParameterList(TypeParameters.SourceTypeParameter, TypeParameters.TargetTypeParameter));

    protected override ExpressionSyntax BuildTargetType() =>
        TypeOfExpression(FullyQualifiedIdentifier(TypeParameters.TargetType.NonNullable()));

    protected override ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax targetType, RuntimeTargetTypeMapping mapping)
    {
        if (mapping.IsAssignableToMethodTargetType)
            return null;

        if (mapping.Mapping.TargetType.ImplementsGeneric(TypeParameters.WellKnownTypes.Get(typeof(IQueryable<>)), out var argument))
            return Invocation(
                MemberAccess(TypeOfExpression(FullyQualifiedIdentifier(TypeParameters.TargetTypeParameter)), IsAssignableFromMethodName),
                TypeOfExpression(FullyQualifiedIdentifier(argument.TypeArguments[0].NonNullable()))
            );

        return null;
    }
}
