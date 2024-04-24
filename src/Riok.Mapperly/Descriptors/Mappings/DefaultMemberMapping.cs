using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping that returns default.
/// <code>
/// target = default;
/// </code>
/// </summary>
public class DefaultMemberMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => DefaultLiteral();
}
