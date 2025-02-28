using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

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
    public Task ShouldUseReferencedMappingOnSelectedPropertiesWithRecordConstructors()
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
            "record A(string StringValue, string StringValue1, string StringValue2);",
            "record B(string StringValue, string StringValue1, string StringValue2);"
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch)
            .HaveAssertedAllDiagnostics()
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
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch,
                "The source type byte of the referenced mapping MyMapping does not match the expected type object"
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch)
            .HaveAssertedAllDiagnostics()
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
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch,
                "The target type object of the referenced mapping MyMapping does not match the expected type byte"
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch)
            .HaveDiagnostic(DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch)
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(int.Parse(MyMapping(source.V.ToString())));
                return target;
                """
            );
    }

    [Fact]
    public Task ShouldUseReferencedUserDefinedMappingOnSelectedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
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
    public Task UserMethodReturnsNullableShouldThrow()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Name", "Value", Use = nameof(ToC))]
            public partial B Map(A source);

            [UserMapping(Default = false)]
            public C? ToC(string name) => new C(name);
            """,
            "class A { public string? Name { get; set; } }",
            "class B { public C Value { get; set; } }",
            "record C(string Name);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldPassNullValueToNullableUserMappingMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(MapString)]
            public partial B Map(A source);

            [UserMapping(Default = false)]
            private string MapString(string? source)
                => source ?? "(null)";
            """,
            """
            class A
            {
                public string? Value { get; }
                public string? Value2 { get; }
            }
            """,
            """
            class B
            {
                public string Value { set; }
                public string Value2 { set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserDefinedExistingTargetMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = "MapValue")]
            public partial void Map(A source, B target);
            private partial void MapValue(C source, D target);
            """,
            """
            public class A
            {
                public C Value { get; set; }
            }
            """,
            """
            public class B
            {
                public D Value { get; set; }
            }
            """,
            """
            public class C
            {
                public string StringValue { get; set; }
            }
            """,
            """
            public class D
            {
                public string StringValue { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedExistingTargetMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Values), nameof(B.Values), Use = "MapValues")]
            public partial void Map(A source, B target);
            private void MapValues(List<string> source, List<string> target) { }
            """,
            """
            public class A
            {
                public List<string> Values { get; set; }
            }
            """,
            """
            public class B
            {
                public List<string> Values { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedExistingTargetMappingWithDifferentSourceType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = "MapValues")]
            public partial void Map(A source, B target);
            private void MapValues(F source, D target) {}
            """,
            """
            public class A
            {
                public C Value { get; set; }
            }
            """,
            """
            public class C
            {
                public List<string> Values { get; set; }
            }
            """,
            """
            public class B
            {
                public D Value { get; set; }
            }
            public class F
            {
                public List<string> Values { get; set; }
            }
            """,
            """
            public class D
            {
                public List<string> Values { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedExistingTargetMappingWithDifferentTargetType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = "MapValues")]
            public partial void Map(A source, B target);
            private void MapValues(C source, F target) {}
            """,
            """
            public class A
            {
                public C Value { get; set; }
            }
            """,
            """
            public class C
            {
                public List<string> Values { get; set; }
            }
            """,
            """
            public class B
            {
                public D Value { get; set; }
            }
            public class F
            {
                public List<string> Values { get; set; }
            }
            """,
            """
            public class D
            {
                public List<string> Values { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldUseReferencedMappingOnSelectedPropertiesWithExistingInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Values1), nameof(B.Values1), Use = nameof(MapV1)]
            [MapProperty(nameof(A.Values2), nameof(B.Values2), Use = nameof(MapV2)]
            public partial B Map(A source);

            [UserMapping(Default = true)]
            private void DefaultMapping(List<string> source, List<string> target) {}
            private void MapV1(List<string> source, List<string> target) {}
            private void MapV2(List<string> source, List<string> target) {}
            """,
            """
            class A
            {
                public List<string> Values { get; } = [];
                public List<string> Values1 { get; } = [];
                public List<string> Values2 { get; } = [];
            }
            """,
            """
            class B
            {
                public List<string> Values { get; } = [];
                public List<string> Values1 { get; } = [];
                public List<string> Values2 { get; } = [];
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }
}
