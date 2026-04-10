namespace Riok.Mapperly.IntegrationTests.Models
{
    public record Document(string Title, User CreatedBy, Optional<User> ModifiedBy);

    public record User(string Name);
}
