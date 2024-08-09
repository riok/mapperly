using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An implementation of <see cref="INewValueTupleBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public class NewValueTupleConstructorBuilderContext<T> : MembersMappingBuilderContext<T>, INewValueTupleBuilderContext<T>
    where T : INewValueTupleMapping
{
    private readonly IReadOnlyDictionary<string, IFieldSymbol> _secondarySourceNames;

    /// <summary>
    /// An implementation of <see cref="INewValueTupleBuilderContext{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the mapping.</typeparam>
    public NewValueTupleConstructorBuilderContext(MappingBuilderContext builderContext, T mapping)
        : base(builderContext, mapping)
    {
        _secondarySourceNames = mapping.SourceType.IsTupleType
            ? BuildSecondarySourceFields()
            : new Dictionary<string, IFieldSymbol>(StringComparer.Ordinal);
    }

    public bool TryMatchTupleElement(IFieldSymbol member, [NotNullWhen(true)] out MemberMappingInfo? memberInfo)
    {
        var fieldMember = new FieldMember(member, BuilderContext.SymbolAccessor);
        if (TryMatchMember(fieldMember, null, out memberInfo))
            return true;

        if (
            member.CorrespondingTupleField != null
            && !string.Equals(member.CorrespondingTupleField.Name, member.Name, StringComparison.Ordinal)
        )
        {
            var tupleFieldMember = new FieldMember(member.CorrespondingTupleField, BuilderContext.SymbolAccessor);
            if (TryMatchMember(tupleFieldMember, null, out memberInfo))
                return true;
        }

        return false;
    }

    public void AddTupleConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping)
    {
        Mapping.AddConstructorParameterMapping(mapping);
        SetTargetMemberMapped(mapping.Parameter.Name);
        MappingAdded(mapping.MemberInfo);
    }

    protected override bool TryFindSourcePath(
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath
    )
    {
        if (base.TryFindSourcePath(pathCandidates, ignoreCase, out sourceMemberPath))
            return true;

        if (TryFindSecondaryTupleSourceField(pathCandidates, out sourceMemberPath))
            return true;

        return false;
    }

    private bool TryFindSecondaryTupleSourceField(
        IEnumerable<StringMemberPath> pathCandidates,
        [NotNullWhen(true)] out SourceMemberPath? sourcePath
    )
    {
        foreach (var pathParts in pathCandidates)
        {
            if (pathParts.Path.Count != 1)
                continue;

            var name = pathParts.Path[0];
            if (_secondarySourceNames.TryGetValue(name, out var sourceField))
            {
                var sourceFieldMember = new FieldMember(sourceField, BuilderContext.SymbolAccessor);
                var sourceMemberPath = new NonEmptyMemberPath(Mapping.SourceType, [sourceFieldMember]);
                sourcePath = new SourceMemberPath(sourceMemberPath, SourceMemberType.Member);
                return true;
            }
        }

        sourcePath = null;
        return false;
    }

    private Dictionary<string, IFieldSymbol> BuildSecondarySourceFields()
    {
        return ((INamedTypeSymbol)Mapping.SourceType)
            .TupleElements.Where(t =>
                t.CorrespondingTupleField != null
                && !string.Equals(t.Name, t.CorrespondingTupleField.Name, StringComparison.Ordinal)
                && !IsIgnoredSourceMember(t.Name)
                && !IsIgnoredSourceMember(t.CorrespondingTupleField.Name)
            )
            .ToDictionary(t => t.CorrespondingTupleField!.Name, t => t, StringComparer.Ordinal);
    }
}
