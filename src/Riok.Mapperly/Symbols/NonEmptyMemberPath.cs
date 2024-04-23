using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

public class NonEmptyMemberPath : MemberPath
{
    public NonEmptyMemberPath(IReadOnlyList<IMappableMember> path)
        : base(path)
    {
        if (path.Count == 0)
            throw new ArgumentException("Parameter can not be empty!", nameof(path));
    }

    /// <summary>
    /// Gets the last part of the path or throw if there is none.
    /// </summary>
    public new IMappableMember Member => Path[^1];

    /// <summary>
    /// Gets the type of the <see cref="Member"/>. If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public new ITypeSymbol MemberType => IsAnyNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;
}
