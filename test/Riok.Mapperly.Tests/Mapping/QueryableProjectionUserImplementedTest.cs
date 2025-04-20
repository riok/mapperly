namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionUserImplementedTest
{
    [Fact]
    public Task ClassToClassInlinedExpression()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v) => new D { Value = v.Value + "-mapped" };
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassInlinedSingleStatement()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                return new D { Value = v.Value + "-mapped" };
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassInlinedSingleDeclaration()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                var dest = new D { Value = v.Value + "-mapped" };
                return dest;
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNonInlinedMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                var dest = new D();
                dest.Value = v.Value + "-mapped";
                return dest;
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrdering()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private List<C> Order(List<C> v)
                => v.OrderBy(x => x.Value).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedParenthesizedLambdaOrdering()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private List<C> Order(List<C> v)
                => v.OrderBy((x) => x.Value).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrderingWithMappingAndParameterHiding()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial D MapToD(C source);

            private List<D> Order(List<C> v)
                => v.OrderBy(x => x.Value).Select(v => MapToD(v)).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<D> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrderingWithTwoNestedMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial D MapToD(C source);
            private string MapString(string s) => s + "-mod";

            private List<D> Order(List<C> v)
                => v.OrderBy(x => x.Value).Select(x => MapToD(x)).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<D> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithUsingMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static DateTimeOffset MapToDateTimeOffset(DateTime dateTime)
                => new DateTimeOffset(dateTime, TimeSpan.Zero);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public DateTimeOffset Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithTargetTypeNew()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static DateTimeOffset MapToDateTimeOffset(DateTime dateTime)
                => new(dateTime, TimeSpan.Zero);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public DateTimeOffset Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithTargetTypeNewInitializer()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static C MapIt(DateTime dateTime)
                => new(1) { Value2 = 10 };
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C(int value1) { public int Value2 { set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedFlatteningWithCallToAnotherMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<D> Map(IQueryable<A> source);

            private static E Map(B source) => Map(source.Child);
            private static partial E Map(C source);
            """,
            "public record A(B Value);",
            "public record B(C Child);",
            "public record C(string Name);",
            "public record D(E Value);",
            "public record E(string Name);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedNullableValueTypeToNonNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<B> Map(this IQueryable<A> query);

            private static int MapValue(int? value) => value ?? 0;
            """,
            "public record A(int? Value);",
            "public record B(int Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
