using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

[DebuggerDisplay("PropertyMapping({_source.Name} => {_target.Name})")]
public class PropertyMapping
{
    private const string NullableValueProperty = "Value";

    private readonly TypeMapping _mapping;
    private readonly IPropertySymbol _source;
    private readonly IPropertySymbol _target;
    private readonly bool _throwInsteadOfConditionalNullMapping;

    public PropertyMapping(
        IPropertySymbol source,
        IPropertySymbol target,
        TypeMapping mapping,
        bool throwInsteadOfConditionalNullMapping)
    {
        _source = source;
        _target = target;
        _mapping = mapping;
        _throwInsteadOfConditionalNullMapping = throwInsteadOfConditionalNullMapping;
    }

    public StatementSyntax Build(
        ExpressionSyntax sourceAccess,
        ExpressionSyntax targetAccess)
    {
        var targetPropertyAccess = MemberAccess(targetAccess, _target.Name);
        ExpressionSyntax sourcePropertyAccess = MemberAccess(sourceAccess, _source.Name);

        // if source is nullable, but mapping doesn't accept nulls
        // condition: source != null
        (var condition, sourcePropertyAccess) = BuildPreMappingCondition(sourcePropertyAccess);
        var mappedValue = _mapping.Build(sourcePropertyAccess);

        // target.Property = mappedValue;
        var assignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            targetPropertyAccess,
            mappedValue);
        var assignmentExpression = ExpressionStatement(assignment);

        // if (source.Value != null)
        //   target.Value = Map(Source.Name);
        // else
        //   throw ...
        return BuildIf(condition, assignmentExpression, sourcePropertyAccess);
    }

    private StatementSyntax BuildIf(ExpressionSyntax? condition, StatementSyntax assignment, ExpressionSyntax sourcePropertyAccess)
    {
        if (condition == null)
            return assignment;

        var elseClause = _throwInsteadOfConditionalNullMapping
            ? ElseClause(ExpressionStatement(ThrowNewArgumentNullException(sourcePropertyAccess)))
            : null;
        return IfStatement(condition, assignment, elseClause);
    }

    private (ExpressionSyntax? Condition, ExpressionSyntax SourceAccess) BuildPreMappingCondition(ExpressionSyntax sourceAccess)
    {
        if (!_source.IsNullable() || _mapping.SourceType.IsNullable() || (_mapping is DirectAssignmentMapping && _target.IsNullable()))
            return (null, sourceAccess);

        // if source is nullable but the mapping does not accept nulls
        // and is also not a direct assignment where the target is also nullable
        // add not null condition
        var condition = IsNotNull(sourceAccess);

        // source != null
        // if the source is a nullable value type
        // replace source by source.Value for the mapping
        if (_source.Type.IsNullableValueType())
        {
            sourceAccess = MemberAccess(sourceAccess, NullableValueProperty);
        }

        return (condition, sourceAccess);
    }
}
