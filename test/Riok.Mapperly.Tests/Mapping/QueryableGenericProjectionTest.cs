namespace Riok.Mapperly.Tests.Mapping;

public class QueryableGenericProjectionTest
{
    [Fact]
    public Task WithGenericSourceAndTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial IQueryable<TTarget> Project<TSource, TTarget>(IQueryable<TSource> source);

            private partial IQueryable<B> ProjectAToB(IQueryable<A> source);
            """,
            "record struct A(string Value);",
            "record struct B(string Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithQueryableSourceAndGenericTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial IQueryable<TTarget> ProjectTo<TTarget>(IQueryable source);
            private partial IQueryable<B> ProjectAToB(IQueryable<A> source);
            """,
            "record struct A(string Value);",
            "record struct B(string Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithGenericTypeConstraints()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial IQueryable<TTarget> ProjectTo<TTarget, TSource>(IQueryable<TSource> source)
                where TSource : A
                where TTarget : B

            private partial IQueryable<B> ProjectAToB(IQueryable<A> source);
            private partial IQueryable<D> ProjectCToD(IQueryable<C> source);
            """,
            "record A(string Value);",
            "record B(string Value);",
            "record struct C(string Value);",
            "record struct D(string Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }
}
