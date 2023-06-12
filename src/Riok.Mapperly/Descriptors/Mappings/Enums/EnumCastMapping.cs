using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

public class EnumCastMapping : CastMapping
{
    private const string IsDefinedMethodName = nameof(Enum.IsDefined);

    private readonly ITypeSymbol _enumType;
    private readonly bool _checkDefined;
    private readonly EnumFallbackValueMapping _fallback;

    public EnumCastMapping(
        ITypeSymbol enumType,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        bool checkDefined,
        EnumFallbackValueMapping fallback
    )
        : base(sourceType, targetType)
    {
        _enumType = enumType;
        _checkDefined = checkDefined;
        _fallback = fallback;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var casted = base.Build(ctx);
        if (!_checkDefined)
            return casted;

        var isDefinedMethod = MemberAccess(FullyQualifiedIdentifier(_enumType), IsDefinedMethodName);
        var isDefined = Invocation(isDefinedMethod, TypeOfExpression(FullyQualifiedIdentifier(TargetType)), casted);
        return ConditionalExpression(isDefined, casted, _fallback.Build(ctx));
    }
}
