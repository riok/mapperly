using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A user defined generic existing target mapping method,
/// which can have a generic source parameter,
/// a generic target parameter or both.
/// Maps to an existing target object instance.
/// Similar to <see cref="UserDefinedNewInstanceGenericTypeMapping"/> but for existing target mappings.
/// </summary>
public class UserDefinedExistingTargetGenericTypeMapping(
    IMethodSymbol method,
    MethodParameter sourceParameter,
    MethodParameter targetParameter,
    MethodParameter? referenceHandlerParameter,
    bool enableReferenceHandling
) : MethodMapping(method, sourceParameter, referenceHandlerParameter, targetParameter.Type), IExistingTargetUserMapping
{
    private const string SourceName = "source";
    private const string TargetName = "target";
    private const string GetTypeMethodName = nameof(GetType);

    private readonly List<IExistingTargetMapping> _mappings = [];

    public new IMethodSymbol Method { get; } = method;

    /// <summary>
    /// Always false, as this cannot be called by other mappings,
    /// this can never be the default.
    /// </summary>
    public bool? Default => false;

    public bool IsExternal => false;

    private MethodParameter TargetParameter { get; } = targetParameter;

    /// <summary>
    /// The reference handling is enabled but is only internal to this method.
    /// No reference handler parameter is passed.
    /// </summary>
    private bool InternalReferenceHandlingEnabled => enableReferenceHandling && ReferenceHandlerParameter == null;

    public void AddMappings(IEnumerable<IExistingTargetMapping> mappings) => _mappings.AddRange(mappings);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        throw new InvalidOperationException($"{nameof(UserDefinedExistingTargetGenericTypeMapping)} does not support {nameof(Build)}");

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return ctx.SyntaxFactory.SingleStatement(
            ctx.SyntaxFactory.Invocation(
                MethodName,
                SourceParameter.WithArgument(ctx.Source),
                TargetParameter.WithArgument(target),
                ReferenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            )
        );
    }

    public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var methodSyntax = (MethodDeclarationSyntax)Method.DeclaringSyntaxReferences.First().GetSyntax();
        return base.BuildMethod(ctx)
            .WithTypeParameterList(methodSyntax.TypeParameterList)
            .WithConstraintClauses(List(GetTypeParameterConstraintClauses()));
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // if the source or target type is nullable or is an unconstrained generic type parameter
        // (which could be null at runtime), add a null guard.
        // if (source == null || target == null)
        //    return;
        if (SourceType.IsNullable() || TargetType.IsNullable() || CanBeNull(SourceType) || CanBeNull(TargetType))
        {
            yield return BuildNullGuard(ctx);
        }

        // if reference handling is enabled and no reference handler parameter is declared
        // a new reference handler is instantiated and used.
        if (InternalReferenceHandlingEnabled)
        {
            // var refHandler = new RefHandler();
            var referenceHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);
            var createRefHandler = ctx.SyntaxFactory.CreateInstance<PreserveReferenceHandler>();
            yield return ctx.SyntaxFactory.DeclareLocalVariable(referenceHandlerName, createRefHandler);
            ctx = ctx.WithRefHandler(referenceHandlerName);
        }

        if (_mappings.Count == 0)
        {
            yield return ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException());
            yield break;
        }

        var targetExpression = IdentifierName(TargetParameter.Name);
        var sourceExpression = TupleExpression(CommaSeparatedList(Argument(ctx.Source), Argument(targetExpression)));
        var (sectionCtx, sourceVariableName) = ctx.WithNewScopedSource(SourceName);
        var targetVariableName = ctx.NameBuilder.New(TargetName);
        var caseSections = _mappings.Select(x => BuildSwitchSection(sectionCtx, x, sourceVariableName, targetVariableName));
        var defaultSection = BuildDefaultSwitchSection(ctx, targetExpression);

        yield return ctx
            .SyntaxFactory.SwitchStatement(sourceExpression, caseSections, defaultSection)
            .AddLeadingLineFeed(ctx.SyntaxFactory.Indentation);
    }

    protected override ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, [SourceParameter, TargetParameter, ReferenceHandlerParameter, .. AdditionalSourceParameters]);

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }

    private IEnumerable<TypeParameterConstraintClauseSyntax> GetTypeParameterConstraintClauses()
    {
        foreach (var tp in Method.TypeParameters)
        {
            var constraints = new List<TypeParameterConstraintSyntax>();

            if (tp.HasUnmanagedTypeConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("unmanaged")).AddLeadingSpace());
            }
            else if (tp.HasValueTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint).AddLeadingSpace());
            }
            else if (tp.HasNotNullConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("notnull")).AddLeadingSpace());
            }
            else if (tp.HasReferenceTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint).AddLeadingSpace());
            }

            foreach (var c in tp.ConstraintTypes)
            {
                constraints.Add(TypeConstraint(FullyQualifiedIdentifier(c)).AddLeadingSpace());
            }

            if (tp.HasConstructorConstraint)
            {
                constraints.Add(ConstructorConstraint().AddLeadingSpace());
            }

            if (!constraints.Any())
            {
                continue;
            }

            yield return TypeParameterConstraintClause(
                    IdentifierName(tp.Name).AddLeadingSpace().AddTrailingSpace(),
                    SeparatedList(constraints)
                )
                .AddLeadingSpace();
        }
    }

    private SwitchSectionSyntax BuildSwitchSection(
        TypeMappingBuildContext ctx,
        IExistingTargetMapping mapping,
        string sourceVariableName,
        string targetVariableName
    )
    {
        var sectionCtx = ctx.AddIndentation();

        // (A source, B target)
        var positionalTypeMatch = PositionalPatternClause(
            CommaSeparatedList(
                Subpattern(DeclarationPattern(mapping.SourceType, sourceVariableName)),
                Subpattern(DeclarationPattern(mapping.TargetType, targetVariableName))
            )
        );
        var pattern = RecursivePattern().WithPositionalPatternClause(positionalTypeMatch);

        // case (A source, B target):
        var caseLabel = CasePatternSwitchLabel(pattern).AddLeadingLineFeed(sectionCtx.SyntaxFactory.Indentation);

        // break;
        var statementContext = sectionCtx.AddIndentation();
        var breakStatement = BreakStatement().AddLeadingLineFeed(statementContext.SyntaxFactory.Indentation);
        var target = IdentifierName(targetVariableName);
        var statements = mapping.Build(statementContext, target).Append(breakStatement);

        return SwitchSection(caseLabel, statements);
    }

    private SwitchSectionSyntax BuildDefaultSwitchSection(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // default:
        var sectionCtx = ctx.SyntaxFactory.AddIndentation();
        var defaultCaseLabel = DefaultSwitchLabel().AddLeadingLineFeed(sectionCtx.Indentation);

        // throw new ArgumentException(msg, nameof(ctx.Source)),
        var sourceTypeExpr = ctx.SyntaxFactory.Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var targetTypeExpr = ctx.SyntaxFactory.Invocation(MemberAccess(target, GetTypeMethodName));
        var statementContext = sectionCtx.AddIndentation();
        var throwExpression = ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceTypeExpr} to {targetTypeExpr} as there is no known type mapping"),
                ctx.Source
            )
            .AddLeadingLineFeed(statementContext.Indentation);

        var statements = new StatementSyntax[] { ExpressionStatement(throwExpression) };

        return SwitchSection(defaultCaseLabel, statements);
    }

    private StatementSyntax BuildNullGuard(TypeMappingBuildContext ctx)
    {
        var nullChecks = new List<ExpressionSyntax>();
        if (SourceType.IsNullable() || CanBeNull(SourceType))
        {
            nullChecks.Add(IsNull(ctx.Source));
        }

        if (TargetType.IsNullable() || CanBeNull(TargetType))
        {
            nullChecks.Add(IsNull(IdentifierName(TargetParameter.Name)));
        }

        return ctx.SyntaxFactory.If(Or(nullChecks), ctx.SyntaxFactory.AddIndentation().Return());
    }

    /// <summary>
    /// Whether the type is a type parameter that is not constrained to be a value type,
    /// meaning null could be passed at runtime.
    /// </summary>
    private static bool CanBeNull(ITypeSymbol type) =>
        type is ITypeParameterSymbol { HasValueTypeConstraint: false, HasUnmanagedTypeConstraint: false };
}
