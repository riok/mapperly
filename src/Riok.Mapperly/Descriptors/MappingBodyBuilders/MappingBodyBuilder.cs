using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Builds bodies mappings (the body of the mapping methods).
/// </summary>
public class MappingBodyBuilder(MappingCollection mappings)
{
    public void BuildMappingBodies(CancellationToken cancellationToken)
    {
        foreach (var (typeMapping, ctx) in mappings.DequeueMappingsToBuildBody())
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (typeMapping)
            {
                case INewInstanceEnumerableMapping mapping:
                    EnumerableMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case IEnumerableMapping mapping:
                    EnumerableMappingBodyBuilder.BuildMappingBody(ctx, mapping);
                    break;
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
                case UserDefinedExpressionMethodMapping mapping:
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
