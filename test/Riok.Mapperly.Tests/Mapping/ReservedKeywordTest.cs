using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class ReservedKeywordTest
{
    [Fact]
    public void ReservedKeywordInCtorArgumentShouldBeEscaped()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Source),
            """
            class A
            {
                public bool Private { get; set; }
            }
            """,
            """
            class B
            {
                public B(string? remark = null, bool @private = false)
                {
                    Remark = remark;
                    Private = @private;
                }

                public bool Private { get; set; }
                public string? Remark { get; set; }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(@private: source.Private);
                return target;
                """
            );
    }

    [Fact]
    public void ReservedKeywordInSourceParameterShouldBeEscaped()
    {
        var source = TestSourceBuilder.MapperWithBody("public partial string Map(int @object);");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return @object.ToString();");
    }

    [Fact]
    public void ReservedKeywordInSourceParameterOfObjectMappingShouldBeEscaped()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A @event);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = @event.Value;
                return target;
                """
            );
    }

    [Fact]
    public void ReservedKeywordInAdditionalParameterShouldBeEscaped()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int @event);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int Event { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    Event = @event,
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }
}
