using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Wraps an <see cref="IExistingTargetMapping"/> as <see cref="MethodMapping"/>.
/// </summary>
public abstract class ExistingTargetMappingMethodWrapper(IExistingTargetMapping mapping)
    : MethodMapping(mapping.SourceType, mapping.TargetType)
{
    private const string TargetVariableName = "target";

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, CreateTargetInstance(ctx));

        foreach (var statement in mapping.Build(ctx, IdentifierName(targetVariableName)))
        {
            yield return statement;
        }

        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }

    protected abstract ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx);
}
