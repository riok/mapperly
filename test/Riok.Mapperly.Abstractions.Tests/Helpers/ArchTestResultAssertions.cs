using NetArchTest.Rules;

namespace Riok.Mapperly.Abstractions.Tests.Helpers;

internal static class ArchTestResultShouldExtensions
{
    public static void ShouldHaveNoViolations(this TestResult result)
    {
        if (!result.IsSuccessful)
        {
            result.IsSuccessful.ShouldBeTrue("the following types do not conform: " + string.Join(", ", result.FailingTypeNames));
        }
    }
}
