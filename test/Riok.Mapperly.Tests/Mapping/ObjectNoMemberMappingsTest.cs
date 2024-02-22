using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectNoMemberMappingsTest
{
    [Fact]
    public void NoMemberMappingsShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "class A;", "class B;");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoMemberMappings, "No members are mapped in the object mapping from A to B")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void NoMemberMappingsInNestedShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C;",
            "class D;"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoMemberMappings, "No members are mapped in the object mapping from C to D")
            .HaveAssertedAllDiagnostics();
    }
}
