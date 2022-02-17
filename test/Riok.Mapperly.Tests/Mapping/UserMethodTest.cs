namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class UserMethodTest
{
    [Fact]
    public Task WithNamespaceShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "int",
            "string",
            TestSourceBuilderOptions.Default with { Namespace = "MyCompany.MyMapper" });
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithImplementationNameShouldWork()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper(ImplementationName = ""MyMapper"")]
public interface IMapper
{
    int Map(string source);
}
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInstanceNameShouldWork()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper(InstanceName = ""MyMapperInstance"")]
public interface IMapper
{
    int Map(string source);
}
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithMultipleUserImplementedMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBody(
            "int ToInt(string i);" +
            "int ToInt2(string i) => int.Parse(i);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return int.Parse(source);");
    }

    [Fact]
    public void WithExistingInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "void Map(A source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("target.StringValue = source.StringValue;");
    }

    [Fact]
    public void WithMultipleUserDefinedMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBody(
            "int ToInt(string i);" +
            "int ToInt2(string i);");

        TestHelper.GenerateMapperMethodBodies(source)
            .Select(x => x.Body)
            .Should()
            .AllBe("return int.Parse(source);");
    }

    [Fact]
    public void WithMultipleUserDefinedMethodDifferentConfigShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(nameof(B.IntValue))] B Map(A source);" +
            "[MapperIgnore(nameof(B.StringValue))] B Map2(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        var mappingMethods = TestHelper.GenerateMapperMethodBodies(source)
            .ToDictionary(x => x.Name, x => x.Body);
        mappingMethods.Should().HaveCount(2);
        mappingMethods["Map"].Should().Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());

        mappingMethods["Map2"].Should().Be(@"var target = new B();
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void WithSameNamesShouldGenerateUniqueMethodNames()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "B MapToB(A source);",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public B? Value { get; set; } }",
            "class B { public B? Value { get; set; } }");

        TestHelper.GenerateMapperMethodBodies(source)
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo("MapToB", "MapToB1");
    }

    [Fact]
    public Task WithInvalidSignatureShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "string ToString(T source, string format);");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInvalidGenericSignatureShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "string ToString<T>(T source);");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInterfaceBaseTypeShouldWork()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

public interface BaseMapper : BaseMapper3
{{
    string MyMapping(int value)
        => $""my-to-string-{{value}}"";
}}

public interface BaseMapper2 : BaseMapper3
{
    long MyMapping2(int value)
        => (long)value;
}

public interface BaseMapper3
{{
    decimal MyMapping3(int value)
        => (decimal)value;
}}

[Mapper]
public interface IMapper : BaseMapper, BaseMapper2
{
    B Map(A source);
}

class A { public int Value { get; set; } public int Value2 { get; set; } public int Value3 { get; set; } }
class B { public string Value { get; set; } public long Value2 { get; set; } public decimal Value3 { get; set; } }
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WitClassBaseTypeShouldWork()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

public class BaseMapper : BaseMapper3
{{
    public string MyMapping(int value)
        => $""my-to-string-{{value}}"";
}}

public interface BaseMapper2 : BaseMapper3
{
    long MyMapping2(int value)
        => (long)value;
}

public interface BaseMapper3
{{
    decimal MyMapping3(int value)
        => (decimal)value;
}}

[Mapper]
public abstract class MyMapper : BaseMapper, BaseMapper2
{
    public abstract B Map(A source);
}

class A { public int Value { get; set; } public int Value2 { get; set; } public int Value3 { get; set; } }
class B { public string Value { get; set; } public long Value2 { get; set; } public decimal Value3 { get; set; } }
";
        return TestHelper.VerifyGenerator(source);
    }
}
