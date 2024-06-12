using System;
using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseExternalMapperFromAnotherAssemblyTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseExternalMapperFromAnotherAssembly));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var model = new UseExternalMapperFromAnotherAssembly.Source { DateTime = new DateTime(2024, 6, 12, 0, 0, 0) };
            var dto = UseExternalMapperFromAnotherAssembly.MapToTarget(model);
            dto.DateTime.Should().Be(new DateTimeOffset(2024, 6, 12, 0, 0, 0, TimeSpan.Zero));
        }
    }
}
