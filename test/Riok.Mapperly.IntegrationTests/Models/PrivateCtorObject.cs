namespace Riok.Mapperly.IntegrationTests.Models
{
    public class PrivateCtorObject
    {
        private PrivateCtorObject() { }

        private int intValue;
        private string stringValue = string.Empty;

        public static PrivateCtorObject CreateObject(int intValue, string stringValue)
        {
            var obj = new PrivateCtorObject { intValue = intValue, stringValue = stringValue };
            return obj;
        }

        public int ExposeIntValue() => intValue;

        public string ExposeStringValue() => stringValue;
    }
}
