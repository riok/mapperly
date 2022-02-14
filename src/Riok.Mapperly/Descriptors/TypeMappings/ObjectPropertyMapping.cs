using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public abstract class ObjectPropertyMapping : MethodMapping
{
    private readonly List<PropertyMappingDescriptor> _propertyMappings = new();

    protected ObjectPropertyMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    public void AddPropertyMapping(PropertyMappingDescriptor propertyMapping)
        => _propertyMappings.Add(propertyMapping);

    internal IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source, ExpressionSyntax target)
    {
        foreach (var propertyMapping in _propertyMappings)
        {
            yield return PropertyMapping(propertyMapping, source, target);
        }
    }

    private static ExpressionStatementSyntax PropertyMapping(
        PropertyMappingDescriptor mapping,
        ExpressionSyntax sourceAccess,
        ExpressionSyntax targetAccess)
    {
        // Map(source.Property)
        var sourcePropertyAccess = MemberAccess(sourceAccess, mapping.Source.Name);
        var sourceMappedExpression = mapping.TypeMapping.Build(sourcePropertyAccess);

        // target.Property = Map(source.Property)
        var assignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            MemberAccess(targetAccess, mapping.Target.Name),
            sourceMappedExpression);

        return ExpressionStatement(assignment);
    }
}
