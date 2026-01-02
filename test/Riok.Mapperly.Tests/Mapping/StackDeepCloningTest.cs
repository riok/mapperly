using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class StackDeepCloningTest
{
    [Fact]
    public void StackToStackDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Stack<string>", "Stack<string>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return new global::System.Collections.Generic.Stack<string>(global::System.Linq.Enumerable.Reverse(source));"
            );
    }

    [Fact]
    public void StackToStackDeepCloningLegacy()
    {
        var source = TestSourceBuilder.Mapping(
            "Stack<string>",
            "Stack<string>",
            TestSourceBuilderOptions.WithDeepCloning with
            {
                StackCloningStrategy = StackCloningStrategy.ReverseOrder,
            }
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::System.Collections.Generic.Stack<string>(source);");
    }

    [Fact]
    public void StackToStackWithConversion()
    {
        var source = TestSourceBuilder.Mapping("Stack<int>", "Stack<string>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                @"return new global::System.Collections.Generic.Stack<string>(
    global::System.Linq.Enumerable.Reverse(global::System.Linq.Enumerable.Select(source, x => x.ToString()))
);"
            );
    }
}
