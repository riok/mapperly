using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A derived type mapping maps one base type or interface to another
/// by implementing a type switch over known types and performs the provided mapping for each type.
/// </summary>
public class DerivedTypeSwitchMapping(ITypeSymbol sourceType, ITypeSymbol targetType, IReadOnlyCollection<INewInstanceMapping> typeMappings)
    : NewInstanceMapping(sourceType, targetType)
{
    private const string GetTypeMethodName = nameof(GetType);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // _ => throw new ArgumentException(msg, nameof(ctx.Source)),
        var sourceTypeExpr = ctx.SyntaxFactory.Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var fallbackArm = SwitchArm(
            DiscardPattern(),
            ThrowArgumentExpression(
                InterpolatedString(
                    $"Cannot map {sourceTypeExpr} to {TargetType.ToDisplayString()} as there is no known derived type mapping"
                ),
                ctx.Source
            )
        );

        // source switch { A x => MapToADto(x), B x => MapToBDto(x) }
        var (typeArmContext, typeArmVariableName) = ctx.WithNewSource();
        var arms = typeMappings.Select(x => BuildSwitchArm(typeArmVariableName, x.SourceType, x.Build(typeArmContext))).Append(fallbackArm);
        return ctx.SyntaxFactory.Switch(ctx.Source, arms);
    }

    private SwitchExpressionArmSyntax BuildSwitchArm(string typeArmVariableName, ITypeSymbol type, ExpressionSyntax mapping)
    {
        // A x => MapToADto(x),
        var declaration = DeclarationPattern(
            FullyQualifiedIdentifier(type).AddTrailingSpace(),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );
        return SwitchArm(declaration, mapping);
    }
}
