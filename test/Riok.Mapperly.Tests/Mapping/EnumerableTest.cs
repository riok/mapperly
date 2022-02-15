namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableTest
{
    [Fact]
    public void ArrayToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int[]");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (int[])source.Clone();");
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(source, x => MapToB(x)));");
    }

    [Fact]
    public void ArrayToArrayOfString()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string[]");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (string[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "long[]",
            "int[]");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "int[]");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToEnumerableOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IEnumerable<int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (System.Collections.Generic.IEnumerable<int>)source;");
    }

    [Fact]
    public void EnumerableToICollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "ICollection<int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfImplicitTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<long>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (long)x));");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            "class B : ICollection<int> {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    foreach (var item in source)
    {
        target.Add((int)item);
    }

    return target;".ReplaceLineEndings());
    }
}
