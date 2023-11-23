using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record HasSyntaxReference
{
    /// <summary>
    /// Gets or sets the syntax reference, from where the data of this configuration was read.
    /// Initialized by <see cref="AttributeDataAccessor"/>.
    /// </summary>
    public SyntaxNode? SyntaxReference { get; set; }

    public Location? Location => SyntaxReference?.GetLocation();
}
