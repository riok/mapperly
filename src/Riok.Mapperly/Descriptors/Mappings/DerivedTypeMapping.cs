using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A derived type mapping maps one base type or interface to another
/// by implementing a type switch over known types and performs the provided mapping for each type.
/// </summary>
public class DerivedTypeMapping : TypeMapping
{
    private const string TypeArmVariableName = "x";
    private const string GetTypeMethodName = "GetType";

    private readonly IReadOnlyDictionary<ITypeSymbol, ITypeMapping> _typeMappings;

    public DerivedTypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType, IReadOnlyDictionary<ITypeSymbol, ITypeMapping> typeMappings)
        : base(sourceType, targetType)
    {
        _typeMappings = typeMappings;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // _ => throw new ArgumentException(msg, nameof(ctx.Source)),
        var sourceType = Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceType} to {TargetType.ToDisplayString()} as there is no known derived type mapping"),
                ctx.Source
            )
        );

        // source switch { A x => MapToA(x), B x => MapToB(x) }
        var typeArmVariableName = ctx.NameBuilder.New(TypeArmVariableName);
        var arms = _typeMappings.Select(x => BuildSwitchArm(ctx, typeArmVariableName, x.Key, x.Value)).Append(fallbackArm);
        return SwitchExpression(ctx.Source).WithArms(CommaSeparatedList(arms, true));
    }

    private SwitchExpressionArmSyntax BuildSwitchArm(
        TypeMappingBuildContext ctx,
        string typeArmVariableName,
        ITypeSymbol type,
        ITypeMapping typeMapping
    )
    {
        // A x => MapToA(x),
        var declaration = DeclarationPattern(FullyQualifiedIdentifier(type), SingleVariableDesignation(Identifier(typeArmVariableName)));
        return SwitchExpressionArm(declaration, typeMapping.Build(ctx.WithSource(typeArmVariableName)));
    }
}
