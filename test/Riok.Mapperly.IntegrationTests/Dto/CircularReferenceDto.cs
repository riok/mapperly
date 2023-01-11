namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class CircularReferenceDto
    {
        public int Value { get; set; }

        public CircularReferenceDto? Parent { get; set; }
    }
}
