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
public class UserDefinedNewInstanceGenericTypeMapping(
    IMethodSymbol method,
    MappingMethodParameters parameters,
    ITypeSymbol targetType,
    bool enableReferenceHandling,
    NullFallbackValue? nullArm,
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
    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var methodSyntax = (MethodDeclarationSyntax)Method.DeclaringSyntaxReferences.First().GetSyntax();
        return base.BuildMethod(ctx).WithTypeParameterList(methodSyntax.TypeParameterList);
    }

    protected override ExpressionSyntax BuildTargetType()
    {
        // typeof(<ReturnType>)
        return TypeOfExpression(FullyQualifiedIdentifier(Method.ReturnType.NonNullable()));
    }

    protected override ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax targetType, RuntimeTargetTypeMapping mapping)
    {
        return mapping.IsAssignableToMethodTargetType ? null : base.BuildSwitchArmWhenClause(targetType, mapping);
    }
}
