namespace Riok.Mapperly.Tests.Mapping;

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
    public Task ClassToClassNullableSourceAndTargetPropertyWithNoNullAssignmentAndThrowShouldBeIgnored()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
                ThrowOnPropertyMappingNullMismatch = true,
            },
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
    public Task NestedPropertyWithDeepCloneable()
    {
        // deep cloneable should be ignored.
        // see https://github.com/riok/mapperly/issues/1710
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapNestedProperties("Nested")]
            public partial B MapConfig(A source);
            """,
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public C Nested { get; set; } }",
            "class B { public string[] Value0 { get; set; } public string Value { get; set; } }",
            "class C { public string[] Value0 { get; set; } public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
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

    [Fact]
    public Task ClassToRecordNoAccessibleSourceCtorShouldNotDiagnostic()
    {
        // see https://github.com/riok/mapperly/issues/972
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            """
            public class A
            {
                private A(C value)
                {
                    Value = value;
                }

                public C Value { get; }
            }
            """,
            "public record B(C Value);",
            "public record C(string StringValue);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RecordToClassDisabledNullableContext()
    {
        // see https://github.com/riok/mapperly/issues/1196
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "public record A(string Value);",
            "public record B(string Value);"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task RecordToRecordMemberMappingDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);

            [MapProperty(nameof(A.Value), nameof(B.OtherValue)]
            private partial B Map(A source);
            """,
            "public record A(string Value);",
            "public record B(string OtherValue);"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ClassToClassMemberMappingDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);

            [MapProperty(nameof(A.Value), nameof(B.OtherValue)]
            private partial B Map(A source);
            """,
            "public class A { public string Value {get; set;} }",
            "public class B { public string OtherValue {get; set;} }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }
}
