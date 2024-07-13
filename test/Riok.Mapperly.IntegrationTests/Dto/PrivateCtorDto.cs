namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class PrivateCtorDto
    {
        private PrivateCtorDto(int intValue)
        {
            this.intValue = intValue;
        }

        private int intValue;
        private string stringValue = string.Empty;

        public int ExposeIntValue() => intValue;

        public string ExposeStringValue() => stringValue;
    }
}
