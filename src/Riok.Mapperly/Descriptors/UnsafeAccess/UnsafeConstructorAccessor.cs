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

/// <summary>
/// Creates an extension method to create an instance using a non-public ctor .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
/// public extern static global::MyClass Create();
/// </code>
/// </summary>
/// <param name="symbol">The symbol of the ctor.</param>
/// <param name="className">The name of the accessor class.</param>
/// <param name="methodName">The name of the accessor method.</param>
/// <param name="enableAggressiveInlining">Whether to add MethodImpl.AggressiveInlining attribute.</param>
public class UnsafeConstructorAccessor(IMethodSymbol symbol, string className, string methodName, bool enableAggressiveInlining)
    : IUnsafeAccessor,
        IInstanceConstructor
{
    public bool SupportsObjectInitializer => false;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var methodSymbol = symbol.ContainingType.IsGenericType ? symbol.OriginalDefinition : symbol;
        var typeToCreate = IdentifierName(methodSymbol.ContainingType.FullyQualifiedIdentifierName()).AddTrailingSpace();
        var parameters = ParameterList(methodSymbol.Parameters);
        var attributes = new SyntaxList<AttributeListSyntax> { ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Constructor) };

        if (enableAggressiveInlining)
        {
            attributes.Add(ctx.SyntaxFactory.MethodImplAttribute());
        }

        return ctx.SyntaxFactory.PublicStaticExternMethod(typeToCreate, methodName, parameters, attributes);
    }

    public ExpressionSyntax CreateInstance(
        TypeMappingBuildContext ctx,
        IEnumerable<ArgumentSyntax> args,
        InitializerExpressionSyntax? initializer = null
    )
    {
        if (!symbol.ContainingType.IsGenericType)
        {
            return ctx.SyntaxFactory.StaticInvocation(className, methodName, args);
        }

        var genericClassName = GenericName(className).WithTypeArgumentList(TypeArgumentList(symbol.ContainingType.TypeArguments));
        return InvocationExpression(MemberAccess(genericClassName, methodName)).WithArgumentList(ArgumentListWithoutIndention(args));
    }
}
