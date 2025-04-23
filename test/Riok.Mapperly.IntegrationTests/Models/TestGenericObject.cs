using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Models
{
    public class TestGenericObject<T, TValue>
        where T : struct
        where TValue : ITestGenericValue<float>
    {
        [MapperIgnore]
        public T ExposedId => Id;

        private T Id { get; set; }

        [MapperIgnore]
        public TValue ExposedValue => Value;

        private TValue Value { get; set; } = default!;

        public static TestGenericObject<int, TestGenericValue> SampleValue =>
            new()
            {
                Id = 10,
                Value = new() { Value = 3.3f },
            };
    }
}
