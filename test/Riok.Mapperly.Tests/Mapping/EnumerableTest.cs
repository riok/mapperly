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
            .Be("return System.Linq.Enumerable.ToArray(source);");
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
