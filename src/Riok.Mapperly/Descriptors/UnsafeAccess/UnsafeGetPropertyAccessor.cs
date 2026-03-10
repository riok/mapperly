using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

/// <summary>
/// Creates an extension method to access an objects non public property using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Property, Name = "get_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
/// <param name="symbol">The symbol of the property.</param>
/// <param name="className">The name of the accessor class.</param>
/// <param name="methodName">The name of the accessor method.</param>
public class UnsafeGetPropertyAccessor(IPropertySymbol symbol, string className, string methodName) : IUnsafeAccessor, IMemberGetter
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
            !symbol.ContainingType.IsGenericType
        );

        var parameters = ParameterList(CommaSeparatedList(source));
        var attribute = ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Method, $"get_{symbol.Name}");
        return ctx.SyntaxFactory.PublicStaticExternMethod(
            IdentifierName(propertySymbol.Type.FullyQualifiedIdentifierName()).AddTrailingSpace(),
            methodName,
            parameters,
            [attribute]
        );
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, INamedTypeSymbol? containingType = null, bool nullConditional = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        if (!symbol.ContainingType.IsGenericType)
        {
            ExpressionSyntax method = nullConditional ? ConditionalAccess(baseAccess, methodName) : MemberAccess(baseAccess, methodName);
            return InvocationWithoutIndention(method);
        }

        // Use the passed containingType for type arguments if provided,
        // otherwise fall back to the symbol's containing type.
        // This is critical for inherited members where the cached symbol's
        // type arguments may differ from the actual derived type being mapped.
        var typeArgs = containingType?.TypeArguments ?? symbol.ContainingType.TypeArguments;
        var genericClassName = GenericName(className).WithTypeArgumentList(TypeArgumentList(typeArgs));
        var invocation = InvocationExpression(MemberAccess(genericClassName, methodName))
            .WithArgumentList(ArgumentListWithoutIndention([baseAccess]));

        if (!nullConditional)
            return invocation;

        return Conditional(IsNotNull(baseAccess), invocation, DefaultLiteral());
    }
}
