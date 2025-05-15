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
    public Task IncludesConfigurationFromBaseClassMapping()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public static partial class Mapper
            {
                [MapProperty(nameof(A.SourceName1), nameof(B.DestinationName1))]
                public static partial B Map(A a);

                [IncludeMappingConfiguration(nameof(Map))]
                [MapProperty(nameof(A.SourceName2), nameof(B.DestinationName2))]
                public static partial BDerived MapDerived(ADerived a);
            }

            class A { public string SourceName1 { get; set; } }
            class ADerived : A { public string SourceName2 { get; set; } }
            class B { public string DestinationName1 { get; set; } }
            class BDerived : B { public string DestinationName2 { get; set; } }
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
            partial B OtherMapper(A a);

            [MapProperty(nameof(B.DestinationName), nameof(A.SourceName))]
            partial A OtherMapper(B b);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            partial B Mapper(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsSourceTypeIsInvalidInInclude()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(C.SourceName), nameof(B.DestinationName))]
            partial B UnrelatedMapper(C a);

            [IncludeMappingConfiguration(nameof(UnrelatedMapper))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string SourceName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsTargetTypeIsInvalidInInclude()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(C.DestinationName))]
            partial C UnrelatedMapper(A a);

            [IncludeMappingConfiguration(nameof(UnrelatedMapper))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
