using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class QueryableProjectionEnumTest
{
    [Fact]
    public Task EnumToAnotherEnum()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "enum C { Value1, Value2 }",
            "enum D { Value1, Value2 }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void EnumToAnotherEnumByNameShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                EnumMappingStrategy = EnumMappingStrategy.ByName
            },
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "enum C { Value1, Value2 }",
            "enum D { Value1, Value2 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumMappingStrategyByNameNotSupportedInProjectionMappings,
                "The enum mapping strategy ByName, ByValueCheckDefined and explicit enum mappings cannot be used in projection mappings to map from C to D"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToAnotherEnumByValueWithExplicitMappingShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);

            [MapEnumValue(C.Value2, D.Value2)]
            partial D MapEnum(C src);
            """,
            TestSourceBuilderOptions.Default with
            {
                EnumMappingStrategy = EnumMappingStrategy.ByValue
            },
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "enum C { Value1 = 10, Value2 = 100 }",
            "enum D { Value1 = 10, Value2 = 200 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumMappingStrategyByNameNotSupportedInProjectionMappings,
                "The enum mapping strategy ByName, ByValueCheckDefined and explicit enum mappings cannot be used in projection mappings to map from C to D"
            )
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member Value2 (200) on D not found on source enum C")
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member Value2 (100) on C not found on target enum D")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task EnumToString()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "enum C { Value1, Value2 }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task EnumFromString()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string Value { get; set; } }",
            "class B { public C Value { get; set; } }",
            "enum C { Value1, Value2 }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
