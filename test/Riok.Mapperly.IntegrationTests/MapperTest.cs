using Riok.Mapperly.IntegrationTests.Mapper;

namespace Riok.Mapperly.IntegrationTests;

[UsesVerify]
public class MapperTest : BaseMapperTest
{
    [Fact]
    public Task SnapshotGeneratedSource()
    {
        var path = GetGeneratedMapperFilePath(nameof(TestMapper));
        return VerifyFile(path);
    }

    [Fact]
    public Task RunMappingShouldWork()
    {
        var model = NewTestObj();
        var dto = new TestMapper().MapToDto(model);
        return Verify(dto);
    }
}
