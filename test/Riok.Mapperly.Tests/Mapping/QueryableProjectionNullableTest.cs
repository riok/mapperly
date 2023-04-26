namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class QueryableProjectionNullableTest
{
    [Fact]
    public Task ClassToClassNullableSourceProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string? StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourceValueTypeProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public int? IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableTargetProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; set; } }",
            "class B { public string? StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableTargetValueTypeProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public int IntValue { get; set; } }",
            "class B { public int? IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourceAndTargetProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string? StringValue { get; set; } }",
            "class B { public string? StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourceAndTargetValueTypeProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public int? IntValue { get; set; } }",
            "class B { public int? IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourcePathAutoFlatten()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public C? Nested { get; set; } }",
            "class B { public int NestedValue { get; set; } }",
            "class C { public int Value { get; set; }}"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourcePathAutoFlattenString()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public C? Nested { get; set; } }",
            "class B { public string NestedValue { get; set; } }",
            "class C { public string Value { get; set; }}"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNullableSourcePathManuallyFlatten()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);
                [MapProperty("Nested.Nested2.Value3", "NestedValue4")] private partial B Map(A source);
                """,
            "class A { public C? Nested { get; set; } }",
            "class B { public int NestedValue4 { get; set; } }",
            "class C { public D? Nested2 { get; set; } }",
            "class D { public int Value3 { get; set; }}"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
