namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class NullableTest
{
    [Fact]
    public void NullableToNonNullableShouldThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    throw new System.ArgumentNullException(nameof(source));
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void NullableToNullableShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B?",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void NullablePrimitiveToOtherNullablePrimitiveShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "decimal?",
            "int?");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"return source == null ? default : (int)source.Value;");
    }

    [Fact]
    public void NonNullableToNullableShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B?",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void NullableToNonNullableWithNoThrowShouldReturnNewInstance()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false },
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return new B();
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public Task NullableToNonNullableWithNoThrowNoAccessibleCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "string?",
            "B",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false },
            "class B { protected B(){} public static B Parse(string v) => new B(); }");
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NullableToNonNullableStringWithNoThrowShouldReturnEmptyString()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "string",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false },
            "class A { }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source == null ? \"\" : source.ToString();");
    }

    [Fact]
    public void NullableToNonNullableValueTypeWithNoThrowShouldReturnDefault()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime?",
            "DateTime",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false });

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source == null ? default : source.Value;");
    }

    [Fact]
    public void NonNullableToNullableValueType()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "DateTime?");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (System.DateTime? )source;");
    }

    [Fact]
    public void WithExistingInstanceNullableSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A? source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return;
                target.StringValue = source.StringValue;
                """);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "class A {}", "class B {}");

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public void NullableDirectiveEnabledTargetWithSameNullableRefTypeAsPropertyAndInEnumerable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } public string[] Descriptions { get; set; } }",
            "#nullable disable\n class B { public string Value { get; set; } public string[] Descriptions { get; set; } }\n#nullable enable");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new B();
                target.Value = source.Value;
                target.Descriptions = (string[])source.Descriptions;
                return target;
                """);
    }
}
