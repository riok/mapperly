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

    [Fact]
    public Task IgnoreShouldNotRequireParameterlessCtorForNullableNavigation()
    {
        // https://github.com/riok/mapperly/issues/2350
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public C? Nested { get; set; } }",
            "class B { public D Nested { get; set; } }",
            "class C { public int Id { get; set; } }",
            "class D { public D(int id) { Id = id; } public int Id { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void IgnoreShouldNotReportNullableSourceDiagnostic()
    {
        // https://github.com/riok/mapperly/issues/1293
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public string? Name { get; set; } }",
            "class B { public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            source,
                            x => new global::B()
                            {
                                Name = x.Name,
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public Task IgnoreDisabledNullableContextShouldMatchEnabled()
    {
        // https://github.com/riok/mapperly/issues/1293 — output must not depend on the nullable context
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public string? Name { get; set; } }",
            "class B { public string Name { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public void IgnoreShouldUnwrapNullableValueTypeMember()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public int? IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            source,
                            x => new global::B()
                            {
                                IntValue = x.IntValue.Value,
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public Task IgnoreShouldProjectDeeplyNestedNullablePathDirectly()
    {
        // relies on Mapperly's automatic flattening (Owner.Address.City => OwnerAddressCity),
        // as MapProperty configuration is not supported directly on queryable projection mappings (RMG065).
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public C? Owner { get; set; } }",
            "class B { public string OwnerAddressCity { get; set; } }",
            "class C { public Address? Address { get; set; } }",
            "class Address { public string City { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IgnoreShouldProjectNullableCollectionMember()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                QueryableProjectionNullHandling = QueryableProjectionNullHandling.Ignore,
            },
            "class A { public System.Collections.Generic.List<C>? Items { get; set; } }",
            "class B { public System.Collections.Generic.List<D> Items { get; set; } }",
            "class C { public int Id { get; set; } }",
            "class D { public int Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
