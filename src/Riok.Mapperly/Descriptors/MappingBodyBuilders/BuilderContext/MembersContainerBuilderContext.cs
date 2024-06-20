using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
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

        var nullConditionSourcePath = new NonEmptyMemberPath(
            memberMapping.MemberInfo.SourceMember.RootType,
            memberMapping.MemberInfo.SourceMember.PathWithoutTrailingNonNullable().ToList()
        );
        var container = GetOrCreateNullDelegateMappingForPath(nullConditionSourcePath);
        AddMemberAssignmentMapping(container, memberMapping);

        // set target member to null if null assignments are allowed
        // and the source is null
        if (
            BuilderContext.Configuration.Mapper.AllowNullPropertyAssignment
            && memberMapping.MemberInfo.TargetMember.Member.Type.IsNullable()
        )
        {
            container.AddNullMemberAssignment(SetterMemberPath.Build(BuilderContext, memberMapping.MemberInfo.TargetMember));
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
    }

    private void AddNullMemberInitializers(IMemberAssignmentMappingContainer container, MemberPath path)
    {
        foreach (var nullableTrailPath in path.ObjectPathNullableSubPaths())
        {
            var nullablePath = new NonEmptyMemberPath(path.RootType, nullableTrailPath);
            var type = nullablePath.Member.Type;

            if (!nullablePath.Member.CanSet)
                continue;

            if (!BuilderContext.SymbolAccessor.HasDirectlyAccessibleParameterlessConstructor(type))
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
            if (
                _nullDelegateMappings.TryGetValue(
                    new NonEmptyMemberPath(nullConditionSourcePath.RootType, nullablePath),
                    out var parentMappingHolder
                )
            )
            {
                parentMapping = parentMappingHolder;
                break;
            }

            needsNullSafeAccess = true;
        }

        mapping = new MemberNullDelegateAssignmentMapping(
            GetterMemberPath.Build(BuilderContext, nullConditionSourcePath),
            parentMapping,
            needsNullSafeAccess
        );
        _nullDelegateMappings[nullConditionSourcePath] = mapping;
        parentMapping.AddMemberMappingContainer(mapping);
        return mapping;
    }
}
