using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public abstract class UserDefinedNewInstanceRuntimeTargetTypeMapping : MethodMapping, IUserMapping
{
    private const string IsAssignableFromMethodName = nameof(Type.IsAssignableFrom);
    private const string GetTypeMethodName = nameof(GetType);

    private readonly List<RuntimeTargetTypeMapping> _mappings = new();
    private readonly bool _enableReferenceHandling;
    private readonly INamedTypeSymbol _referenceHandlerType;
    private readonly NullFallbackValue _nullArm;
    private readonly ITypeSymbol _objectType;

    protected UserDefinedNewInstanceRuntimeTargetTypeMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        bool enableReferenceHandling,
        INamedTypeSymbol referenceHandlerType,
        NullFallbackValue nullArm,
        ITypeSymbol objectType
    )
        : base(method, sourceParameter, referenceHandlerParameter, Array.Empty<MethodParameter>(), method.ReturnType)
    {
        Method = method;
        _enableReferenceHandling = enableReferenceHandling;
        _referenceHandlerType = referenceHandlerType;
        _nullArm = nullArm;
        _objectType = objectType;
    }

    public IMethodSymbol Method { get; }

    public override bool CallableByOtherMappings => false;

    public void AddMappings(IEnumerable<RuntimeTargetTypeMapping> mappings) => _mappings.AddRange(mappings);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // if reference handling is enabled and no reference handler parameter is declared
        // a new reference handler is instantiated and used.
        if (_enableReferenceHandling && ReferenceHandlerParameter == null)
        {
            // var refHandler = new RefHandler();
            var referenceHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);
            var createRefHandler = CreateInstance(_referenceHandlerType);
            yield return DeclareLocalVariable(referenceHandlerName, createRefHandler);

            ctx = ctx.WithRefHandler(referenceHandlerName);
        }

        var targetType = BuildTargetType();

        // _ => throw new ArgumentException(msg, nameof(ctx.Source)),
        var sourceType = Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var fallbackArm = SwitchExpressionArm(
            DiscardPattern(),
            ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceType} to {targetType} as there is no known type mapping"),
                ctx.Source
            )
        );

        // source switch { A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x), B x when targetType.IsAssignableFrom(typeof(BDto)) => MapToBDto(x) }
        var (typeArmContext, typeArmVariableName) = ctx.WithNewScopedSource();
        var arms = _mappings.Select(x => BuildSwitchArm(typeArmContext, typeArmVariableName, x, targetType));

        // null => default / throw
        arms = arms.Append(SwitchExpressionArm(ConstantPattern(NullLiteral()), NullSubstitute(TargetType, ctx.Source, _nullArm)));

        arms = arms.Append(fallbackArm);
        var switchExpression = SwitchExpression(ctx.Source).WithArms(CommaSeparatedList(arms, true));
        yield return ReturnStatement(switchExpression);
    }

    protected abstract ExpressionSyntax BuildTargetType();

    protected virtual ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax targetType, RuntimeTargetTypeMapping mapping)
    {
        // targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x)
        return Invocation(
            MemberAccess(targetType, IsAssignableFromMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(mapping.Mapping.TargetType.NonNullable()))
        );
    }

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }

    private SwitchExpressionArmSyntax BuildSwitchArm(
        TypeMappingBuildContext typeArmContext,
        string typeArmVariableName,
        RuntimeTargetTypeMapping mapping,
        ExpressionSyntax targetType
    )
    {
        // A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x),
        var declaration = DeclarationPattern(
            FullyQualifiedIdentifier(mapping.Mapping.SourceType.NonNullable()),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );
        var whenCondition = BuildSwitchArmWhenClause(targetType, mapping);
        var arm = SwitchExpressionArm(declaration, BuildSwitchArmMapping(mapping, typeArmContext));
        return whenCondition == null ? arm : arm.WithWhenClause(WhenClause(whenCondition));
    }

    private ExpressionSyntax BuildSwitchArmMapping(RuntimeTargetTypeMapping mapping, TypeMappingBuildContext ctx)
    {
        var mappingExpression = mapping.Mapping.Build(ctx);
        if (mapping.IsAssignableToMethodTargetType)
            return mappingExpression;

        // (TTarget)(object)MapToTarget(source);
        return CastExpression(
            FullyQualifiedIdentifier(TargetType),
            CastExpression(FullyQualifiedIdentifier(_objectType), mappingExpression)
        );
    }
}
