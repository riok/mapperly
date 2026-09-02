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
                """
                var target = new string[source.Count];
                var i = target.Length;
                foreach (var item in source)
                {
                    target[--i] = item;
                }
                return new global::System.Collections.Generic.Stack<string>(target);
                """
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
    public void StackToStackWithConversionReverseOrder()
    {
        var source = TestSourceBuilder.Mapping(
            "Stack<int>",
            "Stack<string>",
            TestSourceBuilderOptions.WithDeepCloning with
            {
                StackCloningStrategy = StackCloningStrategy.ReverseOrder,
            }
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<string>(source.Count);
                foreach (var item in source)
                {
                    target.Push(item.ToString());
                }
                return target;
                """
            );
    }

    [Fact]
    public void StackToStackWithConversion()
    {
        var source = TestSourceBuilder.Mapping("Stack<int>", "Stack<string>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new string[source.Count];
                var i = target.Length;
                foreach (var item in source)
                {
                    target[--i] = item.ToString();
                }
                return new global::System.Collections.Generic.Stack<string>(target);
                """
            );
    }

    [Fact]
    public void ArrayToStack()
    {
        var source = TestSourceBuilder.Mapping("int[]", "Stack<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<int>(source.Length);
                for (var i = source.Length - 1; i >= 0; i--)
                {
                    target.Push(source[i]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ListToStack()
    {
        var source = TestSourceBuilder.Mapping("List<int>", "Stack<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<int>(source.Count);
                for (var i = source.Count - 1; i >= 0; i--)
                {
                    target.Push(source[i]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ListToStackWithConversion()
    {
        var source = TestSourceBuilder.Mapping("List<int>", "Stack<string>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new string[source.Count];
                var i = target.Length;
                foreach (var item in source)
                {
                    target[--i] = item.ToString();
                }
                return new global::System.Collections.Generic.Stack<string>(target);
                """
            );
    }

    [Fact]
    public void ArrayToStackWithConversionLegacy()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "Stack<string>",
            TestSourceBuilderOptions.Default with
            {
                StackCloningStrategy = StackCloningStrategy.ReverseOrder,
            }
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<string>(source.Length);
                for (var i = 0; i < source.Length; i++)
                {
                    target.Push(source[i].ToString());
                }
                return target;
                """
            );
    }

    [Fact]
    public void ListToStackWithConversionLegacy()
    {
        var source = TestSourceBuilder.Mapping(
            "List<int>",
            "Stack<string>",
            TestSourceBuilderOptions.Default with
            {
                StackCloningStrategy = StackCloningStrategy.ReverseOrder,
            }
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<string>(source.Count);
                for (var i = 0; i < source.Count; i++)
                {
                    target.Push(source[i].ToString());
                }
                return target;
                """
            );
    }
}
