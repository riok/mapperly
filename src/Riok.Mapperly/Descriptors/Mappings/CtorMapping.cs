using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping where the target type has the source as single ctor argument.
/// </summary>
public class CtorMapping(ITypeSymbol sourceType, ITypeSymbol targetType, IInstanceConstructor constructor)
    : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => constructor.CreateInstance(ctx, [ctx.Source]);
}
