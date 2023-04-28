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
                new(
                    DiagnosticDescriptors.EnumMappingStrategyByNameNotSupportedInProjectionMappings,
                    "The enum mapping strategy ByName cannot be used in projection mappings to map from C to D"
                )
            );
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
