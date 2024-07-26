using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Symbols.Members;

[DebuggerDisplay("{FullName}")]
public class NonEmptyMemberPath : MemberPath
{
    public NonEmptyMemberPath(ITypeSymbol rootType, IReadOnlyList<IMappableMember> path)
        : base(rootType, path)
    {
        if (path.Count == 0)
            throw new ArgumentException("Parameter can not be empty!", nameof(path));
    }

    /// <summary>
    /// Gets the last part of the path.
    /// </summary>
    public override IMappableMember Member => Path[^1];

    /// <summary>
    /// Gets the type of the <see cref="Member"/>. If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public override ITypeSymbol MemberType =>
        IsAnyNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

    public MemberPathSetter BuildSetter(SimpleMappingBuilderContext ctx) => MemberPathSetter.Build(ctx, this);

    public override string ToDisplayString(bool includeRootType = true, bool includeMemberType = true)
    {
        var ofType = includeMemberType ? $" of type {Member.Type.ToDisplayString()}" : null;
        var rootType = includeRootType ? RootType.ToDisplayString() + MemberAccessSeparator : null;
        return rootType + FullName + ofType;
    }
}
