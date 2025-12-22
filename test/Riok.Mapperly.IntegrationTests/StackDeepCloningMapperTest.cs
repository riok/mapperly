using System.Collections.Generic;
using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class StackDeepCloningMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(StackDeepCloningMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void StackDeepCloningShouldPreserveOrder()
        {
            var source = new Stack<int>(new[] { 1, 2, 3 });
            // Stack iterates LIFO.
            // new Stack([1, 2, 3]) -> Push 1, Push 2, Push 3.
            // Pop -> 3, 2, 1.
            // ToArray -> [3, 2, 1]

            var copy = StackDeepCloningMapper.Copy(source);

            copy.ShouldNotBeSameAs(source);
            copy.ToArray().ShouldBe(source.ToArray());
        }

        [Fact]
        public void StackDeepCloningLegacyShouldReverseOrder()
        {
            var source = new Stack<int>(new[] { 1, 2, 3 });
            // Source: [3, 2, 1] (Top is 3)

            var copy = StackDeepCloningLegacyMapper.Copy(source);

            copy.ShouldNotBeSameAs(source);
            // Legacy behavior: new Stack(source)
            // source iterates as 3, 2, 1.
            // Push 3, Push 2, Push 1.
            // Copy: [1, 2, 3] (Top is 1)

            copy.ToArray().ShouldBe(new[] { 1, 2, 3 });
        }
    }
}
