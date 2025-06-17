using NetArchTest.Rules;
using Riok.Mapperly.Abstractions.Tests.Helpers;

namespace Riok.Mapperly.Abstractions.Tests;

public class AbstractionsArchTests
{
    [Fact]
    public void AbstractionsShouldBeSealed()
    {
        Types
            .InAssembly(typeof(MapperAttribute).Assembly)
            .That()
            .AreNotInterfaces()
            .And()
            // exclude MapperAttribute since it is a lot easier to handle the defaults
            // when it can be inherited.
            .DoNotHaveName(nameof(MapperAttribute))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Fact]
    public void AttributesShouldHaveConditionalAttribute()
    {
        Types
            .InAssembly(typeof(MapperAttribute).Assembly)
            .That()
            .Inherit(typeof(Attribute))
            .Should()
            .MeetCustomRule(new ConditionalAttributeSymbolRule("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME"))
            .GetResult()
            .ShouldHaveNoViolations();
    }
}
