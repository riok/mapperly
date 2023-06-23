using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// A default implementation of <see cref="IExistingTargetMapping"/>.
/// </summary>
public abstract class ExistingTargetMapping : IExistingTargetMapping
{
    protected ExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    public MethodParameter[] Parameters { get; set; } = Array.Empty<MethodParameter>();

    public abstract IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
