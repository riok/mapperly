using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class DictionaryCustomTest
{
    [Fact]
    public void CustomDictionaryToIDictionary()
    {
        var source = TestSourceBuilder.Mapping("A", "IDictionary<string, int>", "class A : Dictionary<string, int> {}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::System.Collections.Generic.Dictionary<string, int>(source);");
    }

    [Fact]
    public void CustomKeyValueListToIDictionary()
    {
        var source = TestSourceBuilder.Mapping("A", "IDictionary<string, int>", "class A : List<KeyValuePair<string, int>> {}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::System.Collections.Generic.Dictionary<string, int>(source);");
    }

    [Fact]
    public void DictionaryToCustomDictionary()
    {
        var source = TestSourceBuilder.Mapping("IDictionary<string, int>", "A", "class A : Dictionary<string, int> {}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.EnsureCapacity(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomDictionaryToCustomDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A : Dictionary<string, int> { public int Value { get; set; } }",
            "class B : Dictionary<string, int> { public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.EnsureCapacity(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToCustomDictionaryWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            A CreateA() => new();
            partial A Map(IDictionary<string, int> source);
            """,
            "class A : Dictionary<string, int> {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateA();
                target.EnsureCapacity(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomDictionaryToCustomDictionaryWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            B CreateB() => new();
            partial B Map(A source);
            """,
            "class A : Dictionary<string, int> { public int Value { get; set; } }",
            "class B : Dictionary<string, int> { public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                target.Value = source.Value;
                target.EnsureCapacity(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void MapToExistingCustomDictionary()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IDictionary<string, int> source, A target);",
            "class A : Dictionary<string, int> {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.EnsureCapacity(source.Count + target.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                """
            );
    }

    [Fact]
    public void CustomDictionaryToExistingCustomDictionary()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B target);",
            "class A : Dictionary<string, int> { public int Value { get; } }",
            "class B : Dictionary<string, int> { public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.Value = source.Value;
                target.EnsureCapacity(source.Count + target.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                """
            );
    }

    [Fact]
    public Task CustomDictionaryWithList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A : Dictionary<int, List<C>>;",
            "class B : Dictionary<int, List<D>>;",
            "record C(int Value);",
            "record D(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DictionaryToCustomDictionaryWithPrivateCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping("IDictionary<string, int>", "A", "class A : Dictionary<string, int> { private A(){} }");
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ReadOnlyDictionaryToCustomTypeReadOnlyReadOnlyDictionaryShouldIgnore()
    {
        var source = TestSourceBuilder.Mapping("IReadOnlyDictionary<string, string>", "A", "class A : IReadOnlyDictionary<int, int> {}");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveMapMethodBody(
                """
                var target = new global::A();
                return target;
                """
            );
    }
}
