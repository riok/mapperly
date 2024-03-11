using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record UserMappingConfiguration
{
    /// <summary>
    /// <see cref="UserMappingAttribute.Default"/>
    /// A <c>null</c> value means that this property was not specified.
    /// If <see cref="MapperAttribute.AutoUserMappings"/> is <c>false</c>,
    /// this value should also be considered as <c>false</c>.
    /// If <see cref="MapperAttribute.AutoUserMappings"/> is <c>true</c>,
    /// this should be considered as <c>true</c> for the first mapping of each type pair.
    /// </summary>
    public bool? Default { get; set; }

    /// <inheritdoc cref="UserMappingAttribute.Ignore"/>
    public bool? Ignore { get; set; }
}
