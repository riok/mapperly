using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A source value which is provided by a method.
/// </summary>
public class MethodProvidedSourceValue(string methodName) : ISourceValue
{
    public ExpressionSyntax Build(TypeMappingBuildContext ctx) => Invocation(methodName);
}
