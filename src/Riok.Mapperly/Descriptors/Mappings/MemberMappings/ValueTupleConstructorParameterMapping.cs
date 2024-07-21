using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

public class ValueTupleConstructorParameterMapping(IFieldSymbol parameter, ISourceValue sourceValue, MemberMappingInfo memberInfo)
{
    public MemberMappingInfo MemberInfo { get; } = memberInfo;

    /// <summary>
    /// The parameter the value tuple.
    /// Note: the nullability of it may not be "upgraded".
    /// </summary>
    public IFieldSymbol Parameter { get; } = parameter;

    private readonly ISourceValue _sourceValue = sourceValue;

    public ArgumentSyntax BuildArgument(TypeMappingBuildContext ctx, bool emitFieldName)
    {
        var argumentExpression = _sourceValue.Build(ctx);
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

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (ValueTupleConstructorParameterMapping)obj;
        return Parameter.Equals(other.Parameter, SymbolEqualityComparer.Default) && _sourceValue.Equals(other._sourceValue);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SymbolEqualityComparer.Default.GetHashCode(Parameter);
            hashCode = (hashCode * 397) ^ _sourceValue.GetHashCode();
            return hashCode;
        }
    }
}
