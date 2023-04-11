namespace Riok.Mapperly.Tests.Mapping;

public class InheritanceTest
{
    [Fact]
    public void SimpleClassInheritanceInSource()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "C",
            "class A { public string StringValue1 { get; set; } }",
            "class B : A { public string StringValue2 { get; set; } }",
            "class C { public string StringValue1 { get; set; } public string StringValue2 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::C();
                target.StringValue1 = source.StringValue1;
                target.StringValue2 = source.StringValue2;
                return target;
                """);
    }

    [Fact]
    public void MultipleClassInheritanceInSource()
    {
        var source = TestSourceBuilder.Mapping(
            "D",
            "E",
            "class A { public string StringValue1 { get; set; } }",
            "class B : A { public string StringValue2 { get; set; } }",
            "class C : B { public string StringValue3 { get; set; } }",
            "class D : C { public string StringValue4 { get; set; } }",
            "class E { public string StringValue1 { get; set; } public string StringValue2 { get; set; } public string StringValue3 { get; set; } public string StringValue4 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::E();
                target.StringValue1 = source.StringValue1;
                target.StringValue2 = source.StringValue2;
                target.StringValue3 = source.StringValue3;
                target.StringValue4 = source.StringValue4;
                return target;
                """);
    }

    [Fact]
    public void SimpleClassInheritanceInTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "C",
            "class A { public string StringValue1 { get; set; } public string StringValue2 { get; set; } }",
            "class B { public string StringValue1 { get; set; } }",
            "class C : B { public string StringValue2 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::C();
                target.StringValue2 = source.StringValue2;
                target.StringValue1 = source.StringValue1;
                return target;
                """);
    }

    [Fact]
    public void MultipleClassInheritanceInTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "E",
            "class A { public string StringValue1 { get; set; } public string StringValue2 { get; set; } public string StringValue3 { get; set; } public string StringValue4 { get; set; } }",
            "class B { public string StringValue1 { get; set; } }",
            "class C : B { public string StringValue2 { get; set; } }",
            "class D : C { public string StringValue3 { get; set; } }",
            "class E : D { public string StringValue4 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::E();
                target.StringValue4 = source.StringValue4;
                target.StringValue3 = source.StringValue3;
                target.StringValue2 = source.StringValue2;
                target.StringValue1 = source.StringValue1;
                return target;
                """);
    }

    [Fact]
    public void ClassInheritanceInSourceAndTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "D",
            "class A { public string StringValue1 { get; set; } }",
            "class B : A { public string StringValue2 { get; set; } }",
            "class C { public string StringValue1 { get; set; } }",
            "class D : C { public string StringValue2 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::D();
                target.StringValue2 = source.StringValue2;
                target.StringValue1 = source.StringValue1;
                return target;
                """);
    }

    [Fact]
    public void SimpleInterfaceInheritanceInSource()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "C",
            "interface A { string StringValue1 { get; set; } }",
            "interface B : A { string StringValue2 { get; set; } }",
            "class C { public string StringValue1 { get; set; } public string StringValue2 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::C();
                target.StringValue1 = source.StringValue1;
                target.StringValue2 = source.StringValue2;
                return target;
                """);
    }

    [Fact]
    public void MultipleInterfaceInheritanceInSource()
    {
        var source = TestSourceBuilder.Mapping(
            "D",
            "E",
            "interface A { string StringValue1 { get; set; } }",
            "interface B : A { string StringValue2 { get; set; } }",
            "interface C : B { string StringValue3 { get; set; } }",
            "interface D : C { string StringValue4 { get; set; } }",
            "class E { public string StringValue1 { get; set; } public string StringValue2 { get; set; } public string StringValue3 { get; set; } public string StringValue4 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::E();
                target.StringValue1 = source.StringValue1;
                target.StringValue2 = source.StringValue2;
                target.StringValue3 = source.StringValue3;
                target.StringValue4 = source.StringValue4;
                return target;
                """);
    }

    [Fact]
    public void InterfaceInheritanceInSourceAndTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void ToD(B source, D target);",
            "interface A { string StringValue1 { get; set; } }",
            "interface B : A { string StringValue2 { get; set; } }",
            "interface C : B { string StringValue1 { get; set; } }",
            "interface D : C { string StringValue2 { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.StringValue2 = source.StringValue2;
                target.StringValue1 = source.StringValue1;
                """);
    }
}
