using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Tests.Descriptors;

public class MemberPathCandidateBuilderTest
{
    [Theory]
    [InlineData("", new string[] { }, new string[] { }, new string[] { })]
    [InlineData("a", new[] { "a" }, new[] { "a" }, new[] { "a", "A" })]
    [InlineData("aa", new[] { "aa" }, new[] { "aa" }, new[] { "aa", "AA" })]
    [InlineData("A", new[] { "A" }, new[] { "A", "a" }, new[] { "A" })]
    [InlineData("aA", new[] { "aA", "a.A" }, new[] { "aA", "a_a", "a.a", "a.A" }, new[] { "aA", "A_A", "A.A", "a.A" })]
    [InlineData(
        "aAA",
        new[] { "aAA", "a.AA", "aA.A", "a.A.A" },
        new[] { "aAA", "a_aa", "a.aa", "a.AA", "aA.A", "a.A.A" },
        new[] { "aAA", "A_AA", "A.AA", "a.AA", "aA.A", "a.A.A" }
    )]
    [InlineData("AB", new[] { "AB", "A.B" }, new[] { "AB", "ab", "A.B" }, new[] { "AB", "A.B" })]
    [InlineData("Value", new[] { "Value" }, new[] { "Value", "value" }, new[] { "Value", "VALUE" })]
    [InlineData(
        "MyValue",
        new[] { "MyValue", "My.Value" },
        new[] { "MyValue", "my_value", "my.value", "My.Value" },
        new[] { "MyValue", "MY_VALUE", "MY.VALUE", "My.Value" }
    )]
    [InlineData(
        "MyValueId",
        new[] { "MyValueId", "My.ValueId", "MyValue.Id", "My.Value.Id" },
        new[] { "MyValueId", "my_value_id", "my.value_id", "my_value.id", "my.value.id", "My.ValueId", "MyValue.Id", "My.Value.Id" },
        new[] { "MyValueId", "MY_VALUE_ID", "MY.VALUE_ID", "MY_VALUE.ID", "MY.VALUE.ID", "My.ValueId", "MyValue.Id", "My.Value.Id" }
    )]
    [InlineData(
        "my_value_id",
        new[] { "my_value_id" },
        new[] { "my_value_id", "my.value_id", "my_value.id", "my.value.id", "MyValueId", "My.ValueId", "MyValue.Id", "My.Value.Id" },
        new[]
        {
            "my_value_id",
            "MY_VALUE_ID",
            "MY.VALUE_ID",
            "MY_VALUE.ID",
            "MY.VALUE.ID",
            "MyValueId",
            "My.ValueId",
            "MyValue.Id",
            "My.Value.Id",
        }
    )]
    [InlineData(
        "MY_VALUE_ID",
        null,
        new[]
        {
            "MY_VALUE_ID",
            "my_value_id",
            "my.value_id",
            "my_value.id",
            "my.value.id",
            "MyValueId",
            "My.ValueId",
            "MyValue.Id",
            "My.Value.Id",
        },
        new[] { "MY_VALUE_ID", "MY.VALUE_ID", "MY_VALUE.ID", "MY.VALUE.ID", "MyValueId", "My.ValueId", "MyValue.Id", "My.Value.Id" }
    )]
    [InlineData(
        "MyValueIdNum",
        new[]
        {
            "MyValueIdNum",
            "My.ValueIdNum",
            "MyValue.IdNum",
            "My.Value.IdNum",
            "MyValueId.Num",
            "My.ValueId.Num",
            "MyValue.Id.Num",
            "My.Value.Id.Num",
        },
        new[]
        {
            "MyValueIdNum",
            "my_value_id_num",
            "my.value_id_num",
            "my_value.id_num",
            "my.value.id_num",
            "my_value_id.num",
            "my.value_id.num",
            "my_value.id.num",
            "my.value.id.num",
            "My.ValueIdNum",
            "MyValue.IdNum",
            "My.Value.IdNum",
            "MyValueId.Num",
            "My.ValueId.Num",
            "MyValue.Id.Num",
            "My.Value.Id.Num",
        },
        new[]
        {
            "MyValueIdNum",
            "MY_VALUE_ID_NUM",
            "MY.VALUE_ID_NUM",
            "MY_VALUE.ID_NUM",
            "MY.VALUE.ID_NUM",
            "MY_VALUE_ID.NUM",
            "MY.VALUE_ID.NUM",
            "MY_VALUE.ID.NUM",
            "MY.VALUE.ID.NUM",
            "My.ValueIdNum",
            "MyValue.IdNum",
            "My.Value.IdNum",
            "MyValueId.Num",
            "My.ValueId.Num",
            "MyValue.Id.Num",
            "My.Value.Id.Num",
        }
    )]
    public void BuildMemberPathCandidatesShouldWork(
        string name,
        string[]? caseSensitiveChunks,
        string[]? snakeCaseChunks,
        string[]? upperSnakeCaseChunks
    )
    {
        if (caseSensitiveChunks != null)
        {
            MemberPathCandidateBuilder
                .BuildMemberPathCandidates(name, PropertyNameMappingStrategy.CaseSensitive)
                .Select(x => x.FullName)
                .ShouldBe(caseSensitiveChunks);
        }

        if (snakeCaseChunks != null)
        {
            MemberPathCandidateBuilder
                .BuildMemberPathCandidates(name, PropertyNameMappingStrategy.SnakeCase)
                .Select(x => x.FullName)
                .ShouldBe(snakeCaseChunks);
        }

        if (upperSnakeCaseChunks != null)
        {
            MemberPathCandidateBuilder
                .BuildMemberPathCandidates(name, PropertyNameMappingStrategy.UpperSnakeCase)
                .Select(x => x.FullName)
                .ShouldBe(upperSnakeCaseChunks);
        }
    }

    [Fact]
    public void BuildMemberPathCandidatesShouldLimitPermutations()
    {
        MemberPathCandidateBuilder
            .BuildMemberPathCandidates("NOT_A_PASCAL_CASE_STRING", PropertyNameMappingStrategy.CaseSensitive)
            .Count()
            .ShouldBe(256);
    }
}
