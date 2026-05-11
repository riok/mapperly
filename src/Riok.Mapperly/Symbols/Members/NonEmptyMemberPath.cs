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
    public override ITypeSymbol MemberReadType =>
        _memberReadType ??= IsAnyReadNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

    /// <summary>
    /// Gets the type of the <see cref="Member"/> in the context of write. If last part of the path is nullable, this type will be nullable too.
    /// </summary>
    public override ITypeSymbol MemberWriteType =>
        _memberWriteType ??= IsWriteNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

    public MemberPathSetter BuildSetter(SimpleMappingBuilderContext ctx) => MemberPathSetter.Build(ctx, this);

    public override bool IsAnyReadNullable() => Path.Any(x => x.IsReadNullable);

    public override bool IsWriteNullable() => Path[^1].IsWriteNullable;

    public override string ToDisplayString(bool includeRootType = true, bool includeMemberType = true)
    {
        var ofType = includeMemberType ? $" of type {Member.Type.ToDisplayString()}" : null;
        var rootType = includeRootType ? RootType.ToDisplayString() + MemberAccessSeparator : null;
        return rootType + FullName + ofType;
    }
}
