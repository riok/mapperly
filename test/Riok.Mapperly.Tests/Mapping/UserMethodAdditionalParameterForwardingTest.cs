using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UserMethodAdditionalParameterForwardingTest
{
    [Fact]
    public void MapValueUseMethodWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial B Map(A src, int ctx);
            private int GetValue(int ctx) => ctx * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetValue(ctx);
                return target;
                """
            );
    }

    [Fact]
    public void MapValueUseMethodWithMultipleAdditionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(Combine))]
            partial B Map(A src, int first, int second);
            private int Combine(int first, int second) => first + second;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = Combine(first, second);
                return target;
                """
            );
    }

    [Fact]
    public void MapValueUseMethodWithZeroParamsStillWorks()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetDefault))]
            partial B Map(A src);
            private int GetDefault() => 42;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetDefault();
                return target;
                """
            );
    }

    [Fact]
    public void NestedMappingWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int ctx);
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapNested(src.Nested, ctx);
                return target;
                """
            );
    }

    [Fact]
    public void NestedMappingFallsBackToParameterlessWhenNoMatchingUserMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapToBNested(src.Nested);
                return target;
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void ParameterUsedByBothPropertyAndNestedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("CtxValue", Use = nameof(GetCtx))]
            partial B Map(A src, int ctx);
            private int GetCtx(int ctx) => ctx;
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } public int CtxValue { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapNested(src.Nested, ctx);
                target.CtxValue = GetCtx(ctx);
                return target;
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapValueUseMethodWithUnsatisfiableParametersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial B Map(A src);
            private int GetValue(int ctx) => ctx * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueMethodParametersUnsatisfied,
                "The method GetValue referenced by MapValue has parameters that cannot be matched from the mapping's additional parameters"
            )
            .HaveAssertedAllDiagnostics();
    }
}
