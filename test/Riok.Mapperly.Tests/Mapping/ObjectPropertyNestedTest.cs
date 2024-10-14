using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyNestedTest
{
    [Fact]
    public void ManualNestedToNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(new [] {"Value", "IntValue"}, new [] {"Value", "StringValue"})]
            public partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public int IntValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value.StringValue = source.Value.IntValue.ToString();
                return target;
                """
            );
    }

    [Fact]
    public void ManualNullableNestedToNullableNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(new [] {"Value", "IntValue"}, new [] {"Value", "StringValue"})]
            public partial B Map(A source);
            """,
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public int IntValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value ??= new global::D();
                    target.Value.StringValue = source.Value.IntValue.ToString();
                }
                return target;
                """
            );
    }

    [Fact]
    public void ManualNestedToInitOnlyNestedPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(new [] {"Value", "IntValue"}, new [] {"Value", "StringValue"})]
            public partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public int IntValue { get; init; } }",
            "class D { public string StringValue { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member Value on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member Value on the mapping target type B was not found on the mapping source type A"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.CannotMapToInitOnlyMemberPath,
                "Cannot map from A.Value.IntValue to init only member path B.Value.StringValue"
            )
            .HaveAssertedAllDiagnostics();
    }
}
