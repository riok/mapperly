using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using NetArchTest.Rules;

namespace Riok.Mapperly.Abstractions.Tests.Helpers;

internal class ArchTestResultAssertions(TestResult value)
    : ObjectAssertions<TestResult, ArchTestResultAssertions>(value, AssertionChain.GetOrCreate())
{
    public void BeSuccessful()
    {
        if (!Subject.IsSuccessful)
        {
            Subject.IsSuccessful.Should().BeTrue("the following types do not conform: " + string.Join(", ", Subject.FailingTypeNames));
        }
    }
}
