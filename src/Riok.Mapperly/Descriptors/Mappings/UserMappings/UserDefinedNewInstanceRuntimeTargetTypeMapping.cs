using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public abstract class UserDefinedNewInstanceRuntimeTargetTypeMapping(
    IMethodSymbol method,
    MethodParameter sourceParameter,
    MethodParameter? referenceHandlerParameter,
    ITypeSymbol targetType,
    ITypeSymbol returnType,
    bool enableReferenceHandling,
    NullFallbackValue? nullArm,
    ITypeSymbol objectType,
    MethodParameter? resultParameter = null
) : NewInstanceMethodMapping(method, sourceParameter, referenceHandlerParameter, targetType, returnType), INewInstanceUserMapping
{
    private const string IsAssignableFromMethodName = nameof(Type.IsAssignableFrom);
    private const string GetTypeMethodName = nameof(GetType);

    private readonly List<RuntimeTargetTypeMapping> _mappings = [];

    public new IMethodSymbol Method { get; } = method;

    /// <summary>
    /// Always false, as this cannot be called by other mappings,
    /// this can never be the default.
    /// </summary>
    public bool? Default => false;

    public bool IsExternal => false;

    /// <summary>
    /// The reference handling is enabled but is only internal to this method.
    /// No reference handler parameter is passed.
    /// </summary>
    private bool InternalReferenceHandlingEnabled => enableReferenceHandling && ReferenceHandlerParameter == null;

    public void AddMappings(IEnumerable<RuntimeTargetTypeMapping> mappings) => _mappings.AddRange(mappings);

    protected override ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, [SourceParameter, ReferenceHandlerParameter, .. AdditionalSourceParameters, resultParameter]);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // if reference handling is enabled and no reference handler parameter is declared
        // a new reference handler is instantiated and used.
        if (InternalReferenceHandlingEnabled)
        {
            var referenceHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);
            var createRefHandler = ctx.SyntaxFactory.CreateInstance<PreserveReferenceHandler>();

            yield return ctx.SyntaxFactory.DeclareLocalVariable(referenceHandlerName, createRefHandler);

            ctx = ctx.WithRefHandler(referenceHandlerName);
        }

        var targetTypeExpr = BuildTargetType();
        var sourceType = ctx.SyntaxFactory.Invocation(MemberAccess(ctx.Source, GetTypeMethodName));

        var switchBuilder =
            resultParameter == null
                ? BuildSwitchExpression(ctx, targetTypeExpr, sourceType)
                : BuildSwitchStatement(ctx, targetTypeExpr, resultParameter.Value);

        foreach (var statement in switchBuilder)
            yield return statement;
    }

    protected abstract ExpressionSyntax BuildTargetType();

    protected virtual ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax runtimeTargetType, RuntimeTargetTypeMapping mapping)
    {
        // targetType.IsAssignableFrom(typeof(ADto))
        return InvocationWithoutIndention(
            MemberAccess(runtimeTargetType, IsAssignableFromMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(mapping.Mapping.TargetType.NonNullable()))
        );
    }

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }

    #region Switch Statement Building (Out Parameter)

    /*  Used when the mapping has an out parameter where the result is assigned to that out parameter
        and the method returns a boolean indicating whether the mapping was successful.

        Example Method Signature:

        bool TryMap(A source, Type targetType, out object result)

        Example Body:

        result = default;
        switch (source)
        {
           case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
               result = MapToB(x);
               return true;
           default:
               return false;
        }
     */

    private IEnumerable<StatementSyntax> BuildSwitchStatement(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetTypeExpr,
        MethodParameter targetParameter
    )
    {
        var targetParameterName = IdentifierName(targetParameter.Name);

        // Set the out parameter to default
        yield return ctx.SyntaxFactory.ExpressionStatement(
            Assignment(targetParameterName, NullSubstitute(targetParameter.Type, ctx.Source, NullFallbackValue.Default), false)
        );

        // The Switch
        var (typeArmContext, typeArmVariableName) = ctx.WithNewScopedSource();

        var arms = _mappings.Select(x =>
            CreateMappingSwitchStatement(typeArmContext, typeArmVariableName, targetParameterName, x, targetTypeExpr)
        );
        var defaultArm = CreateSwitchStatement(
            typeArmContext,
            DefaultSwitchLabel(),
            [typeArmContext.SyntaxFactory.Return(BooleanLiteral(false))]
        );

        var src = ParenthesizedExpression(ctx.Source);
        yield return ctx.SyntaxFactory.SwitchStatement(src, arms, defaultArm).AddLeadingLineFeed(ctx.SyntaxFactory.Indentation);
    }

    private SwitchSectionSyntax CreateMappingSwitchStatement(
        TypeMappingBuildContext ctx,
        string typeArmVariableName,
        IdentifierNameSyntax targetParameterName,
        RuntimeTargetTypeMapping mapping,
        ExpressionSyntax targetType
    )
    {
        var declaration = DeclarationPattern(
            FullyQualifiedIdentifier(mapping.Mapping.SourceType.NonNullable()).AddTrailingSpace(),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );

        var caseLabel = CasePatternSwitchLabel(declaration);
        var whenCondition = BuildSwitchArmWhenClause(targetType, mapping);

        if (whenCondition != null)
            caseLabel = caseLabel.WithWhenClause(SwitchWhen(whenCondition));

        var mappingExpression = mapping.Mapping.Build(ctx);

        if (!mapping.IsAssignableToMethodTargetType)
        {
            mappingExpression = CastExpression(
                FullyQualifiedIdentifier(TargetType),
                CastExpression(FullyQualifiedIdentifier(objectType), mappingExpression)
            );
        }

        return CreateSwitchStatement(
            ctx,
            caseLabel,
            [
                ctx.SyntaxFactory.ExpressionStatement(Assignment(targetParameterName, mappingExpression, false)),
                ctx.SyntaxFactory.Return(BooleanLiteral(true)),
            ]
        );
    }

    private static SwitchSectionSyntax CreateSwitchStatement(
        TypeMappingBuildContext ctx,
        SwitchLabelSyntax labelSyntax,
        IEnumerable<StatementSyntax> statements
    )
    {
        var labelCtx = ctx.AddIndentation();
        labelSyntax = labelSyntax.AddLeadingLineFeed(labelCtx.SyntaxFactory.Indentation);

        var statementCtx = labelCtx.AddIndentation();
        statements = statements.Select(s => s.WithoutLeadingTrivia().AddLeadingLineFeed(statementCtx.SyntaxFactory.Indentation));

        return SwitchSection(labelSyntax, statements);
    }

    #endregion

    #region Switch Expression Building

    /*  Used when the mapping returns the result directly

        Example Method Signature(s):

        object Map(A source, Type targetType)
        object Map(A source)

        Example Body:

        return source switch
        {
            global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
            _ => throw new global::System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
        };

     */

    private IEnumerable<StatementSyntax> BuildSwitchExpression(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetTypeExpr,
        InvocationExpressionSyntax sourceType
    )
    {
        var fallbackArm = SwitchArm(
            DiscardPattern(),
            ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceType} to {targetTypeExpr} as there is no known type mapping"),
                ctx.Source
            )
        );

        // source switch { A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x), B x when targetType.IsAssignableFrom(typeof(BDto)) => MapToBDto(x) }
        var (typeArmContext, typeArmVariableName) = ctx.WithNewScopedSource();
        var arms = _mappings.Select(x => BuildSwitchArm(typeArmContext, typeArmVariableName, x, targetTypeExpr));

        // null => default / throw
        if (nullArm.HasValue)
        {
            arms = arms.Append(SwitchArm(ConstantPattern(NullLiteral()), NullSubstitute(TargetType, ctx.Source, nullArm.Value)));
        }

        arms = arms.Append(fallbackArm);

        var switchExpression = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpression);
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
            FullyQualifiedIdentifier(mapping.Mapping.SourceType.NonNullable()).AddTrailingSpace(),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );
        var whenCondition = BuildSwitchArmWhenClause(targetType, mapping);
        var arm = SwitchArm(declaration, BuildSwitchArmMapping(mapping, typeArmContext));
        return whenCondition == null ? arm : arm.WithWhenClause(SwitchWhen(whenCondition));
    }

    private ExpressionSyntax BuildSwitchArmMapping(RuntimeTargetTypeMapping mapping, TypeMappingBuildContext ctx)
    {
        var mappingExpression = mapping.Mapping.Build(ctx);
        if (mapping.IsAssignableToMethodTargetType)
            return mappingExpression;

        // (TTarget)(object)MapToTarget(source);
        return CastExpression(
            FullyQualifiedIdentifier(TargetType),
            CastExpression(FullyQualifiedIdentifier(objectType), mappingExpression)
        );
    }

    #endregion
}
