namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class TestGenericValueDto : ITestGenericValueDto<float>
    {
        public float Value { get; set; }
    }
}
