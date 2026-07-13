using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionNullHandlingTest
{
    [Fact]
    public void IgnoreShouldSkipNullHandlingForNullableMember()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public int Id { get; set; } }",
            "class D { public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            source,
                            x => new global::B()
                            {
                                Value = new global::D()
                                {
                                    Id = x.Value.Id,
                                },
                            }
                        );
                #nullable enable
                """
            );
    }
}
