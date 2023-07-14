using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// An enum cast mapping which casts the source to the target type and optionally checks whether the target is defined.
/// If it is not defined an optional fallback value is used.
/// </summary>
public class EnumCastMapping : CastMapping
{
    private readonly CheckDefinedMode _checkDefinedMode;
    private readonly IReadOnlyCollection<IFieldSymbol> _targetEnumMembers;
    private readonly EnumFallbackValueMapping _fallback;

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

    public EnumCastMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        CheckDefinedMode checkDefinedMode,
        IReadOnlyCollection<IFieldSymbol> targetEnumMembers,
        EnumFallbackValueMapping fallback
    )
        : base(sourceType, targetType)
    {
        _checkDefinedMode = checkDefinedMode;
        _targetEnumMembers = targetEnumMembers;
        _fallback = fallback;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var casted = base.Build(ctx);
        if (_checkDefinedMode == CheckDefinedMode.NoCheck)
            return casted;

        var valueDefinedCondition = BuildIsDefinedCondition(casted);
        return ConditionalExpression(valueDefinedCondition, casted, _fallback.Build(ctx));
    }

    private ExpressionSyntax BuildIsDefinedCondition(ExpressionSyntax convertedSourceValue)
    {
        var allEnumMembers = _targetEnumMembers.Select(x => MemberAccess(FullyQualifiedIdentifier(TargetType), x.Name));
        return _checkDefinedMode switch
        {
            // (TargetEnum)v is TargetEnum.A or TargetEnum.B or ...
            CheckDefinedMode.Value
                => IsPatternExpression(convertedSourceValue, OrPattern(allEnumMembers)),

            // (TargetEnum)v == ((TargetEnum)v & (TargetEnum.A | TargetEnum.B | ...))
            CheckDefinedMode.Flags
                => Equal(
                    convertedSourceValue,
                    ParenthesizedExpression(BitwiseAnd(convertedSourceValue, ParenthesizedExpression(BitwiseOr(allEnumMembers))))
                ),
            _ => throw new ArgumentOutOfRangeException($"{nameof(_checkDefinedMode)} has an unknown value {_checkDefinedMode}")
        };
    }
}
