namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class TestObjectDtoManuallyMappedProjection
    {
        public TestObjectDtoManuallyMappedProjection(int magicIntValue)
        {
            MagicIntValue = magicIntValue;
        }

        public int MagicIntValue { get; }

        public string? StringValue { get; set; }
    }
}
