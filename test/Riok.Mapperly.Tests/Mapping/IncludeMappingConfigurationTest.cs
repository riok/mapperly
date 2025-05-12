namespace Riok.Mapperly.Tests.Mapping;

public class IncludeMappingConfigurationTest
{
    [Fact]
    public Task UsesMapPropertyFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapperIgnoreSourceFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreSource(nameof(A.Ignored))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string Property { get; set; } public string Ignored { get; set; } }",
            "class B { public string Property { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapperIgnoreTargetFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreTarget(nameof(A.Ignored))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string Property { get; set; } }",
            "class B { public string Property { get; set; } public string Ignored { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AppliesRecursively()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B BaseMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(BaseMappingMethod))]
            partial B FirstMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(FirstMappingMethod))]
            partial B SecondMappingMethod(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludesConfigurationFromAnotherClass()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            class A { public string SourceName { get; set; } }
            class B { public string DestinationName { get; set; } }
            [Mapper]
            public static partial class OtherMapper
            {
                [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
                public static partial B Map(A a);
            }

            [Mapper]
            [UseStaticMapper(typeof(OtherMapper))]
            public static partial class TestMapper
            {
                [IncludeMappingConfiguration(nameof(OtherMapper.Map))]
                public static partial B MapAnother(A a);
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsCircularReferences()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            [IncludeMappingConfiguration(nameof(MapAnother))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsDuplicatedName()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B ToDestination(A a);

            [MapProperty(nameof(B.DestinationName), nameof(A.SourceName))]
            partial A ToDestination(B b);

            [IncludeMappingConfiguration(nameof(ToDestination))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
