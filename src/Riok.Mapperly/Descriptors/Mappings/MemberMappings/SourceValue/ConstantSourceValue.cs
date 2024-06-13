using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A constant source value.
/// <example>"fooBar"</example>
/// <example>1</example>
/// <example>MyEnum.MyValue</example>
/// </summary>
public class ConstantSourceValue(ExpressionSyntax value) : ISourceValue
{
    public ExpressionSyntax Build(TypeMappingBuildContext ctx) => value;
}
