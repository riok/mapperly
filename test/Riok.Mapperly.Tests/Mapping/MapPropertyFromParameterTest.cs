using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class MapPropertyFromParameterTest
{
    [Fact]
    public void MapToTopLevelProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            public static partial ParentDomain ToDomain(this ParentDto parent, string parentId);
            """,
            "class ParentDto { public string Name { get; set; } }",
            "class ParentDomain { public string Id { get; init; } public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::ParentDomain()
                {
                    Id = parentId,
                };
                target.Name = parent.Name;
                return target;
                """
            );
    }

    [Fact]
    public Task MapToNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Child.Id), "childId")]
            public partial ParentDomain Map(ParentDto parent, string childId);
            """,
            "class ParentDto { public string Name { get; set; } }",
            "class ParentDomain { public string Name { get; set; } public ChildDomain Child { get; set; } }",
            "class ChildDomain { public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapTopLevelAndNestedProperty()
    {
        // The scenario from https://github.com/riok/mapperly/issues/1719
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            [MapPropertyFromParameter(nameof(@ParentDomain.Child.Id), "childId")]
            public partial ParentDomain Map(ParentDto parent, string parentId, string childId);
            """,
            "class ParentDto { public string Name { get; set; } }",
            "class ParentDomain { public string Id { get; set; } public string Name { get; set; } public ChildDomain Child { get; set; } }",
            "class ChildDomain { public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ToExistingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            public partial void Map(ParentDto parent, ParentDomain target, string parentId);
            """,
            "class ParentDto { }",
            "class ParentDomain { public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ParameterNotMappedByDefaultShouldNotProduceUnusedParameterDiagnostic()
    {
        // Mapping a parameter via MapPropertyFromParameter marks it as used
        // even though it is not matched by its name.
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            public partial ParentDomain Map(ParentDto parent, string parentId);
            """,
            "class ParentDto { }",
            "class ParentDomain { public string Id { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceParameterNotFoundShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            public partial ParentDomain Map(ParentDto parent, string id);
            """,
            "class ParentDto { }",
            "class ParentDomain { public string Id { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ParameterWithIncompatibleTargetTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "child")]
            public partial ParentDomain Map(ParentDto parent, ChildDomain child);
            """,
            "class ParentDto { }",
            "class ParentDomain { public int Id { get; set; } }",
            "class ChildDomain { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExplicitMappingShouldTakePrecedenceOverByNameParameterMatching()
    {
        // The parameter has the same name as the target member,
        // but the explicit mapping still wins.
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Name), "name")]
            public partial ParentDomain Map(ParentDto parent, string name);
            """,
            "class ParentDto { }",
            "class ParentDomain { public string Name { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DuplicateTargetViaMapPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Id), "parentId")]
            [MapProperty(nameof(ParentDto.Id), nameof(ParentDomain.Id))]
            public partial ParentDomain Map(ParentDto parent, string parentId);
            """,
            "class ParentDto { public string Id { get; set; } }",
            "class ParentDomain { public string Id { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Value), "value", StringFormat = "P")]
            public partial ParentDomain Map(ParentDto parent, double value);
            """,
            "class ParentDto { }",
            "class ParentDomain { public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithFullNameOfNestedTargetProperty()
    {
        // the full nameof syntax (@) is required to reference a nested target member path
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromParameter(nameof(@ParentDomain.Child.Id), "childId")]
            public partial ParentDomain Map(ParentDto parent, string childId);
            """,
            "class ParentDto { }",
            "class ParentDomain { public ChildDomain Child { get; set; } }",
            "class ChildDomain { public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
