namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class SupertypeProjectionDto
    {
        public int Id { get; set; }

        public int? MappedValue { get; set; }

        public string MappedName { get; set; } = string.Empty;
    }
}
