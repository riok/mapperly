namespace Riok.Mapperly.IntegrationTests.Dto
{
    public interface ITestGenericValueDto<T>
    {
        T Value { get; set; }
    }
}
