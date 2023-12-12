using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A derived type mapping maps one base type or interface to another
/// by implementing a switch statement over known types and performs the provided mapping for each type.
/// </summary>
public class DerivedExistingTargetTypeSwitchMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    IReadOnlyCollection<IExistingTargetMapping> existingTargetTypeMappings
) : ExistingTargetMapping(sourceType, targetType)
{
    private const string SourceName = "source";
    private const string TargetName = "target";
    private const string GetTypeMethodName = nameof(GetType);

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var sourceExpression = TupleExpression(CommaSeparatedList(Argument(ctx.Source), Argument(target)));
        var caseSections = existingTargetTypeMappings.Select(x => BuildSwitchSection(ctx, x));
        var defaultSection = BuildDefaultSwitchSection(ctx, target);

        yield return ctx.SyntaxFactory
            .SwitchStatement(sourceExpression, caseSections, defaultSection)
            .AddLeadingLineFeed(ctx.SyntaxFactory.Indentation);
    }

    private SwitchSectionSyntax BuildSwitchSection(TypeMappingBuildContext ctx, IExistingTargetMapping mapping)
    {
        var (sectionCtx, sourceVariableName) = ctx.WithNewScopedSource(SourceName);
        var targetVariableName = sectionCtx.NameBuilder.New(TargetName);
        sectionCtx = sectionCtx.AddIndentation();

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
        var sourceTypeExpr = Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var targetTypeExpr = Invocation(MemberAccess(target, GetTypeMethodName));
        var statementContext = sectionCtx.AddIndentation();
        var throwExpression = ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceTypeExpr} to {targetTypeExpr} as there is no known derived type mapping"),
                ctx.Source
            )
            .AddLeadingLineFeed(statementContext.Indentation);

        var statements = new StatementSyntax[] { ExpressionStatement(throwExpression) };

        return SwitchSection(defaultCaseLabel, statements);
    }
}
