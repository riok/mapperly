namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionUseNamedMappingTest
{
    [Fact]
    public Task ShouldUseReferencedMappingOnSelectedPropertiesWithDisabledAutoUserMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue2), Use = nameof(ModifyString2)]
            private partial B Map(A source);

            private string ModifyString(string source) => source + "-modified";
            private string ModifyString2(string source) => source + "-modified2";

            [UserMapping]
            private string DefaultStringMapping(string source) => source;
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
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

    [Fact]
    public Task ShouldUseReferencedUserDefinedMappingOnSelectedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(MapToDWithP)]
            [MapProperty(nameof(A.Value2), nameof(B.Value2), Use = nameof(MapToDWithC)]
            public partial B Map(A source);

            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "P")]
            private partial D MapToDWithP(C source);

            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "C")]
            private partial D MapToDWithC(C source);

            [UserMapping(Default = true)]
            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "N")]
            private partial D MapToDDefault(C source);
            """,
            """
            class A
            {
                public C Value { get; set; }
                public C Value2 { get; set; }
                public C ValueDefault { get; set; }
            }
            """,
            """
            class B
            {
                public D Value { get; set; }
                public D Value2 { get; set; }
                public D ValueDefault { get; set; }
            }
            """,
            "record C(int Value);",
            "record D(string Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldUseReferencedUserDefinedMappingOnSelectedPropertiesWithDisabledAutoUserMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(MapToDWithP)]
            [MapProperty(nameof(A.Value2), nameof(B.Value2), Use = nameof(MapToDWithC)]
            public partial B Map(A source);

            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "P")]
            private partial D MapToDWithP(C source);

            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "C")]
            private partial D MapToDWithC(C source);

            [UserMapping(Default = true)]
            [MapProperty(nameof(C.Value), nameof(D.Value), StringFormat = "N")]
            private partial D MapToDDefault(C source);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            """
            class A
            {
                public C Value { get; set; }
                public C Value2 { get; set; }
                public C ValueDefault { get; set; }
            }
            """,
            """
            class B
            {
                public D Value { get; set; }
                public D Value2 { get; set; }
                public D ValueDefault { get; set; }
            }
            """,
            "record C(int Value);",
            "record D(string Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }
}
