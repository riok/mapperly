using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An <see cref="IMembersContainerBuilderContext{T}"/> implementation.
/// </summary>
/// <typeparam name="T">The type of mapping.</typeparam>
public class MembersContainerBuilderContext<T>(MappingBuilderContext builderContext, T mapping)
    : MembersMappingBuilderContext<T>(builderContext, mapping),
        IMembersContainerBuilderContext<T>
    where T : IMemberAssignmentTypeMapping
{
    private readonly Dictionary<MemberPath, MemberNullDelegateAssignmentMapping> _nullDelegateMappings = new();

    public void AddMemberAssignmentMapping(IMemberAssignmentMapping memberMapping) => AddMemberAssignmentMapping(Mapping, memberMapping);

    public void AddNullDelegateMemberAssignmentMapping(IMemberAssignmentMapping memberMapping)
    {
        var nullConditionSourcePath = new MemberPath(memberMapping.SourcePath.PathWithoutTrailingNonNullable().ToList());
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddMemberAssignmentMapping(container, memberMapping);
    }

    private void AddMemberAssignmentMapping(IMemberAssignmentMappingContainer container, IMemberAssignmentMapping mapping)
    {
        SetSourceMemberMapped(mapping.SourcePath);
        AddNullMemberInitializers(container, mapping.TargetPath);
        container.AddMemberMapping(mapping);
    }

    private void AddNullMemberInitializers(IMemberAssignmentMappingContainer container, MemberPath path)
    {
        foreach (var nullableTrailPath in path.ObjectPathNullableSubPaths())
        {
            var nullablePath = new MemberPath(nullableTrailPath);
            var type = nullablePath.Member.Type;

            if (!nullablePath.Member.CanSet)
                continue;

            if (!BuilderContext.SymbolAccessor.HasAccessibleParameterlessConstructor(type))
            {
                BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, type);
                continue;
            }

            var setterNullablePath = SetterMemberPath.Build(BuilderContext, nullablePath);

            if (setterNullablePath.IsMethod)
            {
                var getterNullablePath = GetterMemberPath.Build(BuilderContext, nullablePath);
                container.AddMemberMappingContainer(
                    new MethodMemberNullAssignmentInitializerMapping(setterNullablePath, getterNullablePath)
                );
                continue;
            }

            container.AddMemberMappingContainer(new MemberNullAssignmentInitializerMapping(setterNullablePath));
        }
    }

    private MemberNullDelegateAssignmentMapping GetOrCreateNullDelegateMappingForPath(MemberPath nullConditionSourcePath)
    {
        // if there is already an exact match return that
        if (_nullDelegateMappings.TryGetValue(nullConditionSourcePath, out var mapping))
            return mapping;

        IMemberAssignmentMappingContainer parentMapping = Mapping;

        // try to reuse parent path mappings and wrap inside them
        // if the parentMapping is the first nullable path, no need to access the path in the condition in a null-safe way.
        var needsNullSafeAccess = false;
        foreach (var nullablePath in nullConditionSourcePath.ObjectPathNullableSubPaths().Reverse())
        {
            if (_nullDelegateMappings.TryGetValue(new MemberPath(nullablePath), out var parentMappingHolder))
            {
                parentMapping = parentMappingHolder;
                break;
            }

            needsNullSafeAccess = true;
        }

        mapping = new MemberNullDelegateAssignmentMapping(
            GetterMemberPath.Build(BuilderContext, nullConditionSourcePath),
            parentMapping,
            BuilderContext.MapperConfiguration.ThrowOnPropertyMappingNullMismatch,
            needsNullSafeAccess
        );
        _nullDelegateMappings[nullConditionSourcePath] = mapping;
        parentMapping.AddMemberMappingContainer(mapping);
        return mapping;
    }
}
