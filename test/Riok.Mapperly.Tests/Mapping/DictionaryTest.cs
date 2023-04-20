using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
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
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, long>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }

                return target;
                """
            );
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
    public void DictionaryToDictionaryNullableToNonNullable()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, int?>", "Dictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value == null ? throw new System.ArgumentNullException(nameof(item.Value)) : item.Value.Value;
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
                ThrowOnMappingNullMismatch = false
            }
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
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
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>();
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }

                return target;
                """
            );
    }

    [Fact]
    public void CustomDictionaryToIDictionary()
    {
        var source = TestSourceBuilder.Mapping("A", "IDictionary<string, int>", "class A : Dictionary<string, int> {}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }

                return target;
                """
            );
    }

    [Fact]
    public void CustomKeyValueListToIDictionary()
    {
        var source = TestSourceBuilder.Mapping("A", "IDictionary<string, int>", "class A : List<KeyValuePair<string, int>> {}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Dictionary<string, int>(source.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }

                return target;
                """
            );
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
                target.EnsureCapacity(source.Count + target.Count);
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
            "[ObjectFactory] A CreateA() => new();" + "partial A Map(IDictionary<string, int> source);",
            "class A : Dictionary<string, int> {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateA();
                target.EnsureCapacity(source.Count + target.Count);
                foreach (var item in source)
                {
                    target[item.Key] = item.Value;
                }

                return target;
                """
            );
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
            "[ObjectFactory] A CreateA() => new();" + "partial A Map(Dictionary<string, string> source);",
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
            "[ObjectFactory] A CreateA() => new();" + "partial A Map(Dictionary<string, string> source);",
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
            .HaveDiagnostic(new(DiagnosticDescriptors.CannotMapToReadOnlyMember))
            .HaveMapMethodBody(
                """
                var target = new global::A();
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "System.Collections.Immutable.ImmutableDictionary<string, string>"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableSortedDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "System.Collections.Immutable.ImmutableSortedDictionary<string, string>"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary(source);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableDictionaryExplicitCastedKeyValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<long, long>", "System.Collections.Immutable.ImmutableDictionary<int, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source, x => (int)x.Key, x => (int)x.Value);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, long>", "System.Collections.Immutable.ImmutableDictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source, x => x.Key, x => (int)x.Value);
                """
            );
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableDictionaryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Dictionary<string, string> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableDictionary<string, string> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CannotMapToReadOnlyMember));
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableSortedDictionaryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Dictionary<string, string> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedDictionary<string, string> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CannotMapToReadOnlyMember));
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
            .HaveDiagnostic(new(DiagnosticDescriptors.CannotMapToReadOnlyMember))
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void DictionaryMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<long, long>",
            "Dictionary<int, int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Dictionary)
        );
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics).Should().HaveDiagnostics();
    }
}
