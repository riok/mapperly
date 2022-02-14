using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors;

[DebuggerDisplay("PropertyMapping({Source.Name} => {Target.Name})")]
public class PropertyMappingDescriptor
{
    public PropertyMappingDescriptor(
        IPropertySymbol source,
        IPropertySymbol target,
        TypeMapping typeMapping)
    {
        Source = source;
        Target = target;
        TypeMapping = typeMapping;
    }

    public IPropertySymbol Source { get; }

    public IPropertySymbol Target { get; }

    public TypeMapping TypeMapping { get; }
}
