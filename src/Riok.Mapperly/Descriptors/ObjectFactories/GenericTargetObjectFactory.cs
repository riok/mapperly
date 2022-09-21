using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

/// <summary>
/// A <see cref="GenericTargetObjectFactory"/> is an <see cref="ObjectFactory"/>
/// without any parameters but a single type parameter which is also the return type.
/// Example signature: <c>T Create&lt;T&gt;();</c>
/// </summary>
public class GenericTargetObjectFactory : ObjectFactory
{
    private readonly Compilation _compilation;

    public GenericTargetObjectFactory(IMethodSymbol method, Compilation compilation) : base(method)
    {
        _compilation = compilation;
    }

    public override bool CanCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate)
        => Method.TypeParameters[0].CanConsumeType(_compilation, targetTypeToCreate);

    protected override ExpressionSyntax BuildCreateType(ITypeSymbol sourceType, ITypeSymbol targetTypeToCreate, ExpressionSyntax source)
        => GenericInvocation(Method.Name, new[] { NonNullableIdentifier(targetTypeToCreate) });
}
