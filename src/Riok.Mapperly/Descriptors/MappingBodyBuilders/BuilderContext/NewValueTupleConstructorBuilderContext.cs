using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols;

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
        if (TryMatchMember(new FieldMember(member, BuilderContext.SymbolAccessor), null, out memberInfo))
            return true;

        if (
            member.CorrespondingTupleField != null
            && !string.Equals(member.CorrespondingTupleField.Name, member.Name, StringComparison.Ordinal)
        )
        {
            if (TryMatchMember(new FieldMember(member.CorrespondingTupleField, BuilderContext.SymbolAccessor), null, out memberInfo))
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
        IReadOnlyList<IReadOnlyList<string>> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? sourceMemberPath
    )
    {
        if (base.TryFindSourcePath(pathCandidates, ignoreCase, out sourceMemberPath))
            return true;

        if (TryFindSecondaryTupleSourceField(pathCandidates, out sourceMemberPath))
            return true;

        return false;
    }

    private bool TryFindSecondaryTupleSourceField(
        IReadOnlyList<IReadOnlyList<string>> pathCandidates,
        [NotNullWhen(true)] out MemberPath? sourceMemberPath
    )
    {
        foreach (var pathParts in pathCandidates)
        {
            if (pathParts.Count != 1)
                continue;

            var name = pathParts[0];
            if (_secondarySourceNames.TryGetValue(name, out var sourceField))
            {
                sourceMemberPath = new NonEmptyMemberPath(
                    Mapping.SourceType,
                    [new FieldMember(sourceField, BuilderContext.SymbolAccessor)]
                );
                return true;
            }
        }

        sourceMemberPath = null;
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
