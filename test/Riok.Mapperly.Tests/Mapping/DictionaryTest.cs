using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class DictionaryTest
{
    [Fact]
    public void DictionaryToSameDictionaryShouldAssign()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, long>", "Dictionary<string, long>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void DictionaryToSameDictionaryDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, long>",
            TestSourceBuilderOptions.WithDeepCloning
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::System.Collections.Generic.Dictionary<string, long>(source);");
    }

    [Fact]
    public void DictionaryToSameDictionaryDeepCloningInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "Dictionary<string, string>",
            TestSourceBuilderOptions.WithDeepCloning
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::System.Collections.Generic.Dictionary<string, string?>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value == null ? default : item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public Task DictionaryToDictionaryWithNonNullableKeyAndValueDeepCloningInDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source)",
            TestSourceBuilderOptions.WithDeepCloning,
            """
            class A
            {
                public Dictionary<string, string> Value { get; set; }
            }
            """,
            """
            class B
            {
                public Dictionary<string, string> Value { get; set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public void DictionaryToDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, long>", "Dictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = (int)item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToDictionaryExplicitCastedValueDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, int>",
            TestSourceBuilderOptions.WithDeepCloning
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = (int)item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToDictionaryNullableToNonNullable()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, int?>", "Dictionary<string, int>");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type int? to target of type int which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value ?? throw new global::System.ArgumentNullException(nameof(item.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToDictionaryNullableToNonNullableWithNoThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, int?>",
            "Dictionary<string, int>",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false,
            }
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type int? to target of type int which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value == null ? default : item.Value.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToIDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, long>", "IDictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = (int)item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void KeyValueEnumerableToIDictionary()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<KeyValuePair<string, int>>", "IDictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::System.Collections.Generic.Dictionary<string, int>(source);");
    }

    [Fact]
    public void MapToExistingDictionary()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IDictionary<string, int> source, Dictionary<string, int> target);"
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
    public Task DictionaryWithList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(Dictionary<int, List<C>> Dict);",
            "record B(Dictionary<int, List<D>> Dict);",
            "record C(int Value);",
            "record D(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void KeyValueEnumerableToExistingDictionary()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<KeyValuePair<string, int>> source, Dictionary<string, int> target);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount))
                {
                    target.EnsureCapacity(sourceCount + target.Count);
                }
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                """
            );
    }

    [Fact]
    public void IDictionaryToExplicitDictionaryShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "IDictionary<string, string>",
            "A",
            """
            public class A : IDictionary<string, string>
            {
                string IDictionary<string, string>.this[string key]
                {
                    get => _dictionaryImplementation[key];
                    set => _dictionaryImplementation[key] = value;
                }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                var targetDict = (global::System.Collections.Generic.IDictionary<string, string>)target;
                foreach (var item in source)
                {
                    targetDict[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToImplicitDictionaryShouldNotCast()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "A",
            """
            public class A : IDictionary<string, string>
            {
                public string this[string key]
                {
                    get => _dictionaryImplementation[key];
                    set => _dictionaryImplementation[key] = value;
                }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToExistingExplicitDictionaryShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Dictionary<string, string> Values { get; } }",
            "class B { public C Values { get; } }",
            """
            public class C : IDictionary<string, string>
            {
                string IDictionary<string, string>.this[string key]
                {
                    get => _dictionaryImplementation[key];
                    set => _dictionaryImplementation[key] = value;
                }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                var targetDict = (global::System.Collections.Generic.IDictionary<string, string>)target.Values;
                foreach (var item in source.Values)
                {
                    targetDict[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToExplicitDictionaryWithObjectFactoryShouldCast()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            A CreateA() => new();
            partial A Map(Dictionary<string, string> source);
            """,
            """
            public class A : IDictionary<string, string>
            {
                string IDictionary<string, string>.this[string key]
                {
                    get => _dictionaryImplementation[key];
                    set => _dictionaryImplementation[key] = value;
                }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateA();
                var targetDict = (global::System.Collections.Generic.IDictionary<string, string>)target;
                foreach (var item in source)
                {
                    targetDict[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToImplicitDictionaryWithObjectFactoryShouldNotCast()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            A CreateA() => new();
            partial A Map(Dictionary<string, string> source);
            """,
            """
            public class A : IDictionary<string, string>
            {
                public string this[string key]
                {
                    get => _dictionaryImplementation[key];
                    set => _dictionaryImplementation[key] = value;
                }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateA();
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlyDictionaryToReadOnlyDictionaryExistingInstanceShouldIgnore()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IReadOnlyDictionary<string, string> Values { get; } }",
            "class B { public IReadOnlyDictionary<string, string> Values { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryMappingDisabledShouldUseEnumerableMapping()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<long, long>",
            "Dictionary<int, int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Dictionary)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                return new global::System.Collections.Generic.Dictionary<int, int>(
                    global::System.Linq.Enumerable.Select(source, x => MapToKeyValuePairOfInt32AndInt32(x))
                );
                """
            );
    }

    [Fact]
    public void DictionaryMappingExistingTargetDisabledShouldIgnore()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Dictionary),
            "class A { public Dictionary<long, long> Value { get; } }",
            "class B { public Dictionary<int, int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public Task DictionaryShouldReuseForReadOnlyDictionaryImplementorsButDifferentForIDictionary()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial BDictionary Map(ADictionary source);
            public partial BDictionaryAgain Map(ADictionaryAgain source);
            public partial BReadOnlyDictionary Map(ADictionary source);
            public partial BCustomDictionary Map(ADictionary source);
            public partial BDictionary Map(ADictionary source);g
            public partial BReadOnlyDictionary Map(AReadOnlyDictionary source);
            public partial BCustomDictionary Map(ACustomReadOnlyDictionary source);
            public partial BDictionary Map(ADictionary source);
            public partial BCustomDictionary Map(ACustomDictionary source);
            """,
            "record ADictionary(Dictionary<int, C> Values);",
            "record ADictionaryAgain(Dictionary<int, C> Values);",
            "record AReadOnlyDictionary(IReadOnlyDictionary<int, C> Values);",
            "record ACustomReadOnlyDictionary(CustomReadOnlyDictionary<C> Values);",
            "record ADictionary(IDictionary<int, C> Values);",
            "record ACustomDictionary(CustomDictionary<C> Values);",
            "record BDictionary(Dictionary<int, D> Values);",
            "record BDictionaryAgain(Dictionary<int, D> Values);",
            "record BReadOnlyDictionary(IReadOnlyDictionary<int, D> Values);",
            "record BDictionary(IDictionary<int, D> Values);",
            "record BCustomDictionary(CustomDictionary<D> Values);",
            "public class CustomReadOnlyDictionary<T> : IReadOnlyDictionary<int, T>;",
            "public class CustomDictionary<T> : IDictionary<int, T>;",
            "record C(int Value);",
            "record D(int Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
