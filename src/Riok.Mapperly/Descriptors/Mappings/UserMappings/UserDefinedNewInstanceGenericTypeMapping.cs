using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
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
    ITypeSymbol returnType,
    bool enableReferenceHandling,
    NullFallbackValue? nullArm,
    ITypeSymbol objectType
)
    : UserDefinedNewInstanceRuntimeTargetTypeMapping(
        method,
        parameters.Source,
        parameters.ReferenceHandler,
        targetType,
        returnType,
        enableReferenceHandling,
        nullArm,
        objectType,
        parameters.ResultOut
    )
{
    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var methodSyntax = (MethodDeclarationSyntax)Method.DeclaringSyntaxReferences.First().GetSyntax();
        return base.BuildMethod(ctx)
            .WithTypeParameterList(methodSyntax.TypeParameterList)
            .WithConstraintClauses(List(GetTypeParameterConstraintClauses()));
    }

    protected override ExpressionSyntax BuildTargetType()
    {
        // typeof(<ReturnType>)
        return TypeOfExpression(FullyQualifiedIdentifier(Method.ReturnType.NonNullable()));
    }

    protected virtual IEnumerable<TypeParameterConstraintClauseSyntax> GetTypeParameterConstraintClauses()
    {
        foreach (var tp in Method.TypeParameters)
        {
            var constraints = new List<TypeParameterConstraintSyntax>();

            if (tp.HasUnmanagedTypeConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("unmanaged")).AddLeadingSpace());
            }
            else if (tp.HasValueTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint).AddLeadingSpace());
            }
            else if (tp.HasNotNullConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("notnull")).AddLeadingSpace());
            }
            else if (tp.HasReferenceTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint).AddLeadingSpace());
            }

            foreach (var c in tp.ConstraintTypes)
            {
                constraints.Add(TypeConstraint(FullyQualifiedIdentifier(c)).AddLeadingSpace());
            }

            if (tp.HasConstructorConstraint)
            {
                constraints.Add(ConstructorConstraint().AddLeadingSpace());
            }

            if (!constraints.Any())
            {
                continue;
            }

            yield return TypeParameterConstraintClause(
                    IdentifierName(tp.Name).AddLeadingSpace().AddTrailingSpace(),
                    SeparatedList(constraints)
                )
                .AddLeadingSpace();
        }
    }

    protected override ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax targetType, RuntimeTargetTypeMapping mapping)
    {
        return mapping.IsAssignableToMethodTargetType ? null : base.BuildSwitchArmWhenClause(targetType, mapping);
    }
}
