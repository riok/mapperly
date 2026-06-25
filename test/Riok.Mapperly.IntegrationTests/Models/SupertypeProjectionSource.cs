namespace Riok.Mapperly.IntegrationTests.Models
{
    public interface ISupertypeProjectionValue
    {
        int? Value { get; }
    }

    public abstract class SupertypeProjectionBase
    {
        public string Name { get; set; } = string.Empty;
    }

    public class SupertypeProjectionSource : SupertypeProjectionBase, ISupertypeProjectionValue
    {
        public int Id { get; set; }

        public int? Value { get; set; }
    }
}
