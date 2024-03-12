namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionDerivedTypeTest
{
    [Fact]
    public Task AbstractBaseClassDerivedTypesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            [MapProperty(nameof(A.BaseValueA), nameof(B.BaseValueB)]
            private partial B Map(A src);
            """,
            "abstract class A { public string BaseValueA { get; set; } }",
            "abstract class B { public string BaseValueB { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value2 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value2 { get; set; } }",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
