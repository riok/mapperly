using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

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
    private readonly HashSet<MemberPath> _initializedNullableTargetPaths = new();

    public void AddMemberAssignmentMapping(IMemberAssignmentMapping memberMapping) => AddMemberAssignmentMapping(Mapping, memberMapping);

    /// <summary>
    /// Adds an if-else style block which only executes the <paramref name="memberMapping"/>
    /// if the source member is not null.
    /// </summary>
    /// <param name="memberMapping">The member mapping to be applied if the source member is not null</param>
    public void AddNullDelegateMemberAssignmentMapping(IMemberAssignmentMapping memberMapping)
    {
        if (memberMapping.MemberInfo.SourceMember == null)
        {
            AddMemberAssignmentMapping(memberMapping);
            return;
        }

        // set target member to null if null assignments are allowed
        // and the source is null
        var setMemberToNull =
            BuilderContext.Configuration.Mapper.AllowNullPropertyAssignment
            && memberMapping.MemberInfo.TargetMember.Member.Type.IsNullable();

        // if the member is explicitly set to null,
        // make sure the parent members are initialized/non-null,
        // no matter of the null conditional source-path.
        // otherwise this initialization would only happen
        // if the source member is not null,
        // and the null assignment in the else case could fail if the target parent member is not initialized.
        if (setMemberToNull)
        {
            AddNullMemberInitializers(Mapping, memberMapping.MemberInfo.TargetMember);
        }

        var nullConditionSourcePath = new NonEmptyMemberPath(
            memberMapping.MemberInfo.SourceMember.MemberPath.RootType,
            memberMapping.MemberInfo.SourceMember.MemberPath.PathWithoutTrailingNonNullable().ToList()
        );
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddMemberAssignmentMapping(container, memberMapping);

        // set target member to null if null assignments are allowed
        // and the source is null
        if (setMemberToNull)
        {
            var targetMemberSetter = memberMapping.MemberInfo.TargetMember.BuildSetter(BuilderContext);
            container.AddNullMemberAssignment(targetMemberSetter);
        }
        else if (BuilderContext.Configuration.Mapper.ThrowOnPropertyMappingNullMismatch)
        {
            container.ThrowOnSourcePathNull();
        }
    }

    private void AddMemberAssignmentMapping(IMemberAssignmentMappingContainer container, IMemberAssignmentMapping mapping)
    {
        AddNullMemberInitializers(container, mapping.MemberInfo.TargetMember);
        container.AddMemberMapping(mapping);
        MappingAdded(mapping.MemberInfo);

        // if the source value is a non-nullable value,
        // the target should be non-null after this assignment and can be set as initialized.
        if (!mapping.MemberInfo.IsSourceNullable && mapping.MemberInfo.TargetMember.MemberType.IsNullable())
        {
            _initializedNullableTargetPaths.Add(mapping.MemberInfo.TargetMember);
        }
    }

    private void AddNullMemberInitializers(IMemberAssignmentMappingContainer container, MemberPath path)
    {
        foreach (var nullablePathList in path.ObjectPathNullableSubPaths())
        {
            var nullablePath = new NonEmptyMemberPath(path.RootType, nullablePathList);
            var type = nullablePath.Member.Type.NonNullable();

            if (!nullablePath.Member.CanSet)
                continue;

            if (!_initializedNullableTargetPaths.Add(nullablePath))
                continue;

            if (!BuilderContext.InstanceConstructors.TryBuild(BuilderContext.Source, type, out var ctor))
            {
                BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, type);
                continue;
            }

            var nullablePathSetter = nullablePath.BuildSetter(BuilderContext);
            if (!nullablePathSetter.SupportsCoalesceAssignment)
            {
                var nullablePathGetter = nullablePath.BuildGetter(BuilderContext);
                container.AddMemberMappingContainer(
                    new MethodMemberNullAssignmentInitializerMapping(nullablePathSetter, nullablePathGetter, ctor)
                );
                continue;
            }

            container.AddMemberMappingContainer(new MemberNullAssignmentInitializerMapping(nullablePathSetter, ctor));
        }
    }

    private MemberNullDelegateAssignmentMapping GetOrCreateNullDelegateMappingForPath(MemberPath nullConditionSourcePath)
    {
        // if there is already an exact match return that
        if (_nullDelegateMappings.TryGetValue(nullConditionSourcePath, out var mapping))
            return mapping;

        var parentMapping = FindParentNonNullContainer(nullConditionSourcePath, out var needsNullSafeAccess);
        var nullConditionSourcePathGetter = nullConditionSourcePath.BuildGetter(BuilderContext);
        mapping = new MemberNullDelegateAssignmentMapping(nullConditionSourcePathGetter, parentMapping, needsNullSafeAccess);
        _nullDelegateMappings[nullConditionSourcePath] = mapping;
        parentMapping.AddMemberMappingContainer(mapping);
        return mapping;
    }

    private IMemberAssignmentMappingContainer FindParentNonNullContainer(MemberPath nullConditionSourcePath, out bool needsNullSafeAccess)
    {
        // try to reuse parent path mappings and wrap inside them
        // if the parentMapping is the first nullable path, no need to access the path in the condition in a null-safe way.
        needsNullSafeAccess = false;
        foreach (var nullablePathList in nullConditionSourcePath.ObjectPathNullableSubPaths().Reverse())
        {
            var nullablePath = new NonEmptyMemberPath(nullConditionSourcePath.RootType, nullablePathList);
            if (_nullDelegateMappings.TryGetValue(nullablePath, out var parentMappingContainer))
                return parentMappingContainer;

            needsNullSafeAccess = true;
        }

        return Mapping;
    }
}
