using Riok.Mapperly.IntegrationTests.Mapper;

namespace Riok.Mapperly.IntegrationTests;

[UsesVerify]
public class StaticMapperTest : BaseMapperTest
{
    [Fact]
    public Task SnapshotGeneratedSource()
    {
        var path = GetGeneratedMapperFilePath(nameof(StaticTestMapper));
        return VerifyFile(path);
    }

    [Fact]
    public Task RunMappingShouldWork()
    {
        var model = NewTestObj();
        var dto = StaticTestMapper.MapToDto(model);
        return Verify(dto);
    }

    [Fact]
    public Task RunExtensionMappingShouldWork()
    {
        var model = NewTestObj();
        var dto = model.MapToDtoExt();
        return Verify(dto);
    }
}
