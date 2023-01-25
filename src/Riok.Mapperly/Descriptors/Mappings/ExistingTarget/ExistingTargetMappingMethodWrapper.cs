using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Wraps an <see cref="IExistingTargetMapping"/> as <see cref="MethodMapping"/>.
/// </summary>
public abstract class ExistingTargetMappingMethodWrapper : MethodMapping
{
    private const string TargetVariableName = "target";

    private readonly IExistingTargetMapping _mapping;

    protected ExistingTargetMappingMethodWrapper(IExistingTargetMapping mapping)
        : base(mapping.SourceType, mapping.TargetType)
    {
        _mapping = mapping;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        yield return DeclareLocalVariable(targetVariableName, CreateTargetInstance(ctx));

        foreach (var statement in _mapping.Build(ctx, IdentifierName(targetVariableName)))
        {
            yield return statement;
        }

        yield return ReturnVariable(targetVariableName);
    }

    protected abstract ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx);
}
