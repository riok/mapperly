// using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Tests.Descriptors;

public class MemberPathCandidateBuilderTest
{
    // [Theory]
    // [InlineData("", new string[] { })]
    // [InlineData("a", new[] { "a" })]
    // [InlineData("A", new[] { "A" })]
    // [InlineData("aA", new[] { "aA", "a.A" })]
    // [InlineData("AB", new[] { "AB", "A.B" })]
    // [InlineData("Value", new[] { "Value" })]
    // [InlineData("MyValue", new[] { "MyValue", "My.Value" })]
    // [InlineData("MyValueId", new[] { "MyValueId", "My.ValueId", "MyValue.Id", "My.Value.Id" })]
    // [InlineData(
    //     "MyValueIdNum",
    //     new[]
    //     {
    //         "MyValueIdNum",
    //         "My.ValueIdNum",
    //         "MyValue.IdNum",
    //         "My.Value.IdNum",
    //         "MyValueId.Num",
    //         "My.ValueId.Num",
    //         "MyValue.Id.Num",
    //         "My.Value.Id.Num"
    //     }
    // )]
    // public void BuildMemberPathCandidatesShouldWork(string name, string[] chunks)
    // {
    //     MemberPathCandidateBuilder
    //         .BuildMemberPathCandidates(name)
    //         .Select(x => string.Join(".", x))
    //         .Should()
    //         .BeEquivalentTo(chunks, o => o.WithStrictOrdering());
    // }
}
