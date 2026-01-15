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
/// Creates an extension method to set a non-public property value using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Value")]
/// public extern static void SetValue(this global::MyClass target, int value);
/// </code>
/// </summary>
/// <param name="symbol">The symbol of the property.</param>
/// <param name="className">The name of the accessor class.</param>
/// <param name="methodName">The name of the accessor method.</param>
/// <param name="enableAggressiveInlining">Whether to add MethodImpl.AggressiveInlining attribute.</param>
public class UnsafeSetPropertyAccessor(IPropertySymbol symbol, string className, string methodName, bool enableAggressiveInlining)
    : IUnsafeAccessor,
        IMemberSetter
{
    private const string DefaultTargetParameterName = "target";
    private const string DefaultValueParameterName = "value";

    public bool SupportsCoalesceAssignment => false;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var targetName = nameBuilder.New(DefaultTargetParameterName);
        var valueName = nameBuilder.New(DefaultValueParameterName);

        var propertySymbol = symbol.ContainingType.IsGenericType ? symbol.OriginalDefinition : symbol;

        var target = Parameter(
            propertySymbol.ContainingType.FullyQualifiedIdentifierName(),
            targetName,
            !propertySymbol.ContainingType.IsGenericType
        );
        var value = Parameter(propertySymbol.Type.FullyQualifiedIdentifierName(), valueName);

        var parameters = ParameterList(CommaSeparatedList(target, value));
        var attributes = new SyntaxList<AttributeListSyntax>
        {
            ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Method, $"set_{propertySymbol.Name}"),
        };

        if (enableAggressiveInlining)
        {
            attributes.Add(ctx.SyntaxFactory.MethodImplAttribute());
        }

        return ctx.SyntaxFactory.PublicStaticExternMethod(
            PredefinedType(Token(SyntaxKind.VoidKeyword)).AddTrailingSpace(),
            methodName,
            parameters,
            attributes
        );
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        if (!symbol.ContainingType.IsGenericType)
        {
            return InvocationWithoutIndention(MemberAccess(baseAccess, methodName), valueToAssign);
        }

        var genericClassName = GenericName(className).WithTypeArgumentList(TypeArgumentList(symbol.ContainingType.TypeArguments));
        return InvocationExpression(MemberAccess(genericClassName, methodName))
            .WithArgumentList(ArgumentListWithoutIndention([baseAccess, valueToAssign]));
    }
}
