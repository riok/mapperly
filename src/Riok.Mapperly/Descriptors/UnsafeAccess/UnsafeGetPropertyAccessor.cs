using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

/// <summary>
/// Creates an extension method to get a non-public property value using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
/// <param name="symbol">The symbol of the property.</param>
/// <param name="className">The name of the accessor class.</param>
/// <param name="methodName">The name of the accessor method.</param>
/// <param name="enableAggressiveInlining">Whether to add MethodImpl.AggressiveInlining attribute.</param>
public class UnsafeGetPropertyAccessor(IPropertySymbol symbol, string className, string methodName, bool enableAggressiveInlining)
    : IUnsafeAccessor,
        IMemberGetter
{
    private const string DefaultSourceParameterName = "source";

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var sourceName = nameBuilder.New(DefaultSourceParameterName);

        var propertySymbol = symbol.ContainingType.IsGenericType ? symbol.OriginalDefinition : symbol;

        var source = Parameter(
            propertySymbol.ContainingType.FullyQualifiedIdentifierName(),
            sourceName,
            !propertySymbol.ContainingType.IsGenericType
        );

        var parameters = ParameterList(CommaSeparatedList(source));
        var attributes = new SyntaxList<AttributeListSyntax>
        {
            ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Method, $"get_{propertySymbol.Name}"),
        };

        if (enableAggressiveInlining)
        {
            attributes.Add(ctx.SyntaxFactory.MethodImplAttribute());
        }

        return ctx.SyntaxFactory.PublicStaticExternMethod(
            IdentifierName(propertySymbol.Type.FullyQualifiedIdentifierName()).AddTrailingSpace(),
            methodName,
            parameters,
            attributes
        );
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        if (!symbol.ContainingType.IsGenericType)
        {
            ExpressionSyntax method = nullConditional ? ConditionalAccess(baseAccess, methodName) : MemberAccess(baseAccess, methodName);
            return InvocationWithoutIndention(method);
        }

        var genericClassName = GenericName(className).WithTypeArgumentList(TypeArgumentList(symbol.ContainingType.TypeArguments));
        var invocation = InvocationExpression(MemberAccess(genericClassName, methodName))
            .WithArgumentList(ArgumentListWithoutIndention([baseAccess]));

        if (!nullConditional)
            return invocation;

        return Conditional(IsNotNull(baseAccess), invocation, DefaultLiteral());
    }
}
