using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings;

public class UnimplementedMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    : MethodMapping(sourceType, targetType),
        IExistingTargetMapping,
        INewInstanceMapping
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx) =>
        [ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException())];

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) => BuildBody(ctx);
}
