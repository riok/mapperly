namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a property or field as a format provider.
/// A format provider needs to be of a type which implements <see cref="IFormatProvider"/> and needs to have a getter.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class FormatProviderAttribute : Attribute
{
    /// <summary>
    /// If set to true, this format provider acts as a default format provider
    /// and is used for all <see cref="IFormattable"/> conversions without an explicit <see cref="MapPropertyAttribute.FormatProvider"/> set.
    /// Only one <see cref="FormatProviderAttribute"/> in a Mapper can be set to <c>true</c>.
    /// </summary>
    public bool Default { get; set; }
}
