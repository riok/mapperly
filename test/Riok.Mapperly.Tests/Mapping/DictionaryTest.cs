namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class DictionaryTest
{
    [Fact]
    public void DictionaryToDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "Dictionary<string, int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target.Add(item.Key, (int)item.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void DictionaryToIDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, long>",
            "IDictionary<string, int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target.Add(item.Key, (int)item.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void KeyValueEnumerableToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<KeyValuePair<string, int>>",
            "IDictionary<string, int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new System.Collections.Generic.Dictionary<string, int>();
    foreach (var item in source)
    {
        target.Add(item.Key, item.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void CustomDictionaryToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "IDictionary<string, int>",
            "class A : Dictionary<string, int> {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target.Add(item.Key, item.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void CustomKeyValueListToIDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "IDictionary<string, int>",
            "class A : List<KeyValuePair<string, int>> {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new System.Collections.Generic.Dictionary<string, int>(source.Count);
    foreach (var item in source)
    {
        target.Add(item.Key, item.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void DictionaryToCustomDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "IDictionary<string, int>",
            "A",
            "class A : Dictionary<string, int> {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new A();
    foreach (var item in source)
    {
        target.Add(item.Key, item.Value);
    }

    return target;".ReplaceLineEndings());
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
