using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

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
                EnumMappingStrategy = EnumMappingStrategy.ByName,
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
                DiagnosticDescriptors.EnumMappingNotSupportedInProjectionMappings,
                "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings to map from C to D, consider applying [MapperNoExpressionInlining] to the mapping method or Mapper(NoExpressionInlining = true) to the containing mapper"
            )
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotMapMember, "Could not map member A.Value of type C to B.Value of type D")
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member Value on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member Value on the mapping target type B was not found on the mapping source type A"
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
                EnumMappingStrategy = EnumMappingStrategy.ByValue,
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
                DiagnosticDescriptors.EnumMappingNotSupportedInProjectionMappings,
                "The enum mapping strategy ByName, ByValueCheckDefined, explicit enum mappings and ignored enum values cannot be used in projection mappings to map from C to D, consider applying [MapperNoExpressionInlining] to the mapping method or Mapper(NoExpressionInlining = true) to the containing mapper"
            )
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
