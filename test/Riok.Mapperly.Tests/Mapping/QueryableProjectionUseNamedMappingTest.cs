namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class QueryableProjectionUseNamedMappingTest
{
    [Fact]
    public Task ShouldUseReferencedMappingOnSelectedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue2), Use = nameof(ModifyString2)]
            private partial B Map(A source);

            [UserMapping(Default = true)]
            private string DefaultStringMapping(string source) => source;
            private string ModifyString(string source) => source + "-modified";
            private string ModifyString2(string source) => source + "-modified2";
            """,
            """
            class A
            {
                public string StringValue { get; set; }
                public string StringValue1 { get; set; }
                public string StringValue2 { get; set; }
            }
            """,
            """
            class B
            {
                public string StringValue { get; set; }
                public string StringValue1 { get; set; }
                public string StringValue2 { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }
}
