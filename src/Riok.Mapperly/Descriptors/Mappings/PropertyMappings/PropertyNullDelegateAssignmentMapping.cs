using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// a property mapping container, which performs a null check before the mappings.
/// </summary>
[DebuggerDisplay("PropertyNullDelegateMapping({_nullConditionalSourcePath} != null)")]
public class PropertyNullDelegateAssignmentMapping : PropertyAssignmentMappingContainer
{
    private readonly PropertyPath _nullConditionalSourcePath;
    private readonly bool _throwInsteadOfConditionalNullMapping;

    public PropertyNullDelegateAssignmentMapping(
        PropertyPath nullConditionalSourcePath,
        IPropertyAssignmentMappingContainer parent,
        bool throwInsteadOfConditionalNullMapping)
        : base(parent)
    {
        _nullConditionalSourcePath = nullConditionalSourcePath;
        _throwInsteadOfConditionalNullMapping = throwInsteadOfConditionalNullMapping;
    }

    public override IEnumerable<StatementSyntax> Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess)
    {
        // if (source.Value != null)
        //   target.Value = Map(Source.Name);
        // else
        //   throw ...
        var sourceNullConditionalAccess = _nullConditionalSourcePath.BuildAccess(ctx.Source, true, true, true);
        var condition = IsNotNull(sourceNullConditionalAccess);
        var elseClause = _throwInsteadOfConditionalNullMapping
            ? ElseClause(Block(ExpressionStatement(ThrowArgumentNullException(sourceNullConditionalAccess))))
            : null;

        return new[]
        {
            IfStatement(condition, Block(base.Build(ctx, targetAccess)), elseClause),
        };
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((PropertyNullDelegateAssignmentMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (_nullConditionalSourcePath.GetHashCode() * 397) ^ _throwInsteadOfConditionalNullMapping.GetHashCode();
        }
    }

    public static bool operator ==(PropertyNullDelegateAssignmentMapping? left, PropertyNullDelegateAssignmentMapping? right)
        => Equals(left, right);

    public static bool operator !=(PropertyNullDelegateAssignmentMapping? left, PropertyNullDelegateAssignmentMapping? right)
        => !Equals(left, right);

    protected bool Equals(PropertyNullDelegateAssignmentMapping other)
    {
        return _nullConditionalSourcePath.Equals(other._nullConditionalSourcePath)
            && _throwInsteadOfConditionalNullMapping == other._throwInsteadOfConditionalNullMapping;
    }
}
