using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Tests.Descriptors;

public class MemberPathCandidateBuilderTest
{
    [Theory]
    [InlineData("Value", new[] { "Value" })]
    [InlineData("MyValue", new[] { "MyValue", "My.Value" })]
    [InlineData("MyValueId", new[] { "MyValueId", "My.ValueId", "MyValue.Id", "My.Value.Id" })]
    public void BuildMemberPathCandidatesShouldWork(string name, string[] chunks)
    {
        MemberPathCandidateBuilder.BuildMemberPathCandidates(name)
            .Select(x => string.Join(".", x))
            .Should()
            .BeEquivalentTo(chunks);
    }
}
