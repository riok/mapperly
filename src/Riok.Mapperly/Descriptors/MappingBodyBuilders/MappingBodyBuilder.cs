using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

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
