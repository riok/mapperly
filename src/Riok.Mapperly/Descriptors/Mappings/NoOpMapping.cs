using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings;

public class NoOpMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    : MethodMapping(sourceType, targetType, enableAggressiveInlining: false),
        IExistingTargetMapping
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx) => [];

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) => [];
}
