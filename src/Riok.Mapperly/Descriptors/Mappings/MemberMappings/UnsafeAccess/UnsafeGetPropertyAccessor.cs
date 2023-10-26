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
public class UnsafeGetPropertyAccessor : IUnsafeAccessor
{
    private const string DefaultSourceParameterName = "source";

    private readonly string _result;
    private readonly string _sourceType;
    private readonly string _memberName;

    public UnsafeGetPropertyAccessor(IPropertySymbol result, string methodName)
    {
        _sourceType = result.ContainingType.FullyQualifiedIdentifierName();
        _result = result.Type.FullyQualifiedIdentifierName();
        _memberName = result.Name;
        MethodName = methodName;
    }

    public string MethodName { get; }

    public MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var sourceName = nameBuilder.New(DefaultSourceParameterName);

        var source = Parameter(_sourceType, sourceName, true);

        var parameters = ParameterList(CommaSeparatedList(source));
        var attributeList = ctx.SyntaxFactory.UnsafeAccessorAttributeList(UnsafeAccessorType.Method, $"get_{_memberName}");
        return PublicStaticExternMethod(IdentifierName(_result).AddTrailingSpace(), MethodName, parameters, attributeList);
    }
}
