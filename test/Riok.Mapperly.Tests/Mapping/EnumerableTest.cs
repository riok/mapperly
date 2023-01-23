namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class EnumerableTest
{
    [Fact]
    public void ArrayToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void NullableArrayToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]?",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source == null ? throw new System.ArgumentNullException(nameof(source)) : source;");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int?[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new int[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i].Value;
    }

    return target;");
    }

    [Fact]
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int?[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new int? [source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = (int? )source[i];
    }

    return target;");
    }

    [Fact]
    public void ArrayToArrayOfPrimitiveTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (int[])source.Clone();");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int?[]",
            "int[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new int[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i].Value;
    }

    return target;");
    }

    [Fact]
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int?[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new int? [source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = (int? )source[i];
    }

    return target;");
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayCustomClassNullableToArrayCustomClassNonNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "B?[]",
            "B[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new B[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i];
    }

    return target;");
    }

    [Fact]
    public void ArrayCustomClassNonNullableToArrayCustomClassNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B?[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (B? [])source;");
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClassDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "class B { public int Value { get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(@"var target = new B[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = MapToB(source[i]);
    }

    return target;");
    }

    [Fact]
    public void ArrayToArrayOfString()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfNullableString()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string?[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string? [])source;");
    }

    [Fact]
    public void ArrayToArrayOfStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfNullableStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string?[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string? [])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfReadonlyStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            "readonly struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfReadonlyStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "readonly struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (A[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfMutableStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(@"var target = new A[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = MapToA(source[i]);
    }

    return target;");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningLoopNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] i);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(@"var target = new A[i.Length];
    for (var i1 = 0; i1 < i.Length; i1++)
    {
        target[i1] = MapToA(i[i1]);
    }

    return target;");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningTargetNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] target);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(@"var target1 = new A[target.Length];
    for (var i = 0; i < target.Length; i++)
    {
        target1[i] = MapToA(target[i]);
    }

    return target1;");
    }

    [Fact]
    public void ArrayToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "long[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new int[source.Length];
    for (var i = 0; i < source.Length; i++)
    {
        target[i] = (int)source[i];
    }

    return target;");
    }

    [Fact]
    public void EnumerableToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToEnumerableOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IEnumerable<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void EnumerableToICollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "ICollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfImplicitTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<long>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (long)x));");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToIListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IList<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "List<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToIReadOnlyListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IReadOnlyList<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            "class B : ICollection<int> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new B();
    foreach (var item in source)
    {
        target.Add((int)item);
    }

    return target;");
    }

    [Fact]
    public void EnumerableToCustomCollectionWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new();"
            + "partial B Map(IEnumerable<long> source);",
            "class B : ICollection<int> {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB();
    foreach (var item in source)
    {
        target.Add((int)item);
    }

    return target;");
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInSelectClause()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C[] Value { get; set;} }",
            "class B { public D[] Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }
}
