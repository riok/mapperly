using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Symbols.Members;

public class NonEmptyMemberPath : MemberPath
{
    public NonEmptyMemberPath(ITypeSymbol rootType, IReadOnlyList<IMappableMember> path)
        : base(rootType, path)
    {
        if (path.Count == 0)
            throw new ArgumentException("Parameter can not be empty!", nameof(path));
    }

    private ITypeSymbol? _memberReadType;

    private ITypeSymbol? _memberWriteType;

    /// <summary>
    /// Gets the last part of the path.
    /// </summary>
    public override IMappableMember Member => Path[^1];

    /// <summary>
    /// Gets the name of the first path segment.
    /// </summary>
    public string RootName => Path[0].Name;

    /// <summary>
    /// Gets the type of the <see cref="Member"/> in the context of read. If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public override ITypeSymbol MemberReadType
    {
        get
        {
            if (_memberReadType != null)
                return _memberReadType;
            var baseType = IsAnyReadNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

            if (Path.Any(m => m is CollectionElementMember))
                return _memberReadType = WrapInIEnumerable(baseType);
            return _memberReadType = baseType;
        }
    }

    /// <summary>
    /// Gets the type of the <see cref="Member"/> in the context of write. If last part of the path is nullable, this type will be nullable too.
    /// </summary>
    public override ITypeSymbol MemberWriteType =>
        _memberWriteType ??= IsWriteNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

    public MemberPathSetter BuildSetter(SimpleMappingBuilderContext ctx) => MemberPathSetter.Build(ctx, this);

    public override bool IsAnyReadNullable() => Path.Any(x => x.IsReadNullable);

    public override bool IsWriteNullable() => Path[^1].IsWriteNullable;

    /// <summary>
    /// If the path contains a <see cref="CollectionElementMember"/>, the member type is wrapped in an <see cref="IEnumerable{T}"/>.
    /// This is required to support mapping of collection elements.
    /// </summary>
    /// <param name="elementType"></param>
    /// <returns></returns>
    private ITypeSymbol WrapInIEnumerable(ITypeSymbol elementType)
    {
        var collectionMember = (CollectionElementMember)Path.First(m => m is CollectionElementMember);
        //var iEnumerable = collectionMember.CollectionType.AllInterfaces.FirstOrDefault(i =>
        //    i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
        var collectionType = collectionMember.CollectionType;

        INamedTypeSymbol? iEnumerable;
        if (
            collectionType is INamedTypeSymbol { IsGenericType: true } namedCollection
            && namedCollection.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
        )
        {
            iEnumerable = namedCollection;
        }
        else
        {
            iEnumerable = collectionMember.CollectionType.AllInterfaces.FirstOrDefault(i =>
                i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
            );
        }

        if (iEnumerable == null)
            return elementType;

        return iEnumerable.OriginalDefinition.Construct(elementType);
    }

    public override string ToDisplayString(bool includeRootType = true, bool includeMemberType = true)
    {
        var ofType = includeMemberType ? $" of type {Member.Type.ToDisplayString()}" : null;
        var rootType = includeRootType ? RootType.ToDisplayString() + MemberAccessSeparator : null;
        return rootType + FullName + ofType;
    }
}
