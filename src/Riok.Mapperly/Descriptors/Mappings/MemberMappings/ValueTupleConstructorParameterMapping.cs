using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

public class ValueTupleConstructorParameterMapping(IFieldSymbol parameter, NullMemberMapping delegateMapping)
{
    /// <summary>
    /// The parameter the value tuple.
    /// Note: the nullability of it may not be "upgraded".
    /// </summary>
    public IFieldSymbol Parameter { get; } = parameter;

    public NullMemberMapping DelegateMapping { get; } = delegateMapping;

    public ArgumentSyntax BuildArgument(TypeMappingBuildContext ctx, bool emitFieldName)
    {
        var argumentExpression = DelegateMapping.Build(ctx);
        var argument = Argument(argumentExpression);

        // tuples inside expression cannot use the expression form (A: .., ..) instead new ValueTuple<>(..) must be used
        // custom field names cannot be used so we return a plain argument
        if (!emitFieldName)
            return argument;

        // add field name if available
        return SymbolEqualityComparer.Default.Equals(Parameter.CorrespondingTupleField, Parameter)
            ? argument
            : argument.WithNameColon(SpacedNameColon(Parameter.Name));
    }

    protected bool Equals(ValueTupleConstructorParameterMapping other) =>
        Parameter.Equals(other.Parameter, SymbolEqualityComparer.Default) && DelegateMapping.Equals(other.DelegateMapping);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((ValueTupleConstructorParameterMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SymbolEqualityComparer.Default.GetHashCode(Parameter);
            hashCode = (hashCode * 397) ^ DelegateMapping.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ValueTupleConstructorParameterMapping? left, ValueTupleConstructorParameterMapping? right) =>
        Equals(left, right);

    public static bool operator !=(ValueTupleConstructorParameterMapping? left, ValueTupleConstructorParameterMapping? right) =>
        !Equals(left, right);
}
