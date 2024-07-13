using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Emit.Syntax;

namespace Riok.Mapperly.Descriptors.Constructors;

public class InstanceConstructor(INamedTypeSymbol type) : IInstanceConstructor
{
    public bool SupportsObjectInitializer => true;

    public ExpressionSyntax CreateInstance(
        TypeMappingBuildContext ctx,
        IEnumerable<ArgumentSyntax> args,
        InitializerExpressionSyntax? initializer = null
    ) => SyntaxFactoryHelper.CreateInstance(type, args).WithInitializer(initializer);
}
