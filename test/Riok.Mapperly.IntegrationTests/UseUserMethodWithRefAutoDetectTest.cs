using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseUserMethodWithRefAutoDetectTest : BaseMapperTest
    {
        [Fact]
        public void RunMappingWithAutoDetectedRefMethod()
        {
            var src = new TestGenericObject<Optional<string>> { Value = new Optional<string>("hello") };
            var target = new TestGenericObject<string>();
            UseUserMethodWithRefAutoDetect.Map(target, src);
            target.Value.ShouldBe("hello");
        }

        private class TestGenericObject<T> : ITestGenericValue<T>
        {
            public T Value { get; set; } = default!;
        }
    }
}
