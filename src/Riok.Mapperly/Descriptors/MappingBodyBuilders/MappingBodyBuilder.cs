using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Builds bodies mappings (the body of the mapping methods).
/// </summary>
public class MappingBodyBuilder
{
    private readonly MappingCollection _mappings;

    public MappingBodyBuilder(MappingCollection mappings)
    {
        _mappings = mappings;
    }

    public void BuildMappingBodies()
    {
        foreach (var (typeMapping, ctx) in _mappings.DequeueMappingsToBuildBody())
        {
            switch (typeMapping)
            {
                case NewInstanceObjectPropertyMethodMapping mapping:
                    NewInstanceObjectPropertyMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case NewInstanceObjectPropertyMapping mapping:
                    NewInstanceObjectPropertyMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case IPropertyAssignmentTypeMapping mapping:
                    ObjectPropertyMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceMethodMapping mapping:
                    UserMethodMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedExistingTargetMethodMapping mapping:
                    UserMethodMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
            }
        }
    }
}
