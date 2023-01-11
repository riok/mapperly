using Riok.Mapperly.Diagnostics;

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
    public Task StaticMapperShouldEmitDiagnosticForInstanceMethods()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class MyStaticMapper
{
    public partial static object StaticToObject(string s);

    public partial object InstanceToObject(string s);
}
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task InstanceMapperShouldEmitDiagnosticForStaticMethods()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
public partial class MyMapper
{
    public partial static object StaticToObject(string s);

    public partial object InstanceToObject(string s);
}
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithMultipleUserImplementedMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBody(
            "partial int ToInt(string i);" +
            "int ToInt2(string i) => int.Parse(i);");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return int.Parse(i);");
    }

    [Fact]
    public void WithExistingInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("target.StringValue = source.StringValue;");
    }

    [Fact]
    public void WithMultipleUserDefinedMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBody(
            "int ToInt(string i);" +
            "int ToInt2(string i);");

        TestHelper.GenerateMapper(source)
            .Should()
            .AllMethodsHaveBody("return int.Parse(source);");
    }

    [Fact]
    public void WithMultipleUserDefinedMethodDifferentConfigShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(nameof(B.IntValue))] partial B Map(A source);" +
            "[MapperIgnore(nameof(B.StringValue))] partial B Map2(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        var mapper = TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics);
        mapper.Should()
            .HaveOnlyMethods("Map", "Map2")
            .HaveMethodBody("Map", @"var target = new B();
    target.StringValue = source.StringValue;
    return target;")
            .HaveMethodBody("Map2", @"var target = new B();
    target.IntValue = source.IntValue;
    return target;");
    }

    [Fact]
    public void WithSameNamesShouldGenerateUniqueMethodNames()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapToB(A source);",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public B? Value { get; set; } }",
            "class B { public B? Value { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveOnlyMethods("MapToB", "MapToB1");
    }

    [Fact]
    public void WithInvalidSignatureShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial string ToString(T source, string format);");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new DiagnosticMatcher(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "ToString has an unsupported mapping method signature"));
    }

    [Fact]
    public void WithInvalidGenericSignatureShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial string ToString<T>(T source);");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new DiagnosticMatcher(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "ToString has an unsupported mapping method signature"));
    }

    [Fact]
    public Task WithClassBaseTypeShouldWork()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
public partial class BaseMapper : BaseMapper3
{
    public string MyMapping(int value)
        => $""my-to-string-{{value}}"";

    protected partial short MyIntToShortMapping(int value);
}

public interface BaseMapper2 : BaseMapper3
{
    long MyMapping2(int value)
        => (long)value;
}

public interface BaseMapper3
{
    decimal MyMapping3(int value)
        => (decimal)value;
}

[Mapper]
public partial class MyMapper : BaseMapper, BaseMapper2
{
    public partial B Map(A source);
}

class A { public int Value { get; set; } public int Value2 { get; set; } public int Value3 { get; set; } public int Value4 { get; set; } }
class B { public string Value { get; set; } public long Value2 { get; set; } public decimal Value3 { get; set; } public short Value4 { get; set; } }
";
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithMapperClassModifiersShouldCopyModifiersToMapper()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
internal sealed abstract partial class BaseMapper
{
    public partial B AToB(A source);

    protected partial short IntToShort(int value);

    protected abstract string IntToString(int value);
}

class A { public int Value { get; set; } public int Value2 { get; set; } }
class B { public string Value { get; set; } public short Value2 { get; set; } }
";
        return TestHelper.VerifyGenerator(source);
    }
}
