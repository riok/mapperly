namespace Riok.Mapperly.Configuration;

/// <summary>
/// Configuration class to represent <see cref="System.ComponentModel.DescriptionAttribute"/>
/// </summary>
public record ComponentModelDescriptionAttributeConfiguration(string? Description)
{
    public ComponentModelDescriptionAttributeConfiguration()
        : this((string?)null) { }
}
