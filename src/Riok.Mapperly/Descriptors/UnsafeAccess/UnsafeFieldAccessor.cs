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
/// Creates an extension method to access an objects non public field using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
/// <param name="symbol">The symbol of the property.</param>
/// <param name="className">The name of the accessor class.</param>
/// <param name="methodName">The name of the accessor method.</param>
public class UnsafeFieldAccessor(IFieldSymbol symbol, string className, string methodName) : IUnsafeAccessor, IMemberSetter, IMemberGetter
{
    private const string DefaultTargetParameterName = "target";

    public bool SupportsCoalesceAssignment => true;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var targetName = nameBuilder.New(DefaultTargetParameterName);

        var fieldSymbol = symbol.ContainingType.IsGenericType ? symbol.OriginalDefinition : symbol;

        var target = Parameter(fieldSymbol.ContainingType.FullyQualifiedIdentifierName(), targetName, !symbol.ContainingType.IsGenericType);

        var parameters = ParameterList(CommaSeparatedList(target));
        var attribute = ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Field, fieldSymbol.Name);
        var returnType = RefType(IdentifierName(fieldSymbol.Type.FullyQualifiedIdentifierName()).AddTrailingSpace())
            .WithRefKeyword(Token(TriviaList(), SyntaxKind.RefKeyword, TriviaList(Space)));

        return ctx.SyntaxFactory.PublicStaticExternMethod(returnType, methodName, parameters, [attribute]);
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
        var typeArgs = containingType?.TypeArguments ?? symbol.ContainingType.TypeArguments;
        var genericClassName = GenericName(className).WithTypeArgumentList(TypeArgumentList(typeArgs));
        var invocation = InvocationExpression(MemberAccess(genericClassName, methodName))
            .WithArgumentList(ArgumentListWithoutIndention([baseAccess]));

        if (!nullConditional)
            return invocation;

        return Conditional(IsNotNull(baseAccess), invocation, DefaultLiteral());
    }

    public ExpressionSyntax BuildAssignment(
        ExpressionSyntax? baseAccess,
        ExpressionSyntax valueToAssign,
        INamedTypeSymbol? containingType = null,
        bool coalesceAssignment = false
    )
    {
        var access = BuildAccess(baseAccess, containingType);
        return Assignment(access, valueToAssign, coalesceAssignment);
    }
}
