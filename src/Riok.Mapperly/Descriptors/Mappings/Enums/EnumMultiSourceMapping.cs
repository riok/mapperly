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
            // Generate multiple yield return statements - one for each source enum
            foreach (var sourceMapping in sourceMappings)
            {
                var switchExpression = BuildSingleSourceSwitch(ctx, sourceMapping);
                yield return ctx.SyntaxFactory.YieldReturn(switchExpression);
            }
        }
        else
        {
            // Build cascading switch expression starting with the first source
            var switchExpression = BuildCascadingSwitch(ctx, sourceMappings, 0);
            yield return ctx.SyntaxFactory.Return(switchExpression);
        }
    }

    private static ExpressionSyntax BuildSingleSourceSwitch(TypeMappingBuildContext ctx, EnumSourceMapping sourceMapping)
    {
        var parameterAccess = IdentifierName(sourceMapping.Parameter.Name);

        // Build switch arms for the source
        var arms = sourceMapping.MemberMappings.Select(x => BuildArm(x.Key, x.Value)).ToList();

        // Add fallback arm that throws for this specific source parameter
        var sourceExpression = IdentifierName(sourceMapping.Parameter.Name);
        var fallbackArm = SwitchArm(
            DiscardPattern(),
            ThrowArgumentOutOfRangeException(sourceExpression, $"The value of enum {sourceMapping.Parameter.Type.Name} is not supported")
        );
        arms.Add(fallbackArm);

        return ctx.SyntaxFactory.Switch(parameterAccess, arms);
    }

    private ExpressionSyntax BuildCascadingSwitch(TypeMappingBuildContext ctx, IReadOnlyList<EnumSourceMapping> sources, int index)
    {
        var currentSource = sources[index];
        var parameterAccess = IdentifierName(currentSource.Parameter.Name);

        // Build switch arms for current source
        var arms = currentSource.MemberMappings.Select(x => BuildArm(x.Key, x.Value)).ToList();

        // Add fallback for current source
        if (index == sources.Count - 1)
        {
            // Last source uses the configured fallback
            arms.Add(fallback.BuildDiscardArm(ctx));
        }
        else
        {
            // Non-last source cascades to next source
            var nextSwitch = BuildCascadingSwitch(ctx, sources, index + 1);
            arms.Add(SwitchArm(DiscardPattern(), nextSwitch));
        }

        return ctx.SyntaxFactory.Switch(parameterAccess, arms);
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
