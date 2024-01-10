using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class DelegateTest
{
    [Fact]
    public void ActionToAction()
    {
        var source = TestSourceBuilder.Mapping("Action<string>", "Action<string>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void ActionToActionWithDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Action<string>", "Action<string>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void FuncToFunc()
    {
        var source = TestSourceBuilder.Mapping("Func<string, string>", "Func<string, string>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void FuncToFuncWithDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Func<string>", "Func<string>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void CustomDelegateToCustomDelegate()
    {
        var source = TestSourceBuilder.Mapping("X", "X", "delegate string X(string value);");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void CustomDelegateToCustomDelegateWithDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("X", "X", TestSourceBuilderOptions.WithDeepCloning, "delegate string X(string value);");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void FuncToCustomDelegateShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping("Func<string, string>", "X", "delegate string X(string value);");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }
}
