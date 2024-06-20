namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class ConstantValuesDto
    {
        public ConstantValuesDto(string ctorConstantValue, int ctorMappedValue)
        {
            CtorMappedValue = ctorMappedValue;
            CtorConstantValue = ctorConstantValue;
        }

        public int MappedValue { get; set; }
        public int CtorMappedValue { get; }
        public string CtorConstantValue { get; }

        public int ConstantValue { get; set; }
        public int ConstantValueByMethod { get; set; }
    }
}
