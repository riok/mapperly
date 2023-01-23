namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class DictionaryTest
{
    [Fact]
    public void DictionaryToSameDictionaryShouldAssign()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, long>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void DictionaryToSameDictionaryDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, long>",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, long>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = (int)item.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToDictionaryNullableToNonNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, int?>",
            "Dictionary<string, int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = item.Value == null ? throw new System.ArgumentNullException(nameof(item.Value)) : item.Value.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToDictionaryNullableToNonNullableWithNoThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, int?>",
            "Dictionary<string, int>",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false });
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = item.Value == null ? default : item.Value.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToIDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "IDictionary<string, int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = (int)item.Value;
    }

    return target;");
    }

    [Fact]
    public void KeyValueEnumerableToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<KeyValuePair<string, int>>",
            "IDictionary<string, int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>();
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public void CustomDictionaryToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "IDictionary<string, int>",
            "class A : Dictionary<string, int> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public void CustomKeyValueListToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "IDictionary<string, int>",
            "class A : List<KeyValuePair<string, int>> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToCustomDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "IDictionary<string, int>",
            "A",
            "class A : Dictionary<string, int> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new A();
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public void DictionaryToCustomDictionaryWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] A CreateA() => new();"
            + "partial A Map(IDictionary<string, int> source);",
            "class A : Dictionary<string, int> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateA();
    foreach (var item in source)
    {
        target[item.Key] = item.Value;
    }

    return target;");
    }

    [Fact]
    public Task DictionaryToCustomDictionaryWithPrivateCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "IDictionary<string, int>",
            "A",
            "class A : Dictionary<string, int> { private A(){} }");
        return TestHelper.VerifyGenerator(source);
    }
}
