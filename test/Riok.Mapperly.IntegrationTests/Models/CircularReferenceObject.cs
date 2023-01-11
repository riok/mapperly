namespace Riok.Mapperly.IntegrationTests.Models
{
    public class CircularReferenceObject
    {
        public int Value { get; set; }

        public CircularReferenceObject? Parent { get; set; }
    }
}
