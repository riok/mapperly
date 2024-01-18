using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.UnsafeAccess;

/// <summary>
/// Creates an extension method to access an objects non public property using .Net 8's UnsafeAccessor.
/// /// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Property, Name = "get_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
public class UnsafeGetPropertyAccessor(IPropertySymbol result, string methodName) : IUnsafeAccessor
{
    private const string DefaultSourceParameterName = "source";

    private readonly string _result = result.Type.FullyQualifiedIdentifierName();
    private readonly string _sourceType = result.ContainingType.FullyQualifiedIdentifierName();
    private readonly string _memberName = result.Name;

    public string MethodName { get; } = methodName;

    public MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var sourceName = nameBuilder.New(DefaultSourceParameterName);

        var source = Parameter(_sourceType, sourceName, true);

        var parameters = ParameterList(CommaSeparatedList(source));
        var attributeList = ctx.SyntaxFactory.UnsafeAccessorAttributeList(UnsafeAccessorType.Method, $"get_{_memberName}");
        return PublicStaticExternMethod(ctx, IdentifierName(_result).AddTrailingSpace(), MethodName, parameters, attributeList);
    }
}
