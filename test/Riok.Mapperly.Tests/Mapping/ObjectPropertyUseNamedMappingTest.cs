using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyUseNamedMappingTest
{
    [Fact]
    public Task ShouldUseReferencedMappingOnSelectedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue2), Use = nameof(ModifyString2)]
            public partial B Map(A source);

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
    public Task MultipleNonDefaultUserImplementedButReferencedMappingMethodsShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue2), Use = nameof(ModifyString2)]
            public partial B Map(A source);

            [UserMapping(Default = false)]
            private string ModifyString(string source) => source + "-modified";

            [UserMapping(Default = false)]
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
    public Task ShouldUseReferencedMappingWithDisabledAutoUserMappingsOnSelectedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue2), Use = nameof(ModifyString2)]
            public partial B Map(A source);

            [UserMapping(Default = false)]
            private string ModifyString(string source) => source + "-modified";

            [UserMapping(Default = false)]
            private string ModifyString2(string source) => source + "-modified2";
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
    public void DuplicatedNameShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping)]
            public partial B Map(A source);
            private string MyMapping(string value) => value + "-mod";
            private string MyMapping(int value) => value.ToString();
            """,
            "record A(string V);",
            "record B(string V);"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferencedMappingAmbiguous,
                "The referenced mapping name MyMapping is ambiguous, use a unique name"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(MyMapping(source.V));
                return target;
                """
            );
    }

    [Fact]
    public void UnknownNameShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = "Unknown")]
            public partial B Map(A source);
            private string MyMapping(string value) => value + "-mod";
            """,
            "record A(string V);",
            "record B(string V);"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, "The referenced mapping named Unknown was not found")
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(source.V);
                return target;
                """
            );
    }

    [Fact]
    public void WithMappedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping))]
            public partial B Map(A source);
            private string MyMapping(string value) => value + "-mod";
            """,
            "record A(int V);",
            "record B(string V);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MyMapping(source.V.ToString()));
                return target;
                """
            );
    }

    [Fact]
    public void WithUnmappableSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping))]
            public partial B Map(A source);
            private string MyMapping(byte value) => value + "-mod";
            """,
            "record A(object V);",
            "record B(string V);"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                "Could not create mapping from object to byte. Consider implementing the mapping manually."
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(source.V.ToString());
                return target;
                """
            );
    }

    [Fact]
    public void WithMappedTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping))]
            public partial B Map(A source);
            private string MyMapping(string value) => "100" + value;
            """,
            "record A(string V);",
            "record B(int V);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(int.Parse(MyMapping(source.V)));
                return target;
                """
            );
    }

    [Fact]
    public void WithUnmappableTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping))]
            public partial B Map(A source);
            private object MyMapping(string value) => value + "-mod";
            """,
            "record A(string V);",
            "record B(byte V);"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                "Could not create mapping from object to byte. Consider implementing the mapping manually."
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(byte.Parse(source.V));
                return target;
                """
            );
    }

    [Fact]
    public void WithMappedSourceAndTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.V), nameof(B.V), Use = nameof(MyMapping))]
            public partial B Map(A source);
            private string MyMapping(string value) => "100" + value;
            """,
            "record A(int V);",
            "record B(int V);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(int.Parse(MyMapping(source.V.ToString())));
                return target;
                """
            );
    }
}
