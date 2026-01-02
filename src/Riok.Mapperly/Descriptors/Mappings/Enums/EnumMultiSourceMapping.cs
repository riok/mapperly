using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents a mapping from multiple enum sources to a single target enum.
/// When useYieldReturn is true, generates multiple yield return statements (one for each source).
/// Otherwise, uses a cascading switch approach where the first source has highest priority.
/// </summary>
public class EnumMultiSourceMapping(
    ITypeSymbol source,
    ITypeSymbol target,
    IReadOnlyList<EnumSourceMapping> sourceMappings,
    EnumFallbackValueMapping fallback,
    bool useYieldReturn = false
) : NewInstanceMethodMapping(source, target)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (useYieldReturn)
        {
            // Generate yield return statements - one for each source mapping.
            foreach (var sourceMapping in sourceMappings)
            {
                var switchExpression = BuildSingleSourceSwitch(ctx, sourceMapping);
                yield return ctx.SyntaxFactory.YieldReturn(switchExpression);
            }
        }
        else
        {
            // Build a single switch expression that cascades through sources using an iterative approach.
            var switchExpression = BuildCascadingSwitch(ctx, sourceMappings);
            yield return ctx.SyntaxFactory.Return(switchExpression);
        }
    }

    private static ExpressionSyntax BuildSingleSourceSwitch(TypeMappingBuildContext ctx, EnumSourceMapping sourceMapping)
    {
        var parameterAccess = IdentifierName(sourceMapping.Parameter.Name);

        // Build switch arms for the source mappings.
        var arms = new List<SwitchExpressionArmSyntax>(sourceMapping.MemberMappings.Count + 1);
        foreach (var (sourceMember, targetMember) in sourceMapping.MemberMappings)
        {
            arms.Add(BuildArm(sourceMember, targetMember));
        }

        // Add fallback arm that throws for unsupported values.
        var sourceExpression = IdentifierName(sourceMapping.Parameter.Name);
        var fallbackArm = SwitchArm(
            DiscardPattern(),
            ThrowArgumentOutOfRangeException(sourceExpression, $"The value of enum {sourceMapping.Parameter.Type.Name} is not supported")
        );
        arms.Add(fallbackArm);

        return ctx.SyntaxFactory.Switch(parameterAccess, arms);
    }

    private ExpressionSyntax BuildCascadingSwitch(TypeMappingBuildContext ctx, IReadOnlyList<EnumSourceMapping> sources)
    {
        var innerExpression = fallback.BuildDiscardArm(ctx).Expression;

        for (var i = sources.Count - 1; i >= 0; i--)
        {
            var currentSource = sources[i];
            var parameterAccess = IdentifierName(currentSource.Parameter.Name);

            // Collect arms for current source.
            var arms = new List<SwitchExpressionArmSyntax>(currentSource.MemberMappings.Count + 1);
            foreach (var (sourceMember, targetMember) in currentSource.MemberMappings)
            {
                arms.Add(BuildArm(sourceMember, targetMember));
            }

            // Add discard arm that cascades to the inner expression (next source or fallback).
            arms.Add(SwitchArm(DiscardPattern(), innerExpression));

            // Update inner expression for the next (outer) source.
            innerExpression = ctx.SyntaxFactory.Switch(parameterAccess, arms);
        }

        return innerExpression;
    }

    private static SwitchExpressionArmSyntax BuildArm(IFieldSymbol sourceMemberField, IFieldSymbol targetMemberField)
    {
        var sourceMember = MemberAccess(FullyQualifiedIdentifier(sourceMemberField.ContainingType), sourceMemberField.Name);
        // Use the targetMemberField's ContainingType (the actual enum type) instead of TargetType
        // which could be IEnumerable<EnumType> when yield return is used
        var targetMember = MemberAccess(FullyQualifiedIdentifier(targetMemberField.ContainingType), targetMemberField.Name);
        var pattern = ConstantPattern(sourceMember);
        return SwitchArm(pattern, targetMember);
    }
}
