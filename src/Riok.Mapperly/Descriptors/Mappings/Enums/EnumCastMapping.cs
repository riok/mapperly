using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// An enum cast mapping which casts the source to the target type and optionally checks whether the target is defined.
/// If it is not defined an optional fallback value is used.
/// </summary>
public class EnumCastMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    EnumCastMapping.CheckDefinedMode checkDefinedMode,
    IReadOnlyCollection<IFieldSymbol> targetEnumMembers,
    EnumFallbackValueMapping fallback
) : CastMapping(sourceType, targetType)
{
    public enum CheckDefinedMode
    {
        /// <summary>
        /// No check is performed at all, the value is just casted.
        /// </summary>
        NoCheck,

        /// <summary>
        /// It is checked if the casted value is defined in the target enum.
        /// </summary>
        Value,

        /// <summary>
        /// It is checked if the casted value is a defined flags combination of the target enum.
        /// </summary>
        Flags
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var casted = base.Build(ctx);
        if (checkDefinedMode == CheckDefinedMode.NoCheck)
            return casted;

        var valueDefinedCondition = BuildIsDefinedCondition(casted);
        return Conditional(valueDefinedCondition, casted, fallback.Build(ctx));
    }

    private ExpressionSyntax BuildIsDefinedCondition(ExpressionSyntax convertedSourceValue)
    {
        var allEnumMembers = targetEnumMembers.Select(x => MemberAccess(FullyQualifiedIdentifier(TargetType), x.Name));
        return checkDefinedMode switch
        {
            // (TargetEnum)v is TargetEnum.A or TargetEnum.B or ...
            CheckDefinedMode.Value
                => IsPattern(convertedSourceValue, OrPattern(allEnumMembers)),

            // (TargetEnum)v == ((TargetEnum)v & (TargetEnum.A | TargetEnum.B | ...))
            CheckDefinedMode.Flags
                => Equal(
                    convertedSourceValue,
                    ParenthesizedExpression(BitwiseAnd(convertedSourceValue, ParenthesizedExpression(BitwiseOr(allEnumMembers))))
                ),
            _ => throw new ArgumentOutOfRangeException($"{nameof(checkDefinedMode)} has an unknown value {checkDefinedMode}")
        };
    }
}
