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
