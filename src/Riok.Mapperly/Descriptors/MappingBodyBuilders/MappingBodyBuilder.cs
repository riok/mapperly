using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

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

    public void BuildMappingBodies(CancellationToken cancellationToken)
    {
        foreach (var (typeMapping, ctx) in _mappings.DequeueMappingsToBuildBody())
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (typeMapping)
            {
                case NewInstanceObjectMemberMethodMapping mapping:
                    NewInstanceObjectMemberMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case NewInstanceObjectMemberMapping mapping:
                    NewInstanceObjectMemberMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case NewValueTupleExpressionMapping mapping:
                    NewValueTupleMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case NewValueTupleConstructorMapping mapping:
                    NewValueTupleMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case IMemberAssignmentTypeMapping mapping:
                    ObjectMemberMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceMethodMapping mapping:
                    UserMethodMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedExistingTargetMethodMapping mapping:
                    UserMethodMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceRuntimeTargetTypeParameterMapping mapping:
                    RuntimeTargetTypeMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceGenericTypeMapping mapping:
                    RuntimeTargetTypeMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
            }
        }
    }
}
