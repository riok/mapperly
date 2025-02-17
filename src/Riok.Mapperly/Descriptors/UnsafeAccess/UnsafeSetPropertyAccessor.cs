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
/// Creates an extension method to set an objects non public property using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Property, Name = "set_value")]
/// public extern static void SetValue(this global::MyClass source, int value);
/// </code>
/// </summary>
public class UnsafeSetPropertyAccessor(IPropertySymbol symbol, string methodName) : IUnsafeAccessor, IMemberSetter
{
    private const string DefaultTargetParameterName = "target";
    private const string DefaultValueParameterName = "value";

    public bool SupportsCoalesceAssignment => false;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var targetName = nameBuilder.New(DefaultTargetParameterName);
        var valueName = nameBuilder.New(DefaultValueParameterName);

        var target = Parameter(symbol.ContainingType.FullyQualifiedIdentifierName(), targetName, true);
        var targetValue = Parameter(symbol.Type.FullyQualifiedIdentifierName(), valueName);

        var parameters = ParameterList(CommaSeparatedList(target, targetValue));
        var attribute = ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Method, $"set_{symbol.Name}");

        return PublicStaticExternMethod(
            ctx,
            PredefinedType(Token(SyntaxKind.VoidKeyword)).AddTrailingSpace(),
            methodName,
            parameters,
            [attribute]
        );
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        return InvocationWithoutIndention(MemberAccess(baseAccess, methodName), valueToAssign);
    }
}
