using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public class UserDefinedNewInstanceRuntimeTargetTypeMapping : MethodMapping, IUserMapping
{
    private const string IsAssignableFromMethodName = nameof(Type.IsAssignableFrom);
    private const string GetTypeMethodName = nameof(GetType);

    private readonly MethodParameter _targetTypeParameter;
    private readonly List<ITypeMapping> _mappings = new();
    private readonly bool _addNullArm;
    private readonly bool _enableReferenceHandling;
    private readonly INamedTypeSymbol _referenceHandlerType;

    public UserDefinedNewInstanceRuntimeTargetTypeMapping(
        IMethodSymbol method,
        RuntimeTargetTypeMappingMethodParameters parameters,
        bool enableReferenceHandling,
        INamedTypeSymbol referenceHandlerType,
        bool addNullArm
    )
        : base(method, parameters.Source, parameters.ReferenceHandler, method.ReturnType)
    {
        Method = method;
        _enableReferenceHandling = enableReferenceHandling;
        _referenceHandlerType = referenceHandlerType;
        _addNullArm = addNullArm;
        _targetTypeParameter = parameters.TargetType;
    }

    public IMethodSymbol Method { get; }

    public override bool CallableByOtherMappings => false;

    public void AddMappings(IEnumerable<ITypeMapping> mappings) => _mappings.AddRange(mappings);

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

        var targetType = IdentifierName(_targetTypeParameter.Name);

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

        // null => null
        if (_addNullArm)
        {
            arms = arms.Append(SwitchExpressionArm(ConstantPattern(NullLiteral()), DefaultLiteral()));
        }

        arms = arms.Append(fallbackArm);
        var switchExpression = SwitchExpression(ctx.Source).WithArms(CommaSeparatedList(arms, true));
        yield return ReturnStatement(switchExpression);
    }

    protected override ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, SourceParameter, _targetTypeParameter, ReferenceHandlerParameter);

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }

    private SwitchExpressionArmSyntax BuildSwitchArm(
        TypeMappingBuildContext typeArmContext,
        string typeArmVariableName,
        ITypeMapping mapping,
        ExpressionSyntax reflectionTargetType
    )
    {
        // A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x),
        var declaration = DeclarationPattern(
            FullyQualifiedIdentifier(mapping.SourceType.NonNullable()),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );
        var whenCondition = Invocation(
            MemberAccess(reflectionTargetType, IsAssignableFromMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(mapping.TargetType.NonNullable()))
        );
        return SwitchExpressionArm(declaration, mapping.Build(typeArmContext)).WithWhenClause(WhenClause(whenCondition));
    }
}
