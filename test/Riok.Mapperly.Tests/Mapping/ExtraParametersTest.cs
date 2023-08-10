using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ExtraParametersTest
{
    [Fact]
    public Task MapWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("partial string MapTo(int source, string format);");
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapShouldNotPassParametersDown()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapTo(A source, string stringValue);",
            "class A { public int[] Collection { get; set; } }",
            "class B { public string[] Collection { get; set; } public string StringValue { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MapWithParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source, string stringValue);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.StringValue = stringValue;
                return target;
                """
            );
    }

    [Fact]
    public void MapConstructorWithParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source, string stringValue);",
            "class A { public int Value { get; set; } }",
            "class B { public B(string stringValue) { } public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(stringValue);
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapRequiredWithParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source, string stringValue);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } public required string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    StringValue = stringValue
                };
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapWithParameterShouldConvert()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapTo(A source, int stringValue);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.StringValue = stringValue.ToString();
                return target;
                """
            );
    }

    [Fact]
    public void ExistingTargetMapWithParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapTo(A source, IEnumerable<int> collection);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } public List<int> Collection { get; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(collection, out var sourceCount))
                {
                    target.Collection.EnsureCapacity(sourceCount + target.Collection.Count);
                }

                foreach (var item in collection)
                {
                    target.Collection.Add(item);
                }

                return target;
                """
            );
    }

    [Fact]
    public void TwoExtraParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value, int id);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; set; } public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.Value = value.ToString();
                target.Id = id;
                return target;
                """
            );
    }

    [Fact]
    public void WithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
        [MapProperty("value", "Id")]
        partial B Map(A src, int value);
        """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.Id = value;
                return target;
                """
            );
    }

    [Fact]
    public void MapRequiredWithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("stringValue", "StringValue")]
                partial B MapTo(A source, string stringValue);
                """,
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } public required string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    StringValue = stringValue
                };
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapConstructorWithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("input", "stringValue")]
                partial B MapTo(A source, string input);
                """,
            "class A { public int Value { get; set; } }",
            "class B { public B(string stringValue) { } public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(input);
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void WithNestedMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
        [MapProperty("value.Id", "Id")]
        partial B Map(A src, C value);
        """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int Id { get; set; } }",
            "class C { public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.Id = value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void UnusedParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceParameterNotMapped)
            .HaveAssertedAllDiagnostics();
    }
}
