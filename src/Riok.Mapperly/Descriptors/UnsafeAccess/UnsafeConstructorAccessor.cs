using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

public class UnsafeConstructorAccessor(IMethodSymbol symbol, string className, string methodName, bool enableAggressiveInlining)
    : IUnsafeAccessor,
        IInstanceConstructor
{
    public bool SupportsObjectInitializer => false;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var typeToCreate = IdentifierName(symbol.ContainingType.FullyQualifiedIdentifierName()).AddTrailingSpace();
        var parameters = ParameterList(symbol.Parameters);
        var attributeList = ctx.SyntaxFactory.UnsafeAccessorAttributeList(UnsafeAccessorType.Constructor);
        if (enableAggressiveInlining)
            attributeList = attributeList.AddRange(ctx.SyntaxFactory.MethodImplAttributeList());
        return PublicStaticExternMethod(ctx, typeToCreate, methodName, parameters, attributeList);
    }

    public ExpressionSyntax CreateInstance(
        TypeMappingBuildContext ctx,
        IEnumerable<ArgumentSyntax> args,
        InitializerExpressionSyntax? initializer = null
    )
    {
        return ctx.SyntaxFactory.StaticInvocation(className, methodName, args);
    }
}
