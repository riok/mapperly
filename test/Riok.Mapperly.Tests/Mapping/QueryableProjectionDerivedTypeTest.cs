namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class QueryableProjectionDerivedTypeTest
{
    [Fact]
    public Task AbstractBaseClassDerivedTypesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            partial B Map(A src);
            """,
            "abstract class A { public string BaseValue { get; set; } }",
            "abstract class B { public string BaseValue { get; set; } }",
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
