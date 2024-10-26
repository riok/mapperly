using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings;

public class NoOpMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : IExistingTargetMapping
{
    public ITypeSymbol SourceType => sourceType;
    public ITypeSymbol TargetType => targetType;
    public bool IsSynthetic => true;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) => [];
}
