using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ExtensionMethodTest
{
    [Fact]
    public Task ExtensionMapMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial B MapToB(this A source);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExtensionExistingTargetShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial void MapToB(this A source, B target);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExtensionExistingTargetAsFirstParamShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial void MapToB([MappingTarget] this B target, A source);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ExtensionExistingTargetDuplicatedParamShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial void MapToB([MappingTarget] this B target, A source, [MappingTarget] B anotherTarget);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "MapToB has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }
}
