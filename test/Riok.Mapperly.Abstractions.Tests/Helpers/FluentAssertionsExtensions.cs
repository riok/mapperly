using NetArchTest.Rules;

namespace Riok.Mapperly.Abstractions.Tests.Helpers;

internal static class FluentAssertionsExtensions
{
    public static ArchTestResultAssertions Should(this TestResult result) => new(result);
}
