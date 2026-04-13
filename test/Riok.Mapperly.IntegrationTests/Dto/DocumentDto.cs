using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public record DocumentDto(string Title, UserDto CreatedBy, Optional<UserDto> ModifiedBy);

    public record UserDto(string Name);
}
