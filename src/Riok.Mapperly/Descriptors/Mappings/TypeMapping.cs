using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <inheritdoc cref="ITypeMapping"/>
[DebuggerDisplay("{GetType()}({SourceType} => {TargetType})")]
public abstract class TypeMapping : ITypeMapping
{
    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
        Parameters = Array.Empty<MethodParameter>();
    }

    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType, MethodParameter[] parameters)
    {
        SourceType = sourceType;
        TargetType = targetType;
        Parameters = parameters;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    public MethodParameter[] Parameters { get; set; } = Array.Empty<MethodParameter>();

    /// <inheritdoc cref="ITypeMapping.CallableByOtherMappings"/>
    public virtual bool CallableByOtherMappings => true;

    /// <inheritdoc cref="ITypeMapping.IsSynthetic"/>
    public virtual bool IsSynthetic => false;

    public abstract ExpressionSyntax Build(TypeMappingBuildContext ctx);

    public void AddParameters(MethodParameter[] parameters)
    {
        var hash = new HashSet<MethodParameter>(Parameters);
        foreach (var item in parameters)
        {
            hash.Add(item);
        }
        Parameters = hash.ToArray();
    }
}
